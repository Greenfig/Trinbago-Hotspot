using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Trinbago_MVC5.Areas.Account.Models;
using Trinbago_MVC5.Areas.User.Managers;

namespace Trinbago_MVC5.IdentityExtensions
{
    public static class IdentityExtensions
    {
        /// <summary>
        /// Checks if the user has verified email using the UserIdClaimType
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static bool IsEmailConfirmed(this IIdentity identity)
        {            
            return HttpContext.Current.Request.GetOwinContext().GetUserManager<AccountManager>().FindById(HttpContext.Current.User.Identity.GetUserId()).EmailConfirmed;
        }
        /// <summary>
        /// Return the user email using the UserIdClaimType
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetUserEmail(this IIdentity identity)
        {
            var email = HttpContext.Current.Request.GetOwinContext().GetUserManager<AccountManager>().FindById(HttpContext.Current.User.Identity.GetUserId());
            return (email != null) ? email.Email : null;
        }
        /// <summary>
        /// Return the loged in poster's name
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetPosterName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("PosterName");
            return claim != null ? claim.Value : string.Empty;
        }
    }
}