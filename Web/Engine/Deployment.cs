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
            get { return Exception.ToString(); }
        }

        public bool Succeeded { get { return Exception == null; } }

        private StringBuilder _log = new StringBuilder();

        public string Log { get { return _log.ToString(); } }

        public Deployment(Hub hub, Instance instance)
        {
            Hub = hub;
            Instance = instance;
            Created = DateTime.UtcNow;
        }

        public bool IsAllowed(HttpContext context)
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

        public void Execute()
        {
            try
            {
                Started = DateTime.UtcNow;
                if (Status != DeploymentStatus.Pending)
                {
                    throw new Exception("Cannot execute a Deployment already in status: " + Status);
                }
                _log.AppendFormat("Started ({0})", Started.ToString("s"));
                _log.AppendLine();
                Status = DeploymentStatus.InProgress;
                if (!Directory.Exists(Path.Combine(Instance.Folder, ".git")))
                {
                    throw new Exception("The specified folder is not a git repository workspace: " + Instance.Folder);
                }

                // TO-DO: execute pre-deploy tasks (allow validations)

                _log.AppendLine("running git pull");
                var process = Process.Start(new ProcessStartInfo("git", "pull")
                    {
                        UseShellExecute = false,
                        WorkingDirectory = Instance.Folder,
                        RedirectStandardOutput = true
                    });
                process.WaitForExit();
                _log.Append(process.StandardOutput.ReadToEnd());
                if (process.ExitCode != 0)
                {
                    throw new Exception("git pull failed with exit code: " + process.ExitCode);
                }

                // TO-DO: execute post-deploy tasks
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
                Hub.DeploymentHistory.Add(this);
                Hub.DeploymentQueue.Remove(this);
                Instance.LastDeployment = this;
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

        }

        public Task ExecuteAsync()
        {
            return Task.Factory.StartNew(Execute);
        }
    }
}
