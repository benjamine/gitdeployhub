using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GitDeployHub.Web.Engine.Processes;

namespace GitDeployHub.Web.Engine.Notifiers
{
    public abstract class Notifier
    {
        public string Key { get; set; }

        public IDictionary<string, string> Settings { get; set; }

        public void Notify(BaseProcess process)
        {
            try
            {
                if (!ShouldNotify(process))
                {
                    return;
                }
                process.Log(string.Format("Notifying using {0}:{1}", this.GetType().Name, Key));
                DoNotify(process);
                process.Log("Notified");
            }
            catch (Exception ex)
            {
                process.Log(string.Format("Error executing {0}: {1}", this.GetType().Name, ex.Message));
            }
        }

        protected abstract void DoNotify(BaseProcess process);

        protected virtual bool ShouldNotify(BaseProcess process)
        {
            if (!(process is Deployment))
            {
                return false;
            }
            if (((Deployment) process).Dry)
            {
                return false;
            }
            if (process.Skipped && Settings.ContainsKey("IgnoreSkipped") && Settings["IgnoreSkipped"].ToLowerInvariant() == "true")
            {
                return false;
            }
            if (process.Succeeded && Settings.ContainsKey("IgnoreSucceeded") && Settings["IgnoreSucceeded"].ToLowerInvariant() == "true")
            {
                return false;
            }
            if (!process.Succeeded && Settings.ContainsKey("IgnoreFailed") && Settings["IgnoreFailed"].ToLowerInvariant() == "true")
            {
                return false;
            }
            return true;
        }

        public string ParseTemplateTextKeywords(string template, IDictionary<string, Func<string>> keywords, Func<string, string> valueEncoder = null)
        {
            foreach (var keyword in keywords)
            {
                var keywordLabel = "${" + keyword.Key + "}";
                if (template.Contains(keywordLabel))
                {
                    template = template.Replace(keywordLabel, valueEncoder == null ?
                        keyword.Value() : valueEncoder(keyword.Value()));
                }
            }
            return template;
        }

        public string ParseTemplateText(string template, BaseProcess process, Func<string, string> valueEncoder = null)
        {
            return ParseTemplateTextKeywords(template, new Dictionary<string, Func<string>>()
                {
                    {"instanceName", () => process.InstanceName},
                    {"result", () => process.Skipped ? "skipped" : (process.Succeeded ? "OK" : "failed")},
                    {"tags", () => string.Join(",", process.Instance.Tags)},
                    {"log", () => process.FullLog },
                    {"changeLog", () => string.Join(Environment.NewLine, process.Instance.ChangeLog)},
                    {"changeLogShort", () => string.Join(Environment.NewLine, process.Instance.ChangeLogShort)},
                    {"changeLogLast", () => string.Join(Environment.NewLine, process.Instance.ChangeLogLast)},
                    {"treeish", () => process.Instance.Treeish},
                    {"machineName", () => Environment.MachineName},
                }, valueEncoder);
        }

    }
}