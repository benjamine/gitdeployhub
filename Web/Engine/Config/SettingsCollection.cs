using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GitDeployHub.Web.Engine.Config
{

    public class SettingsCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public SettingElement this[string key]
        {
            get { return BaseGet(key) as SettingElement; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SettingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as SettingElement).Key;
        }

        protected override string ElementName
        {
            get { return "setting"; }
        }

        public IDictionary<string, string> ToDictionary()
        {
            return this.OfType<SettingElement>().ToDictionary(setting => setting.Key, setting => setting.Value);
        }

        public class SettingElement : ConfigurationElement
        {
            [ConfigurationProperty("key", IsKey = true, IsRequired = true)]
            public string Key
            {
                get { return (string)base["key"]; }
                set { base["key"] = value; }
            }

            [ConfigurationProperty("value", DefaultValue = "")]
            public string Value
            {
                get { return (string)base["value"]; }
                set { base["value"] = value; }
            }
        }

    }
}