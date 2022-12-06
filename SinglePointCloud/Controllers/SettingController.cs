using Database;
using Database.DAL;
using LogApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SinglePointCloud.Controllers
{
    public class SettingController : BaseController
    {
        SettingDAL _settingDAL;
        SecurityDAL _securityDAL;
        public SettingController()
        {
            _settingDAL = new SettingDAL();
            _securityDAL = new SecurityDAL();
        }
        // GET: Settings
        public ActionResult Index()
        {
            if (UserDAL.GetCurrentUser() == null)
                return RedirectToAction("Login", "User");
            ViewBag.Settings = _settingDAL.GetSettings();
            return View();
        }
        public JsonResult SaveSettings(Settings settings)
        {
            return Json(new { Feedback = _settingDAL.SaveSettings(settings) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EnableDisableTwoFA(bool state)
        {
            return Json(new { Feedback = _settingDAL.EnableDisableTwoFA(state) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult TestSMTPSettings(string toEmail, Settings smtpSettings)
        {
            return Json(new { Feedback = _settingDAL.TestSMTPSettings(toEmail, smtpSettings) }, JsonRequestBehavior.AllowGet);


          

        }
        public JsonResult DisableGoogleAuthenticator()
        {
            try
            {
                return Json(new { Feedback = _securityDAL.DisableGoogleAuthenticator() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                throw ex;
            }
        }
        public JsonResult EnableGoogleAuthenticator()
        {
            try
            {
                return Json(new { Feedback = _securityDAL.EnableGoogleAuthenticator() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                throw ex;
            }
        }
        public JsonResult EnableGoogleAuthenticatorWithVerificationCode(string Code, string SecretKey)
        {
            try
            {
                return Json(new { Feedback = _securityDAL.EnableGoogleAuthenticatorWithVerificationCode(Code, SecretKey) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                throw ex;
            }
        }

    }
}