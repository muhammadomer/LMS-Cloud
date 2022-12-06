using Database.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SinglePointCloud.Controllers
{
    public class ActivityLogController : BaseController
    {
        // GET: ActivityLog
        public ActionResult List()
        {
            if (UserDAL.GetCurrentUser() == null)
                return RedirectToAction("Login", "User");
            ViewBag.Activities = new ActivityLogDAL().GetActivityLogs();
            return View();
        }
    }
}