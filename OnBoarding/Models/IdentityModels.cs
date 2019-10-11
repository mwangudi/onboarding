<<<<<<< HEAD
﻿using System.Security.Claims;
=======
﻿using System;
using System.Security.Claims;
>>>>>>> 4a57cb65d9ff4345fc860a183a3da86a25aa01d6
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OnBoarding.ViewModels
{
    //
    // ApplicationDbContext
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        //Extended Properties
        public string CompanyName { get; internal set; }
        public string StaffNumber { get; internal set; }
        public string PasswordResetCode { get; internal set; }
<<<<<<< HEAD
=======
        public DateTime LastPasswordChangedDate { get; set; }
>>>>>>> 4a57cb65d9ff4345fc860a183a3da86a25aa01d6

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
<<<<<<< HEAD
            
=======
            //Add Custom claim here
>>>>>>> 4a57cb65d9ff4345fc860a183a3da86a25aa01d6
            userIdentity.AddClaim(new Claim("CompanyName", this.CompanyName));
            return userIdentity;
        }
    }
}