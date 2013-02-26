using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GitDeployHub.Web.Engine.Config
{
    public class GitDeployHub : ConfigurationSection
    {
        private static GitDeployHub _Settings = ConfigurationManager.GetSection("gitDeployHub") as GitDeployHub;

        public static GitDeployHub Settings
        {
            get
            {
                return _Settings;
            }
        }

        [ConfigurationProperty("allowedAddresses", DefaultValue = "127.0.0.1;::1")]
        public string AllowedAddresses
        {
            get { return this["allowedAddresses"] as string; }
            set { this["allowedAddresses"] = value; }
        }

        [ConfigurationProperty("instances")]
        public InstanceCollection Instances
        {
            get
            {
                return this["instances"] as InstanceCollection;
            }
        }
    }
}