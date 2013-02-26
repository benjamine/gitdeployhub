using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace GitDeployHub.Web.Engine
{
    public class Instance
    {
        public string Name { get; private set; }

        public string Folder { get; set; }

        private static string _mappedApplicationPath;

        public static string MappedApplicationPath
        {
            get
            {
                if (_mappedApplicationPath == null)
                {
                    var appPath = HttpContext.Current.Request.ApplicationPath.ToLower();
                    if (appPath == "/")
                    {
                        //a site
                        appPath = "/";
                    }
                    else if (!appPath.EndsWith(@"/"))
                    {
                        //a virtual
                        appPath += @"/";
                    }
                    var mappedPath = HttpContext.Current.Server.MapPath(appPath);
                    if (!mappedPath.EndsWith(@"\"))
                        mappedPath += @"\";
                    _mappedApplicationPath = mappedPath;
                }
                return _mappedApplicationPath;
            }
        }

        public Deployment LastDeployment { get; set; }

        public Instance(string name, string folder = null)
        {
            Name = name;
            Folder = folder;
            if (string.IsNullOrWhiteSpace(Folder))
            {
                Folder = Path.Combine(MappedApplicationPath, "../" + name);
            }
        }
    }
}
