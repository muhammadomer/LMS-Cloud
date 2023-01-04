using Base32;
using Database;
using Database.DAL;
using LogApp;
using OtpSharp;
using SinglePointCloud.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SinglePointCloud.Controllers
{
    public class UserController : BaseController
    {
        SecurityDAL _securityDAL;
        public UserController()
        {
            _securityDAL = new SecurityDAL();
        }
        // GET: User
        public ActionResult List()
        {



           


            if (UserDAL.GetCurrentUser() == null)
                return RedirectToAction("Login", "User");
            ViewBag.Users = new UserDAL().GetUsers();
            return View();
        }
        public ActionResult Login()
        {
            
            ViewBag.OtherApplication = ConfigurationManager.AppSettings["OtherApplication"].ToString();
            return View();
        }

        public ActionResult Testlogin()
        {
            ViewBag.OtherApplication = ConfigurationManager.AppSettings["OtherApplication"].ToString();
            return View();
        }
        public ActionResult ChangePassword(string id = null)
        {
            ViewBag.OtherApplication = ConfigurationManager.AppSettings["OtherApplication"].ToString();
            Users SignedInUser = UserDAL.GetCurrentUser();
            if (id != null)
            {
                ViewBag.userDetail = _securityDAL.GetUserIdForPasswordReset(id);
            }
            else if (SignedInUser != null)
            {
                ViewBag.userDetail = SignedInUser;
            }
            else
            {
                return View("Login");
            }
            if (ViewBag.userDetail == null)
            {
                return View("Login");
            }
            return View();
        }
        public ActionResult ForgotPassword()
        {
            ViewBag.OtherApplication = ConfigurationManager.AppSettings["OtherApplication"].ToString();
            return View();
        }
        public ActionResult Logout()
        {
            HttpContext.Session["UserEntity"] = null;
            return RedirectToAction("Login", "User");
        }

        public JsonResult CheckUserAuthentication(string Username, string Password)
        {
            Users userDetail = new UserDAL().CheckUserAuthentication(Username, Password);
            Session["Email"] = userDetail.UserEmail;
            Session["Password"] = Password;
            return Json(new { Feedback = new UserDAL().LoginUser(userDetail), LoggeninUserEmail = userDetail.UserEmail }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddOrUpdateUser(Users user)
        {
            return Json(new { Feedback = new UserDAL().AddOrUpdateUser(user) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteUser(string userId)
        {
            return Json(new { Feedback = new UserDAL().DeleteUser(userId) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetUsers()
        {
            return Json(new { UserList = new UserDAL().GetUsers() }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ChangePassword(string oldPassword,string newPassword)
        {
            try
            {
                return Json(new { Feedback = _securityDAL.ChangePassword(oldPassword, newPassword) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        [HttpPost]
        public JsonResult ResetPassword(string password, string userId, string userName)
        {
            try
            {
                return Json(new { Feedback = _securityDAL.ResetPassword(password,userId,userName) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult SendResetPasswordLink(string userName)
        {
            try
            {
                return Json(new { Feedback = _securityDAL.SendResetPasswordLink(userName) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                throw ex;
            }
        }
        public JsonResult VerifyCodeOnLogin(string userName, string password, string code)
        {
            try
            {
               // LogApp.Log4Net.WriteLog("userName: " + userName + "|" + code, LogType.GENERALLOG);
                UserDAL userDAL = new UserDAL();
                Users userEntity = userDAL.CheckUserAuthentication(userName, password);
                string feedBack = "Invalid verification code";
                //LogApp.Log4Net.WriteLog("userEntity.TwoFactorEnabled: " + userEntity.TwoFactorEnabled.ToString(), LogType.GENERALLOG);
                //if (userEntity.TwoFactorEnabled)
                //{
                //    //LogApp.Log4Net.WriteLog("TRUE", LogType.GENERALLOG);
                //    byte[] secretKey = Base32Encoder.Decode(userEntity.GoogleAuthenticatorSecretKey);
                //    long timeStepMatched = 0;
                //    var otp = new Totp(secretKey);
                //    if (otp.VerifyTotp(code.Trim(), out timeStepMatched, new VerificationWindow(2, 2)))
                //    {
                //        Log4Net.WriteLog("Verified 2FA Code", LogType.GENERALLOG);
                //        if (String.IsNullOrWhiteSpace(userEntity.LastPasswordChange.ToString()))
                //        {
                //            Session["UserEntity"] = userEntity;
                //            feedBack = "2";
                //        }
                //        else
                //        {
                //            Session["UserEntity"] = userEntity;
                //            feedBack = "1";
                //        }
                //    }
                //    else
                //    {
                //        Session["UserEntity"] = null;
                //    }
                //}
                //else
                //{
                //   // LogApp.Log4Net.WriteLog("FALSE", LogType.GENERALLOG);
                //    if (userDAL.VerifyEmailCode(userEntity, code))
                //    {
                //       // Log4Net.WriteLog("Verified Email Code", LogType.GENERALLOG);
                //        if (String.IsNullOrWhiteSpace(userEntity.LastPasswordChange.ToString()))
                //        {
                //            Session["UserEntity"] = userEntity;
                //            feedBack = "2";
                //        }
                //        else
                //        {
                //            Session["UserEntity"] = userEntity;
                //            feedBack = "1";
                //        }
                //    }
                //    else
                //    {
                //        Session["UserEntity"] = null;
                //    }
                //}
                return Json(new { Feedback = feedBack }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                throw ex;
            }
        }
        public JsonResult Reset2FA(string userName, string password)
        {
            try
            {
                UserDAL userDAL = new UserDAL();
                Users userEntity = userDAL.CheckUserAuthentication(userName, password);
                return Json(new { Feedback = userDAL.Reset2FA(userEntity) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                throw ex;
            }
        }

    }
}