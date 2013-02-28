using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace GitDeployHub.Web.Engine
{
    public class Deployment
    {
        public enum DeploymentStatus
        {
            Pending,
            InProgress,
            Complete
        }

        [ScriptIgnore]
        public Instance Instance { get; private set; }

        public string InstanceName
        {
            get { return Instance.Name; }
        }

        [ScriptIgnore]
        public Hub Hub { get; private set; }

        public DateTime Created { get; private set; }

        public DateTime Started { get; private set; }

        public DateTime Completed { get; private set; }

        public IDictionary<string, string> Parameters { get; set; }

        public DeploymentStatus Status { get; private set; }

        [ScriptIgnore]
        public Exception Exception { get; set; }

        public string ExceptionStackTrace
        {
            get { return Exception == null ? "" : Exception.ToString(); }
        }

        public bool Succeeded { get { return Exception == null; } }

        public string Tags { get; private set; }

        private StringBuilder _log = new StringBuilder();

        public string Log { get { return _log.ToString(); } }

        public Task CurrentTask { get; private set; }

        private object syncRoot = new object();

        public Deployment(Hub hub, Instance instance)
        {
            Hub = hub;
            Instance = instance;
            Created = DateTime.UtcNow;
        }

        public bool IsAllowed(HttpContextBase context)
        {
            var allowedAddresses = Config.GitDeployHub.Settings.AllowedAddresses ?? "";
            if (string.IsNullOrWhiteSpace(allowedAddresses) || allowedAddresses.ToLowerInvariant() == "none")
            {
                return false;
            }
            var method = context.Request.HttpMethod.ToUpperInvariant();
            if (method != "POST" && method != "PUT")
            {
                return false;
            }
            var allowed = false;
            var requestAddress = context.Request.UserHostAddress ?? "null";
            foreach (var address in allowedAddresses.Split(new[] { ',', ';', ' ' }))
            {
                if (!string.IsNullOrWhiteSpace(address))
                {
                    if (address == "*")
                    {
                        return true;
                    }
                    var regex = new Regex("^" + address.Replace(".", "\\.").Replace("*", "\\d+") + "$");
                    if (regex.IsMatch(requestAddress))
                    {
                        allowed = true;
                        break;
                    }
                }
            }
            return allowed;
        }

        private void Execute()
        {
            bool pullNeeded = true;

            try
            {
                while (Instance.CurrentDeployment != this)
                {
                    lock (syncRoot)
                    {
                        if (Instance.CurrentDeployment == null)
                        {
                            Instance.CurrentDeployment = this;
                            break;
                        }
                    }
                    _log.AppendLine("Waiting for current deployment to finish");
                    Instance.CurrentDeployment.CurrentTask.Wait();
                }

                Started = DateTime.UtcNow;
                if (Status != DeploymentStatus.Pending)
                {
                    throw new Exception("Cannot execute a Deployment already in status: " + Status);
                }
                _log.AppendFormat("Started ({0})", Started.ToString("s"));
                _log.AppendLine();
                Status = DeploymentStatus.InProgress;
                if (!Instance.HasFolder(".git"))
                {
                    throw new Exception("The specified folder is not a git repository workspace: " + Instance.Folder);
                }

                bool isBehind, canFastForward;
                Instance.Fetch(_log);
                Instance.Status(_log, out isBehind, out canFastForward);

                pullNeeded = isBehind;
                if (pullNeeded)
                {
                    if (!canFastForward)
                    {
                        throw new Exception("There are changes, but I can't fast-forward!");
                    }

                    Instance.ExecutePreDeploy(_log);

                    Instance.Pull(_log);

                    Instance.ExecutePostDeploy(_log);
                }
            }
            catch (Exception ex)
            {
                _log.AppendLine("Error: " + ex.Message);
                Exception = ex;
            }
            finally
            {
                Completed = DateTime.UtcNow;
                _log.AppendFormat("Completed ({0})", Completed - Started);
                Status = DeploymentStatus.Complete;
                if (pullNeeded || Instance.LastDeployment == null)
                {
                    Instance.LastDeployment = this;
                    Hub.DeploymentHistory.Add(this);
                    while (Hub.DeploymentHistory.Count > 50 || DateTime.UtcNow.Subtract(Hub.DeploymentHistory[0].Completed).TotalDays > 14)
                    {
                        Hub.DeploymentHistory.RemoveAt(0);
                    }
                    try
                    {
                        var serializer = new JavaScriptSerializer();
                        var json = serializer.Serialize(this);
                        File.AppendAllText(Path.Combine(Instance.MappedApplicationPath, "history.log"),
                                           json + Environment.NewLine);
                    }
                    catch
                    {
                        // failed to log
                    }
                }
                Hub.DeploymentQueue.Remove(this);
                Instance.CurrentDeployment = null;
            }

        }

        public Task ExecuteAsync()
        {
            return CurrentTask = Task.Factory.StartNew(Execute);
        }

    }
}
