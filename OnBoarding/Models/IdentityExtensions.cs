using System.Security.Claims;
using System.Security.Principal;

namespace OnBoarding.ViewModels
{
    public static class IdentityExtensions
    {
        public static string GetUserCompanyName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("CompanyName");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}