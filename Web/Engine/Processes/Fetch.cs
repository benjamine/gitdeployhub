using System;
using System.Text.RegularExpressions;
using System.Web;

namespace GitDeployHub.Web.Engine.Processes
{
    public class Fetch : BaseProcess
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

    }
}
