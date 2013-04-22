using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GitDeployHub.Web.Engine;
using GitDeployHub.Web.Engine.Notifiers;

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

        public ActionResult Info(string id)
        {
            var instance = Hub.Instance.GetInstance(id);
            var status = new
                {
                    instance.Name,
                    Notifiers = instance.Notifiers == null ? null : instance.Notifiers
                        .ToDictionary(notifier => notifier.Key, notifier => notifier.GetType().Name),
                    instance.Tags,
                    instance.Treeish,
                    LastDeployment = instance.LastDeployment == null ? null : new
                    {
                        instance.LastDeployment.Status,
                        instance.LastDeployment.Succeeded,
                        instance.LastDeployment.Created,
                        instance.LastDeployment.Completed,
                        Exception = instance.LastDeployment.Exception == null ? null : new
                            {
                                instance.LastDeployment.Exception.Message,
                                TypeFullName = instance.LastDeployment.Exception.GetType().FullName,
                                StackTrace = instance.LastDeployment.ExceptionStackTrace,
                            },
                    }
                };
            return Json(status, JsonRequestBehavior.AllowGet);
        }
    }
}
