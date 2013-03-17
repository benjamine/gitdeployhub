using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace GitDeployHub.Web.Engine
{
    public class StringLog : ILog
    {
        private ILog innerLog;

        private StringBuilder stringBuilder = new StringBuilder();

        public string Output { get { return stringBuilder.ToString(); } }

        public StringLog(ILog inner = null)
        {
            innerLog = inner;
        }

        public void Log(string messsage)
        {
            stringBuilder.AppendLine(messsage);
            if (innerLog != null)
            {
                innerLog.Log(messsage);
            }
        }

        public void LogNewLine()
        {
            stringBuilder.AppendLine();
            if (innerLog != null)
            {
                innerLog.LogNewLine();
            }
        }
    }
}