using AutoMapper;
using Ganss.XSS;
using Microsoft.AspNet.Identity;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Trinbago_MVC5.Areas.ClassifiedAd.Managers;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Extensions.AttributeClasses;
using Trinbago_MVC5.Controllers;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Controllers
{
    [RouteArea("ClassifiedAd")]
    [Route("{action}")]
    [AuthorizeEmailConfirm(Roles = "Admin, Moderator, User")]
    public class ClassifiedAdController : ClassifiedAdBaseController
    {
        
        [AllowAnonymous]
        [Route("AdList/{catId:int}")]
        public ActionResult AdListCategoryOldRoute(int catId)
        {
            var url = SeoManager.GenerateAdListUrl(catId, 0);
            if (url == null) return new HttpStatusCodeResult(404);
            // Skip UrlCheck
            TempData["wasRedirected"] = true;
            return RedirectPermanent(url);
        }

        [AllowAnonymous]
        [Route("AdList/{catId:int}/{subCatId}")]
        public ActionResult AdListSubCatOldRoute(int catId, string subCatId)
        {
            var url = SeoManager.GenerateAdListOldUrl(catId, subCatId);
            if (url == null) return new HttpStatusCodeResult(404);
            // Skip UrlCheck
            TempData["wasRedirected"] = true;
            return RedirectPermanent(url);
        }

        /// <summary>
        /// Handles old url ListDetails calls
        /// </summary>
        /// <param name="stringId"></param>
        /// <returns></returns>
        [AllowAnonymous, OldListDetailRoute]
        [Route("ListDetails/{stringId}")]
        public ActionResult ListDetails(string stringId)
        {
            var url = SeoManager.GenerateListDetailsOldUrl(stringId);
            if (url == null) return new HttpStatusCodeResult(410);
            // Skip UrlCheck
            TempData["wasRedirected"] = true;
            return RedirectPermanent(url);
        }

        [AllowAnonymous]
        [Route("ShortAdDetails/{adId:int}")]
        public ActionResult ShortAdDetails (int adId)
        {
            var url = SeoManager.GenerateAdDetailUrl(adId);
            if (url == null)
                return new HttpStatusCodeResult(410);
            return RedirectPermanent(url);            
        }

        //
        // GET: /ClassifiedAd/ListDetails/stringId
        [AllowAnonymous, UrlCorrect]
        [Route("~/cld-{category}/{location}/{Id:int}/{title}")]
        public ActionResult AdDetails(string category, string location, int? Id, string title)
        {
            // error if no string
            if (!Id.HasValue)
                return new HttpStatusCodeResult(404);

            // Reset from AdminEditUserAd
            TempData["editUserAdPosted"] = null;
            // Reset from CreateAd
            TempData["hasBeenPosted"] = null;
            // Reset from MyAdEdit
            TempData["editPosted"] = null;

            var UrlId = Id;
            //check if the user opening the site for the first time 

            if (Url.RequestContext.HttpContext.Session["URLHistory"] != null)
            {
                //The session variable exists. So the user has already visited this site and sessions is still alive. Check if this page is already visited by the user
                List<string> HistoryURLs = (List<string>)Url.RequestContext.HttpContext.Session["URLHistory"];
                if (HistoryURLs.Exists((element => element.ToString().Equals(UrlId.ToString()))))
                {
                    //If the user has already visited this page in this session, then we can ignore this visit. No need to update the counter.
                    Url.RequestContext.HttpContext.Session["VisitedURL"] = 0;
                }
                else
                {
                    //if the user is visting this page for the first time in this session, then count this visit and also add this page to the list of visited pages(URLHistory variable)
                    HistoryURLs.Add(UrlId.ToString());
                    Url.RequestContext.HttpContext.Session["URLHistory"] = HistoryURLs;

                    //Make a note of the page Id to update the database later 
                    Url.RequestContext.HttpContext.Session["VisitedURL"] = UrlId;
                }
            }
            else
            {
                //if there is no session variable already created, then the user is visiting this page for the first time in this session. Then create a session variable and take the count of the page Id
                List<string> HistoryURLs = new List<string>();
                HistoryURLs.Add(UrlId.ToString());
                Url.RequestContext.HttpContext.Session["URLHistory"] = HistoryURLs;
                Url.RequestContext.HttpContext.Session["VisitedURL"] = UrlId;
            }
            var mod1 = SearchEngineManager.GetClassifiedAdWithDetails(Id.Value);
            if (mod1 == null)
            {
                if (SearchEngineManager.IsAdSuspended(Id.Value))
                    return RedirectToAction("AdSuspended", "Errors", new { Area = "" });
                return new HttpStatusCodeResult(410);
            }
            string pageid = Url.RequestContext.HttpContext.Session["VisitedUrl"].ToString();

            mod1.AdViewsCount = ClassifiedAdManager.ClassifiedAdViewIncrement(Id.Value, User.Identity.GetUserId(), (!pageid.Equals("0") && mod1.Status == 0));


            // Get related

            if (mod1.Photos != null)
            {
                if (mod1.Photos.Count() > 0)
                {
                    var raw = mod1.Photos.FirstOrDefault().Raw_FileName;
                    ViewBag.ImgLinkOverride = new HtmlString("<link rel='img_src' href='" + Url.Action("LoadLucenePhoto", "Photo", new { Area = "", adId = mod1.Id, FileName = raw}) + "' />");
                    ViewBag.ImgOpenGraphMeta = new HtmlString("<meta property='og:image' content='" + Url.Action("LoadLucenePhoto", "Photo", new { Area = "", adId = mod1.Id, FileName = raw }) + "' />");
                }
            }

            var seotitleformat = string.Format("{0} | {1} | {2} | {3} Region, {4}", mod1.Title, mod1.Category.Name, mod1.SubCategory.Name, mod1.Region.Name, mod1.Country.Name);
            ViewBag.TitleOpenGraphMeta = new HtmlString(string.Format("<meta property='og:title' content='{0} - {1}'/>", HttpUtility.HtmlEncode(mod1.Title), mod1.Price));
            var tempd = mod1.HtmlFreeDescription.Length > 100 ? mod1.HtmlFreeDescription.Substring(0, 100) + "..." : "View ad for details!";
            ViewBag.DesOpenGraphMeta = new HtmlString(string.Format("<meta property='og:description' content='{0}'/>", HttpUtility.HtmlEncode(tempd)));
            ViewBag.MetaDesc = new HtmlString("<meta name='description' content='" + HttpUtility.HtmlEncode(mod1.Title) + " in " + mod1.SubCategory.Name + "|" + mod1.Category.Name + " Classifieds. Located in " + mod1.Country.Name + ", " + mod1.Region.Name + "'/>");
            ViewBag.PageTitle = seotitleformat;

            ViewBag.PrevPage = (TempData["PrevPage"] == null) ? ((HttpContext.Request.UrlReferrer != null) ? HttpContext.Request.UrlReferrer.ToString() : Request.ApplicationPath) : TempData["PrevPage"].ToString();
            // Get # search id if any
            ViewBag.SearchItem = TempData["SearchItem"];
            //Set Temp Data
            int cid = 0;
            int rid = 0;
            int catid = 0;
            int scid = 0;
            if (TempData["CountryId"] != null)
            {
                int.TryParse(TempData["CountryId"].ToString(), out cid);
                ViewBag.CountryId = cid;
                TempData.Keep("CountryId");
            }
            if (TempData["RegionId"] != null)
            {
                int.TryParse(TempData["RegionId"].ToString(), out rid);
                ViewBag.RegionId = rid;
                TempData.Keep("RegionId");
            }
            if (TempData["CategoryId"] != null)
            {
                int.TryParse(TempData["CategoryId"].ToString(), out catid);
                ViewBag.CategoryId = catid;
                TempData.Keep("CategoryId");
            }
            if (TempData["SubCategoryId"] != null)
            {
                ViewBag.SubCategoryId = scid = (int)TempData["SubCategoryId"];
                TempData.Keep("SubCategoryId");
            }
            else
            {
                ViewBag.SubCateogryId = "sc";
            }
            if (TempData["SeoLocation"] != null)
            {
                ViewBag.SeoLocation = TempData["SeoLocation"];
                TempData.Keep("SeoLocation");
            }
            if (TempData["SeoCurrentCategory"] != null)
            {
                ViewBag.SeoCurrentCategory = TempData["SeoCurrentCategory"];
                TempData.Keep("SeoCurrentCategory");
            }
            if (TempData["searchString"] != null)
            {
                ViewBag.SearchItem = TempData["searchString"].ToString();
                TempData.Keep("searchString");
            }


            if (!mod1.Category.Name.Equals("Jobs"))
            {
                if (mod1.SubCategory.Name.Equals("Automotive Parts"))
                {
                    var years = mod1.AdInfo.Where(x => x.Name.Equals("Year")).ToList();
                    if (years != null && years.Count > 1)
                    {
                        mod1.AdInfo.FirstOrDefault(x => x.Name.Equals("Year")).Description = years.FirstOrDefault().Description + "-" + years.LastOrDefault().Description;
                        foreach (var y in years)
                        {
                            if (y != years.FirstOrDefault())
                                mod1.AdInfo.Remove(y);
                        }
                    }
                }

                if (mod1.AdInfo.FirstOrDefault(x => x.Name.Equals("Age")) != null)
                {
                    var firstage = new InfoForm() { Name = "Age", Description = mod1.AdInfo.FirstOrDefault(x => x.Name.Equals("Age")).Description };
                    if (firstage.Description != null)
                    {
                        var allage = mod1.AdInfo.Where(x => x.Name.Equals("Age")).ToList();
                        foreach (var toremove in allage)
                        {
                            mod1.AdInfo.Remove(toremove);
                        }
                        mod1.AdInfo.Add(firstage);
                    }
                }
                return View(new AdListDetailParent(catid, scid, cid, rid)
                {
                    Model1 = mod1,
                    Model2 = new ClassifiedAdEmailUserForm() { StringId = mod1.StringId, ItemUrl = Request.Url.AbsoluteUri, AdContactName = mod1.AdContactName, Title = mod1.Title },
                    Model3 = new ClassifiedAdReportForm() { Id = mod1.Id, Title = mod1.Title },
                    RelatedAds = SearchEngineManager.GetRelatedClassifiedAd(mod1.StringId, mod1.Price.ToString(), mod1.SubCategory.Id)
                });
            }
            else
            {
                try { ViewBag.Message = TempData["Message"].ToString(); }
                catch (Exception) { ViewBag.Message = null; }
                return View(new AdListDetailParent(catid, scid, cid, rid)
                {
                    Model1 = mod1,
                    Model3 = new ClassifiedAdReportForm() { Id = mod1.Id, Title = mod1.Title },
                    Model4 = new ClassifiedAdApplyToForm() { stringId = mod1.StringId, ItemUrl = Request.Url.AbsoluteUri },
                    RelatedAds = SearchEngineManager.GetRelatedClassifiedAd(mod1.StringId, mod1.Price.ToString(), mod1.SubCategory.Id)
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SearchBar(int CountryId = 0, int RegionId = 0, string searchBarId = null, string searchBoxString = null, string modelBodyType = null)
        {
            // Check SearchBar Id
            int catId = 0;
            int subCatId = 0;
            if (!string.IsNullOrEmpty(searchBarId))
            {
                searchBarId = new HtmlSanitizer().Sanitize(searchBarId);
                if (!int.TryParse(searchBarId, out catId))
                {
                    var scid = ClassifiedAdManager.GetSubCatId(searchBarId);
                    if (scid != null)
                    {
                        subCatId = scid.Id;
                        catId = scid.CategoryId;
                    }
                }
                else
                {
                    subCatId = 0; ;
                }
            }
            // check for #ID
            if (!string.IsNullOrEmpty(searchBoxString) && searchBoxString.StartsWith("#"))
            {
                // Sanitize
                searchBoxString = new HtmlSanitizer().Sanitize(searchBoxString).Replace("#","");
                if (Int32.TryParse(searchBoxString, out int adId))
                {
                    // Find relevand ad
                    var adurl = SeoManager.GenerateAdDetailUrl(adId);
                    TempData["SearchItem"] = "#" + adId;
                    if (adurl != null)
                    {                        
                        return Redirect(adurl);
                    }
                    TempData["AdSearchIdNotFound"] = string.Format("The ad with Id #{0} was not found in our database!", adId);
                }
            }
            else
            {
                // Sanitize
                searchBoxString = new HtmlSanitizer().Sanitize(searchBoxString);
            }

            var url = SeoManager.GenerateAdListUrl(catId, subCatId, CountryId, RegionId);
            if (url == null) return new HttpStatusCodeResult(404);
            // Skip UrlCheck
            TempData["wasRedirected"] = true;

            // insert modelbodytype into url
            if (modelBodyType != null && url.Contains("vehicles-cars-trucks"))
                url = url.Replace("vehicles-cars-trucks/", "vehicles-cars-trucks/" + modelBodyType + "/");
            // Append searchString to url
            url = string.Format("{0}{1}", url, (TempData["SearchItem"] == null ? (searchBoxString != "" && searchBoxString != " " ? "/" + SeoManager.GetSeoTitle(searchBoxString) : null) : null));
            return RedirectPermanent(url);
        }

        /// <summary>
        /// Gets ad list from lucene data store
        /// </summary>
        [AllowAnonymous, UrlCorrect, ValidateFilter]
        [Route(@"~/{category:regex(vehicles-cars-trucks)}/{modelBodyType}/{location=trinidad-tobago}/lc{CountryId=0}lr{RegionId=0}-c{catId=0}sc{subCatId=0}/{searchString?}", Name = "CarSeoAdList")]
        [Route(@"~/{category:regex(real-estate-apartments-condos-rental)}/{modelRentalType}/{location=trinidad-tobago}/lc{CountryId=0}lr{RegionId=0}-c{catId=0}sc{subCatId=0}/{searchString?}", Name = "ARentSeoAdList")]
        [Route(@"~/{category:regex(real-estate-land-rental-leasing)}/{modelRentalType}/{location=trinidad-tobago}/lc{CountryId=0}lr{RegionId=0}-c{catId=0}sc{subCatId=0}/{searchString?}", Name = "LRentSeoAdList")]
        [Route(@"~/{category:regex(real-estate-house-rental)}/{modelRentalType}/{location=trinidad-tobago}/lc{CountryId=0}lr{RegionId=0}-c{catId=0}sc{subCatId=0}/{searchString?}", Name = "HRentSeoAdList")]
        [Route("~/{category=classified-ads}/{location=trinidad-tobago}/lc{CountryId=0}lr{RegionId=0}-c{catId=0}sc{subCatId=0}/{searchString?}", Name = "DefaultAdList")]
        public ActionResult AdList(string category, string location, int catId, int subCatId, int CountryId = 0, int RegionId = 0, string searchBarId = null, string searchString = null, int? pageNumber = 1, string adtypeString = "ALL",
            string searchonlyOption = "All Ads", string minPrice = "", string maxPrice = "", string minMile = "", string maxMile = "", string modelName = null,
            string minYear = "", string maxYear = "", string modEngineSize = null, string modelMake = null, string modelBodyType = null, string modelDrivetrain = null,
            string modelTransmission = null, string modelCondition = null, string modelColour = null, string modelJobType = null, string modelSalaryInfo = null, string modelRentalType = null,
            string modelBedrooms = null, string modelBathrooms = null, string modelFurnished = null, string minSize = "", string maxSize = "",
            string modelSpecies = null, string minAge = "", string maxAge = "", string ageType = null)
        {
            AdlistPage alp;
#if DEBUG
            var profiler = MiniProfiler.Current;
            using (profiler.Step("init alp"))
            {
#endif
                alp = new AdlistPage(CountryId, RegionId, catId, subCatId);
#if DEBUG
            }
#endif
            string metaDesc = null;
            string metaTitle = null;
            string wwdDesc = null;
            string descriptionLocaton = null;
#if DEBUG
            using (profiler.Step("seo setup"))
            {
#endif
                /*************************************************************/
                /**************************SEO********************************/
                /*************************************************************/
                // Get location
                if (RegionId > 0)
                {
                    descriptionLocaton = ClassifiedAdManager.GetCountryRegionNameById(RegionId);
                }
                else if (CountryId > 0)
                {
                    descriptionLocaton = ClassifiedAdManager.GetCountryNameById(CountryId);
                }
                else
                {
                    descriptionLocaton = "Trinidad and Tobago";
                }
                // When the adlist is set to a subcategory
                if (alp.Category != null)
                {
                    // Vehicles
                    if (alp.Category.Name.Equals("Vehicles"))
                    {
                        if (alp.SubCategory != null)
                        {
                            if (alp.SubCategory.Name.Equals("Automotive Accessories"))
                            {
                                // Meta description
                                metaDesc = "Choose from a wide range of pickup, truck and car accessories for sale! From floor mats, seat covers, speakers, steering covers and more all";
                                // Title
                                metaTitle = "Car & Truck Accessories for Sale";                            
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot automotive accessories for sale page promotes the buying and selling of automotive accessories for your trucks, pickups and cars. " +
                                    "Find floor mats for sale, seat covers, car speakers, car steering covers and more.";
                            }
                            else if (alp.SubCategory.Name.Equals("Automotive Parts"))
                            {
                                // Meta description
                                metaDesc = "Get replacement parts for your car and truck. Rims, tires, engines, original wheels, headlights, radiators and more from our car parts for sale ads";
                                // Title
                                metaTitle = "Truck & Car Parts for Sale";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot Automotive Parts page promotes the buying and selling of auto parts, car parts, car tires, car engines, car wheels, car headlights, " +
                                    "car radiators and more Automotive Parts for your trucks, pickups and cars.";
                            }
                            else if (alp.SubCategory.Name.Equals("Boat Parts"))
                            {
                                // Meta description
                                metaDesc = "Get ready to hit the waters! Boat parts, boating accessories and more for sale";
                                // Title
                                metaTitle = "New & Used Boat Parts for Sale";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot Boat Parts page allows users to buy and sell Boat Parts including boat motors, boat anchors & ropes, boat covers & tarps, " +
                                    "boat fenders, boat seats and more.";
                            }
                            else if (alp.SubCategory.Name.Equals("Boats"))
                            {
                                // Meta description
                                metaDesc = "Buy or sell your sailboats, houseboats, boatels, yachts, sport boats, pirogues and more! New and used boats for sale";
                                // Title
                                metaTitle = "Used & New Boats for Sale";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "On the TrinbagoHotspot Boats for Sale page. Here you can find sailboats for sale, boatels for sale, sport boats for sale, pirogues for sale, yachts for sale, " +
                                    "houseboats for sale and more.";
                            }
                            else if (alp.SubCategory.Name.Equals("Cars/Trucks"))
                            {
                                // Target Keywords
                                // trini cars for sale, cars for sale trinidad, trini cars for sale nissan, trini cars for sale toyota, trini trucks for sale
                                var btype = "cars";
                                if (modelBodyType == null)
                                {
                                    // Meta description
                                    metaDesc = "New, roll on roll off, local, foreign used trini cars for sale. Kia, Hyundai, Nissan, Toyota. Buy cars";
                                    // Title
                                    metaTitle = "Trini Cars for Sale";
                                }
                                else if(modelBodyType == "convertible")
                                {
                                    // Meta description
                                    metaDesc = "New, roll on roll off, local and foreign used trini convertibles for sale. Kia, Hyundai, Nissan, Toyota. Buy convertibles";
                                    // Title
                                    metaTitle = "Trini Convertibles for Sale";
                                    btype = "Convertibles";
                                }
                                else if (modelBodyType == "coupe")
                                {
                                    // Meta description
                                    metaDesc = "New, roll on roll off, local and foreign used trini coupes (2 doors) for sale. Kia, Hyundai, Nissan, Toyota. Buy coupes";
                                    // Title
                                    metaTitle = "Trini Coupes for Sale";
                                    btype = "Coupes";
                                }
                                else if (modelBodyType == "hatchback")
                                {
                                    // Meta description
                                    metaDesc = "New, roll on roll off, local and foreign used trini hatchbacks for sale. Kia, Hyundai, Nissan, Toyota. Buy hatchbacks";
                                    // Title
                                    metaTitle = "Trini Hatchback for Sale";
                                    btype = "Hatchback";
                                }
                                else if (modelBodyType == "minivan")
                                {
                                    // Meta description
                                    metaDesc = "New, roll on roll off, local and foreign used trini minivans for sale. Kia, Hyundai, Nissan, Toyota. Buy minivans";
                                    // Title
                                    metaTitle = "Trini Minivans for Sale";
                                    btype = "Minivans";
                                }
                                else if (modelBodyType == "van")
                                {
                                    // Meta description
                                    metaDesc = "New, roll on roll off, local and foreign used trini vans for sale. Kia, Hyundai, Nissan, Toyota. Buy vans";
                                    // Title
                                    metaTitle = "Trini Vans for Sale";
                                    btype = "Vans";
                                }
                                else if (modelBodyType == "pickup-truck")
                                {
                                    // Meta description
                                    metaDesc = "New, roll on roll off, local and foreign used pickups for sale, trini trucks for sale. Kia, Hyundai, Nissan, Toyota. Buy trucks";
                                    // Title
                                    metaTitle = "Trini Trucks for Sale";
                                    btype = "Trucks";
                                }
                                else if (modelBodyType == "sedan")
                                {
                                    // Meta description
                                    metaDesc = "New, roll on roll off, local and foreign used trini sedans for sale. Kia, Hyundai, Nissan, Toyota. Buy sedans";
                                    // Title
                                    metaTitle = "Trini Sedans for Sale";
                                    btype = "Sedans";
                                }
                                else if (modelBodyType == "suv-crossover")
                                {
                                    // Meta description
                                    metaDesc = "New, roll on roll off, local and foreign used trini suvs for sale, trini crossovers for sale. Kia, Hyundai, Nissan, Toyota. Buy SUVs";
                                    // Title
                                    metaTitle = "Trini SUVs for Sale";
                                    btype = "SUVs";
                                }
                                else if (modelBodyType == "wagon")
                                {
                                    // Meta description
                                    metaDesc = "New, roll on roll off, local and foreign used trini wagons for sale. Kia, Hyundai, Nissan, Toyota. Buy wagons";
                                    // Title
                                    metaTitle = "Trini Wagons for Sale";
                                    btype = "Wagons";
                                }
                                // What we do desc
                                wwdDesc = string.Format("TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot Trini {0} for Sale page allows users to search new and used {0}, roll on roll off {0}, foreign used {0}, repossessed {0}." +
                                    "Filter by brands like Kia & Hyundai in Trinidad and Japanese used {0} like Nissan, Toyota and more from individuals and trini {0} dealers.", btype);
                            }
                            else if (alp.SubCategory.Name.Equals("Motorcycles/ATVs"))
                            {
                                // Meta description
                                metaDesc = "Motorcycles and ATVs for sale! Search new, local and foreign used brand names like Yamaha, Suzuki, BMW, Honda, Kawasaki, KTM and more";
                                // Title
                                metaTitle = "Motorcycle and ATV for Sale";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot Motorcycles and ATVs for Sale page. Trini New & Used ATVs and Motorcycles from Yamaha, Suzuki, Kawasaki and more.";
                            }
                            else if (alp.SubCategory.Name.Equals("Vehicle Services"))
                            {
                                // Meta description
                                metaDesc = "Discover quality car, truck and vehicle services including mechanics, wrecking, oil change, wheel alignment and more";
                                // Title
                                metaTitle = "Car & Truck Services";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "Trini car and truck services page. Advertise or find truck and car services including oil change, wheel alignment, wrecking and more.";
                            }
                            else if (alp.SubCategory.Name.Equals("Other"))
                            {
                                // Meta description
                                metaDesc = "Buy and sell planes, lawn tractors, buses and more for sale on TrinbagoHotspot";
                                // Title
                                metaTitle = "Other New & Used Vehicles Ads";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "Buy and sell, lawn tractors, planes, heavy trucks, buses and more for sale on TrinbagoHotspot other vehicles page.";
                            }
                        }
                        else
                        {
                            // Meta Description
                            metaDesc = "Vehicles for sale! Cars, boats, trucks, SUVs, motorcycles, ATVs and more";
                            // Title
                            metaTitle = "Vehicle Classifieds";
                            // What we do desc
                            wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                "We provide users the opportunity to post free vehicle classifieds on a platform that is mobile friendly " +
                                "and easily accessable with powerful search engine capabilities. TrinbagoHotspot Vehicles page " +
                                "targets the buying and selling of trini vehicles, including vehicle accessories and vehicle services.";
                        }
                    }
                    // Real Estate
                    else if (alp.Category.Name.Equals("Real Estate"))
                    {
                        if (alp.SubCategory != null)
                        {
                            if (alp.SubCategory.Name.Equals("Apartments/Condos For Sale"))
                            {
                                // Meta description
                                metaDesc = "Condos for sale, townhouses for sale, trini apartments for sale on TrinbagoHotspot the fastest growing classified ads site";
                                // Title
                                metaTitle = "Trini Apartments for Sale";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot trini apartments for sale page. Cheap trini apartments for sale, condos for sale and townhouses for sale in Trinidad.";
                            }
                            else if (alp.SubCategory.Name.Equals("Apartments/Condos Rental"))
                            {
                                string tempkeyword = null;
                                if (modelRentalType == null)
                                {
                                    // Meta description
                                    metaDesc = "Browse our trini apartments for rent and trini condo rental listings all on the fastest growing classified ads site";
                                    // Title
                                    metaTitle = "Trini Apartments for Rent";
                                    tempkeyword = "trini apartments for rent";                                    
                                }
                                else if (modelRentalType == "rental-only")
                                {
                                    // Meta description
                                    metaDesc = "Browse our apartments for rent and condo rental listings all on the fastest growing classified ads site";
                                    // Title
                                    metaTitle = "Rent Only Apartments";
                                    tempkeyword = "rent only apartments";
                                }
                                else if (modelRentalType == "rent-to-own")
                                {
                                    // Meta description
                                    metaDesc = "Browse our apartment for rent and condo rental listings all on the fastest growing classified ads site";
                                    // Title
                                    metaTitle = "Rent To Own Apartments";
                                    tempkeyword = "rent to own apartments";
                                }
                                // What we do desc
                                wwdDesc = string.Format("TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot {0} page. Find cheap furnished and unfurnished {0} " +
                                    "in all areas of Trinidad and Tobago.",tempkeyword);
                            }
                            else if (alp.SubCategory.Name.Equals("Commercial Office Space"))
                            {
                                // Meta Description
                                metaDesc = "Browse our commercial office space ads and buy, sell, rent or lease your office space today";
                                // Title
                                metaTitle = "Commercial Office Space for Rent & Sale";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot commercial office space. Sell rent or buy commercial office space in Trinidad and Tobago.";
                            }
                            else if (alp.SubCategory.Name.Equals("House For Sale"))
                            {
                                // Meta Description
                                metaDesc = "Homes for sale, houses for sale on T&T's fastest growing classified ads website. Discover your dream house today";
                                // Title
                                metaTitle = "Trini Homes for Sale";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot trini homes for sale page. Homes for sale including beach houses for sale in Trinidad and Tobago.";
                            }
                            else if (alp.SubCategory.Name.Equals("House Rental"))
                            {
                                string tempkeyword = null; 
                                if(modelRentalType == null)
                                {
                                    // Meta Description
                                    metaDesc = "Search for trini homes for rent and houses for rent from our classfified ads and find your next home with TrinbagoHotpsot Real Estate - House Rental";
                                    // Title
                                    metaTitle = "Trini Homes for Rent";
                                    tempkeyword = "trini homes for rent";
                                }
                                else if (modelRentalType == "rental-only")
                                {
                                    // Meta Description
                                    metaDesc = "Search for homes for rent and houses for rent from our classfified ads and find your next home with TrinbagoHotpsot Real Estate - House Rental";
                                    // Title
                                    metaTitle = "Rent Only Homes";
                                    tempkeyword = "rent only homes";
                                }
                                else if (modelRentalType == "rent-to-own")
                                {
                                    // Meta Description
                                    metaDesc = "Search for rent to own trini homes, rent to own houses from our classfified ads and find your next rent to own home with TrinbagoHotpsot Real Estate - House Rental";
                                    // Title
                                    metaTitle = "Rent To Own Homes";
                                    tempkeyword = "rent to own homes";
                                }

                                // What we do desc
                                wwdDesc = string.Format("TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot {0} page. Use our search filters to find homes and houses in Trinidad and Tobago.",tempkeyword);
                            }
                            else if (alp.SubCategory.Name.Equals("Land For Sale"))
                            {
                                // Meta Description
                                metaDesc = "Buy and sell cheap and affordable land for sale on the fastest growing classified ad website";
                                // Title
                                metaTitle = "Buy & Sell Land";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot trini land for sale page. Find cheap land for sale or affordable town and country approved land for sale in all areas of Trinidad and Tobago.";
                            }
                            else if (alp.SubCategory.Name.Equals("Land Rental/Leasing"))
                            {
                                string tempkeyword = null;
                                if (modelRentalType == null)
                                {
                                    // Meta Description
                                    metaDesc = "Find cheap and affordable land rentals on TrinbagoHotspot the fastest growing classified ad site";
                                    // Title
                                    metaTitle = "Trini Land for Rent";
                                    tempkeyword = "trini land for rent";
                                }
                                else if( modelRentalType == "rental-only")
                                {
                                    // Meta Description
                                    metaDesc = "Find cheap and affordable trini land for rent, trini land leasing on TrinbagoHotspot the fastest growing classified ad site";
                                    // Title
                                    metaTitle = "Rent Only Land";
                                    tempkeyword = "rent only land";
                                }
                                else if (modelRentalType == "rent-to-own")
                                {
                                    // Meta Description
                                    metaDesc = "Aquire cheap and affordable rent to own land on TrinbagoHotspot the fastest growing classified ad site";
                                    // Title
                                    metaTitle = "Rent To Own Land";
                                    tempkeyword = "rent to own land";
                                }

                                // What we do desc
                                wwdDesc = string.Format("TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot {0} page. Find cheap {0} in Trinidad and Tobago.",tempkeyword);
                            }
                            else if (alp.SubCategory.Name.Equals("Real Estate Services"))
                            {
                                // Meta Description
                                metaDesc = "Real estate services TrinbagoHotspot. Property management, inspections, realtors and more";
                                // Title
                                metaTitle = "Advertise or Find Real Estate Services";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot trini real estate services page. Advertise or find property management, trini realtors, property inspections and more.";
                            }
                            else if (alp.SubCategory.Name.Equals("Room Rental"))
                            {
                                // Meta Description
                                metaDesc = "Room rental and accomodations ideal for students on a budget. TrinbagoHotspot the fastest growing classifed site";
                                // Title
                                metaTitle = "Room Rental";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot trini room for rent page. Post or find student rooms for rent in Trinidad and Tobago.";
                            }
                            else if (alp.SubCategory.Name.Equals("Other"))
                            {
                                // Meta Description
                                metaDesc = "Buy and sell commercial buildings, income earning properties, small store spaces";
                                // Title
                                metaTitle = "Other Real Estate Classifieds";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot other real estate page. Sell or buy commercial buildings, income earning properties, small store spaces and more.";
                            }
                        }
                        else
                        {
                            // Meta Description
                            metaDesc = "In need of houses, apartments, condos, rooms, land and commercial office spaces? For sale, rent or leasing, we have them all";
                            // Title
                            metaTitle = "Real Estate Classifieds";
                            // What we do desc
                            wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                "TrinbagoHotspot real estate page provides a platform for the buying, selling and rental of trini apartments, trini land, trini homes, beach houses and more.";
                        }
                    }
                    // Pets
                    else if (alp.Category.Name.Equals("Pets"))
                    {
                        if (alp.SubCategory != null)
                        {
                            if (alp.SubCategory.Name.Equals("Lost Pet"))
                            {
                                // Meta Description
                                metaDesc = "Search and advertise for lost pets, dogs, cats and more on TrinbagoHotspot lost pet classifieds";
                                // Title
                                ViewBag.Title = "Advertise & Search for Lost Pets";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot Lost Pets page. Find or advertise a lost pet.";
                            }
                            else if (alp.SubCategory.Name.Equals("Pet & Animal Services"))
                            {
                                // Meta Description
                                metaDesc = "Pet and animal services. Dog walking, grooming, boarding, kenels, training, stud services and more";
                                // Title
                                ViewBag.Title = "Pet & Animal Services";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot Pet & Animal Services page. Advertise a pet services including pet grooming, kenels, pet training, stud services, dog walking and more.";
                            }
                            else if (alp.SubCategory.Name.Equals("Pet Accessories"))
                            {
                                // Meta Description
                                metaDesc = "Buy and Sell pet accessories on TrinbagoHotsptot. Get dog houses, cages, crates, cat trees, beds, supplies, aquariums and more";
                                // Title
                                metaTitle = "Buy & Sell Pet Accessories";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot Pet Accessories page. Buy and sell cages, crates, dog houses, pet supplies and more.";
                            }
                            else if (alp.SubCategory.Name.Equals("Pet Adoption"))
                            {
                                // Meta Description
                                metaDesc = "Pet rehoming & adoption on TrinbagoHotspot. Adopt a kitten, puppy, dog, cat and more today";
                                // Title
                                ViewBag.Title = "Pet Adoption";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "Rehoming and adoption of pets, kittens, puppies on TrinbagoHotspot Pet Rehoming & Adoption page.";
                            }
                            else if (alp.SubCategory.Name.Equals("Pet Hub"))
                            {
                                // Meta Description
                                metaDesc = "Search our pet hub for trini pets for sale on TrinbagoHotspot. Sell and buy kittens, pups, puppies, dogs, cats and more";
                                // Title
                                metaTitle = "Trini Pets for Sale";
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot trini pets for sale page. Dogs, puppies, cats and kittens for sale. German shepherds, Husky puppies, Maltipoo, Pompeks and more.";
                            }
                            else if (alp.SubCategory.Name.Equals("Other"))
                            {
                                // Meta Description
                                metaDesc = "Other pets for sale or adoption. Hamsters, guinea pigs, turtles and more on T&T's fastest growing classified site";
                                // Title
                                ViewBag.Title = "Other Pets for Sale or Adoption";    
                                // What we do desc
                                wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                    "TrinbagoHotspot other trini pets for sale page. Find or sell guinea pigs, turtles, hamsters and more.";
                            }
                        }
                        else
                        {
                            // Meta Description
                            metaDesc = "Puppies, kittens, dogs, cats, fishes, birds, pet accessories, adoption, sales and more on the fastest growing classified site";
                            // Title
                            metaTitle = "Pet Classifieds";
                            // What we do desc
                            wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                                "TrinbagoHotspot Pets page enables users to buy and sell pets, pet accessories & services and pet adoptions.";
                        }
                    }
                    // Business Services
                    else if (alp.Category.Name.Equals("Business Services"))
                    {
                        if (alp.SubCategory != null)
                        {
                            // Meta Description
                            metaDesc = "Advertise or find " + alp.SubCategory.Name.Replace("/", " and ") + " Services on the fastest growing classified site";
                            // Title
                            metaTitle = "Browse " + alp.SubCategory.Name.Replace("/", " & ") + " Services";
                        }
                        else
                        {
                            // Meta Description
                            metaDesc = "Advertise or find business services in T&T. Consultants, management, security, design, advertising, events, janitorial, catering, repairs services and more";
                            // Title
                            metaTitle = "Business Serivce Classifieds";
                        }
                        // What we do desc
                        wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                            "TrinbagoHotspot Business Services page. Find and advertise your local management, security, consultant, event panning and catering business services.";
                    }
                    // Buy Sell
                    else if (alp.Category.Name.Equals("Buy and Sell"))
                    {
                        if (alp.SubCategory != null)
                        {
                            // Meta Description
                            metaDesc = "Buy and sell " + alp.SubCategory.Name.Replace("/", " and ") + " on TrinbagoHotspot. The fastest growing classified site";
                            // Title
                            metaTitle = "Buy & Sell " + alp.SubCategory.Name.Replace("/", " & ") + " Classifieds";
                        }
                        else
                        {
                            // Meta description
                            metaDesc = "Buy and sell computers, laptops, phones, appliances, apparel and more";
                            // Title
                            metaTitle = "Buy & Sell Classifieds";
                        }
                        // What we do desc
                        wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                            "Buy & Sell pages allows you to buy and sell phones, furnature, clothing, appliances, computers and more.";
                    }
                    // Jobs
                    else if (alp.Category.Name.Equals("Jobs"))
                    {
                        // What we do desc
                        wwdDesc = "TrinbagoHotspot is a Free Classified ad website for Triniadad & Tobago. " +
                            "TrinBagoHotspot Jobs page provides a platform for companies and users to post and find trini jobs in Trinidad and Tobago.";
                    }
                }

                /*************************************************************/
                /*************************************************************/
                /*************************************************************/
                TempData["CountryId"] = ViewBag.CountryId = CountryId;
                TempData["RegionId"] = ViewBag.RegionId = RegionId;
                TempData["SeoLocation"] = ViewBag.SeoLocation = location;
                TempData["CategoryId"] = ViewBag.CategoryId = catId;
                TempData["SubCategoryId"] = ViewBag.SubCategoryId = subCatId;
                TempData["SeoCurrentCategory"] = ViewBag.SeoCurrentCategory = category;

                ViewBag.HeaderTitle = string.Format("{0} in {1}", (TempData["SearchItem"] == null ? (searchString != null ? searchString + " | " : null) : null) + (alp.SubCategory != null ? alp.SubCategory.Name : alp.Category != null ? alp.Category.Name : "Classified Ads"), descriptionLocaton);
                ViewBag.NotFoundText = string.Format("{0} in {1}", (alp.SubCategory != null ? alp.SubCategory.Name : alp.Category != null ? alp.Category.Name : "Classified Ads"), descriptionLocaton);
                ViewBag.SearchItem = TempData["SearchItem"] != null ? TempData["SearchItem"] : searchString == "" ? null : searchString;
                ViewBag.SearchOnlyOption = searchonlyOption == "All Ads" ? null : searchonlyOption;
                ViewBag.AdType = adtypeString == "ALL" ? null : adtypeString;
                ViewBag.DescriptionLocation = descriptionLocaton;
                ViewBag.AdSearchIdNotFound = TempData["AdSearchIdNotFound"];

                ViewBag.Filter_maxPrice = maxPrice;
                ViewBag.Filter_minPrice = minPrice;

                // Pets
                ViewBag.Filter_modelageType = ageType;
                ViewBag.Filter_minAge = minAge;
                ViewBag.Filter_maxAge = maxAge;

                // Jobs
                ViewBag.Filter_modelJobType = modelJobType;
                ViewBag.Filter_modelSalaryInfo = modelSalaryInfo;

                // RealEstate
                ViewBag.Filter_modelRentalType = modelRentalType;
                ViewBag.Filter_minSize = minSize;
                ViewBag.Filter_maxSize = maxSize;
                ViewBag.Filter_modelBedrooms = modelBedrooms;
                ViewBag.Filter_modelBathrooms = modelBathrooms;
                ViewBag.Filter_modelFurnished = modelFurnished;

                // Vehicles
                ViewBag.Filter_modelName = modelName;
                ViewBag.Filter_modelMake = modelMake;
                ViewBag.Filter_modelBodyType = modelBodyType;
                ViewBag.Filter_modelDrivetrain = modelDrivetrain;
                ViewBag.Filter_modelEngineSize = modEngineSize;
                ViewBag.Filter_modelColour = modelColour;
                ViewBag.Filter_modelTransmission = modelTransmission;
                ViewBag.Filter_modelCondition = modelCondition;
                ViewBag.Filter_maxMile = maxMile;
                ViewBag.Filter_minMile = minMile;
                ViewBag.Filter_minYear = minYear;
                ViewBag.Filter_maxYear = maxYear;

                // Bot index follow
                ViewBag.MetaIndexFollow = new HtmlString("<meta name='robots' content='INDEX,FOLLOW'/>");
                if (TempData["SearchItem"] != null)
                    searchString = TempData["SearchItem"].ToString();
#if DEBUG
            }
#endif
            // Escape if regex fails
            if (!ModelState.IsValid)
            {
                alp.ClassifiedAdList = SearchEngineManager.GetClassifiedAdListSearchEngine(catId, subCatId,
                    searchString, pageNumber, adtypeString, searchonlyOption);
                return View(alp);
            }
            else
            {
                alp.ClassifiedAdList = SearchEngineManager.GetClassifiedAdListSearchEngine(catId, subCatId,
                    searchString, pageNumber, adtypeString, searchonlyOption, minPrice, maxPrice, minMile, maxMile, modelName,
                    minYear, maxYear, modEngineSize, modelMake, modelBodyType, modelDrivetrain, modelTransmission, modelCondition,
                    modelColour, modelJobType, modelSalaryInfo, modelRentalType, modelBedrooms, modelBathrooms, modelFurnished, (CountryId > 0 ? CountryId : new int?()), (RegionId > 0 ? RegionId : new int?()),
                    minSize, maxSize, modelSpecies, minAge, maxAge, ageType);
            }
            // Setup rel = next / prev
            var query = HttpUtility.ParseQueryString(Request.Url.Query);
            string prev = null;
            string next = null;
            var uribuilder = new UriBuilder(Request.Url.GetLeftPart(UriPartial.Path));
            // set prev
            if (alp.ClassifiedAdList.HasPreviousPage)
            {
                if (pageNumber.Value - 1 > 1)
                    query.Set("pageNumber", (pageNumber.Value - 1).ToString());
                else
                    query.Remove("pageNumber");
                uribuilder.Query = query.ToString();                
                prev = "<link rel=\"prev\" href=\"" + uribuilder.Uri.ToString() + " \">";
            }
            // set next
            if (alp.ClassifiedAdList.HasNextPage)
            {
                query.Set("pageNumber", (pageNumber.Value + 1).ToString());
                uribuilder.Query = query.ToString();
                next = "<link rel=\"next\" href=\"" + uribuilder.Uri.ToString() + " \">";
            }
            
            uribuilder.Query = null;
            if (pageNumber.Value > 1)
                uribuilder.Query = "pageNumber=" + pageNumber.Value;
            string uricanonical = null;
            if(!string.IsNullOrEmpty(Request.Url.Query))
                uricanonical = "<link rel=\"canonical\" href=\"" + uribuilder.Uri.ToString() + " \">";

            // Strictly for Seo
            // remove trini from all meta tags when the location is set by user
            // attempt to target keyword 'trini' on one page
            if (!(CountryId == 0 && RegionId == 0))
            {
                metaDesc = metaDesc?.Replace("Trini ", "").Replace("trini ", "");
                metaTitle = metaTitle?.Replace("Trini ", "").Replace("trini ", "");
                wwdDesc = wwdDesc?.Replace("Trini ", "").Replace("trini ", "");
            }

            var keyword1 = "TrinbagoHotspot";
            var keyword2 = "Ads";
            var keyword3 = HttpUtility.HtmlEncode(alp.SubCategory != null ? alp.SubCategory.Name != "Other" ? alp.SubCategory.Name.Replace("/", " & ") : alp.SubCategory.Name + " " + alp.Category.Name : alp.Category != null ? alp.Category.Name : "Classifieds");
            var keyword4 = searchString != null ? ", " + searchString : null;
            // Keywords
            ViewBag.KeywordMeta = new HtmlString("<meta name='keywords' content='" + string.Format("{0},{1},{2}{3}", keyword1, keyword2, keyword3, keyword4) + "'/>");
            // Meta description
            if (searchString == null)
            {
                ViewBag.MetaDesc = new HtmlString("<meta name='description' content='" + (metaDesc != null ? HttpUtility.HtmlEncode(metaDesc) : "Browse our " + keyword3.ToLower() + " ads") + " in " + descriptionLocaton + "' />");
            }
            else
            {
                ViewBag.MetaDesc = new HtmlString("<meta name='description' content='Find " + searchString + " in " + descriptionLocaton + " | " + (metaDesc != null ? HttpUtility.HtmlEncode(metaDesc) : "Browse our " + keyword3.ToLower() + " ads") + " in T&T!' />");
            }
            if (metaTitle == null)
            {
                var c = alp.Category != null ? alp.Category.Name : null;
                var sc = alp.SubCategory != null ? alp.SubCategory.Name.Replace("/", " & ") + " | " : null;
                metaTitle = string.Format("{0}{1} Classifieds", sc, c);
            }
            // Title
            ViewBag.Title = (TempData["SearchItem"] == null ? searchString != null ? searchString + " | " : null : null) + metaTitle + " in " + descriptionLocaton;
            if(pageNumber.Value > 1)
            {
                // Add page number to title
                ViewBag.Title += " | Page " + pageNumber.Value;
            }
            // Footer
            ViewBag.WhatWeDoTitle = alp.SubCategory != null ? alp.SubCategory.Name : alp.Category != null ? alp.Category.Name : null;
            ViewBag.WhatWeDoDesc = wwdDesc;
            // Cononical
            ViewBag.Canonical = new HtmlString(uricanonical + Environment.NewLine + prev + Environment.NewLine + next);            
            
            // Featured Ad setup
            alp.FeaturedAds = SearchEngineManager.GetRefinedFeaturedAds(catId, subCatId, CountryId, RegionId, searchonlyOption, searchString, adtypeString);
            return View(alp);
        }

        [AllowAnonymous, HttpGet]
        public JsonResult FetchRegions(int Id)
        {
            var regions = ClassifiedAdManager.GetAllRegionsByCountryId(Id).Select(e => new
            {
                ID = e.Id,
                Name = e.Name
            });
            return Json(regions, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous, HttpGet]
        public JsonResult FetchRegionLocaton(int Id)
        {
            var coordinates = ClassifiedAdManager.GetRegionById<RegionCoordinate>(Id);
            return Json(coordinates, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateRecaptcha2(ErrorMessage = @"Recaptcha is required!")]
        public ActionResult ApplyToJob(ClassifiedAdApplyTo msg)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = ModelState.Values.SelectMany(s => s.Errors).Select(em => em.ErrorMessage).Aggregate((U, V) => U + ", " + V);
                return RedirectToAction("ListDetails", "ClassifiedAd", new { @stringId = msg.stringId });
            }

            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                msg.To = newthreadcontext.ClassifiedDB.SingleOrDefault(x => x.StringId.Equals(msg.stringId)).AdContactEmail;
            }

            bool Successful = ClassifiedAdManager.ApplyToJob(msg);

            if (Successful)
            {
                TempData["Message"] = "Application Sent!";
                return RedirectToAction("ListDetails", "ClassifiedAd", new { @stringId = msg.stringId });
            }
            else
            {
                TempData["Message"] = "Application Failed To Send. Please try again.";
                return RedirectToAction("ListDetails", "ClassifiedAd", new { @stringId = msg.stringId });
            }
        }

        [HttpPost, AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateRecaptcha2(ErrorMessage = @"Recaptcha is required!")]
        public ActionResult EmailUser(ClassifiedAdEmailUserPost msg)
        {
            if (!ModelState.IsValid) return PartialView("_AdDetailsEmailUserPartial", Mapper.Map<ClassifiedAdEmailUserForm>(msg));

            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var ca = newthreadcontext.ClassifiedDB.SingleOrDefault(x => x.StringId.Equals(msg.StringId));
                msg.To = ca.AdContactEmail;
                msg.AdTitle = ca.Title;
            }

            if (msg.From != null && msg.To != null && msg.Message != null && msg.ItemUrl != null)
                if (EmailMessenger.SendMessage(msg))
                {
                    return PartialView("_SuccessMessagePartial");
                }

            return PartialView("_FailedMessagePartial");
        }

        [HttpPost, AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ReportAd(ClassifiedAdReportPost rep)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_ReportPartial", Mapper.Map<ClassifiedAdReportForm>(rep));
            }

            if (ClassifiedAdManager.ReportClassifiedAd(rep))
            {
                return PartialView("_SuccessReportMessagePartial");
            }
            else
            {
                return PartialView("_FailedReportMessagePartial");
            }
        }
    }
}
