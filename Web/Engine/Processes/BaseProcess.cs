using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace GitDeployHub.Web.Engine.Processes
{
    public abstract class BaseProcess : ILog
    {
        public enum ProcessStatus
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

        public bool Skipped { get; protected set; }

        public IDictionary<string, string> Parameters { get; set; }

        public ProcessStatus Status { get; private set; }

        [ScriptIgnore]
        public Exception Exception { get; set; }

        public string ExceptionStackTrace
        {
            get { return Exception == null ? "" : Exception.ToString(); }
        }

        public bool Succeeded { get { return Exception == null; } }

        public Task CurrentTask { get; private set; }

        private object syncRoot = new object();

        private StringLog _log = new StringLog();

        public string FullLog { get { return _log.Output; } }

        public BaseProcess(Hub hub, Instance instance)
        {
            Hub = hub;
            Instance = instance;
            Created = DateTime.UtcNow;
        }

        private void Execute()
        {
            try
            {
                while (Instance.CurrentProcess != this)
                {
                    lock (syncRoot)
                    {
                        if (Instance.CurrentProcess == null)
                        {
                            Instance.CurrentProcess = this;
                            break;
                        }
                    }
                    Log("Waiting for current process to finish");
                    Instance.CurrentProcess.CurrentTask.Wait();
                }

                Started = DateTime.UtcNow;
                if (Status != ProcessStatus.Pending)
                {
                    throw new Exception("Cannot execute a process already in status: " + Status);
                }
                Log(string.Format("Started ({0})", Started.ToString("s")));
                Log(string.Format("Instance: {0}", Instance.Name));
                LogNewLine();
                Status = ProcessStatus.InProgress;

                DoExecute();

            }
            catch (Exception ex)
            {
                Exception = ex;
                Log("Error: " + ex.Message);
                if (Hub.TraceSource.Switch.ShouldTrace(TraceEventType.Error))
                {
                    Hub.TraceSource.TraceEvent(TraceEventType.Error, -1, ex.Message, ex);
                }
            }
            finally
            {
                Status = ProcessStatus.Complete;
                Completed = DateTime.UtcNow;

                if (Instance.Notifiers != null)
                {
                    foreach (var notifier in Instance.Notifiers)
                    {
                        notifier.Notify(this);
                    }
                }

                Log(string.Format("Completed ({0})", Completed - Started));
                if (!Skipped)
                {
                    Hub.ProcessHistory.Add(this);
                    while (Hub.ProcessHistory.Count > 10 || DateTime.UtcNow.Subtract(Hub.ProcessHistory[0].Completed).TotalDays > 14)
                    {
                        Hub.ProcessHistory.RemoveAt(0);
                    }
                }

                Hub.Queue.Remove(this);
                Instance.CurrentProcess = null;
            }

        }

        public Task ExecuteAsync()
        {
            return CurrentTask = Task.Factory.StartNew(Execute);
        }

        protected abstract void DoExecute();

        public void Log(string message)
        {
            _log.Log(message);
            Instance.Log(message, this);
        }

        public void LogNewLine()
        {
            _log.LogNewLine();
            Instance.Log("", this);
        }
    }
}
