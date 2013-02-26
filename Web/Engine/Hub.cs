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
                var instance = new Instance(instanceConfig.Name, instanceConfig.Folder);
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

        public Deployment CreateDeployment(string instanceName, IDictionary<string,string> parameters = null)
        {
            Instance instance;
            if (!_instances.TryGetValue(instanceName, out instance))
            {
                throw new Exception("Instance not found: " + instanceName);
            }
            var deployment = new Deployment(this, _instances[instanceName])
                {
                    Parameters = parameters
                };
            DeploymentQueue.Add(deployment);
            return deployment;
        }

    }
}