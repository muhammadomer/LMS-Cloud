using Database;
using Database.Cryptography;
using Database.DAL;
using LogApp;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SinglePointCloud.Controllers
{
    public class ManageAccountController : BaseController
    {
        private string TC_APIUrl = "";
        public ManageAccountController()
        {
            TC_APIUrl = ConfigurationManager.AppSettings["TC_URL"];
        }
        // GET: ManageAccount
        public void Index(string accountId)
        {
            try
            {                
                int ID = Convert.ToInt32(Cryptography.Decrypt(accountId));               
                Accounts accountDetail = new AccountDAL().GetAccountDetail(ID);
                string remoteUrl = "";
                try
                {
                    remoteUrl = WebConfigurationManager.AppSettings["SinglePointURL"];
                }
                catch (Exception ex)
                {

                }
                remoteUrl += "Login.aspx";
                string html = "<html><head>";
                html += "</head><body onload='document.forms[0].submit()'>";
                html += string.Format("<form name='PostForm' method='POST' action='{0}' style='visibility:hidden'>", remoteUrl);
                html += "<input id='AccountId' name='AccountId' type='text' value='" + Cryptography.Encrypt(accountDetail.Password + "$$$" + accountDetail.UserName + "$$$" + accountDetail.UserID) + "'>";

                html += "</form></body></html>";
                HttpContext.Response.Clear();
                HttpContext.Response.ContentEncoding = Encoding.GetEncoding("ISO-8859-1");
                HttpContext.Response.HeaderEncoding = Encoding.GetEncoding("ISO-8859-1");
                HttpContext.Response.Charset = "ISO-8859-1";
                HttpContext.Response.Write(html);
                HttpContext.Response.Flush(); // Sends all currently buffered output to the client.
                HttpContext.Response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
                HttpContext.ApplicationInstance.CompleteRequest();
              //  Log4Net.WriteLog("------ Successfully transfer Mitigate Admin to Client Portal1  ------", LogType.GENERALLOG);
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
            }
        }
        public void ManageTrainingCourses() 
        {
            string Email = Session["Email"].ToString();
            string Password = Session["Password"].ToString();
            //Log4Net.WriteLog("Logging To Training Courses", LogType.GENERALLOG);
            string queryParameter = Cryptography.Encrypt(Email + "$$$" + Password+ "$$$0$$$true$$$");
            queryParameter = queryParameter.Replace("+", "%2B");
            queryParameter = queryParameter.Replace(",", "%2C%20");
            queryParameter = queryParameter.Replace("/", "%2F");
            //Log4Net.WriteLog("Query String: " + queryParameter, LogType.GENERALLOG);
            TC_APIUrl += "Auth/LoginForSinglePoint";

            //Log4Net.WriteLog(TC_APIUrl + "?AccountId=" + queryParameter, LogType.GENERALLOG);
            Response.Redirect(TC_APIUrl + "?AccountId=" + queryParameter, false);        //write redirect
            HttpContext.ApplicationInstance.CompleteRequest(); // end response
        }
    }
}