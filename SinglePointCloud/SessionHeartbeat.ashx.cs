using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace SinglePointCloud
{
    /// <summary>
    /// Summary description for SessionHeartbeat
    /// </summary>
    public class SessionHeartbeat : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Session["Heartbeat"] = DateTime.Now;
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            context.Response.Cache.SetNoStore();
            context.Response.Cache.SetNoServerCaching();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}