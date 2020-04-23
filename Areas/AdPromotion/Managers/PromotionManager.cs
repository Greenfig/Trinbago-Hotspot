using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trinbago_MVC5.Areas.Admin.Models;
using Trinbago_MVC5.Areas.Promotion.Models;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.Promotion.Managers
{
    /// <summary>
    /// Complicated methods will be declared here for all other simple access methods refer to the ApplicationUserManager
    /// </summary>
    public class PromotionManager : BaseApplicationManager
    {
        private string _userId;

        private bool SetUrgentPromotionAd(Trinbago_MVC5.Models.ClassifiedAd ad)
        {
            if (ad.AdPromotion.UrgentAd != null && !ad.AdPromotion.UrgentAd.Status && ad.AdPromotion.UrgentAd.Duration > 0)
            {
                ad.AdPromotion.UrgentAd.Status = true;
                ad.TimeStamp = DateTime.Now;
                ad.AdPromotion.UrgentAd.StartDate = DateTime.Now;
                ad.AdPromotion.UrgentAd.EndDate = ad.AdPromotion.UrgentAd.StartDate.Value.AddDays(PromotionStaticInfo.PromotionDuration.DurationRange.SingleOrDefault(x => x.Weeks == ad.AdPromotion.UrgentAd.Duration).Days);
                ad.ExpiryTimeStamp = ad.TimeStamp.AddMonths(3);
                CurrentDbContext.SaveChanges();
                HangfireManager.ScheduleUrgentAdExpiry(ad.Id, ad.AdPromotion.UrgentAd.StartDate.Value, ad.AdPromotion.UrgentAd.EndDate.Value);
                return true;
            }
            return false;
        }

        private bool SetTopPromotionAd(Trinbago_MVC5.Models.ClassifiedAd ad)
        {
            if (ad.AdPromotion.TopAd != null && !ad.AdPromotion.TopAd.Status && ad.AdPromotion.TopAd.Duration > 0)
            {
                ad.AdPromotion.TopAd.Status = true;
                ad.TimeStamp = DateTime.Now;
                ad.AdPromotion.TopAd.StartDate = DateTime.Now;
                ad.AdPromotion.TopAd.EndDate = ad.AdPromotion.TopAd.StartDate.Value.AddDays(PromotionStaticInfo.PromotionDuration.DurationRange.SingleOrDefault(x => x.Weeks == ad.AdPromotion.TopAd.Duration).Days);
                ad.ExpiryTimeStamp = ad.TimeStamp.AddMonths(3);
                CurrentDbContext.SaveChanges();
                HangfireManager.ScheduleTopAdExpiry(ad.Id, ad.AdPromotion.TopAd.StartDate.Value, ad.AdPromotion.TopAd.EndDate.Value);
                return true;
            }
            return false;
        }

        private bool SetFeaturedPromotionAd(Trinbago_MVC5.Models.ClassifiedAd ad)
        {
            if (ad.AdPromotion.FeaturedAd != null && !ad.AdPromotion.FeaturedAd.Status && ad.AdPromotion.FeaturedAd.Duration > 0)
            {
                ad.AdPromotion.FeaturedAd.Status = true;
                ad.TimeStamp = DateTime.Now;
                ad.AdPromotion.FeaturedAd.StartDate = DateTime.Now;
                ad.AdPromotion.FeaturedAd.EndDate = ad.AdPromotion.FeaturedAd.StartDate.Value.AddDays(PromotionStaticInfo.PromotionDuration.DurationRange.SingleOrDefault(x => x.Weeks == ad.AdPromotion.FeaturedAd.Duration).Days);
                ad.ExpiryTimeStamp = ad.TimeStamp.AddMonths(3);
                CurrentDbContext.SaveChanges();
                HangfireManager.ScheduleFeaturedAdExpiry(ad.Id, ad.AdPromotion.FeaturedAd.StartDate.Value, ad.AdPromotion.FeaturedAd.EndDate.Value);
                return true;
            }
            return false;
        }

        public string UserId
        {
            get
            {
                return _userId ?? HttpContext.Current.User.Identity.GetUserId();
            }
            private set
            {
                _userId = value;
            }
        }

        /// <summary>
        /// Promote ads
        /// </summary>
        /// <param name="paymentMethod"></param>
        /// <returns></returns>
        public bool AddPromotionsToClassifiedAd(string paymentMethod)
        {
            var itemsToPromote = GetUserShoppingCart();
            var total = 0.0;
            var order = new Order_History();

            using (var dbContextTransaction = CurrentDbContext.Database.BeginTransaction())
            {
                try
                {
                    var user = CurrentDbContext.Users.Include("PromotedClassifiedAds").Include("OrderHistory").SingleOrDefault(x => x.Id == CurrentUser.Id);
                    foreach (var itp in itemsToPromote)
                    {
                        var classified = CurrentDbContext.ClassifiedDB.Include("AdPromotion").SingleOrDefault(x => x.Id == itp.ClassifiedAdId);
                        order.Description += string.Format("<b>{0}</b><br>", classified.Title);
                        if (itp.BumpAdPIDays.HasValue)
                        {
                            classified.AdPromotion.BumpAd.StartDate = DateTime.Now;
                            classified.AdPromotion.BumpAd.EndDate = DateTime.Now.AddDays(itp.BumpAdPIDays.Value);
                            classified.AdPromotion.BumpAd.Duration = itp.BumpAdPIInterval.Value;
                            classified.AdPromotion.BumpAd.Status = true;
                            // order summary
                            order.Description += string.Format("Bump Ad : {0:C}<br>", itp.BumpAdPIPrice);
                            total += itp.BumpAdPIPrice.Value;
                            // Add to hangfire Queue
                        }
                        if (itp.TopAdPIDays.HasValue)
                        {
                            classified.AdPromotion.TopAd.EndDate = classified.AdPromotion.TopAd.StartDate = DateTime.Now;
                            classified.AdPromotion.TopAd.Duration = itp.TopAdPIDays.Value;
                            classified.AdPromotion.TopAd.EndDate.Value.AddDays(itp.TopAdPIDays.Value);
                            classified.AdPromotion.TopAd.Status = true;
                            // order summary
                            order.Description += string.Format("Top Ad: {0:C}<br>", itp.TopAdPIPrice);
                            total += itp.TopAdPIPrice.Value;
                            // Add to hangfire Queue
                        }
                        if (itp.FeaturedAdPIDays.HasValue)
                        {
                            classified.AdPromotion.FeaturedAd.EndDate = classified.AdPromotion.FeaturedAd.StartDate = DateTime.Now;
                            classified.AdPromotion.FeaturedAd.Duration = itp.FeaturedAdPIDays.Value;
                            classified.AdPromotion.FeaturedAd.EndDate.Value.AddDays(itp.FeaturedAdPIDays.Value);
                            classified.AdPromotion.FeaturedAd.Status = true;
                            // order summary
                            order.Description += string.Format("Featured Ad: {0:C}<br>", itp.FeaturedAdPIPrice);
                            total += itp.FeaturedAdPIDays.Value;
                            // Add to hangfire Queue
                        }
                        if (itp.UrgentAdPIDays.HasValue)
                        {
                            classified.AdPromotion.UrgentAd.StartDate = DateTime.Now;
                            classified.AdPromotion.UrgentAd.Status = true;
                            // order summary
                            order.Description += string.Format("Urgent Ad: {0:C}<br>", itp.UrgentAdPIPrice);
                            total += itp.UrgentAdPIPrice.Value;
                        }
                        CurrentDbContext.SaveChanges();
                        // Remove old Lucene
                        LuceneSearch.ClearLuceneIndexRecord(itp.ClassifiedAdId);
                        // Add to Lucene
                        LuceneSearch.AddUpdateLuceneIndex(classified);
                    }
                    order.Description += string.Format("<b>Payment Method: {0:C} via {1}</b>", total, paymentMethod);
                    order.Date = DateTime.Now;
                    // Ad order to user 
                    CurrentDbContext.UserOrdersDB.Add(order);
                    user.OrderHistory.Add(order);
                    CurrentDbContext.SaveChanges();
                    dbContextTransaction.Commit();

                    foreach (var itp in itemsToPromote)
                    {
                        if (itp.BumpAdPIDays.HasValue)
                        {
                            // Add to hangfire Queue
                        }
                        if (itp.TopAdPIDays.HasValue)
                        {
                            // Add to hangfire Queue
                        }
                        if (itp.FeaturedAdPIDays.HasValue)
                        {
                            // Add to hangfire Queue
                        }
                        RemoveFromShoppingCart(itp.ClassifiedAdId);
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public AdminPromote PromoteAd(AdminPromoteClassifiedAd ad) 
        {
            // Check if ad paramiter is not null
            if (ad == null) return null;
            // Get Ad
            var pro = CurrentDbContext.ClassifiedDB.Include("AdPromotion").SingleOrDefault(x => x.Id == ad.Id);
            if (pro == null) return null;
            // Save promotion changes
            if (pro.AdPromotion != null)
            {
                // save urgent ad
                if(ad.AdPromotion.UrgentAd.Duration > 0)
                {
                    pro.AdPromotion.UrgentAd.Duration = ad.AdPromotion.UrgentAd.Duration;
                    SetUrgentPromotionAd(pro);
                }
                // save featured ad  
                if (ad.AdPromotion.FeaturedAd.Duration > 0)
                {
                    pro.AdPromotion.FeaturedAd.Duration = ad.AdPromotion.FeaturedAd.Duration;
                    SetFeaturedPromotionAd(pro);
                }
                // save Top ad
                if (ad.AdPromotion.TopAd.Duration > 0)
                {
                    pro.AdPromotion.TopAd.Duration = ad.AdPromotion.TopAd.Duration;
                    SetTopPromotionAd(pro);
                }
                LuceneSearch.AddUpdateLuceneIndex(pro);                
            }
            return Mapper.Map<AdminPromote>(pro);
        }

        /// <summary>
        /// Remove ad promotion
        /// </summary>
        public AdminPromote RemovePromoteAd(int adId)
        {
            var demote = CurrentDbContext.ClassifiedDB.Include("AdPromotion").SingleOrDefault(x => x.Id == adId);
            if (demote == null) return null;
            // Urgent Ad
            demote.AdPromotion.UrgentAd.Duration = 0;
            demote.AdPromotion.UrgentAd.EndDate = null;
            demote.AdPromotion.UrgentAd.StartDate = null;
            demote.AdPromotion.UrgentAd.Status = false;
            // Top Ad
            demote.AdPromotion.TopAd.Duration = 0;
            demote.AdPromotion.TopAd.EndDate = null;
            demote.AdPromotion.TopAd.StartDate = null;
            demote.AdPromotion.TopAd.Status = false;
            // Featured Ad
            demote.AdPromotion.FeaturedAd.Duration = 0;
            demote.AdPromotion.FeaturedAd.StartDate = null;
            demote.AdPromotion.FeaturedAd.EndDate = null;
            demote.AdPromotion.FeaturedAd.Status = false;
            CurrentDbContext.SaveChanges();
            LuceneSearch.AddUpdateLuceneIndex(demote);
            return Mapper.Map<AdminPromote>(demote);
        }

        public IEnumerable<Order_History> GetUserOrderHistory()
        {
            return UserManager.AccountManager.Users.Include(u => u.OrderHistory).Select(x => new { Id = x.Id, OrderHistory = x.OrderHistory.OrderByDescending(o => o.Date) }).FirstOrDefault(f => f.Id == CurrentUser.Id).OrderHistory;
        }

        public bool AddPromotionToCart(int adId, int? bumpAd, bool urgentAd, int? topAd, int? featuredAd)
        {
            if (!bumpAd.HasValue && !urgentAd && !topAd.HasValue && !featuredAd.HasValue) return false;
            if (!IsAdInCart(adId))
            {
                using (var dbContextTransaction = CurrentDbContext.Database.BeginTransaction())
                {
                    CartItem newCartItem = new CartItem();
                    newCartItem.ClassifiedAd = CurrentDbContext.ClassifiedDB.SingleOrDefault(x => x.Id == adId);
                    newCartItem.User = CurrentUser;
                    CurrentDbContext.PromotionCartDB.Add(newCartItem);
                    CurrentDbContext.SaveChanges();
                }
            }
            return true;
        }

        /// <summary>
        /// Get ad to be promoted by Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="adId"></param>
        /// <returns></returns>
        public T GetPromoteAdById<T>(int adId)
        {
            var ad = CurrentDbContext.ClassifiedDB.Include("AdPromotion").Where(x => x.Id == adId).ProjectTo<T>().FirstOrDefault();
            return ad != null ? ad : default(T);
        }

        public IPagedList<ClassifiedAdPromotionList> GetUserNonPromotedAdList(int pageNumber = 1)
        {
            return GetUserNonPromotedAdList().ToPagedList(pageNumber, RecordsPerPage.Records);
        }

        public IEnumerable<CartItemList> GetUserPromotedAds()
        {
            return null;//CurrentUser.GetUserPromotedAds();
        }

        public IEnumerable<CartItemList> GetUserShoppingCart()
        {
            return UserManager.AccountManager.Users
                .Include(x => x.PromotionCart)
                .Include(x => x.PromotionCart.Select(p => p.TopAdPI))
                .Include(x => x.PromotionCart.Select(p => p.UrgentAdPI))
                .Include(x => x.PromotionCart.Select(p => p.BumpAdPI))
                .Include(x => x.PromotionCart.Select(p => p.ClassifiedAd))
                .Include(x => x.PromotionCart.Select(p => p.FeaturedAdPI))
                .ProjectTo<UserCartItemSlim>()
                .FirstOrDefault(x => x.Id == UserId).PromotionCart;
        }

        public bool RemoveFromShoppingCart(int adId)
        {

            {
                using (var dbTansactions = CurrentDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var cartitem = CurrentDbContext.Users
                            .Include("PromotionCart.ClassifiedAd").Select(u => new { Id = u.Id, PromotionCart = u.PromotionCart })
                            .FirstOrDefault(x => x.Id == UserId).PromotionCart.FirstOrDefault(p => p.ClassifiedAd.Id == adId);
                        var pI = CurrentDbContext.PromotionInfoDB.Include("CartItem").Where(x => x.CartItem.Id == cartitem.Id);
                        CurrentDbContext.PromotionInfoDB.RemoveRange(pI);
                        CurrentDbContext.SaveChanges();
                        CurrentDbContext.PromotionCartDB.Remove(cartitem);
                        CurrentDbContext.SaveChanges();
                        dbTansactions.Commit();
                        return true;
                    }
                    catch (Exception)
                    { return false; }
                }
            }
        }

        public bool RemoveFromPromotedList(string userId, int adId)
        {
            using (var dbTansactions = CurrentDbContext.Database.BeginTransaction())
            {
                try
                {
                    /*var cartitem = newthreadcontext.Users
                        .Include("PromotedClassfiedAds").Select(u => new { Id = u.Id, PromotedAds = u.PromotedClassfiedAds })
                        .SingleOrDefault(x => x.Id == userId).PromotedAds.SingleOrDefault(p => p.StringId == adStringId);
                    var pI = newthreadcontext.PromotionInfoDB.Include("CartItem").Where(x => x.CartItem.Id == cartitem.Id);
                    newthreadcontext.PromotionInfoDB.RemoveRange(pI);
                    newthreadcontext.SaveChanges();
                    newthreadcontext.PromotionCartDB.Remove(cartitem);
                    newthreadcontext.SaveChanges();
                    dbTansactions.Commit();*/
                    return true;
                }
                catch (Exception)
                { return false; }
            }
        }

        /// <summary>
        /// Check if Ad exists in cart
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="adId"></param>
        /// <returns></returns>
        public bool IsAdInCart(int adId)
        {
            return UserManager.AccountManager.Users
                .Include(x => x.PromotionCart.Select(c => c.ClassifiedAd.Id)).Select(s => new { uid = s.Id, adids = s.PromotionCart.Select(p => p.ClassifiedAd.Id) })
                .FirstOrDefault(x => x.uid == UserId).adids.Any(ai => ai == adId);
        }

        /// <summary>
        /// Check if Ad is promoted
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="adStringId"></param>
        /// <returns></returns>
        public bool IsAdPromoted(string userId, int adId)
        {
            return UserManager.AccountManager.Users
                .Include(x => x.ClassifiedAds)
                .Select(s => new { uid = s.Id, adids = s.ClassifiedAds.Select(cs => cs.Id)})
                .FirstOrDefault(x => x.uid == userId).adids.Any(c => c == adId);
        }

        public IEnumerable<ClassifiedAdPromotionList> GetUserNonPromotedAdList(string id)
        {
            return UserManager.AccountManager.Users
                    .Include(x => x.ClassifiedAds)
                    .Include(x => x.ClassifiedAds.Select(ap => ap.Photos))
                    .Include(x => x.ClassifiedAds.Select(ai => ai.AdInfo))
                    .Include(x => x.ClassifiedAds.Select(v => v.AdViews))
                    .ProjectTo<UserAdListSlim>()
                    .FirstOrDefault(u => u.Id == id).MyAdList;
        }

        public IEnumerable<ClassifiedAdPromoted> GetPromotedAds(string id)
        {
            return UserManager.AccountManager.Users.Include(x => x.ClassifiedAds).ProjectTo<UserPromotedList>().FirstOrDefault(x => x.Id == id).PromotedList;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}