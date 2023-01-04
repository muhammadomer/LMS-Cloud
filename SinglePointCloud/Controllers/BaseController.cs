using Database;
using Database.DAL;
using SinglePointCloud.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace SinglePointCloud.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    [ValidateAntiForgeryTokenOnAllPosts]
    public class BaseController : Controller
    {
        protected string TC_APIUrl = "";
        public BaseController()
        {
            TC_APIUrl = ConfigurationManager.AppSettings["TC_URL"];
            Users userEntity = UserDAL.GetCurrentUser();
            if (userEntity != null)
            {
                ViewBag.UserDetail = userEntity;
                ViewBag.Settings = new SettingDAL().GetSettings();
                ViewBag.OtherApplication = ConfigurationManager.AppSettings["OtherApplication"].ToString();

              
            }
        }

        public JsonResult GetDLLVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            version = version.Substring(0, 5); 
            
            return Json(new { DllVersion = version }, JsonRequestBehavior.AllowGet);
        }

    }


}