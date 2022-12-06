using LogApp;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SinglePointCloud
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            //Log4Net.FilePath = Server.MapPath("./Logs");
            //Log4Net.FileName = "Log";
            //Log4Net.EnableDBLOG = true;
            //Log4Net.EnableERRORLOG = true;
            //Log4Net.EnableGENERALLOG = true;
            //Log4Net.FileSize = 10;
            //Log4Net.TotalFiles = 10;
            //Log4Net.Activate(true);
            //Log4Net.WriteLog("Application Log initiazlize successfully", LogType.GENERALLOG);

            GlobalConfiguration.Configuration.EnsureInitialized();

            MvcHandler.DisableMvcResponseHeader = true;
        }
    }
}
