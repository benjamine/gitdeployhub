using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GitDeployHub.Web.Engine.Config
{
    public class InstanceElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("folder")]
        public string Folder
        {
            get { return this["folder"] as string; }
            set { this["folder"] = value; }
        }

        [ConfigurationProperty("treeish", DefaultValue = "origin/master")]
        public string Treeish
        {
            get { return this["treeish"] as string; }
            set { this["treeish"] = value; }
        }

        [ConfigurationProperty("environmentVariables")]
        public string EnvironmentVariables
        {
            get { return this["environmentVariables"] as string; }
            set { this["environmentVariables"] = value; }
        }

        [ConfigurationProperty("notifiers")]
        public NotifierCollection Notifiers
        {
            get
            {
                return this["notifiers"] as NotifierCollection;
            }
        }
    }
}