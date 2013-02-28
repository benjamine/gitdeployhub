using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GitDeployHub.Web.Engine.Config;

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
                var instance = new Instance(this, instanceConfig.Name, instanceConfig.Folder);
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
                Register(instance);
            }
        }

        #endregion

        private IDictionary<string, Instance> _instances = new Dictionary<string, Instance>();

        private IList<Deployment> _deploymentQueue = new List<Deployment>();

        public IList<Deployment> DeploymentQueue
        {
            get { return _deploymentQueue; }
        }

        private IList<Deployment> _deploymentHistory = new List<Deployment>();

        public IList<Deployment> DeploymentHistory
        {
            get { return _deploymentHistory; }
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
    }
}