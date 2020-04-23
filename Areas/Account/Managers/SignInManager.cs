using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.Account.Models
{
    // Configure the application sign-in manager which is used in this application.
    public class SignInManager : SignInManager<ApplicationUser, string>
    {
        public SignInManager(AccountManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((AccountManager)UserManager);
        }

        public static SignInManager Create(IdentityFactoryOptions<SignInManager> options, IOwinContext context)
        {
            return new SignInManager(context.GetUserManager<AccountManager>(), context.Authentication);
        }

        public string FindUserByLoginKey(string key)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var userId = newthreadcontext.Database.SqlQuery<UserLogin>("SELECT UserId, ProviderKey FROM dbo.AspNetUserLogins").SingleOrDefault(x => x.ProviderKey == key).UserId;
                return userId;
            }
        }
        private class UserLogin
        {
            public string UserId { get; set; }
            public string ProviderKey { get; set; }
        }
    }
}