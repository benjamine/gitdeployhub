using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GitDeployHub.Web.Engine.Config
{
    public class NotifierElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return this["key"] as string; }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
            set { this["type"] = value; }
        }

        [ConfigurationProperty("environmentVariables")]
        public string EnvironmentVariables
        {
            get { return this["environmentVariables"] as string; }
            set { this["environmentVariables"] = value; }
        }

        [ConfigurationProperty("settings")]
        public SettingsCollection Settings
        {
            get
            {
                return this["settings"] as SettingsCollection;
            }
        }
    }
}