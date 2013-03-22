using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GitDeployHub.Web.Engine;

namespace GitDeployHub.Web.Controllers
{
    public class InstanceController : Controller
    {
        //
        // GET: /Instance/

        public ActionResult ChangeLog(string id)
        {
            return View(Hub.Instance.GetInstance(id));
        }

        public ActionResult ChangeLogShort(string id)
        {
            ViewBag.ShortVersion = true;
            return View("ChangeLog", Hub.Instance.GetInstance(id));
        }

    }
}
