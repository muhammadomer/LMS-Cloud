using LogApp;
using System;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI;

namespace SinglePointCloud.Models
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateAntiForgeryTokenOnAllPosts : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                var request = filterContext.HttpContext.Request;

                

                //  Only validate POSTs
                if (request.HttpMethod == WebRequestMethods.Http.Post)
                {
                    //  Ajax POSTs and normal form posts have to be treated differently when it comes
                    //  to validating the AntiForgeryToken
                    if (request.IsAjaxRequest())
                    {
                        var antiForgeryCookie = request.Cookies[AntiForgeryConfig.CookieName];

                        var cookieValue = antiForgeryCookie != null
                            ? antiForgeryCookie.Value
                            : null;

                    if(request.IsSecureConnection)
                        AntiForgery.Validate(cookieValue, request.Headers["__requestverificationtoken"]);
                    else
                        AntiForgery.Validate(cookieValue, request.Headers["__RequestVerificationToken"]);
                    }
                    else
                    {
                        new ValidateAntiForgeryTokenAttribute()
                            .OnAuthorization(filterContext);
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.WriteLog("Exception Occurred in Validate AntiForgery Tokens: " + filterContext.HttpContext.Request.Url, LogType.GENERALLOG);
                Log4Net.WriteException(ex);
                throw ex;
            }

        }
    }
}