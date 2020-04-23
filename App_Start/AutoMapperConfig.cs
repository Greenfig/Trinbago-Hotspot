using AutoMapper;
using Lucene.Net.Documents;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.WebPages;
using Trinbago_MVC5.Areas.Admin.Models;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Areas.Promotion.Models;
using Trinbago_MVC5.Areas.User.Models;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5
{
    public static class AutoMapperConfig
    {
        private static string CurrentUserId
        {
            get { return HttpContext.Current?.User?.Identity?.GetUserId(); }
        }

        private static string CurrentAnonymousUserId
        {
            get { return HttpContext.Current?.Request?.Cookies["MyFavourites"]?.Value?.ToString(); }
        }

        private static IEnumerable<int> CurrentUserFavouries
        {
            get
            {
                IEnumerable<int> cuf = new List<int>();
                // Check current user
                if (CurrentUserId != null)
                {
                    cuf = CacheHelper.GetFromCache<IEnumerable<int>>(string.Format("{0}-Favourites", CurrentUserId));
                    if (cuf == null)
                    {
                        // Create list from current user
                        using (var newthreadcontext = new ApplicationDbContext())
                        {
                            cuf = newthreadcontext.Users.SingleOrDefault(x => x.Id == CurrentUserId).Favourites.Select(s => s.ClassifiedAdId);
                            CacheHelper.SaveTocache(string.Format("{0}-Favourites", CurrentUserId), cuf, DateTime.Now.AddMinutes(5));
                        }
                    }
                }
                else if (CurrentAnonymousUserId != null)
                {
                    cuf = CacheHelper.GetFromCache<IEnumerable<int>>(string.Format("{0}-Favourites", CurrentAnonymousUserId));
                    if (cuf == null)
                    {
                        using (var newthreadcontext = new ApplicationDbContext())
                        {
                            cuf = newthreadcontext.AnonymousUserDB.SingleOrDefault(x => x.Id.ToString() == CurrentAnonymousUserId)?.Favourites.Select(s => s.ClassifiedAdId);
                            // cleanup cookie if user does not exist in db
                            if (cuf == null)
                            {
                                var deletecookie = new HttpCookie("MyFavourites", CurrentAnonymousUserId) { Secure = true, HttpOnly = true, Expires = DateTime.Now.AddDays(-1) };
                                HttpContext.Current.Response.Cookies.Add(deletecookie);
                                HttpContext.Current.Response.Cookies.Set(deletecookie);
                                return new List<int>();
                            }
                            CacheHelper.SaveTocache(string.Format("{0}-Favourites", CurrentAnonymousUserId), cuf, DateTime.Now.AddMinutes(5));
                        }
                    }
                }

                return cuf;
            }
        }

        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<AdminPromoteClassifiedAd, AdminPromote>()
                    .ForMember(dest => dest.Ad, opt => opt.MapFrom(x => x));
                cfg.CreateMap<ClassifiedAd, AdminPromote>()
                    .ForMember(dest => dest.Ad, opt => opt.MapFrom(x => x));
                cfg.CreateMap<ClassifiedAd, ClassifiedAdLucene>()
                    .ForMember(dest => dest.PosterName, opt => opt.MapFrom(x => x.Poster.PosterName));
                cfg.CreateMap<ClassifiedAd, ClassifiedAdBase>();
                cfg.CreateMap<ClassifiedAd, ClassifiedAdAdminBase>();
                cfg.CreateMap<ClassifiedAd, ClassifiedAdQueueList>();
                cfg.CreateMap<ClassifiedAd, ClassifiedAdMyList>()
                    .ForMember(dest => dest.AdPhoto, opt => opt.MapFrom(x => x.Photos.FirstOrDefault(y => y.PhotoLayoutIndex == 0).AdList_FileName));
                cfg.CreateMap<ClassifiedAd, ClassifiedAdMyAdRenew>()
                    .ForMember(dest => dest.AdPhoto, opt => opt.MapFrom(x => x.Photos.FirstOrDefault(y => y.PhotoLayoutIndex == 0).AdList_FileName));
                cfg.CreateMap<ClassifiedAd, ClassifiedAdEditForm>();
                cfg.CreateMap<ClassifiedAd, HangfireClassifiedAd>()
                    .ForMember(dest => dest.PosterName, opt => opt.MapFrom(x => x.Poster.PosterName));
                cfg.CreateMap<ClassifiedAd, AdminPromoteClassifiedAd>();
                cfg.CreateMap<ClassifiedAdEditForm, AdminClassifiedAdEditForm>();
                cfg.CreateMap<AdminClassifiedAdEdit, AdminClassifiedAdEditForm>();
                cfg.CreateMap<ClassifiedAdAdd, ClassifiedAdAddForm>();
                cfg.CreateMap<ClassifiedAdEdit, ClassifiedAdEditForm>();
                // Edit Ad
                // Proof of concept.. Ideally you would NOT want business logic inside an automap
                cfg.CreateMap<ClassifiedAdEdit, ClassifiedAdBase>()
                    .AfterMap((editItem, dest) =>
                    {
                        using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
                        {
                            using (var dbContextTransaction = newthreadcontext.Database.BeginTransaction())
                            {
                                // Updadte Ad in database
                                var o = newthreadcontext.ClassifiedDB.Include("Poster").SingleOrDefault(x => x.StringId == editItem.StringId);
                                var newphotolist = Mapper.Map<IEnumerable<ClassifiedAdPhoto>>(editItem.Photos);
                                // get removed photos that exists on the database
                                newthreadcontext.AdPhotosDB.RemoveRange(o.Photos);
                                if (newphotolist != null)
                                {
                                    foreach (var pho in newphotolist)
                                    {
                                        o.Photos.Add(pho);
                                    }
                                }

                                o.EditTimeStamp = DateTime.Now;

                                if (o.ContactPrivacy != editItem.ContactPrivacy)
                                {
                                    o.ContactPrivacy = editItem.ContactPrivacy;
                                }

                                /// Upadate Country/Region
                                /// 
                                if (o.Country.Id != editItem.CountryId)
                                {
                                    var country = newthreadcontext.CountryDB.SingleOrDefault(a => a.Id == editItem.CountryId);
                                    o.Country = country;
                                }
                                if (o.Region.Id != editItem.RegionId)
                                {
                                    var region = newthreadcontext.RegionDB.SingleOrDefault(a => a.Id == editItem.RegionId);
                                    o.Region = region;
                                }

                                if (editItem.Price == "Please Contact" && editItem.PriceInfo != "Please Contact")
                                    editItem.PriceInfo = "Please Contact";

                                // update price
                                var pri = Convert.ToInt32(editItem.Price.Equals("Please Contact") ? "0" : editItem.Price.Replace(",", ""));
                                if (o.Price != pri)
                                    o.Price = pri;

                                // update desc
                                if (o.Description != editItem.Description)
                                {
                                    o.Description = editItem.Description;
                                    o.HtmlFreeDescription = HtmlFilter.RemoveUnwantedTags(o.Description);
                                }

                                // update title
                                if (o.Title != editItem.Title)
                                    o.Title = editItem.Title.Trim();

                                // update price info
                                if (o.PriceInfo != editItem.PriceInfo)
                                {
                                    o.PriceInfo = editItem.PriceInfo;
                                }

                                // update AdInfo
                                if (editItem.AdInfo != null && o.AdInfo != null)
                                {
                                    if (editItem.AdInfo.Count > 0 && o.AdInfo.Count > 0)
                                    {
                                        foreach (var ai in editItem.AdInfo)
                                        {
                                            // get the current adInfo from the original object
                                            var currentai = o.AdInfo.SingleOrDefault(x => x.Name.Equals(ai.Name));
                                            // if currentai is null add it to list
                                            if (currentai == null)
                                            {
                                                o.AdInfo.Add(new Info() { ClassifiedAd = o, Description = ai.Description.Trim(), Name = ai.Name });
                                            }
                                            // if both adinfo are not equal to null ..then compare them
                                            else if (currentai.Description != null && ai.Description != null)
                                            {
                                                // Check if desc is equal to what is on database
                                                if (!currentai.Description.Equals(ai.Description))
                                                {
                                                    // Format logic for adinfo
                                                    if (currentai.Name == "Engine Size")
                                                        currentai.Description = ai.Description.Replace(" ", "");
                                                    else
                                                        currentai.Description = ai.Description.Trim();
                                                }
                                            }
                                            // one of the two adinfo is null
                                            else
                                            {
                                                if (ai.Description != null)
                                                {
                                                    // Format logic for adinfo
                                                    if (ai.Name == "Engine Size")
                                                        currentai.Description = ai.Description.Replace(" ", "");
                                                    else
                                                        currentai.Description = ai.Description.Trim();
                                                }
                                                else
                                                    currentai.Description = null;
                                            }
                                        }
                                    }
                                }

                                if (!editItem.AdContactName.Equals(o.AdContactName))
                                    o.AdContactName = TextEditor.CleanAdContactName(editItem.AdContactName);

                                if (!editItem.AdContactPhone.Equals(o.AdContactPhone))
                                {
                                    editItem.AdContactPhone = (editItem.AdContactPhone.Contains("-") ? editItem.AdContactPhone : editItem.AdContactPhone.Insert(3, "-").ToString());
                                    o.AdContactPhone = editItem.AdContactPhone;
                                }
                                if (!string.IsNullOrEmpty(editItem.AdContactPhone2))
                                {
                                    if (!editItem.AdContactPhone2.Equals(o.AdContactPhone2))
                                    {
                                        editItem.AdContactPhone2 = (editItem.AdContactPhone2.Contains("-") ? editItem.AdContactPhone2 : editItem.AdContactPhone2.Insert(3, "-").ToString());
                                        o.AdContactPhone2 = editItem.AdContactPhone2;
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(o.AdContactPhone2))
                                        o.AdContactPhone2 = null;
                                }
                                if (!string.IsNullOrEmpty(editItem.AdContactPhone3))
                                {
                                    if (!editItem.AdContactPhone3.Equals(o.AdContactPhone3))
                                    {
                                        editItem.AdContactPhone3 = (editItem.AdContactPhone3.Contains("-") ? editItem.AdContactPhone3 : editItem.AdContactPhone3.Insert(3, "-").ToString());
                                        o.AdContactPhone3 = editItem.AdContactPhone3;
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(o.AdContactPhone3))
                                        o.AdContactPhone3 = null;
                                }

                                if (!editItem.AdContactEmail.Equals(o.AdContactEmail))
                                    o.AdContactEmail = editItem.AdContactEmail;

                                // Flag for screening
                                if (HttpContext.Current.User.IsInRole("User") && !HttpContext.Current.User.IsInRole("Premium"))
                                {
                                    o.Status = -2;
                                }

                                if (HttpContext.Current.User.IsInRole("Admin") || HttpContext.Current.User.IsInRole("Moderator"))
                                    o.NeedApproval = false;
                                else
                                    o.NeedApproval = true;
                                newthreadcontext.SaveChanges();
                                dbContextTransaction.Commit();

                                // Delete temp folder
                                PhotoFileManager.DeleteTempPhotos(o.Id, o.StringId);

                                CacheHelper.RemoveFromCache(string.Format("ld-{0}", o.Id));
                                // update lucene only if moderator
                                if (HttpContext.Current.User.IsInRole("Admin") || HttpContext.Current.User.IsInRole("Moderator"))
                                {
                                    LuceneSearch.ClearLuceneIndexRecord(o.Id, o.Photos);
                                    //Add to Lucene
                                    LuceneSearch.AddUpdateLuceneIndex(o);
                                }
                                dest.Id = o.Id;
                            }
                        }
                    });
                // Add Ad                
                // Proof of concept.. Ideally you would NOT want business logic inside an automap
                cfg.CreateMap<ClassifiedAdAdd, ClassifiedAdBase>()
                    .AfterMap((src, dest) =>
                    {
                        using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
                        {
                            using (var dbContextTransaction = newthreadcontext.Database.BeginTransaction())
                            {
                                var sc = newthreadcontext.SubCategoryDB.Include("Category").SingleOrDefault(i => i.Id == src.SubCatId);
                                var maxphotos = 0;
                                // Initalize photos
                                if (sc.Category.Name.Equals("Jobs"))
                                {
                                    maxphotos = 1;
                                }
                                else if (sc.Name.Contains("Apartments/Condos") || sc.Name.Contains("House") || sc.Name.Contains("Office") || sc.Name.Contains("Cars/Trucks"))
                                {
                                    maxphotos = 12;
                                }
                                else
                                {
                                    maxphotos = 6;
                                }
                                if (src.Photos != null)
                                {
                                    src.Photos = src.Photos.OrderBy(o => o.PhotoLayoutIndex).Take(maxphotos).ToArray();
                                }

                                var addedItem = newthreadcontext.ClassifiedDB.Add(Mapper.Map<ClassifiedAd>(src));
                                // add html free string
                                addedItem.HtmlFreeDescription = HtmlFilter.RemoveUnwantedTags(addedItem.Description);


                                addedItem.Title = addedItem.Title.Trim();
                                addedItem.Category = sc.Category;
                                addedItem.SubCategory = sc;

                                // 5
                                var region = newthreadcontext.RegionDB.Include("Country").SingleOrDefault(a => a.Id == src.RegionId);

                                addedItem.Country = region.Country;
                                addedItem.Region = region;

                                // Initilize the user associated with the Ad
                                var user = newthreadcontext.Users.Include("ClassifiedAds").SingleOrDefault(x => x.Id == CurrentUserId);
                                addedItem.Poster = user;
                                user.ClassifiedAds.Add(addedItem);
                                //format the phone number
                                addedItem.AdContactPhone = (addedItem.AdContactPhone.Contains("-") ? addedItem.AdContactPhone : addedItem.AdContactPhone.Insert(3, "-").ToString());
                                if (!string.IsNullOrEmpty(addedItem.AdContactPhone2))
                                    addedItem.AdContactPhone2 = (addedItem.AdContactPhone2.Contains("-") ? addedItem.AdContactPhone2 : addedItem.AdContactPhone2.Insert(3, "-").ToString());
                                if (!string.IsNullOrEmpty(addedItem.AdContactPhone3))
                                    addedItem.AdContactPhone3 = (addedItem.AdContactPhone3.Contains("-") ? addedItem.AdContactPhone3 : addedItem.AdContactPhone3.Insert(3, "-").ToString());

                                // increment count
                                addedItem.Category.TotalClassifiedAdsCount++;
                                addedItem.SubCategory.ClassifiedAdsCount++;

                                // Flag for screening
                                if (HttpContext.Current.User.IsInRole("User") && !HttpContext.Current.User.IsInRole("Premium"))
                                {
                                    addedItem.Status = -2;
                                }

                                // Expiry
                                addedItem.ExpiryTimeStamp = ExpiryDate.CalculateDate(addedItem.TimeStamp, addedItem.AdType, addedItem.Category.Name, addedItem.SubCategory.Name);

                                if (HttpContext.Current.User.IsInRole("Admin") || HttpContext.Current.User.IsInRole("Moderator"))
                                    addedItem.NeedApproval = false;

                                // Add adpromotion
                                newthreadcontext.SaveChanges();
                                var ap = new AdPromotion() { ClassifiedAd = addedItem };
                                newthreadcontext.AdPromotionDB.Add(ap);
                                newthreadcontext.SaveChanges();
                                ap.FeaturedAd = new FeaturedAd() { AdPromotion = ap };
                                ap.TopAd = new TopAd() { AdPromotion = ap };
                                ap.UrgentAd = new UrgentAd() { AdPromotion = ap };
                                ap.BumpAd = new BumpAd() { AdPromotion = ap };
                                newthreadcontext.PromotionsDB.AddRange(new List<Promotion>() { ap.FeaturedAd, ap.TopAd, ap.UrgentAd, ap.BumpAd });
                                newthreadcontext.SaveChanges();
                                var v = new AdViews() { ClassifiedAdId = addedItem.Id, UserId = new Guid(CurrentUserId) };
                                newthreadcontext.AdViewsDB.Add(v);
                                addedItem.AdViews = v;
                                newthreadcontext.SaveChanges();
                                dbContextTransaction.Commit();
                                dest.Id = addedItem.Id;
                                dest.StringId = addedItem.StringId;
                                dest.Status = addedItem.Status;

                                if (HttpContext.Current.User.IsInRole("Admin") || HttpContext.Current.User.IsInRole("Moderator") || HttpContext.Current.User.IsInRole("Premium"))
                                {
                                    // Add to Lucene
                                    LuceneSearch.AddUpdateLuceneIndex(addedItem);
                                    // hangfire delete
                                    // admins do not recieve renewal emails
                                    HangfireManager.ScheduleRemoval(addedItem.Id, addedItem.TimeStamp, addedItem.ExpiryTimeStamp.Value);
                                }
                                else
                                {
                                    // add queue
                                    HangfireManager.ScheduleDateCheck(addedItem.Id, addedItem.TimeStamp, addedItem.ExpiryTimeStamp.Value);
                                }
                            }
                        }
                    });
                cfg.CreateMap<ClassifiedAdWithDetail, ClassifiedAdEditForm>();
                // Ad Details
                cfg.CreateMap<ClassifiedAd, ClassifiedAdWithDetail>()
                    .ForMember(dest => dest.AdInfo, opt => opt.MapFrom(x => x.AdInfo.Where(a => a.Description != null)))
                    .ForMember(dest => dest.IsOwner, opt => opt.MapFrom(x => x.Poster.Id == CurrentUserId ? true : false))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(x => x.Price))
                    .ForMember(dest => dest.AdType, opt => opt.MapFrom(x => x.SubCategory.Name.Equals("Lost Pet") ? x.AdType.Equals("SELL") ? "Found" : "Missing" : x.AdType.Equals("SELL") ? "Offering" : x.AdType.Equals("TRADE") ? "Trading" : "Looking For"));
                cfg.CreateMap<ClassifiedAdList, ClassifiedAdList>()
                    .AfterMap((src, dest) => { dest.SeoTitle = new SeoManager().GetSeoTitle(src.Title); });
                cfg.CreateMap<ClassifiedAdAdd, ClassifiedAd>()
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(x => (x.Price == "Please Contact" ? 0 : int.Parse(x.Price.Replace(",", "")))))
                    .ForMember(dest => dest.AdContactName, opt => opt.MapFrom(x => x.AdContactName.Trim()))
                    .AfterMap((src, dest) =>
                    {
                        // src = newItem  dest = addedItem
                        dest.TimeStamp = DateTime.Now;
                        if (src.Price == "Please Contact" && src.PriceInfo != "Please Contact" && !src.Price.IsInt())
                        {
                            dest.PriceInfo = "Please Contact";
                        }

                        // format adinfo
                        var format_EngineSize = dest.AdInfo.SingleOrDefault(x => x.Name.Equals("Engine Size"));
                        if (format_EngineSize != null)
                            if (!string.IsNullOrEmpty(format_EngineSize.Description))
                                format_EngineSize.Description = format_EngineSize.Description.Replace(" ", "");

                        var format_Colour = dest.AdInfo.SingleOrDefault(x => x.Name.Equals("Colour"));
                        if (format_Colour != null)
                            if (!string.IsNullOrEmpty(format_Colour.Description))
                                format_Colour.Description = format_Colour.Description.Trim();
                    });
                cfg.CreateMap<ClassifiedAd, ClassifiedAdReportForm>();
                cfg.CreateMap<ClassifiedAdReportPost, ClassifiedAdReport>()
                    .AfterMap((src, dest) => dest.CreatedDate = DateTime.Now);
                cfg.CreateMap<ClassifiedAdReport, ClassifiedAdReportList>();
                cfg.CreateMap<ClassifiedAd, AdminAdListDetail>()
                    .ForMember(dest => dest.AdInfo, opt => opt.MapFrom(x => x.AdInfo.Where(a => a.Description != null)))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(x => x.Price))
                    .ForMember(dest => dest.AdPhotos, opt => opt.MapFrom(x => x.Photos))
                    .ForMember(dest => dest.AdType, opt => opt.MapFrom(x => x.SubCategory.Name.Equals("Lost Pet") ? x.AdType.Equals("SELL") ? "Found" : "Missing" : x.AdType.Equals("SELL") ? "Offering" : x.AdType.Equals("TRADE") ? "Trading" : "Looking For"));
                cfg.CreateMap<ClassifiedAd, AdminClassifiedAdEditForm>();
                // Ad List/Detail Image
                cfg.CreateMap<Document, ClassifiedAdListPhoto>()
                    .ForMember(dest => dest.FilePath, opt => opt.MapFrom(doc => doc.Get("FilePath")))
                    .ForMember(dest => dest.ContentType, opt => opt.MapFrom(doc => doc.Get("ContentType")));
                // Ad List
                cfg.CreateMap<Document, ClassifiedAdList>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(doc => doc.Get("Id")))
                    .ForMember(dest => dest.StringId, opt => opt.MapFrom(doc => doc.Get("StringId")))
                    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(doc => doc.Get("CategoryName")))
                    .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(doc => doc.Get("SubCategoryName")))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(doc => doc.Get("Price")))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(doc => doc.Get("Status")))
                    .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(doc => new DateTime(long.Parse(doc.Get("TimeStampTicks")))))
                    .ForMember(dest => dest.UrgentAdStatus, opt => opt.MapFrom(doc => doc.Get("UrgentAdStatus")))
                    .ForMember(dest => dest.TopAdStatus, opt => opt.MapFrom(doc => doc.Get("TopAdStatus")))
                    .ForMember(dest => dest.PriceInfo, opt => opt.MapFrom(doc => doc.Get("PriceInfo")))
                    .AfterMap((doc, dest) =>
                    {
                        dest.IsFavourited = CurrentUserFavouries.Any(a => a == dest.Id);
                    })
                    .AfterMap((doc, dest) =>
                    {
                        dest.Price = dest.Price.Equals("0") && dest.PriceInfo.Equals("Please Contact") ? "Please Contact" : string.Format("{0:C0}", dest.Price.AsFloat());
                    })
                    .AfterMap((doc, dest) =>
                    {
                        dest.SeoTitle = doc.Get("SeoTitle");
                        dest.SeoLocation = doc.Get("SeoLocation");
                        dest.SeoCategory = doc.Get("SeoCategory");
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var ap = doc.Get("AdPhotos");
                        if (ap != null && !ap.Equals("_NULL_"))
                        {
                            dest.AdPhoto = true;
                            var deserializer = new JavaScriptSerializer();
                            var photos = deserializer.Deserialize<IEnumerable<ClassifiedAdPhoto>>(ap);
                            if (photos != null && photos.Count() > 0)
                            {
                                dest.AdList_FileName = photos.First().AdList_FileName;
                            }
                        }
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var ais = doc.Get("AdInfo");
                        if (!string.IsNullOrEmpty(ais))
                        {
                            var deserializer = new JavaScriptSerializer();
                            var adinfos = deserializer.Deserialize<ICollection<InfoForm>>(ais);
                            if (adinfos.Any(x => x.Name.Equals("Make")) && adinfos.Any(x => x.Name.Equals("Model")) && !doc.Get("SubCategoryName").Contains("Parts"))
                            {
                                dest.ModelName = adinfos.FirstOrDefault(x => x.Name.Equals("Make"))?.Description;
                                dest.ModelName += " " + adinfos.FirstOrDefault(x => x.Name.Equals("Model"))?.Description;
                            }
                        }
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var hfd = doc.Get("HtmlFreeDescription");
                        hfd = ((hfd != null) ? hfd.Length > 100 ? hfd.Substring(0, 100) + "..." : hfd.TrimEnd(',', ' ') : hfd);
                        dest.HtmlFreeDescription = hfd;
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var adType = doc.Get("AdType");
                        adType = dest.SubCategoryName.Equals("Lost Pet") ? adType.Equals("SELL") ? "Found" : "Missing" : adType.Equals("SELL") ? "Offering" : adType.Equals("TRADE") ? "Trading" : "Looking For";
                        var adTitle = doc.Get("Title");
                        dest.Title = adType.Equals("Missing") ? "Missing: " + adTitle : adType.Equals("Found") ? "Found: " + adTitle : adType.Equals("Looking For") ? "Looking For: " + adTitle : adType.Equals("Trading") ? "Trading: " + adTitle : adTitle;
                    });
                cfg.CreateMap<Document, ClassifiedAdTitle>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(doc => doc.Get("Id")))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(doc => doc.Get("Price")))
                    .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(doc => doc.Get("SubCategoryName")))
                    .AfterMap((doc, dest) =>
                    {
                        dest.IsNew = DateTime.Now.Subtract(new DateTime(long.Parse(doc.Get("TimeStampTicks")))).Days < 2;
                    })
                    .AfterMap((doc, dest) =>
                    {
                        dest.Price = dest.Price.Equals("0") && doc.Get("PriceInfo").Equals("Please Contact") ? "Please Contact" : string.Format("{0:C0}", dest.Price.AsFloat());
                    })
                    .AfterMap((doc, dest) =>
                    {
                        dest.SeoTitle = doc.Get("SeoTitle");
                        dest.SeoLocation = doc.Get("SeoLocation");
                        dest.SeoCategory = doc.Get("SeoCategory");
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var adType = doc.Get("AdType");
                        adType = doc.Get("SubCategoryName").Equals("Lost Pet") ? adType.Equals("SELL") ? "Found" : "Missing" : adType.Equals("SELL") ? "Offering" : adType.Equals("TRADE") ? "Trading" : "Looking For";
                        var adTitle = doc.Get("Title");
                        dest.Title = adType.Equals("Missing") ? "Missing: " + adTitle : adType.Equals("Found") ? "Found: " + adTitle : adType.Equals("Looking For") ? "Looking For: " + adTitle : adType.Equals("Trading") ? "Trading: " + adTitle : adTitle;
                    });
                // Related Ad
                cfg.CreateMap<Document, ClassifiedAdMinimal>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(doc => doc.Get("Id")))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(doc => doc.Get("Price")))
                    .ForMember(dest => dest.FeaturedAdStatus, opt => opt.MapFrom(doc => doc.Get("FeaturedAdStatus")))
                    .ForMember(dest => dest.UrgentAdStatus, opt => opt.MapFrom(doc => doc.Get("UrgentAdStatus")))
                    .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(doc => doc.Get("SubCategoryName")))
                    .AfterMap((doc, dest) =>
                    {
                        dest.Price = dest.Price.Equals("0") && doc.Get("PriceInfo").Equals("Please Contact") ? "Please Contact" : string.Format("{0:C0}", dest.Price.AsFloat());
                    })
                    .AfterMap((doc, dest) =>
                    {
                        dest.SeoTitle = doc.Get("SeoTitle");
                        dest.SeoLocation = doc.Get("SeoLocation");
                        dest.SeoCategory = doc.Get("SeoCategory");
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var ap = doc.Get("AdPhotos");
                        if (ap != null && !ap.Equals("_NULL_"))
                        {
                            dest.AdPhoto = true;
                            var deserializer = new JavaScriptSerializer();
                            var photos = deserializer.Deserialize<IEnumerable<ClassifiedAdPhoto>>(ap);
                            if (photos != null && photos.Count() > 0)
                            {
                                dest.AdList_FileName = photos.First().AdList_FileName;
                            }
                        }
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var ai = doc.Get("AdInfo");
                        if (!string.IsNullOrEmpty(ai))
                        {
                            var deserializer = new JavaScriptSerializer();
                            var adinfos = deserializer.Deserialize<ICollection<InfoForm>>(ai);
                            if (adinfos.Any(x => x.Name.Equals("Make")) && adinfos.Any(x => x.Name.Equals("Model")) && !doc.Get("SubCategoryName").Contains("Parts"))
                            {
                                dest.ModelName = adinfos.FirstOrDefault(x => x.Name.Equals("Make"))?.Description;
                                dest.ModelName += " " + adinfos.FirstOrDefault(x => x.Name.Equals("Model"))?.Description;
                            }
                        }
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var adType = doc.Get("AdType");
                        adType = doc.Get("SubCategoryName").Equals("Lost Pet") ? adType.Equals("SELL") ? "Found" : "Missing" : adType.Equals("SELL") ? "Offering" : adType.Equals("TRADE") ? "Trading" : "Looking For";
                        var adTitle = doc.Get("Title");
                        dest.Title = adType.Equals("Missing") ? "Missing: " + adTitle : adType.Equals("Found") ? "Found: " + adTitle : adType.Equals("Looking For") ? "Looking For: " + adTitle : adType.Equals("Trading") ? "Trading: " + adTitle : adTitle;
                    });
                // List Detail
                cfg.CreateMap<Document, ClassifiedAdWithDetail>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(doc => doc.Get("Id")))
                    .ForMember(dest => dest.StringId, opt => opt.MapFrom(doc => doc.Get("StringId")))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(doc => doc.Get("Price")))
                    .ForMember(dest => dest.PriceInfo, opt => opt.MapFrom(doc => doc.Get("PriceInfo")))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(doc => doc.Get("Status")))
                    .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(doc => new DateTime(long.Parse(doc.Get("TimeStampTicks")))))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(doc => doc.Get("Description")))
                    .ForMember(dest => dest.HtmlFreeDescription, opt => opt.MapFrom(doc => doc.Get("HtmlFreeDescription")))
                    .ForMember(dest => dest.IsOwner, opt => opt.MapFrom(doc => doc.Get("PosterId") == CurrentUserId ? true : false))
                    .ForMember(dest => dest.ContactPrivacy, opt => opt.MapFrom(doc => doc.Get("ContactPrivacy")))
                    .ForMember(dest => dest.AdContactName, opt => opt.MapFrom(doc => doc.Get("AdContactName")))
                    .ForMember(dest => dest.AdContactPhone, opt => opt.MapFrom(doc => doc.Get("AdContactPhone")))
                    .ForMember(dest => dest.AdContactPhone2, opt => opt.MapFrom(doc => doc.Get("AdContactPhone2")))
                    .ForMember(dest => dest.AdContactPhone3, opt => opt.MapFrom(doc => doc.Get("AdContactPhone3")))
                    .ForMember(dest => dest.AdContactEmail, opt => opt.MapFrom(doc => doc.Get("AdContactEmail")))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(doc => doc.Get("Title")))
                    .AfterMap((doc, dest) =>
                    {
                        dest.IsFavourited = CurrentUserFavouries.Any(a => a == dest.Id);
                    })
                    .AfterMap((doc, dest) =>
                    {
                        dest.Price = dest.Price.Equals("0") && dest.PriceInfo.Equals("Please Contact") ? "Please Contact" : string.Format("{0:C0}", dest.Price.AsFloat());
                    })
                    .AfterMap((doc, dest) =>
                    {
                        dest.Category = new CategoryList()
                        {
                            Id = doc.Get("CategoryId").AsInt(),
                            Name = doc.Get("CategoryName"),
                            SeoName = doc.Get("CategorySeoName")
                        };
                        dest.SubCategory = new SubCategoryList()
                        {
                            Id = doc.Get("SubCategoryId").AsInt(),
                            Name = doc.Get("SubCategoryName"),
                            SeoName = doc.Get("SubCategorySeoName")
                        };
                    })
                    .AfterMap((doc, dest) =>
                    {
                        dest.Country = new CountryBase()
                        {
                            Id = doc.Get("CountryId").AsInt(),
                            Name = doc.Get("CountryName")
                        };
                        dest.Region = new RegionBase()
                        {
                            Id = doc.Get("RegionId").AsInt(),
                            Name = doc.Get("RegionName")
                        };

                    })
                    .AfterMap((doc, dest) =>
                    {
                        dest.SeoLocation = doc.Get("SeoLocation");
                        dest.SeoCategory = doc.Get("SeoCategory");
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var adType = doc.Get("AdType");
                        dest.AdType = dest.SubCategory.Name.Equals("Lost Pet") ? adType.Equals("SELL") ? "Found" : "Missing" : adType.Equals("SELL") ? "Offering" : adType.Equals("TRADE") ? "Trading" : "Looking For";
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var ap = doc.Get("AdPhotos");
                        if (ap != null && !ap.Equals("_NULL_"))
                        {
                            var deserializer = new JavaScriptSerializer();
                            var photos = deserializer.Deserialize<IEnumerable<ClassifiedAdPhoto>>(ap);
                            if (photos != null && photos.Count() > 0)
                            {
                                dest.Photos = photos;
                            }
                        }
                    })
                    .AfterMap((doc, dest) =>
                    {
                        var ai = doc.Get("AdInfo");
                        if (!string.IsNullOrEmpty(ai))
                        {
                            var deserializer = new JavaScriptSerializer();
                            var adinfos = deserializer.Deserialize<ICollection<InfoForm>>(ai);
                            dest.AdInfo = adinfos;
                            if (adinfos.Any(x => x.Name.Equals("Make")) && adinfos.Any(x => x.Name.Equals("Model")) && !doc.Get("SubCategoryName").Contains("Parts"))
                            {
                                dest.ModelName = adinfos.FirstOrDefault(x => x.Name.Equals("Make"))?.Description;
                                dest.ModelName += " " + adinfos.FirstOrDefault(x => x.Name.Equals("Model"))?.Description;
                            }
                        }
                    });
                // Favourite List
                cfg.CreateMap<Document, ClassifiedAdFavouriteList>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(doc => doc.Get("Id")))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(doc => doc.Get("Price")))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(doc => doc.Get("Status")))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(doc => doc.Get("Title")))
                    .AfterMap((doc, dest) =>
                    {
                        var created = new DateTime(long.Parse(doc.Get("TimeStampTicks")));
                        var edited = new DateTime(long.Parse(doc.Get("EditStampTicks")));
                        dest.EditTimeStamp = created == edited ? new DateTime() : edited;
                    })
                    .ForMember(dest => dest.EditTimeStamp, opt => opt.MapFrom(doc => new DateTime(long.Parse(doc.Get("EditStampTicks")))))
                    .AfterMap((doc, dest) =>
                    {
                        var ap = doc.Get("AdPhotos");
                        if (ap != null && !ap.Equals("_NULL_"))
                        {
                            var deserializer = new JavaScriptSerializer();
                            var photos = deserializer.Deserialize<IEnumerable<ClassifiedAdPhoto>>(ap);
                            if (photos != null && photos.Count() > 0)
                            {
                                dest.AdPhoto = photos.First().AdList_FileName;
                            }
                        }
                    });
                cfg.CreateMap<ClassifiedAd, ClassifiedAdPromoted>()
                    .ForMember(dest => dest.BumpAdPIInterval, opt => opt.MapFrom(x => x.AdPromotion.BumpAd.Duration))
                    .ForMember(dest => dest.TopAdPIDays, opt => opt.MapFrom(x => x.AdPromotion.TopAd.Duration))
                    .ForMember(dest => dest.UrgentAdPIDays, opt => opt.MapFrom(x => x.AdPromotion.UrgentAd.Duration))
                    .ForMember(dest => dest.FeaturedAdPIDays, opt => opt.MapFrom(x => x.AdPromotion.FeaturedAd.Duration)); ;
                cfg.CreateMap<ClassifiedAd, ClassifiedAdPromotionList>();
                // Country
                // Summary:
                //      Contains country mappings
                cfg.CreateMap<CountryAdd, Country>();
                cfg.CreateMap<Country, CountryBase>();
                cfg.CreateMap<Country, CountryList>();
                cfg.CreateMap<Country, RegionBase>();
                cfg.CreateMap<Country, CountryWithDetail>()
                    .ForMember(dest => dest.Regions, opt => opt.MapFrom(x => x.Regions.OrderBy(r => r.Name)));
                cfg.CreateMap<Country, CountryLucene>();
                cfg.CreateMap<Country, CountryAdList>()
                    .ForMember(dest => dest.Regions, opt => opt.MapFrom(x => x.Regions.OrderBy(r => r.Name)));
                // Region
                // Summary:
                //      Contains region mappings
                cfg.CreateMap<Region, RegionBase>();
                cfg.CreateMap<Region, RegionEditForm>();
                cfg.CreateMap<Region, RegionCoordinate>();
                cfg.CreateMap<RegionAdd, RegionBase>();
                cfg.CreateMap<RegionAdd, Region>();
                cfg.CreateMap<RegionAddForm, RegionAdd>();
                cfg.CreateMap<RegionEdit, RegionBase>();
                cfg.CreateMap<Region, RegionLucene>();
                // Info
                // Summary:
                //      Contains info mappings
                cfg.CreateMap<Info, InfoForm>();
                cfg.CreateMap<InfoForm, Info>();
                cfg.CreateMap<MiscInfo, MiscInfoNoId>();
                cfg.CreateMap<AdInfoString, InfoForm>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(x => x.Name));
                // User
                // Summary:
                //      Contains user mappings
                cfg.CreateMap<PremiumUserPhoto, PremiumUserPhotoBase>();
                cfg.CreateMap<PremiumUserInfo, PremiumUserInfoBase>();
                cfg.CreateMap<ApplicationUser, UserProfileContact>();
                cfg.CreateMap<ApplicationUser, MyProfile>()
                    .AfterMap((src, dest) => dest.ContactNumber = src.PhoneNumber)
                    .AfterMap((src, dest) => dest.ContactNumber2 = src.PhoneNumber2)
                    .AfterMap((src, dest) => dest.ContactNumber3 = src.PhoneNumber3);
                cfg.CreateMap<ApplicationUser, AdminMemberDetails>()
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(x => x.Id));
                // Category
                // Summary:
                //      Contains category mappings                
                cfg.CreateMap<Category, DropDownCategory>();
                cfg.CreateMap<Category, CategoryList>()
                    .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(x => x.SubCategories.OrderBy(o => o.Name == "Other").ThenBy(t => t.Name)));
                cfg.CreateMap<Category, CategoryBase>();
                cfg.CreateMap<CategoryList, CategoryList>();
                cfg.CreateMap<Category, CategoryLucene>();
                cfg.CreateMap<Category, CategorySeoEditForm>();
                cfg.CreateMap<Category, CategoryTile>()
                    .ForMember(dest => dest.PopularSubcats, opt => opt.MapFrom(x => x.SubCategories.OrderByDescending(o => o.ClassifiedAdsCount).Take(4)));
                // SubCategory
                // Summary:
                //      Contains subcategory mappings
                cfg.CreateMap<SubCategory, DropDownCategory>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.StringId));
                cfg.CreateMap<SubCategory, SubCategoryList>()
                    .AfterMap((src, dest) =>
                    {
                        dest.SeoName = new SeoManager().GetSeoCategory(src.Category.Id, src.Id);
                    });
                cfg.CreateMap<SubCategory, SubCategoryEditForm>();
                cfg.CreateMap<SubCategory, AdInfoTemplateSlim>()
                    .ForMember(dest => dest.RecommendedInfo, opt => opt.MapFrom(x => x.AdInfoTemplate.RecommendedInfo));
                cfg.CreateMap<SubCategoryAdd, SubCategory>();
                cfg.CreateMap<SubCategory, SearchBarCategory>()
                    .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(x => x.Category.Id));
                cfg.CreateMap<SubCategory, SubCategoryLucene>();
                cfg.CreateMap<SubCategory, SubCategoryCreateAd>();
                // Other
                // Summary:
                //      Contains miscellaneous mappings
                cfg.CreateMap<ClassifiedAdPhoto, UploadPhoto>();
                cfg.CreateMap<UploadPhoto, ClassifiedAdPhoto>();
                cfg.CreateMap<ContactUs, ContactUsForm>();
                cfg.CreateMap<ContactUs, GenericMessage>();
                cfg.CreateMap<GenericMessage, GenericMessageQueue>();
                cfg.CreateMap<HttpPostedFileBase, string>();
                cfg.CreateMap<ClassifiedAdPhoto, UploadPhoto>();
                //
                cfg.CreateMap<CartItem, CartItemList>()
                    .ForMember(dest => dest.BumpAdPIInterval, opt => opt.MapFrom(x => x.BumpAdPI.Interval))
                    .ForMember(dest => dest.BumpAdPIDays, opt => opt.MapFrom(x => x.BumpAdPI.Days))
                    .ForMember(dest => dest.BumpAdPIPrice, opt => opt.MapFrom(x => x.BumpAdPI.Price))
                    .ForMember(dest => dest.TopAdPIPrice, opt => opt.MapFrom(x => x.TopAdPI.Price))
                    .ForMember(dest => dest.TopAdPIDays, opt => opt.MapFrom(x => x.TopAdPI.Days))
                    .ForMember(dest => dest.UrgentAdPIPrice, opt => opt.MapFrom(x => x.UrgentAdPI.Price))
                    .ForMember(dest => dest.UrgentAdPIDays, opt => opt.MapFrom(x => x.UrgentAdPI.Days))
                    .ForMember(dest => dest.FeaturedAdPIPrice, opt => opt.MapFrom(x => x.FeaturedAdPI.Price))
                    .ForMember(dest => dest.FeaturedAdPIDays, opt => opt.MapFrom(x => x.FeaturedAdPI.Days));
                // ApplicationUser
                cfg.CreateMap<ApplicationUser, ContactInfo>()
                    .ForMember(dest => dest.ContactNumber, opt => opt.MapFrom(x => x.PhoneNumber))
                    .ForMember(dest => dest.ContactNumber2, opt => opt.MapFrom(x => x.PhoneNumber2))
                    .ForMember(dest => dest.ContactNumber3, opt => opt.MapFrom(x => x.PhoneNumber3));
                cfg.CreateMap<ApplicationUser, UserAdList>()
                    .ForMember(dest => dest.ClassifiedAds, opt => opt.MapFrom(x => x.ClassifiedAds.OrderByDescending(o => o.TimeStamp)));
                cfg.CreateMap<ApplicationUser, UserAdListPending>()
                    .ForMember(dest => dest.ClassifiedAds, opt => opt.MapFrom(x => x.ClassifiedAds.Where(c => c.Status == -2).OrderByDescending(o => o.TimeStamp)));
                cfg.CreateMap<ApplicationUser, UserAdListSuspended>()
                    .ForMember(dest => dest.ClassifiedAds, opt => opt.MapFrom(x => x.ClassifiedAds.Where(c => c.Status == -1).OrderByDescending(o => o.TimeStamp)));
                cfg.CreateMap<ApplicationUser, UserAdListOpen>()
                    .ForMember(dest => dest.ClassifiedAds, opt => opt.MapFrom(x => x.ClassifiedAds.Where(c => c.Status == 0).OrderByDescending(o => o.TimeStamp)));
                cfg.CreateMap<ApplicationUser, UserAdListSold>()
                    .ForMember(dest => dest.ClassifiedAds, opt => opt.MapFrom(x => x.ClassifiedAds.Where(c => c.Status == 1).OrderByDescending(o => o.TimeStamp)));
                cfg.CreateMap<ApplicationUser, UserAdListRented>()
                    .ForMember(dest => dest.ClassifiedAds, opt => opt.MapFrom(x => x.ClassifiedAds.Where(c => c.Status == 2).OrderByDescending(o => o.TimeStamp)));
                // 
                cfg.CreateMap<ApplicationUser, UserCartItemSlim>();
                cfg.CreateMap<ApplicationUser, UserPromotedList>()
                    .ForMember(dest => dest.PromotedList, opt => opt.MapFrom(x => x.ClassifiedAds.Where(c => c.AdPromotion.FeaturedAd.Status || c.AdPromotion.BumpAd.Status || c.AdPromotion.TopAd.Status || c.AdPromotion.UrgentAd.Status)));
                cfg.CreateMap<ApplicationUser, UserAdListSlim>()
                    .ForMember(dest => dest.MyAdList, opt =>
                        opt.MapFrom(x => from left in x.ClassifiedAds.Where(c => c.Status == 0 && !c.AdPromotion.FeaturedAd.Status && !c.AdPromotion.BumpAd.Status && !c.AdPromotion.TopAd.Status && !c.AdPromotion.UrgentAd.Status).OrderByDescending(o => o.TimeStamp)
                                         where !x.PromotionCart.Select(pc => pc.ClassifiedAd).Any(a => a.Id == left.Id)
                                         select new ClassifiedAdPromotionList() { Title = left.Title, StringId = left.StringId, AdPhoto = left.Photos.FirstOrDefault(y => y.PhotoLayoutIndex == 0).AdList_FileName }));
                //
                cfg.CreateMap<ClassifiedAdEmailUserPost, ClassifiedAdEmailUserForm>();
                cfg.CreateMap<ClassifiedAdReportPost, ClassifiedAdReportForm>();
                cfg.CreateMap<ClassifiedAdApplyTo, ClassifiedAdApplyToForm>();
            });
        }
    }
}