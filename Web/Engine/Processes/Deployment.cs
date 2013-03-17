using System;

namespace GitDeployHub.Web.Engine.Processes
{
    public class Deployment : Fetch
    {
        public bool Dry { get; set; }

        public Deployment(Hub hub, Instance instance)
            : base(hub, instance)
        {
        }

        protected override void DoExecute()
        {
            base.DoExecute();
            if (Instance.FilesChangedToTreeish.Length > 0)
            {
                if (!Dry)
                {
                    Instance.ExecutePreDeploy(this);
                    Instance.Stash(this);
                    Instance.Checkout(Instance.Treeish, this);
                    Instance.ExecutePostDeploy(this);
                    LogNewLine();
                    Log("Instance Deployed: " + Instance.Name);
                }
            }
            else
            {
                Skipped = true;
                Log("Skipping deployment.");
            }
        }

    }
}
