using System;
using System.Web;
using System.Web.Mvc;
using OnBoarding.Models;
using System.Web.Routing;
using System.Web.Optimization;
using Microsoft.AspNet.Identity;
using System.Linq;

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
            //Database.SetInitializer<DBModel>(new DropCreateDatabaseIfModelChanges<DBModel>());
            Startup.createRolesandUsers();
            //WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Response.AddHeader("X-Frame-Options", "DENY");
            //HttpContext.Current.Response.AddHeader("X-Frame-Options", "sameorigin");
        }
    }
}
