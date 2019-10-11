using OnBoarding.Web.Domain;
using System.Linq;
using System.Web.Mvc;

namespace OnBoarding.Web.Controllers
{
    public class NavbarController : Controller
    {
        // GET: Navbar
        public ActionResult Index()
        {
            var data = new Data();            
            return PartialView("_Navbar", data.navbarItems().ToList());
        }

        // GET: Admin Navbar
        public ActionResult Admin()
        {
            var data = new Data();
            return PartialView("_AdminNewNavbar", data.navbarItems().ToList());
        }
    }
}