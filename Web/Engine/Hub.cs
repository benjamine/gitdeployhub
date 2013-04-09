using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using GitDeployHub.Web.Engine.Config;
using GitDeployHub.Web.Engine.Notifiers;
using GitDeployHub.Web.Engine.Processes;

namespace GitDeployHub.Web.Engine
{
    public class Hub
    {
        #region SingletonImpl

        private static Hub _hub;

        public static Hub Instance
        {
            get
            {
                if (_hub == null)
                {
                    _hub = new Hub();
                }
                return _hub;
            }
        }

        private Hub()
        {
            foreach (var instanceConfig in Config.GitDeployHub.Settings.Instances.OfType<InstanceElement>())
            {
                var instance = new Instance(this, instanceConfig.Name, instanceConfig.Treeish, instanceConfig.Folder);
                if (!string.IsNullOrWhiteSpace(instanceConfig.EnvironmentVariables))
                {
                    foreach (var pair in instanceConfig.EnvironmentVariables
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var equalsIndex = pair.IndexOf('=');
                        if (equalsIndex >= 0)
                        {
                            var name = pair.Substring(0, equalsIndex).Trim();
                            var value = pair.Substring(equalsIndex + 1).Trim();
                            instance.EnvironmentVariables[name] = value;
                        }
                    }
                }
                if (instanceConfig.Notifiers != null)
                {
                    instance.Notifiers = instanceConfig.Notifiers.OfType<NotifierElement>().Select(notifierConfig =>
                        {
                            var typeName = notifierConfig.Type.Substring(0, 1).ToUpperInvariant() + notifierConfig.Type.Substring(1).ToLowerInvariant();
                            typeName = string.Format("GitDeployHub.Web.Engine.Notifiers.{0}Notifier", typeName);
                            var notifier = (Notifier)Activator.CreateInstance(Type.GetType(typeName, true));
                            notifier.Key = notifierConfig.Key;
                            notifier.Settings = notifierConfig.Settings == null ? new Dictionary<string, string>() : notifierConfig.Settings.ToDictionary();
                            return notifier;
                        }).ToArray();
                }
                Register(instance);
            }
        }

        #endregion

        public static TraceSource TraceSource = new TraceSource("gitDeployHub", SourceLevels.All);

        private readonly IDictionary<string, Instance> _instances = new Dictionary<string, Instance>(StringComparer.InvariantCultureIgnoreCase);

        private IList<BaseProcess> _queue = new List<BaseProcess>();

        public IList<BaseProcess> Queue
        {
            get { return _queue; }
        }

        private IList<BaseProcess> _processHistory = new List<BaseProcess>();

        public IList<BaseProcess> ProcessHistory
        {
            get { return _processHistory; }
        }

        public IEnumerable<Instance> Instances
        {
            get { return _instances.Values; }
        }

        public void Register(Instance instance)
        {
            if (_instances.ContainsKey(instance.Name))
            {
                throw new Exception("An instance with the same name already exists: " + instance.Name);
            }
            _instances[instance.Name] = instance;
        }

        public Instance this[string name]
        {
            get
            {
                return _instances[name];
            }
        }

        public Instance GetInstance(string name)
        {
            Instance instance;
            if (!_instances.TryGetValue(name, out instance))
            {
                throw new Exception("Instance not found: " + name);
            }
            return instance;
        }

        internal static void Log(string message, Instance instance, BaseProcess process)
        {
            TraceSource.TraceInformation(message);
        }
    }
}