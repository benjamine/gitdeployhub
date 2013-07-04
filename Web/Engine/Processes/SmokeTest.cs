using System;
using System.Text.RegularExpressions;
using System.Web;

namespace GitDeployHub.Web.Engine.Processes
{
    public class SmokeTest : BaseProcess
    {
        public SmokeTest(Hub hub, Instance instance)
            : base(hub, instance)
        {
        }

        protected override void DoExecute()
        {
            Instance.ExecuteSmokeTest(this);
            LogNewLine();
            Log("Instance Smoke Tested: " + Instance.Name);
            Instance.LastSmokeTest = this;
        }
    }
}
