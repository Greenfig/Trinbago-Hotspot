using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Areas.Promotion.Managers;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Models;
using ClassifedAdAlias = Trinbago_MVC5.Models.ClassifiedAd;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Managers
{
    public class ClassifiedAdManager : BaseApplicationManager
    {
        private string _userId;

        // Access Promotion Manager
        private PromotionManager _promoManager;

        public PromotionManager PromoManager
        {
            get
            {
                return _promoManager ?? new PromotionManager();
            }
            private set
            {
                _promoManager = value;
            }
        }

        private string UserId
        {
            get
            {
                return _userId ?? HttpContext.Current.User.Identity.GetUserId();
            }
            set
            {
                _userId = value;
            }
        }

        // Add new Classified Ad
        public ClassifiedAdBase AddClassifiedAd(ClassifiedAdAdd newItem)
        {
            // 1. Set time
            // 2. Add item to db
            // 3. Add photos to data store then to temporary collection
            // 4. Create thumbnail image
            // 5. Add Location
            // 6. Add collection to db item
            // 7. Save database 
            // set up automapper to map classified

            return Mapper.Map<ClassifiedAdBase>(newItem);
        }

        /// <summary>
        /// EDIT FORM
        /// </summary>
        /// <returns></returns>
        public ClassifiedAdBase EditClassifiedAd(ClassifiedAdEdit editItem)
        {

            // 1. Ensure user credibility
            // 2. Attempt to fetch obj
            // 3. Set last edited time
            // 4. Pull existing item
            // 5. Compare photos for changes and add/remove
            // 6. Save changes
            return Mapper.Map<ClassifiedAdBase>(editItem);
        }

        public bool RemoveClassifiedAd(int Id)
        {
            using (var _CurrentDbContext = CurrentDbContext.Database.BeginTransaction())
            {
                try
                {
                    var ad = CurrentDbContext.ClassifiedDB
                        .Include("Poster")
                        .Include("Country")
                        .Include("Region")
                        .Include("SubCategory")
                        .Include("Category")
                        .Include("AdPromotion")
                        .Include("Photos")
                        .Include("AdInfo")
                        .Include("Reports")
                        .Include("AdViews")
                        .SingleOrDefault(x => x.Id == Id);
                    if (ad == null && (ad.Poster != CurrentUser && (!HttpContext.Current.User.IsInRole("Admin") || !HttpContext.Current.User.IsInRole("Moderator")))) return false;
                    //
                    var photolist = ad.Photos.ToList();
                    // remove category
                    if (ad.Category != null)
                    {
                        if (ad.Status == 0)
                            ad.Category.TotalClassifiedAdsCount--;
                        CurrentDbContext.SaveChanges();
                    }
                    // remove subcategory
                    if (ad.SubCategory != null)
                    {
                        if (ad.Status == 0)
                            ad.SubCategory.ClassifiedAdsCount--;
                        CurrentDbContext.SaveChanges();
                    }
                    // remove photos
                    if (ad.Photos != null)
                    {
                        CurrentDbContext.AdPhotosDB.RemoveRange(ad.Photos);
                        CurrentDbContext.SaveChanges();
                    }
                    // remove info
                    if (ad.AdInfo != null)
                    {
                        CurrentDbContext.InfosDB.RemoveRange(ad.AdInfo);
                        CurrentDbContext.SaveChanges();
                    }
                    // remove reports
                    if (ad.Reports != null)
                    {
                        CurrentDbContext.ReportDB.RemoveRange(ad.Reports);
                        ad.Reports = null;
                        CurrentDbContext.SaveChanges();
                    }
                    // remove ad promotions
                    if (ad.AdPromotion != null)
                    {
                        //ap.ClassifiedAd = null;
                        CurrentDbContext.AdPromotionDB.Remove(ad.AdPromotion);
                        CurrentDbContext.SaveChanges();
                    }
                    // remove ad
                    CurrentDbContext.ClassifiedDB.Remove(ad);
                    CurrentDbContext.SaveChanges();
                    // remove ad views
                    if (ad.AdViews != null)
                    {
                        CurrentDbContext.AdViewsDB.Remove(ad.AdViews);
                        CurrentDbContext.SaveChanges();
                    }
                    // Remove from favourites
                    if (CurrentDbContext.FavouirtedDb.Any(x => x.ClassifiedAdId == ad.Id))
                    {
                        var rem = CurrentDbContext.FavouirtedDb.Where(x => x.ClassifiedAdId == ad.Id);
                        CurrentDbContext.FavouirtedDb.RemoveRange(rem);
                        CurrentDbContext.SaveChanges();
                    }
                    _CurrentDbContext.Commit();
                    // Remove all images from filestore
                    LuceneSearch.DeletePhotosFromLuceneIndex(ad.Id, ad.StringId, photolist);
                    // remove from lucene index
                    LuceneSearch.ClearLuceneIndexRecord(ad.Id, photolist);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool IsAdSuspended(int value)
        {
            var ad = CurrentDbContext.ClassifiedDB.Select(x => new { Id = x.Id, Status = x.Status }).SingleOrDefault(s => s.Id == value);
            if (ad == null)
                return false;
            if (ad.Status == -1)
                return true;
            return false;
        }

        public ClassifiedAdWithDetail MyAdPreview(int adId)
        {
            if(HttpContext.Current.User.IsInRole("Admin") || HttpContext.Current.User.IsInRole("Moderator"))
                return CurrentDbContext.ClassifiedDB.ProjectTo<ClassifiedAdWithDetail>().FirstOrDefault(x => x.Id == adId);
            var toret = CurrentDbContext.ClassifiedDB.Include("Poster").FirstOrDefault(x => x.Id == adId && x.Poster.Id == UserId);
            return toret != null ? Mapper.Map<ClassifiedAdWithDetail>(toret) : null;
        }

        public string GetCountryBySeoName(string location)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get User's Posted Ad List
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="searchType"></param>
        /// <returns></returns>
        public IPagedList<ClassifiedAdMyList> GetUserAdList(string searchType = "All", int pageNumber = 1)
        {
            switch (searchType)
            {
                case "All":
                    return UserManager.AccountManager.Users
                        .Include(x => x.ClassifiedAds)
                        .Include(x => x.ClassifiedAds.Select(ap => ap.Photos))
                        .Include(x => x.ClassifiedAds.Select(ai => ai.AdInfo))
                        .Include(x => x.ClassifiedAds.Select(v => v.AdViews))
                        .ProjectTo<UserAdList>()
                        .FirstOrDefault(u => u.Id == UserId).ClassifiedAds.ToPagedList(pageNumber, RecordsPerPage.Records);
                case "Pending":
                    return UserManager.AccountManager.Users
                        .Include(x => x.ClassifiedAds)
                        .Include(x => x.ClassifiedAds.Select(ap => ap.Photos))
                        .Include(x => x.ClassifiedAds.Select(ai => ai.AdInfo))
                        .Include(x => x.ClassifiedAds.Select(v => v.AdViews))
                        .ProjectTo<UserAdListPending>()
                        .FirstOrDefault(u => u.Id == UserId).ClassifiedAds.ToPagedList(pageNumber, RecordsPerPage.Records);
                case "Suspended":
                    return UserManager.AccountManager.Users
                        .Include(x => x.ClassifiedAds)
                        .Include(x => x.ClassifiedAds.Select(ap => ap.Photos))
                        .Include(x => x.ClassifiedAds.Select(ai => ai.AdInfo))
                        .Include(x => x.ClassifiedAds.Select(v => v.AdViews))
                        .ProjectTo<UserAdListSuspended>()
                        .FirstOrDefault(u => u.Id == UserId).ClassifiedAds.ToPagedList(pageNumber, RecordsPerPage.Records);
                case "Open":
                    return UserManager.AccountManager.Users
                        .Include(x => x.ClassifiedAds)
                        .Include(x => x.ClassifiedAds.Select(ap => ap.Photos))
                        .Include(x => x.ClassifiedAds.Select(ai => ai.AdInfo))
                        .Include(x => x.ClassifiedAds.Select(v => v.AdViews))
                        .ProjectTo<UserAdListOpen>()
                        .FirstOrDefault(u => u.Id == UserId).ClassifiedAds.ToPagedList(pageNumber, RecordsPerPage.Records);
                case "Renewable":
                    return UserManager.AccountManager.Users
                        .Include(x => x.ClassifiedAds)
                        .Include(x => x.ClassifiedAds.Select(ap => ap.Photos))
                        .Include(x => x.ClassifiedAds.Select(ai => ai.AdInfo))
                        .Include(x => x.ClassifiedAds.Select(v => v.AdViews))
                        .ProjectTo<UserAdListOpen>()
                        .FirstOrDefault(u => u.Id == UserId).ClassifiedAds.Where(x => x.ExpiryTimeStamp.Subtract(DateTime.Now).Days <= 14).ToPagedList(pageNumber, RecordsPerPage.Records);
                case "Sold":
                    return UserManager.AccountManager.Users
                        .Include(x => x.ClassifiedAds)
                        .Include(x => x.ClassifiedAds.Select(ap => ap.Photos))
                        .Include(x => x.ClassifiedAds.Select(ai => ai.AdInfo))
                        .Include(x => x.ClassifiedAds.Select(v => v.AdViews))
                        .ProjectTo<UserAdListSold>()
                        .FirstOrDefault(u => u.Id == UserId).ClassifiedAds.ToPagedList(pageNumber, RecordsPerPage.Records);
                case "Rented":
                    return UserManager.AccountManager.Users
                        .Include(x => x.ClassifiedAds)
                        .Include(x => x.ClassifiedAds.Select(ap => ap.Photos))
                        .Include(x => x.ClassifiedAds.Select(ai => ai.AdInfo))
                        .Include(x => x.ClassifiedAds.Select(v => v.AdViews))
                        .ProjectTo<UserAdListRented>()
                        .FirstOrDefault(u => u.Id == UserId).ClassifiedAds.ToPagedList(pageNumber, RecordsPerPage.Records);
                default:
                    return new List<ClassifiedAdMyList>().ToPagedList(1,1);
            }
        }

        public ClassifiedAdMyAdRenew GetClassifiedAdRenew(int adId)
        {
            var ad = CurrentDbContext.ClassifiedDB.Include("Poster").Where(x => x.Id == adId && x.Poster.Id == UserId).ProjectTo<ClassifiedAdMyAdRenew>().FirstOrDefault();
            if (ad == null) return null;
            return ad;
        }

        // Get edit ad
        public ClassifiedAdEditForm GetClassifiedAdWithAll(int adId)
        {
            var val = CurrentDbContext.ClassifiedDB
                    .FirstOrDefault(a => a.Id == adId && a.Poster.Id == UserId && a.Status == 0
                    || a.Id == adId && a.Poster.Id == UserId && a.Status == -1
                    || a.Id == adId && a.Poster.Id == UserId && a.Status == -2);
            return val != null ? Mapper.Map<ClassifiedAdEditForm>(val) : null;
        }

        /// <summary>
        /// Get one ad with details for ListDetail.cshtml
        /// </summary>
        /// <param name="stringId"></param>
        /// <returns></returns>w
        public ClassifiedAdWithDetail GetClassifiedAdWithDetails(string stringId)
        {
            // Check for session id
            var ad = CurrentDbContext.ClassifiedDB.ProjectTo<ClassifiedAdWithDetail>().FirstOrDefault(x => x.StringId == stringId);
            if (ad != null)
            {
                ad.SeoCategory = SeoManager.GetSeoCategory(ad.Category.Id, ad.SubCategory.Id);
                ad.SeoLocation = SeoManager.GetSeoLocation(ad.Country.Id, ad.Region.Id);
            }
            return ad;
        }

        public bool MyAdClose(int adId, string closingOptions, HttpServerUtilityBase server)
        {
            // get existing
            try
            {
                ClassifedAdAlias obj = null;
                switch (closingOptions)
                {
                    case "Remove":
                        if (PromoManager.IsAdInCart(adId)) PromoManager.RemoveFromShoppingCart(adId);
                        obj = CurrentDbContext.ClassifiedDB
                            .Include("Poster")
                            .SingleOrDefault(x => x.Id == adId && x.Status != 1 && x.Status != 2 && x.Status != 3);
                        obj.Status = 3;
                        obj.NeedApproval = true;
                        CurrentDbContext.SaveChanges();
                        break;
                    case "Sold":
                        obj = CurrentDbContext.ClassifiedDB
                            .Include("Poster")
                            .SingleOrDefault(x => x.Id == adId && x.Status != 1 && x.Status != 2 && x.Status != 3);
                        if (obj == null && obj.Poster != CurrentUser) return false;
                        obj.Status = 1;
                        obj.NeedApproval = true;
                        CurrentDbContext.SaveChanges();
                        goto default;
                    case "Rented":
                        obj = CurrentDbContext.ClassifiedDB
                            .Include("Poster")
                            .SingleOrDefault(x => x.Id == adId && x.Status != 1 && x.Status != 2 && x.Status != 3);
                        if (obj == null && obj.Poster != CurrentUser) return false;
                        obj.Status = 2;
                        obj.NeedApproval = true;
                        CurrentDbContext.SaveChanges();
                        goto default;
                    default:
                        //LuceneSearch.AddUpdateLuceneIndex(obj);
                        break;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RenewMyAd(int adId)
        {
            var now = DateTime.Now;
            var ad = CurrentDbContext.ClassifiedDB.Include("Poster").SingleOrDefault(x => x.Id == adId && x.Poster.Id == UserId);
            if (ad == null || ad.ExpiryTimeStamp.Value.Subtract(now).Days > 14) return false;
            ad.ExpiryTimeStamp = ExpiryDate.CalculateDate(DateTime.Now, ad.AdType, ad.Category.Name, ad.SubCategory.Name);
            CurrentDbContext.SaveChanges();
            HangfireManager.ScheduleDateCheck(ad.Id, DateTime.Now, ad.ExpiryTimeStamp.Value);
            return true;
        }

        public override void Dispose()
        {
            if(_promoManager != null)
            {
                _promoManager.Dispose();
                _promoManager = null;
            }
            base.Dispose();
        }
    }
}