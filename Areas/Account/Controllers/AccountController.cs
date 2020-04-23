using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Trinbago_MVC5.Models;
using System.Configuration;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Areas.Account.Models;
using System.Data.Entity;
using System.Collections.Generic;

namespace Trinbago_MVC5.Areas.User.Controllers
{
    [Authorize]
    [RouteArea("Account")]
    [Route("{action}")]
    public class AccountController : Controller
    {
        private SignInManager _signInManager;
        private AccountManager _userManager;

        public AccountController()
        {
        }

        public AccountController(AccountManager userManager, SignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public SignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<SignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public AccountManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AccountManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            ViewBag.ReturnUrl = returnUrl;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                if (returnUrl == "/ClassifiedAd/Manage/CreateAd")
                {
                    ModelState.AddModelError(string.Empty, "You must be logged in to post an ad");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "You must be logged in");
                }
            }
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(Login model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var user = await UserManager.FindByEmailAsync(model.Email);

            if (user == null) { ModelState.AddModelError("", "Invalid login attempt."); return View(model); }

            // check if account has password
            if (user.PasswordHash == null)
            {
                // Password not set for user
                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Pasword Reset", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a><br/>");
                return MigrationResetSent();
            }          
            
            SignInStatus result;
            
            result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, true, shouldLockout: true);

            switch (result)
            {
                case SignInStatus.Success:
                    UserManager.SetUserLastLogin(user.Id);
                    TransferCookieDataToUser(user.Id);
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = true });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");                    
                    return View(model);
            }
        }

        public async Task<ActionResult> SendConfirmEmail()
        {
            if (Request.UrlReferrer != null)
            {
                var userId = User.Identity.GetUserId();
                // Send an email with this link
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(userId);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = userId, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(userId, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a><br/>");
                // Set lockout strike
                await UserManager.AccessFailedAsync(userId);
                return VerificationSent();
            }
            return RedirectToAction("Index", "Home", new { Area = ""});            
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if(User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home", new { Area = "" });
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(Register model)
        {
            if (ModelState.IsValid)
            {
                // Check for spam
                if (EmailSpamFilter.CheckEmailSpam(model.Email))
                    return RedirectToAction("VerificationSent");

                var user = new ApplicationUser { PosterName = TextEditor.CleanPosterName(model.PosterName), UserName = model.Email, Email = model.Email, ContactName = model.ContactName != null ?
                    TextEditor.CleanAdContactName(model.ContactName) :
                    TextEditor.GetUserContactFromEmail(model.Email), PhoneNumber = model.ContactNumber };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Add to default role
                    await UserManager.AddToRoleAsync(user.Id, "User");
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a><br/>");
                    await SignInManager.SignInAsync(user, false, false);
                    TransferCookieDataToUser(user.Id);
                    return RedirectToAction("VerificationSent");
                }
                AddErrors(result);                
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("ErrorPage", "Errors", new { Area = "" });
            }
            if(await UserManager.FindByIdAsync(userId) == null)
            {
                return RedirectToAction("Login");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }
            return RedirectToAction("ErrorPage", "Errors", new { Area = "" });
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPass()
        {
            return View(new ForgotPass());
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPass(ForgotPass model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);

                if(user != null && user.Logins.Count == 0)
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    return PasswordResetSent();
                }
                else
                {
                    return PasswordResetSent();
                }

            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult PasswordResetSent()
        {
            if (Request.UrlReferrer != null)
                return View("PasswordResetKey");
            else
                return RedirectToAction("Index", "Home", new { Area = "" });
        }

        [AllowAnonymous]
        public ActionResult MigrationResetSent()
        {
            if (Request.UrlReferrer != null)
                return View("MigrationResetKey");
            else
                return RedirectToAction("Index", "Home", new { Area = "" });
        }

        [AllowAnonymous]
        public ActionResult VerificationSent()
        {
            if (Request.UrlReferrer != null)            
                return View("VerificationSent");            
            else
                return RedirectToAction("Index", "Home", new { Area = "" });
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            if (Request.UrlReferrer != null)            
                return View();            
            else            
                return RedirectToAction("Index", "Home", new { Area = "" });
            
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            if (code == null)
                return RedirectToAction("ErrorPage", "Errors", new { Area = "" });
            else
                return View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPassword model)
        {
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                // confirm if user is not already confirmed
                if (!user.EmailConfirmed)
                {
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    await UserManager.ConfirmEmailAsync(user.Id, code);
                }
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            else
            {
                AddErrors(result);
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            return RedirectToAction("ResetPasswordConfirmation", "Account");
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            if (Request.UrlReferrer != null)            
                return View();            
            else            
                return RedirectToAction("Index", "Home", new { Area = "" });            
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            ControllerContext.HttpContext.Session.RemoveAll();
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }


        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }
            if (loginInfo.Login.LoginProvider.ToLower() == "twitter")
            {
                string access_token = loginInfo.ExternalIdentity.Claims.Where(x => x.Type == "urn:twitter:access_token").Select(x => x.Value).FirstOrDefault();
                string access_secret = loginInfo.ExternalIdentity.Claims.Where(x => x.Type == "urn:twitter:access_secret").Select(x => x.Value).FirstOrDefault();
                TwitterHelper.TwitterDto response = TwitterHelper.TwitterLogin(access_token, access_secret, ConfigurationManager.AppSettings["twitterConsumerKey"], ConfigurationManager.AppSettings["twitterConsumerSecret"]);
                // by now response.email should possess the email value you need
                loginInfo.Email = response.email;
            }
            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    // confirm if user is not already confirmed
                    var id = SignInManager.FindUserByLoginKey(loginInfo.Login.ProviderKey);                   
                    var user = UserManager.FindById(id);
                    if (!user.EmailConfirmed)
                    {
                        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        await UserManager.ConfirmEmailAsync(user.Id, code);
                    }
                    UserManager.SetUserLastLogin(user.Id);
                    TransferCookieDataToUser(user.Id);
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmation { Email = loginInfo.Email, PosterName = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmation model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { PosterName = TextEditor.CleanPosterName(model.PosterName), ContactName = TextEditor.CleanPosterName(model.PosterName), Email = model.Email, UserName = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    // Add to role
                    await UserManager.AddToRoleAsync(user.Id, "User");
                    // Set email confirm
                    // confirm if user is not already confirmed
                    if (!user.EmailConfirmed)
                    {
                        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        await UserManager.ConfirmEmailAsync(user.Id, code);
                    }
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        user.LastLogin = DateTime.Now;
                        // transfer
                        TransferCookieDataToUser(user.Id);
                        return RedirectToLocal(returnUrl);
                    }
                }
                ViewBag.LoginProvider = info.Login.LoginProvider;
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        public async Task<ActionResult> ChangePassword()
        {
            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            if (UserManager.CurrentUser.Logins.Count == 0)
            {
                string code = await UserManager.GeneratePasswordResetTokenAsync(UserManager.CurrentUser.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(UserManager.CurrentUser.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("PasswordResetSent", "Account");
            }
            return ExternalPassChange();
        }

        public ActionResult ExternalPassChange()
        {
            if (Request.UrlReferrer != null)
            {
                ViewBag.Provider = UserManager.GetLogins(User.Identity.GetUserId())[0].LoginProvider.ToUpper();
                return View("ExternalPassChange");
            }
            else
                return RedirectToAction("Index", "Home", new { Area = "" });

        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [Authorize]
        public ActionResult EmailConfirmNeeded()
        {
            if (Request.UrlReferrer != null)
                return View();
            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        protected override void Dispose(bool disposing)
        {
            if(_userManager != null)
            {
                if(_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
                if(_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }
            base.Dispose(disposing);
        }

        private void TransferCookieDataToUser(string userId)
        {
            // check if user has anonymous "MyFavourites" cookie
            var anonymousUserId = Request.Cookies["MyFavourites"]?.Value?.ToString();
            if (anonymousUserId != null)
            {
                // remove any old cache of this user
                CacheHelper.RemoveFromCache(string.Format("{0}-Favourites", userId));
                // add favourites to this user then delete cookie
                using (var newthreadcontext = new ApplicationDbContext())
                {
                    var au = newthreadcontext.AnonymousUserDB.Include("Favourites").SingleOrDefault(x => x.Id.ToString() == anonymousUserId);
                    if (au != null)
                    {
                        var cu = newthreadcontext.Users.Include("Favourites").SingleOrDefault(x => x.Id == userId);
                        foreach (var f in au.Favourites.Except(cu.Favourites, new MyFavouriteCookieComparer()))
                        {
                            cu.Favourites.Add(f);
                        }
                        newthreadcontext.AnonymousUserDB.Remove(au);
                        newthreadcontext.SaveChanges();
                    }
                    var deletecookie = new HttpCookie("MyFavourites", anonymousUserId) { Secure = true, HttpOnly = true, Expires = DateTime.Now.AddDays(-1) };
                    Response.Cookies.Add(deletecookie);
                    Response.Cookies.Set(deletecookie);
                }
            }
        }

        // Custom comparer for the favourited class
        class MyFavouriteCookieComparer : IEqualityComparer<Favourited>
        {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(Favourited x, Favourited y)
            {

                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                return x.ClassifiedAdId == y.ClassifiedAdId;
            }

            // If Equals() returns true for a pair of objects 
            // then GetHashCode() must return the same value for these objects.

            public int GetHashCode(Favourited fav)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(fav, null)) return 0;

                //Get hash code for the Name field if it is not null.
                int hashClassifiedAdId = fav.ClassifiedAdId == 0 ? 0 : fav.ClassifiedAdId.GetHashCode();

                //Calculate the hash code for the product.
                return hashClassifiedAdId;
            }

        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                if (error.Contains("Email"))
                    ModelState.AddModelError("Email", error);
                if (error.Contains("Name"))
                    ModelState.AddModelError("Name", error);
                if (error.Contains("Password"))
                    ModelState.AddModelError("Password", error);
                if (error.Contains("Invalid token"))
                    ModelState.AddModelError("", "This password reset token has expired! You must request another reset.");
                else
                    ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        public class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}