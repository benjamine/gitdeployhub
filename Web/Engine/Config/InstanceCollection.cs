using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GitDeployHub.Web.Engine.Config
{
    public class InstanceCollection : ConfigurationElementCollection
    {
        public InstanceElement this[object key]
        {
            get
            {
                return base.BaseGet(key) as InstanceElement;
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "instance";
            }
        }

        protected override bool IsElementName(string elementName)
        {
            bool isName = false;
            if (!String.IsNullOrEmpty(elementName))
                isName = elementName.Equals("instance");
            return isName;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new InstanceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((InstanceElement)element).Name;
        }
    }
}