using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace GitDeployHub.Web.Engine.Notifiers
{
    public class HttpNotifier : Notifier
    {
        protected override void DoNotify(Processes.BaseProcess process)
        {
            var url = ParseTemplateText(Settings["url"], process, HttpUtility.UrlEncode);
            var webRequest = WebRequest.Create(url);
            webRequest.Method = Settings.ContainsKey("method") ? Settings["method"] : "POST";
            var body = Settings.ContainsKey("body") ? Settings["body"] : "";
            if (!string.IsNullOrEmpty(body))
            {
                webRequest.ContentType = "application/json";
                var dataStream = webRequest.GetRequestStream();
                using (var writer = new StreamWriter(dataStream))
                {
                    var parsedBody = ParseTemplateText(body, process, HttpUtility.JavaScriptStringEncode);
                    writer.Write(parsedBody);
                }
                dataStream.Close();
            }
            webRequest.GetResponse();
        }
    }
}