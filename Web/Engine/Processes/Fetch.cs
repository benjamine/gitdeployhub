using System;
using System.Text.RegularExpressions;
using System.Web;

namespace GitDeployHub.Web.Engine.Processes
{
    public class Fetch : Process
    {
        public Fetch(Hub hub, Instance instance)
            : base(hub, instance)
        {
        }

        protected override void DoExecute()
        {
            if (!Instance.HasFolder(".git"))
            {
                throw new Exception("The specified folder is not a git repository workspace: " + Instance.Folder);
            }

            Instance.Fetch(this);
        }

        public bool IsAllowed(HttpContextBase context)
        {
            var allowedAddresses = Config.GitDeployHub.Settings.AllowedAddresses ?? "";
            if (string.IsNullOrWhiteSpace(allowedAddresses) || allowedAddresses.ToLowerInvariant() == "none")
            {
                return false;
            }
            var method = context.Request.HttpMethod.ToUpperInvariant();
            if (method != "POST" && method != "PUT")
            {
                return false;
            }
            var allowed = false;
            var requestAddress = context.Request.UserHostAddress ?? "null";
            foreach (var address in allowedAddresses.Split(new[] { ',', ';', ' ' }))
            {
                if (!string.IsNullOrWhiteSpace(address))
                {
                    if (address == "*")
                    {
                        return true;
                    }
                    var regex = new Regex("^" + address.Replace(".", "\\.").Replace("*", "\\d+") + "$");
                    if (regex.IsMatch(requestAddress))
                    {
                        allowed = true;
                        break;
                    }
                }
            }
            return allowed;
        }

    }
}
