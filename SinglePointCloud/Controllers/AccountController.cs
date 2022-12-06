using Database;
using Database.DAL;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace SinglePointCloud.Controllers
{
    public class AccountController : BaseController
    {
        // GET: Account
        public ActionResult List()
        {
            AccountDAL accountDAL = new AccountDAL();
            if (UserDAL.GetCurrentUser() == null)
                return RedirectToAction("Login", "User");
            ViewBag.accounts = accountDAL.GetAccounts();
            var client = new RestClient(TC_APIUrl + "api/UserService/GetCoursesList");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.RequestFormat = DataFormat.Json;
            IRestResponse response = client.Execute(request);
            JsonDeserializer deserial = new JsonDeserializer();
            int StatusCode = (int)response.StatusCode;
            //Console.WriteLine(response.Content);
            if (StatusCode == 200)
            {
                ViewBag.coursesList = deserial.Deserialize<List<SelectListItem>>(response);
            }
            return View();
        }
        public JsonResult GetAccounts() 
        {
            return Json(new { AccountList = new AccountDAL().GetAccounts() }, JsonRequestBehavior.AllowGet);
        }
        [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
        public JsonResult AddOrUpdateAccount(Accounts account)
        {
            return Json(new { Feedback = new AccountDAL().AddOrUpdateAccount(account) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteAccount(string accountId)
        {
            return Json(new { Feedback = new AccountDAL().DeleteAccount(accountId) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    string newFileName = "";

                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }
                        if (!Directory.Exists(Server.MapPath("~/Content/CompanyLogo")))
                        {
                            Directory.CreateDirectory(Server.MapPath("~/Content/CompanyLogo"));
                        }
                        string extension = Path.GetExtension(fname);
                        newFileName = Guid.NewGuid() + extension;
                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Content/CompanyLogo"), newFileName);
                        file.SaveAs(fname);
                    }
                    // Returns message that successfully uploaded
                    return Json(new { Data = "/Content/CompanyLogo/" + newFileName }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { Message = "Error occurred. Error details: " + ex.Message, Data = "" });
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
    }
}