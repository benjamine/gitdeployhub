using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitDeployHub.Web.Engine
{
    public interface ILog
    {
        void Log(string messsage);
        void LogNewLine();
    }
}