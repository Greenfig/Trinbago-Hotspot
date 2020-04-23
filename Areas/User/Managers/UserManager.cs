using AutoMapper;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Trinbago_MVC5.Areas.Account.Models;
using Trinbago_MVC5.Areas.User.Models;
using Trinbago_MVC5.Controllers;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.User.Managers
{
    public class UserManager : BaseApplicationManager
    {
        private AccountManager _accountManager;

        public AccountManager AccountManager
        {
            get
            {
                return _accountManager ?? new AccountManager(new UserStore<ApplicationUser>(HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>()));
            }
            set
            {
                _accountManager = value;
            }
        }

        public ApplicationUser FindByIdWithAds(string userId)
        {
            return AccountManager.Users
                .Include(u => u.ClassifiedAds)
                .Include(u => u.ClassifiedAds.Select(x => x.Reports))
                .FirstOrDefault(u => u.Id == userId);
        }

        public Task<ApplicationUser> FindByIdWithAdsAsync(string userId)
        {
            return AccountManager.Users
                .Include(u => u.ClassifiedAds)
                .Include(u => u.ClassifiedAds.Select(x => x.Reports))
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public IPagedList<ClassifiedAdFavouriteList> GetMyFavouritedAds(int pageNumber = 1)
        {
            IEnumerable<int> cuf = new List<int>();

            if (CurrentUser != null)
            {
                cuf = CacheHelper.GetFromCache<IEnumerable<int>>(string.Format("{0}-Favourites", CurrentUser.Id));
                if (cuf == null)
                {
                    using (var newthreadcontext = new ApplicationDbContext())
                    {
                        cuf = newthreadcontext.Users.SingleOrDefault(x => x.Id == CurrentUser.Id).Favourites.Select(s => s.ClassifiedAdId);
                        CacheHelper.SaveTocache(string.Format("{0}-Favourites", CurrentUser.Id), cuf, DateTime.Now.AddMinutes(5));
                    }
                }
                return SearchEngineManager.GetUserFavouritedAds(cuf, pageNumber);
            }
            else
            {
                var anonymousUserId = HttpContext.Current.Request.Cookies["MyFavourites"]?.Value?.ToString();
                if (anonymousUserId != null)
                {
                    cuf = CacheHelper.GetFromCache<IEnumerable<int>>(string.Format("{0}-Favourites", anonymousUserId));
                    if (cuf == null)
                    {
                        using (var newthreadcontext = new ApplicationDbContext())
                        {
                            cuf = newthreadcontext.AnonymousUserDB.SingleOrDefault(x => x.Id.ToString() == anonymousUserId)?.Favourites.Select(s => s.ClassifiedAdId);
                            // cleanup cookie if user does not exist in db
                            if (cuf == null)
                            {
                                var deletecookie = new HttpCookie("MyFavourites", anonymousUserId) { Secure = true, HttpOnly = true, Expires = DateTime.Now.AddDays(-1) };
                                HttpContext.Current.Response.Cookies.Add(deletecookie);
                                HttpContext.Current.Response.Cookies.Set(deletecookie);
                                HttpContext.Current.Request.Cookies.Remove("MyFavourites");
                                return null;
                            }
                            CacheHelper.SaveTocache(string.Format("{0}-Favourites", anonymousUserId), cuf, DateTime.Now.AddMinutes(5));
                        }
                    }
                    return SearchEngineManager.GetUserFavouritedAds(cuf, pageNumber);
                }
            }

            return new List<ClassifiedAdFavouriteList>().ToPagedList(1,1);
        }

        public ApplicationUser FindByEmailWithAds(string email)
        {
            return AccountManager.Users.Include(u => u.ClassifiedAds).Include(u => u.ClassifiedAds.Select(x => x.Category)).Include(u => u.ClassifiedAds.Select(x => x.SubCategory)).Include(u => u.ClassifiedAds.Select(x => x.Country)).Include(u => u.ClassifiedAds.Select(x => x.Region)).Include(p => p.ClassifiedAds.Select(x => x.Photos)).FirstOrDefault(u => u.Email == email);
        }

        public Task<ApplicationUser> FindByEmailWithAdsAsync(string email)
        {
            return AccountManager.Users.Include(u => u.ClassifiedAds).Include(u => u.ClassifiedAds.Select(x => x.Category)).Include(u => u.ClassifiedAds.Select(x => x.SubCategory)).Include(u => u.ClassifiedAds.Select(x => x.Country)).Include(u => u.ClassifiedAds.Select(x => x.Region)).FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<ApplicationUser> FindByEmailWithPremium(string email)
        {
            return AccountManager.Users.Include(u => u.PremiumUserData).Include(u => u.PremiumUserData.PremiumUserInfos).Include(u => u.PremiumUserData.PremiumUserPhotos).FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Return false when removed and true when added
        /// </summary>
        /// <param name="adId"></param>
        /// <returns></returns>
        public bool Favourite(int adId)
        {
            // If user is logged in store on database
            if (AccountManager.CurrentUser != null)
            {
                // check if favourite exists
                var fav = AccountManager.CurrentUser.Favourites.Select(s => s.ClassifiedAdId).Any(x => x == adId);
                if (!fav)
                {
                    AccountManager.CurrentUser.Favourites.Add(new Favourited() { ClassifiedAdId = adId });
                    CurrentDbContext.SaveChanges();
                    // update old
                    var cuf = AccountManager.CurrentUser.Favourites.Select(s => s.ClassifiedAdId);
                    CacheHelper.SaveTocache(string.Format("{0}-Favourites", AccountManager.CurrentUser.Id), cuf, DateTime.Now.AddMinutes(5));
                    return true;
                }
                else
                {
                    var torem = AccountManager.CurrentUser.Favourites.FirstOrDefault(x => x.ClassifiedAdId == adId);
                    AccountManager.CurrentUser.Favourites.Remove(torem);
                    CurrentDbContext.FavouirtedDb.Remove(torem);
                    CurrentDbContext.SaveChanges();
                    var cuf = AccountManager.CurrentUser.Favourites.Select(s => s.ClassifiedAdId);
                    CacheHelper.SaveTocache(string.Format("{0}-Favourites", AccountManager.CurrentUser.Id), cuf, DateTime.Now.AddMinutes(5));
                    // update old
                    return false;
                }
            }
            // Anonymous user
            else
            {
                // check if anonymous user has a "MyFavourites" cookie
                var anonymousUserId = HttpContext.Current.Request.Cookies["MyFavourites"]?.Value?.ToString();
                // User exists
                if(anonymousUserId != null)
                {
                    var user = CurrentDbContext.AnonymousUserDB.SingleOrDefault(x => x.Id.ToString() == anonymousUserId);
                    // Check if the item is in the cart on the database
                    if (!user.Favourites.Select(s => s.ClassifiedAdId).Any(a => a == adId)){
                        // item is not in cart
                        user.Favourites.Add(new Favourited() { ClassifiedAdId = adId });
                        user.LastUsed = DateTime.Now;
                        CurrentDbContext.SaveChanges();
                        var cuf = user.Favourites.Select(s => s.ClassifiedAdId);
                        CacheHelper.SaveTocache(string.Format("{0}-Favourites", anonymousUserId), cuf, DateTime.Now.AddMinutes(5));
                        return true;
                    }
                    else
                    {
                        // Remove from cart
                        var torem = user.Favourites.FirstOrDefault(x => x.ClassifiedAdId == adId);
                        user.Favourites.Remove(torem);
                        user.LastUsed = DateTime.Now;
                        // if user has no ids delete from db
                        if (user.Favourites.Count == 0)
                        {
                            CurrentDbContext.AnonymousUserDB.Remove(user);
                            CurrentDbContext.FavouirtedDb.Remove(torem);
                            CurrentDbContext.SaveChanges();
                            // delete cookies
                            var deletecookie = new HttpCookie("MyFavourites", anonymousUserId) { Secure = true, HttpOnly = true, Expires = DateTime.Now.AddDays(-1) };
                            HttpContext.Current.Response.Cookies.Add(deletecookie);
                            HttpContext.Current.Response.Cookies.Set(deletecookie);
                            HttpContext.Current.Request.Cookies.Remove("MyFavourites");
                            CacheHelper.RemoveFromCache(string.Format("{0}-Favourites", anonymousUserId));
                        }
                        else
                        {
                            CurrentDbContext.SaveChanges();
                            var cuf = user.Favourites.Select(s => s.ClassifiedAdId);
                            CacheHelper.SaveTocache(string.Format("{0}-Favourites", anonymousUserId), cuf, DateTime.Now.AddMinutes(5));
                        }
                        return false;
                    }
                }
                else
                {
                    // Create user cookie
                    var newuser = CurrentDbContext.AnonymousUserDB.Add(new AnonymousUser());
                    // Add to cart
                    newuser.Favourites.Add(new Favourited() { ClassifiedAdId = adId });
                    CurrentDbContext.SaveChanges();
                    var c = new HttpCookie("MyFavourites", newuser.Id.ToString()) { Expires = DateTime.Now.AddDays(30), Secure = true , HttpOnly = true};
                    HttpContext.Current.Response.Cookies.Add(c);
                    HttpContext.Current.Response.Cookies.Set(c);
                    var cuf = newuser.Favourites.Select(s => s.ClassifiedAdId);
                    CacheHelper.SaveTocache(string.Format("{0}-Favourites", newuser.Id.ToString()), cuf, DateTime.Now.AddMinutes(5));
                    return true;
                }
            }
        }

        public MyProfile GetCurrentUserProfile()
        {
            return (AccountManager.CurrentUser == null) ? null : Mapper.Map<MyProfile>(AccountManager.CurrentUser);
        }

        public ContactInfo GetContactInfo()
        {
            return Mapper.Map<ContactInfo>(AccountManager.CurrentUser);
        }

        public ContactInfo EditContactInfo(ContactInfo model)
        {
            
            {
                AccountManager.CurrentUser.ContactName = model.ContactName != null ? TextEditor.CleanAdContactName(model.ContactName) : null;
                AccountManager.CurrentUser.PhoneNumber = (!string.IsNullOrEmpty(model.ContactNumber) ? (model.ContactNumber.Contains("-") ? model.ContactNumber : model.ContactNumber.Insert(3, "-").ToString()) : model.ContactNumber);
                AccountManager.CurrentUser.PhoneNumber2 = (!string.IsNullOrEmpty(model.ContactNumber2) ? (model.ContactNumber2.Contains("-") ? model.ContactNumber2 : model.ContactNumber2.Insert(3, "-").ToString()) : model.ContactNumber2);
                AccountManager.CurrentUser.PhoneNumber3 = (!string.IsNullOrEmpty(model.ContactNumber3) ? (model.ContactNumber3.Contains("-") ? model.ContactNumber3 : model.ContactNumber3.Insert(3, "-").ToString()) : model.ContactNumber3);
                CurrentDbContext.SaveChanges();
            }
            return new ContactInfo() { ContactName = AccountManager.CurrentUser.ContactName, ContactNumber = AccountManager.CurrentUser.PhoneNumber };
        }

        public override void Dispose()
        {
            if(_accountManager != null)
            {
                _accountManager.Dispose();
                _accountManager = null;
            }
            base.Dispose();
        }
    }
}