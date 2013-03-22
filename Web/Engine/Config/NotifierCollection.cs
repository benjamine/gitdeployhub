using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GitDeployHub.Web.Engine.Config
{

    public class NotifierCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public NotifierElement this[string key]
        {
            get { return BaseGet(key) as NotifierElement; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NotifierElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as NotifierElement).Type;
        }

        protected override string ElementName
        {
            get { return "notifier"; }
        }
    }
}