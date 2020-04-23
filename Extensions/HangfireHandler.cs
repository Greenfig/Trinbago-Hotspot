using AutoMapper.QueryableExtensions;
using Hangfire;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Extensions
{
    public static class HangfireManager
    {
        public static void ScheduleDateCheck(int adId, DateTime start, DateTime end)
        {
            BackgroundJob.Schedule(() => ExpirationDateCheck(adId), end.Subtract(start).Subtract(new TimeSpan(14, 0, 0, 0)));
        }

        public static void ScheduleRemoval(int adId, DateTime start, DateTime end)
        {
            BackgroundJob.Schedule(() => AdRemoval(adId), end.Subtract(start));
        }

        public static void ScheduleUrgentAdExpiry(int adId, DateTime start, DateTime end)
        {
            BackgroundJob.Schedule(() => UrgentAdPromotionExpiry(adId), end.Subtract(start));
        }

        public static void ScheduleFeaturedAdExpiry(int adId, DateTime start, DateTime end)
        {
            BackgroundJob.Schedule(() => FeaturedAdPromotionExpiry(adId), end.Subtract(start));
        }

        public static void ScheduleTopAdExpiry(int adId, DateTime start, DateTime end)
        {
            BackgroundJob.Schedule(() => TopAdPromotionExpiry(adId), end.Subtract(start));
        }

        /// <summary>
        /// Check ad's expiry date
        /// </summary>
        /// <param name="adId"></param>
        public static void ExpirationDateCheck(int adId)
        {
            HangfireClassifiedAd ad;
            using (var newthreadcontext = new ApplicationDbContext())
            {
                // get user email and send first message to renew
                ad = newthreadcontext.ClassifiedDB.Include("Poster").ProjectTo<HangfireClassifiedAd>().SingleOrDefault(x => x.Id == adId);
                if (ad == null) return;
                // Check if user has already renewed ad
                if (ad.ExpiryTimeStamp.Subtract(DateTime.Now).Days > 14)
                {
                    ScheduleDateCheck(ad.Id, DateTime.Now, ad.ExpiryTimeStamp);
                    return;
                }
            }
#if DEBUG
            var request = new HttpRequest("/", "https://localhost:44333", "");
#else
            var request = new HttpRequest("/", "https://trinbagohotspot.com", "");
#endif
            var response = new HttpResponse(new StringWriter());
            var httpContext = new HttpContext(request, response);

            var httpContextBase = new HttpContextWrapper(httpContext);
            var routeData = new RouteData();
            var requestContext = new RequestContext(httpContextBase, routeData);

            var urlHelper = new UrlHelper(requestContext);
            // send email
            EmailMessenger.SendMessage(new HangfireMessage()
            {
                UserName = ad.PosterName,
                To = ad.PosterEmail,
                AdUrl = request.Url.OriginalString + urlHelper.Action("ShortAdDetails", "ClassifiedAd", new { Area = "ClassifiedAd", adId = ad.Id }),
                ExtendUrl = request.Url.OriginalString + urlHelper.Action("MyAdRenew", "ClassifiedAdManage", new { Area = "ClassifiedAd", adId = ad.Id }),
                DateTime = ad.ExpiryTimeStamp.ToString("yyyy-MMMM-dd")
            });
            // set delete job
            BackgroundJob.Schedule(() => AdRemoval(ad.Id), ad.ExpiryTimeStamp.Subtract(DateTime.Now));

        }

        /// <summary>
        /// Removes a classified ad with the specified adid
        /// </summary>
        /// <param name="adId"></param>
        /// <returns></returns>
        public static bool AdRemoval(int adId)
        {
            using (var CurrentDbContext = new ApplicationDbContext())
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
                            .SingleOrDefault(x => x.Id == adId);
                        if (ad == null) return false;
                        else if (ad.ExpiryTimeStamp.Value.Subtract(DateTime.Now).Ticks > 0)
                        {
                            ScheduleRemoval(ad.Id, DateTime.Now, ad.ExpiryTimeStamp.Value);
                            return false;
                        }
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
                        ad.Photos = photolist;
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
        }

        public static bool UrgentAdPromotionExpiry(int adId)
        {
            using (var CurrentDbContext = new ApplicationDbContext())
            {
                using (var _CurrentDbContext = CurrentDbContext.Database.BeginTransaction())
                {
                    var ad = CurrentDbContext.ClassifiedDB.Include("AdPromotion").FirstOrDefault(x => x.Id == adId);
                    if (ad != null)
                    {
                        if (ad.AdPromotion.UrgentAd != null && ad.AdPromotion.UrgentAd.Status)
                        {
                            if (ad.AdPromotion.UrgentAd.StartDate.Value.AddDays(ad.AdPromotion.UrgentAd.Duration).Ticks - ad.AdPromotion.UrgentAd.EndDate.Value.Ticks <= 0)
                            {
                                ad.AdPromotion.UrgentAd.Status = false;
                                ad.AdPromotion.UrgentAd.Duration = 0;
                                CurrentDbContext.SaveChanges();
                                _CurrentDbContext.Commit();
                                // Update lucene
                                LuceneSearch.AddUpdateLuceneIndex(ad);
                                return true;
                            }
                            else
                            {
                                BackgroundJob.Schedule(() => UrgentAdPromotionExpiry(adId), ad.AdPromotion.UrgentAd.EndDate.Value.Subtract(ad.AdPromotion.UrgentAd.StartDate.Value));
                                return false;
                            }
                        }
                    }
                    return false;
                }
            }
        }

        public static bool TopAdPromotionExpiry(int adId)
        {
            using (var CurrentDbContext = new ApplicationDbContext())
            {
                using (var _CurrentDbContext = CurrentDbContext.Database.BeginTransaction())
                {
                    var ad = CurrentDbContext.ClassifiedDB.Include("AdPromotion").FirstOrDefault(x => x.Id == adId);
                    if (ad != null)
                    {
                        if (ad.AdPromotion.TopAd != null && ad.AdPromotion.TopAd.Status)
                        {
                            if (ad.AdPromotion.TopAd.StartDate.Value.AddDays(ad.AdPromotion.TopAd.Duration).Ticks - ad.AdPromotion.TopAd.EndDate.Value.Ticks <= 0)
                            {
                                ad.AdPromotion.TopAd.Status = false;
                                ad.AdPromotion.TopAd.Duration = 0;
                                CurrentDbContext.SaveChanges();
                                _CurrentDbContext.Commit();
                                // Update lucene
                                LuceneSearch.AddUpdateLuceneIndex(ad);
                                return true;
                            }
                            else
                            {
                                BackgroundJob.Schedule(() => TopAdPromotionExpiry(adId), ad.AdPromotion.TopAd.EndDate.Value.Subtract(ad.AdPromotion.TopAd.StartDate.Value));
                                return false;
                            }
                        }
                    }
                    return false;
                }
            }
        }

        public static bool FeaturedAdPromotionExpiry(int adId)
        {
            using (var CurrentDbContext = new ApplicationDbContext())
            {
                using (var _CurrentDbContext = CurrentDbContext.Database.BeginTransaction())
                {
                    var ad = CurrentDbContext.ClassifiedDB.Include("AdPromotion").FirstOrDefault(x => x.Id == adId);
                    if (ad != null)
                    {
                        if (ad.AdPromotion.FeaturedAd != null && ad.AdPromotion.FeaturedAd.Status)
                        {
                            if (ad.AdPromotion.FeaturedAd.StartDate.Value.AddDays(ad.AdPromotion.FeaturedAd.Duration).Ticks - ad.AdPromotion.FeaturedAd.EndDate.Value.Ticks <= 0)
                            {
                                ad.AdPromotion.FeaturedAd.Status = false;
                                ad.AdPromotion.FeaturedAd.Duration = 0;
                                CurrentDbContext.SaveChanges();
                                _CurrentDbContext.Commit();
                                // Update lucene
                                LuceneSearch.AddUpdateLuceneIndex(ad);
                                return true;
                            }
                            else
                            {
                                BackgroundJob.Schedule(() => FeaturedAdPromotionExpiry(adId), ad.AdPromotion.FeaturedAd.EndDate.Value.Subtract(ad.AdPromotion.FeaturedAd.StartDate.Value));
                                return false;
                            }
                        }
                    }
                    return false;
                }
            }
        }

    }

    public class HangfireClassifiedAd
    {
        public int Id { get; set; }
        public string PosterName { get; set; }
        public string PosterEmail { get; set; }
        public DateTime ExpiryTimeStamp { get; set; }
    }
}