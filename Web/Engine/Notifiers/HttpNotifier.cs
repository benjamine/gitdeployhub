using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace GitDeployHub.Web.Engine.Notifiers
{
    public class HttpNotifier : Notifier
    {
        protected override void DoNotify(Processes.BaseProcess process)
        {
            var url = ParseTemplateText(Settings["url"], process, value => HttpUtility.UrlEncode(value));
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = Settings.ContainsKey("method") ? Settings["method"] : "POST";
            webRequest.GetResponse();
        }
    }
}