using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using OnBoarding.ViewModels;

[assembly: OwinStartupAttribute(typeof(OnBoarding.Startup))]
namespace OnBoarding
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        // Creating default User, roles and Admin user for login   
        public static void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Create a default Admin Role and User    
            if (!roleManager.RoleExists("Admin"))
            {
                // Creating Admin Role   
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);

                //Create a Admin super user who will maintain the website
                string userPassword = "Admin101";
                var user = new ApplicationUser { UserName = "emt.admin@stanbic.com", Email = "emt.admin@stanbic.com", CompanyName = "Administrator" };
                var result = UserManager.Create(user, userPassword);

                //Add default User to Role Admin
                if (result.Succeeded)
                {
                    var result2 = UserManager.AddToRole(user.Id, "Admin");

                }
            }
        }
    }
}
