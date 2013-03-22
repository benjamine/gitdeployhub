using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace GitDeployHub.Web.Engine.Notifiers
{
    public class MailNotifier : Notifier
    {
        protected override void DoNotify(Processes.BaseProcess process)
        {
            var from = ParseTemplateText(Settings["from"], process);
            var to = ParseTemplateText(Settings["to"], process);
            var subject = ParseTemplateText(Settings["subject"], process);
            var body = ParseTemplateText((Settings["body"] ?? "").Replace("\\n", Environment.NewLine), process);
            var client = new SmtpClient();
            client.Send(new MailMessage(from, to, subject, body));
        }
    }
}