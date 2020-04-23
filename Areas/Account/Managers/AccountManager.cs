using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Linq;
using System.Web;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.Account.Models
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class AccountManager : UserManager<ApplicationUser>
    {
        private ApplicationUser _currentUser;

        public AccountManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }


        public ApplicationUser CurrentUser
        {
            get
            {
                using (var manager = new AccountManager(new UserStore<ApplicationUser>(HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>())))
                {
                    return _currentUser ?? manager.FindById(HttpContext.Current.User.Identity.GetUserId());
                }

            }
            set
            {
                _currentUser = value;
            }
        }

        public static AccountManager Create(IdentityFactoryOptions<AccountManager> options, IOwinContext context)
        {
            var manager = new AccountManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        public void SetUserLastLogin(string userId)
        {
            using (var CurrentDbContext = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>())
            {

                var user = CurrentDbContext.Users.SingleOrDefault(x => x.Id == userId);
                if (user != null)
                {
                    user.LastLogin = DateTime.Now;
                    CurrentDbContext.SaveChanges();
                    return;
                }
            }
        }
    }
}