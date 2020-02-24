using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using OnBoarding.Web.Models;
using OnBoarding.Models;

namespace OnBoarding.Web.Domain
{
    public class Data
    {
        public IEnumerable<Navbar> navbarItems()
        {
            using (DBModel db = new DBModel())
            {
                IPrincipal currentUser = HttpContext.Current.User;
                var currentUserDetails = db.AspNetUsers.FirstOrDefault(a => a.UserName == currentUser.Identity.Name);
                
                var menu = db.Database.SqlQuery<Navbar>("SELECT s.* FROM SystemMenus s INNER JOIN SystemMenuAccess a on a.menuId = s.id INNER JOIN AspNetUserRoles r on r.RoleId = a.roleId WHERE s.status = 'True' AND r.UserId = " + "'" + currentUserDetails.Id + "' ORDER BY a.displayId ASC");
                return menu.ToList();
            }
        }
    }
}