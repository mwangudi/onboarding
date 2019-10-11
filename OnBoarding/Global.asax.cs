using System;
using System.Web;
using System.Web.Mvc;
<<<<<<< HEAD
using System.Web.Optimization;
using System.Web.Routing;
=======
using System.Web.Routing;
using System.Web.Optimization;
>>>>>>> 4a57cb65d9ff4345fc860a183a3da86a25aa01d6

namespace OnBoarding.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
<<<<<<< HEAD
            BundleConfig.RegisterAdminBundles(BundleTable.Bundles);
=======
>>>>>>> 4a57cb65d9ff4345fc860a183a3da86a25aa01d6
            //Database.SetInitializer<DBModel>(new DropCreateDatabaseIfModelChanges<DBModel>());
            Startup.createRolesandUsers();
            //WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
<<<<<<< HEAD
            HttpContext.Current.Response.AddHeader("x-frame-options", "DENY");
=======
            HttpContext.Current.Response.AddHeader("X-Frame-Options", "DENY");
>>>>>>> 4a57cb65d9ff4345fc860a183a3da86a25aa01d6
        }
    }
}
