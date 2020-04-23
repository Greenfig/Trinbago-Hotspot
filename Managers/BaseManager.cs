using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml.Linq;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Areas.User.Managers;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Managers
{
    public class BaseApplicationManager : IDisposable
    {
        private UserManager _userManager;
        private ApplicationUser _currentUser;
        private ApplicationDbContext _currentDbContext;
        private SeoManager _seoManager;

        protected UserManager UserManager
        {
            get
            {
                return _userManager ?? new UserManager();
            }
        }

        protected ApplicationUser CurrentUser
        {
            get
            {
                return _currentUser ?? UserManager.AccountManager.FindById(HttpContext.Current.User.Identity.GetUserId());
            }
            set
            {
                _currentUser = value;
            }
        }

        protected ApplicationDbContext CurrentDbContext
        {
            get
            {
                return _currentDbContext ?? HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            }
            private set
            {
                _currentDbContext = value;
            }
        }

        protected SeoManager SeoManager
        {
            get
            {
                return _seoManager ?? new SeoManager();
            }
            private set
            {
                _seoManager = value;
            }
        }

        public IEnumerable<CountryList> GetAllCountriesWithRegionCount()
        {
            var val = CurrentDbContext.CountryDB.Include("Regions").ProjectTo<CountryList>().ToList();
            return val ?? null;
        }

        public IEnumerable<CountryBase> GetAllCountries()
        {
            var val = CurrentDbContext.CountryDB.ProjectTo<CountryBase>().ToList();
            return val ?? null;
        }

        public IEnumerable<CountryBase> GetAllCountriesWithDefault()
        {
            var cwd = CacheHelper.GetFromCache<IEnumerable<CountryBase>>("GetAllCountriesWithDefault");
            if (cwd == null)
            {
                var val = CurrentDbContext.CountryDB.ProjectTo<CountryBase>().ToList();
                if (val != null)
                {
                    val.Insert(0, new CountryBase() { Id = 0, Name = "Trinidad & Tobago" });
                    CacheHelper.SaveTocache("GetAllCountriesWithDefault", val, DateTime.Now.AddDays(7));
                    return val;
                }
            }
            return cwd;
        }

        public IEnumerable<CountryAdList> GetCountryAdLists(int countryId = 0)
        {
            var key = string.Format("GetCountryAdLists-{0}", countryId);
            var ccal = CacheHelper.GetFromCache<IEnumerable<CountryAdList>>(key);
            if (ccal == null)
            {
                if (countryId > 0)
                {
                    var item = CurrentDbContext.CountryDB.ProjectTo<CountryAdList>().Where(x => x.Id == countryId).ToList();
                    CacheHelper.SaveTocache(key, item, DateTime.Now.AddDays(1));
                    return item;
                }
                else
                {
                    var item = CurrentDbContext.CountryDB.Include("Regions").ProjectTo<CountryAdList>().ToList();
                    CacheHelper.SaveTocache(key, item, DateTime.Now.AddDays(1));
                    return item;
                }
            }
            return ccal;
        }

        public CountryWithDetail GetCountryById(int id)
        {
            var val = CurrentDbContext.CountryDB.Include("Regions").Where(a => a.Id == id).ProjectTo<CountryWithDetail>().FirstOrDefault();
            return (val != null) ? val : null;           
        }

        public XDocument GenerateSiteMap()
        {
            //Check disk for file
            var dir = HostingEnvironment.MapPath("~/sitemap.txt");
            try
            {
                var doc = XDocument.Load(dir);
                if (doc != null)
                {
                    var lastmod = Convert.ToDateTime(doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "lastmod").Value);
                    if ((DateTime.Now - lastmod).TotalDays > 14)
                    {
                        File.Delete(dir);
                        throw new FileNotFoundException();
                    }
                    else
                    {
                        return doc;
                    }
                }
            }
            catch (Exception)
            {
                XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
                string host = "https://trinbagohotspot.com";
                var bodytypes = BodyTypeGetAll();
                var rentaltypes = new List<string> { "Rental Only", "Rent to Own" };
                var regions = CurrentDbContext.RegionDB.Include("Country").Select(x => new { CountryId = x.Country.Id, CountrySeoName = x.SeoName, RegionId = x.Id, RegionSeoName = x.SeoName }).ToList();
                // filter out regions that contain classifieds
                var all = CurrentDbContext.ClassifiedDB.Include("Region").Include("SubCategory").Select(x => new { RegionId = x.Region.Id, SubCatId = x.SubCategory.Id }).Distinct().ToList();
                regions = regions.Where(x => all.Any(c => c.RegionId == x.RegionId)).ToList();
                var subcats = CurrentDbContext.SubCategoryDB.Include("Category").Select(x => new { CategoryId = x.Category.Id, CategoryName = x.Category.Name, CategorySeoName = x.Category.SeoName, SubCategoryId = x.Id, SubCategoryName = x.Name, SubCategorySeoName = x.SeoName }).ToList();
                subcats = subcats.Where(x => all.Any(c => c.SubCatId == x.SubCategoryId)).ToList();
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var sitemap = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), null);
                // apartement rental type
                var artt = CurrentDbContext.ClassifiedDB.Include("AdInfo").Where(x => x.SubCategory.Name == "Apartments/Condos Rental").Select(s => s.AdInfo.FirstOrDefault(a => a.Name == "Rental Type").Description).Distinct().ToList();
                var lrtt = CurrentDbContext.ClassifiedDB.Include("AdInfo").Where(x => x.SubCategory.Name == "Land Rental/Leasing").Select(s => s.AdInfo.FirstOrDefault(a => a.Name == "Rental Type").Description).Distinct().ToList();
                var hrtt = CurrentDbContext.ClassifiedDB.Include("AdInfo").Where(x => x.SubCategory.Name == "House Rental").Select(s => s.AdInfo.FirstOrDefault(a => a.Name == "Rental Type").Description).Distinct().ToList();
                // All Categories
                sitemap.Add(
                    new XElement(ns + "urlset",
                        new XElement(ns + "url",
                            new XElement(ns + "loc", host),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "weekly"),
                            new XElement(ns + "priority", "1.0")
                            ),
                        new XElement(ns + "url",
                            new XElement(ns + "loc", host + "/Home/Guides"),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "yearly"),
                            new XElement(ns + "priority", "0.1")
                            ),
                        new XElement(ns + "url",
                            new XElement(ns + "loc", host + "/Home/Terms"),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "yearly"),
                            new XElement(ns + "priority", "0.1")
                            ),
                        new XElement(ns + "url",
                            new XElement(ns + "loc", host + "/Home/Privacy"),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "yearly"),
                            new XElement(ns + "priority", "0.1")
                            ),
                        new XElement(ns + "url",
                            new XElement(ns + "loc", host + "/Home/About"),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "yearly"),
                            new XElement(ns + "priority", "0.1")
                            ),
                        new XElement(ns + "url",
                            new XElement(ns + "loc", host + "/Home/Contact"),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "yearly"),
                            new XElement(ns + "priority", "0.1")
                            ),
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.Action("AdList", "ClassifiedAd", new { Area = "ClassifiedAd", location = "trinidad-tobago", CountryId = 0, RegionId = 0 }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "daily"),
                            new XElement(ns + "priority", "0.5")
                            ),
                        // Trinidad and Tobago for all categories
                        from subcat in subcats.Select(x => new { CategoryName = x.CategoryName, CategorySeoName = x.CategorySeoName, CategoryId = x.CategoryId }).Distinct()
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.Action("AdList", "ClassifiedAd", new { Area = "ClassifiedAd", category = subcat.CategorySeoName, location = "trinidad-tobago", CountryId = 0, RegionId = 0, catId = subcat.CategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "daily"),
                            new XElement(ns + "priority", subcat.CategoryName.Equals("Vehicles") || subcat.CategoryName.Equals("Pets") || subcat.CategoryName.Equals("Real Estate") ? "0.8" : "0.2")
                            ),
                        // Trinidad for all categories
                        from subcat in subcats.Select(x => new { CategoryName = x.CategoryName, CategorySeoName = x.CategorySeoName, CategoryId = x.CategoryId }).Distinct().Where(x => x.CategoryName == "Real Estate")
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.Action("AdList", "ClassifiedAd", new { Area = "ClassifiedAd", category = subcat.CategorySeoName, location = "trinidad", CountryId = 1, RegionId = 0, catId = subcat.CategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "daily"),
                            new XElement(ns + "priority", subcat.CategoryName.Equals("Vehicles") || subcat.CategoryName.Equals("Pets") || subcat.CategoryName.Equals("Real Estate") ? "0.8" : "0.2")
                            ),
                        // Tobago for all categories
                        from subcat in subcats.Select(x => new { CategoryName = x.CategoryName, CategorySeoName = x.CategorySeoName, CategoryId = x.CategoryId }).Distinct().Where(x => x.CategoryName == "Real Estate")
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.Action("AdList", "ClassifiedAd", new { Area = "ClassifiedAd", category = subcat.CategorySeoName, location = "tobago", CountryId = 2, RegionId = 0, catId = subcat.CategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "daily"),
                            new XElement(ns + "priority", subcat.CategoryName.Equals("Vehicles") || subcat.CategoryName.Equals("Pets") || subcat.CategoryName.Equals("Real Estate") ? "0.8" : "0.2")
                            ),
                        // Trinidad and Tobago regions for all categories
                        from subcat in subcats.Select(x => new { CategoryName = x.CategoryName, CategorySeoName = x.CategorySeoName, CategoryId = x.CategoryId }).Distinct().Where(x => x.CategoryName == "Real Estate")
                        from region in regions
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.Action("AdList", "ClassifiedAd", new { Area = "ClassifiedAd", category = subcat.CategorySeoName, location = region.RegionSeoName, CountryId = region.CountryId, RegionId = region.RegionId, catId = subcat.CategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "weekly"),
                            new XElement(ns + "priority", subcat.CategoryName.Equals("Vehicles") || subcat.CategoryName.Equals("Pets") || subcat.CategoryName.Equals("Real Estate") ? "0.8" : "0.2")
                            ),
                        // Rent To Own & Rental
                        from subcat in subcats.Where(x => x.SubCategoryName.Equals("Apartments/Condos Rental"))
                        from rtt in artt
                        where rtt != null
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("ARentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = "trinidad-tobago", CountryId = 0, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "weekly"),
                            new XElement(ns + "priority", "0.8")
                            ),
                                                from subcat in subcats.Where(x => x.SubCategoryName.Equals("Apartments/Condos Rental"))
                                                from rtt in artt
                                                where rtt != null
                                                select
                                                new XElement(ns + "url",
                                                    new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("ARentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = "trinidad", CountryId = 1, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                                        new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                                    new XElement(ns + "changefreq", "weekly"),
                                                    new XElement(ns + "priority", "0.8")
                                                    ),
                                                                        from subcat in subcats.Where(x => x.SubCategoryName.Equals("Apartments/Condos Rental"))
                                                                        from rtt in artt
                                                                        select
                                                                        new XElement(ns + "url",
                                                                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("ARentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = "tobago", CountryId = 2, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                                                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                                                            new XElement(ns + "changefreq", "weekly"),
                                                                            new XElement(ns + "priority", "0.8")
                                                                            ),
                                                                                                from subcat in subcats.Where(x => x.SubCategoryName.Equals("Apartments/Condos Rental"))
                                                                                                from rtt in artt
                                                                                                from region in regions
                                                                                                select
                                                                                                new XElement(ns + "url",
                                                                                                    new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("ARentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = region.RegionSeoName, CountryId = region.CountryId, RegionId = region.RegionId, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                                                                                        new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                                                                                    new XElement(ns + "changefreq", "weekly"),
                                                                                                    new XElement(ns + "priority", "0.8")
                                                                                                    ),
                        from subcat in subcats.Where(x => x.SubCategoryName.Equals("Land Rental/Leasing"))
                        from rtt in lrtt
                        where rtt != null
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("LRentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = "trinidad-tobago", CountryId = 0, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "weekly"),
                            new XElement(ns + "priority", "0.8")
                            ),
                                                from subcat in subcats.Where(x => x.SubCategoryName.Equals("Land Rental/Leasing"))
                                                from rtt in lrtt
                                                where rtt != null
                                                select
                                                new XElement(ns + "url",
                                                    new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("LRentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = "trinidad", CountryId = 1, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                                        new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                                    new XElement(ns + "changefreq", "weekly"),
                                                    new XElement(ns + "priority", "0.8")
                                                    ),
                                                                        from subcat in subcats.Where(x => x.SubCategoryName.Equals("Land Rental/Leasing"))
                                                                        from rtt in lrtt
                                                                        where rtt != null
                                                                        select
                                                                        new XElement(ns + "url",
                                                                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("LRentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = "tobago", CountryId = 2, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                                                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                                                            new XElement(ns + "changefreq", "weekly"),
                                                                            new XElement(ns + "priority", "0.8")
                                                                            ),
                                                                                                from subcat in subcats.Where(x => x.SubCategoryName.Equals("Land Rental/Leasing"))
                                                                                                from rtt in lrtt
                                                                                                where rtt != null
                                                                                                from region in regions
                                                                                                select
                                                                                                new XElement(ns + "url",
                                                                                                    new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("LRentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = region.RegionSeoName, CountryId = region.CountryId, RegionId = region.RegionId, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                                                                                        new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                                                                                    new XElement(ns + "changefreq", "weekly"),
                                                                                                    new XElement(ns + "priority", "0.8")
                                                                                                    ),
                        from subcat in subcats.Where(x => x.SubCategoryName.Equals("House Rental"))
                        from rtt in hrtt
                        where rtt != null
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("HRentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = "trinidad-tobago", CountryId = 0, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "weekly"),
                            new XElement(ns + "priority", "0.8")
                            ),
                                        from subcat in subcats.Where(x => x.SubCategoryName.Equals("House Rental"))
                                        from rtt in hrtt
                                        where rtt != null
                                        select
                                        new XElement(ns + "url",
                                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("HRentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = "trinidad", CountryId = 1, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                            new XElement(ns + "changefreq", "weekly"),
                                            new XElement(ns + "priority", "0.8")
                                            ),
                                                                from subcat in subcats.Where(x => x.SubCategoryName.Equals("House Rental"))
                                                                from rtt in hrtt
                                                                where rtt != null
                                                                select
                                                                new XElement(ns + "url",
                                                                    new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("HRentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = "tobago", CountryId = 2, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                                                        new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                                                    new XElement(ns + "changefreq", "weekly"),
                                                                    new XElement(ns + "priority", "0.8")
                                                                    ),
                                                                                        from subcat in subcats.Where(x => x.SubCategoryName.Equals("House Rental"))
                                                                                        from rtt in hrtt
                                                                                        where rtt != null
                                                                                        from region in regions
                                                                                        select
                                                                                        new XElement(ns + "url",
                                                                                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("HRentSeoAdList", new { modelRentalType = new SeoManager().GetSeoTitle(rtt), category = subcat.SubCategorySeoName, location = region.RegionSeoName, CountryId = region.CountryId, RegionId = region.RegionId, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                                                                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                                                                            new XElement(ns + "changefreq", "weekly"),
                                                                                            new XElement(ns + "priority", "0.8")
                                                                                            ),
                        // Cars and trucks body types for Trinidad and Tobago
                        from subcat in subcats.Where(x => x.SubCategoryName.Equals("Cars/Trucks"))
                        from mbt in bodytypes
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.RouteUrl("CarSeoAdList", new { modelBodyType = new SeoManager().GetSeoTitle(mbt.Value.Replace("(2 door)", "")), category = subcat.SubCategorySeoName, location = "trinidad-tobago", CountryId = 0, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "weekly"),
                            new XElement(ns + "priority", "0.8")
                            ),
                        // Trinidad and Tobago for all subcategories
                        from subcat in subcats
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.Action("AdList", "ClassifiedAd", new { Area = "ClassifiedAd", category = subcat.SubCategorySeoName, location = "trinidad-tobago", CountryId = 0, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "weekly"),
                            new XElement(ns + "priority", !subcat.CategoryName.Equals("Jobs") ? "0.8" : "0.2")
                            ),
                        // Trinidad for all subcats
                        from subcat in subcats 
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.Action("AdList", "ClassifiedAd", new { Area = "ClassifiedAd", category = subcat.SubCategorySeoName, location = "trinidad", CountryId = 1, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "weekly"),
                            new XElement(ns + "priority", subcat.CategoryName.Equals("Real Estate") ? "0.8" : "0.2")
                            ),
                        // Tobago for all subcats
                        from subcat in subcats.ToList()
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.Action("AdList", "ClassifiedAd", new { Area = "ClassifiedAd", category = subcat.SubCategorySeoName, location = "tobago", CountryId = 2, RegionId = 0, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "weekly"),
                            new XElement(ns + "priority", subcat.CategoryName.Equals("Real Estate") ? "0.8" : "0.2")
                            ),
                        // Trinidad and Tobago Regions for all Subcats
                        from subcat in subcats.ToList()
                        from region in regions
                        select
                        new XElement(ns + "url",
                            new XElement(ns + "loc", string.Format("{0}{1}", host, urlHelper.Action("AdList", "ClassifiedAd", new { Area = "ClassifiedAd", category = subcat.SubCategorySeoName, location = region.RegionSeoName, CountryId = region.CountryId, RegionId = region.RegionId, catId = subcat.CategoryId, subCatId = subcat.SubCategoryId }))),
                                new XElement(ns + "lastmod", string.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                            new XElement(ns + "changefreq", "weekly"),
                            new XElement(ns + "priority", subcat.CategoryName.Equals("Real Estate") ? "0.8" : "0.2")
                            )
                        )
                    );
                //Write to file
                sitemap.Save(dir);
                return sitemap;
            }
            return null;
        }

        public IEnumerable<RegionBase> GetAllRegionsByCountryId(int id)
        {
            var val = CurrentDbContext.RegionDB.Include("Country").Where(x => x.Country.Id == id).OrderBy(x => x.Name).ProjectTo<RegionBase>().ToList();
            return val ?? null;
        }

        /// <summary>
        /// Not to be used with ClassifiedAd Add / Edit Form
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<RegionBase> GetAllRegionsByCountryIdWithDefault(int id)
        {

            var val = CurrentDbContext.RegionDB.Include("Country").Where(x => x.Country.Id == id).OrderBy(x => x.Name).ProjectTo<RegionBase>().ToList();
            if (val != null)
            {
                val.Insert(0, new RegionBase() { Id = 0, Name = "-Region-" });
                return val;
            }
            return null;
        }

        public T GetRegionById<T>(int id)
        {
            var val = CurrentDbContext.RegionDB.Where(a => a.Id == id).ProjectTo<T>().FirstOrDefault();
            return (val != null) ? val : default(T);
        }
        
        public IEnumerable<object> GetAllPriceInfo()
        {
            var val = CurrentDbContext.MiscInfoDB.Where(x => x.Descriptor.Equals("PriceInfo")).Select(s => new { Id = s.Id, Value = s.Value }).ToList();
            return val ?? null;
        }

        /// <summary>
        /// Get all categories if stringId is null.. else find the subcategory and append to the category list (Used in dropdown list for search bar)
        /// </summary>
        /// <param name="stringId"></param>
        /// <returns></returns>
        public IEnumerable<DropDownCategory> GetAllCategoriesOnly(string stringId = null)
        {
            if (!string.IsNullOrEmpty(stringId))
            {
                var toret = CurrentDbContext.CategoryDB.ProjectTo<DropDownCategory>().ToList();
                var subcatinsert = CurrentDbContext.SubCategoryDB.ProjectTo<DropDownCategory>().FirstOrDefault(x => x.Id.Equals(stringId));
                if (subcatinsert != null)
                    toret.Add(subcatinsert);

                return toret ?? null;
            }
            else
            {
                return CurrentDbContext.CategoryDB.ProjectTo<DropDownCategory>().ToList();
            }
        }

        public IEnumerable<InfoForm> GetAdTemplate(int subCatId)
        {
            try
            {
                var ai = CurrentDbContext.SubCategoryDB.SingleOrDefault(x => x.Id == subCatId).AdInfoTemplate.RecommendedInfo.AsQueryable().ProjectTo<InfoForm>().ToList();
                return (ai != null) ? ai : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<InfoForm> GetOneTemplateInfo(int catId, string subCatId)
        {
            return CurrentDbContext.SubCategoryDB.Include("AdInfoTemplate.RecommendedInfo").ProjectTo<AdInfoTemplateSlim>()
                .SingleOrDefault(x => x.StringId.Equals(subCatId)).RecommendedInfo;
        }

        public IEnumerable<MiscInfo> GetAllMake()
        {
            var val = CurrentDbContext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleMake")).OrderByDescending(y => y.Value);
            return val?.ToList();
        }

        public IEnumerable<MiscInfo> BodyTypeGetAll()
        {
            var val = CurrentDbContext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleBodyType"));
            return val?.ToList();
        }

        public IEnumerable<MiscInfo> GetAllTransmission()
        {
            IEnumerable<MiscInfo> val = CurrentDbContext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleTransmission"));
            return val?.ToList();
        }

        public IEnumerable<MiscInfo> GetAllFuelType()
        {
            IEnumerable<MiscInfo> val = CurrentDbContext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleFuelType"));
            return val?.ToList();
        }

        public IEnumerable<MiscInfo> GetAllCondition()
        {
            IEnumerable<MiscInfo> val = CurrentDbContext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleCondition"));
            return val?.ToList();
        }

        public IEnumerable<MiscInfo> GetAllDrivetrain()
        {
            IEnumerable<MiscInfo> val = CurrentDbContext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleDrivetrain"));
            return val?.ToList();
        }

        public IEnumerable<MiscInfoNoId> GetAllAdTypes()
        {
            var val = CurrentDbContext.MiscInfoDB.Where(x => x.Descriptor.Equals("AdType")).ProjectTo<MiscInfoNoId>().ToList();
            return val ?? null;
        }

        public void SetContanctInfo(ref string userContactName, ref string userContactEmail, ref string userContactPhone, ref string userContactPhone2, ref string userContactPhone3)
        {
            userContactEmail = CurrentUser.Email;
            userContactName = CurrentUser.ContactName ?? TextEditor.GetUserContactFromEmail(CurrentUser.Email);
            userContactPhone = CurrentUser.PhoneNumber ?? null;
            userContactPhone2 = CurrentUser.PhoneNumber2 ?? null;
            userContactPhone3 = CurrentUser.PhoneNumber3 ?? null;
        }

        /// <summary>
        /// Gets details for report form
        /// </summary>
        /// <param name="stringId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public ClassifiedAdReportForm ClassifiedAdReportDetails(int adId, string url)
        {
            var obj = CurrentDbContext.ClassifiedDB.Where(x => x.Id == adId).ProjectTo<ClassifiedAdReportForm>().FirstOrDefault();
            return obj ?? new ClassifiedAdReportForm();
        }

        public IEnumerable<CategoryTile> GetCategoryTiles()
        {
            // get top 3
            return CurrentDbContext.CategoryDB.Include("SubCategories").OrderByDescending(o => o.TotalClassifiedAdsCount).Take(3).ProjectTo<CategoryTile>().ToList();
        }

        /// <summary>
        /// Get Category with or without list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CategoryList> GetCategoryList(int catId = 0)
        {
            // cache
            var key = string.Format("getcatlist-{0}", catId);
            var list = CacheHelper.GetFromCache<IEnumerable<CategoryList>>(key);
            if (list != null)
                return list;

            // if category is specified get only subcats for it
            if (catId > 0)
            {
                list = CurrentDbContext.CategoryDB.Include("SubCategories").Select(x => new CategoryList()
                {
                    Id = x.Id,
                    Name = x.Name,
                    SeoName = x.SeoName,
                    TotalClassifiedAdsCount = x.TotalClassifiedAdsCount,
                    SubCategories = x.SubCategories.Where(w => w.Category.Id == catId).Select(sc => new SubCategoryList()
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        SeoName = sc.SeoName,
                        StringId = sc.StringId,
                        CategoryName = sc.Category.Name,
                        ClassifiedAdsCount = sc.ClassifiedAdsCount
                    }).OrderBy(o => o.Name == "Other").ThenBy(t => t.Name).ToList()
                }).ToList();
            }
            else
            {
                list = CurrentDbContext.CategoryDB.Include("SubCategories").ProjectTo<CategoryList>().ToList();
            }
            CacheHelper.SaveTocache(key, list, DateTime.Now.AddDays(7));
            return list;
        }
        
        public IEnumerable<CategoryList> GetCategoryListSlim()
        {
            return CurrentDbContext.CategoryDB.Include("SubCategories").Select(x => new CategoryList() { Name = x.Name, SubCategories = x.SubCategories.Select(s => new SubCategoryList() { Id = s.Id, StringId = s.StringId, Name = s.Name }).OrderBy(o => o.Name == "Other").ThenBy(t => t.Name).ToList() }).ToList();
        }

        public SearchBarCategory GetSubCatId(string searchBarId)
        {
            var obj = CurrentDbContext.SubCategoryDB.Include("Category").Where(f => f.StringId == searchBarId).ProjectTo<SearchBarCategory>().FirstOrDefault();
            return obj ?? null;
        }

        // get subcategory with its related category using id
        public SubCategoryCreateAd GetSubCatWithCat(int subCatId)
        {
            var val = CurrentDbContext.SubCategoryDB.Include("Category").Include("AdInfoTemplate.RecommendedInfo").ProjectTo<SubCategoryCreateAd>().SingleOrDefault(x => x.Id == subCatId);
            return val ?? null;
        }

        public Category GetCategoryByName(string name)
        {
            var obj = CurrentDbContext.CategoryDB.SingleOrDefault(x => x.Name.Contains(name));
            return obj ?? null;
        }

        // get category using int id
        public Category GetCatById(int catId)
        {
            Category val = CurrentDbContext.CategoryDB.Include("SubCategories").SingleOrDefault(x => x.Id == catId);
            return val;
        }

        // get subcategory using string id
        public SubCategory GetSubCatWithCatAndClass(int subCatId)
        {
            SubCategory val = new SubCategory();

            val = CurrentDbContext.SubCategoryDB.Include("Category").Include("ClassifiedAds").SingleOrDefault(x => x.Id == subCatId);

            return val;
        }

        // get subcategory using id
        public SubCategory GetSubCatWithCatID(int catId)
        {
            SubCategory val = CurrentDbContext.SubCategoryDB.Include("Category").SingleOrDefault(x => x.Id == catId);
            return val;
        }

        public SubCategory GetSubCategoryByName(string name)
        {
            var obj = CurrentDbContext.SubCategoryDB.Include("Category").SingleOrDefault(x => x.Name.Contains(name));
            return obj ?? null;
        }

        public SubCategory GetSubCategoryById(string stringId)
        {
            var obj = CurrentDbContext.SubCategoryDB.SingleOrDefault(x => x.StringId.Equals(stringId));
            return obj ?? null;
        }

        public bool CheckEmailDuplicate(string email)
        {
            var user = UserManager.AccountManager.FindByEmail(email);
            if (user != null)
                return true;

            return false;
        }

        public bool ApplyToJob(ClassifiedAdApplyTo msg)
        {
            var message = new MailMessage();
            try
            {
                var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p><p>Related Ad:</p><p>{3}</p>";
                message.To.Add(new MailAddress(msg.To));
                message.CC.Add(new MailAddress(msg.From));
                message.From = new MailAddress("DoNotReply@trinbagohotspot.com", "TrinbagoHotspot.com");
                message.Subject = "Job Application Message From " + msg.Name + " via 'TrinbagoHotSpot.com'";
                message.Body = string.Format(body, msg.Name, msg.From, msg.Message, msg.ItemUrl);
                message.IsBodyHtml = true;

                string fileName = Path.GetFileName(msg.FileUpload.FileName);
                var attachment = new Attachment(msg.FileUpload.InputStream, fileName);

                message.Attachments.Add(attachment);

                using (var smtp = new SmtpClient("trinbagohotspot.com", 26))
                {
                    var credentials = new NetworkCredential(ConfigurationManager.AppSettings["mailAccount"], ConfigurationManager.AppSettings["mailPassword"]);
                    smtp.Credentials = credentials;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 5000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool ReportClassifiedAd(ClassifiedAdReportPost rep)
        {
            var obj = CurrentDbContext.ClassifiedDB.SingleOrDefault(x => x.Id.Equals(rep.Id));
            if (obj != null)
            {
                var newitem = Mapper.Map<ClassifiedAdReport>(rep);
                newitem.ClassifiedAd = obj;
                CurrentDbContext.ReportDB.Add(newitem);
                CurrentDbContext.SaveChanges();
                return true;
            }
            else
                return false;
        }

        public bool AddGenericMessage(GenericMessage newmsg)
        {
            newmsg.PostDate = DateTime.Now;
            var o = CurrentDbContext.GenMessageDB.Add(newmsg);
            CurrentDbContext.SaveChanges();
            return (o == null) ? false : true;
        }

        public bool OpenRequestMyAd(int adId)
        {
            var obj = CurrentDbContext.ReportDB.Include("ClassifiedAd").Where(x => x.ClassifiedAd.Id == adId);
            if (obj != null)
            {
                foreach (var o in obj)
                {
                    o.OpenRequest = true;
                }
                CurrentDbContext.SaveChanges();
                return true;
            }
            else
                return false;
        }

        public string GetCountryNameById(int id)
        {
            var c = CurrentDbContext.CountryDB.ProjectTo<CountryBase>().FirstOrDefault(x => x.Id == id);
            return c?.Name;
        }

        public string GetCountryRegionNameById(int id)
        {
            var c = CurrentDbContext.RegionDB.Select(s => new { RegionId = s.Id, RegionName = s.Name }).FirstOrDefault(x => x.RegionId == id);
            return c != null ? string.Format("{0}", c.RegionName) : null;
        }

        public string GetAdTypeByName(string adName)
        {
            return CurrentDbContext.MiscInfoDB.Where(x => x.Descriptor.Equals("AdType")).SingleOrDefault(x => x.Value == adName).Name;
        }

        public int GetAdCount()
        {
            var key = "-ad-count-";
            var stringCount = CacheHelper.GetFromCache<string>(key);
            if (stringCount != null) return int.Parse(stringCount);

            var count = CurrentDbContext.ClassifiedDB.Count(x => x.Status == 0);
            CacheHelper.SaveTocache(key, count.ToString(), DateTime.Now.AddDays(3), TimeSpan.Zero);
            return count;
        }

        public int GetSubCatAdCount(int subCatId)
        {
            int val = CurrentDbContext.SubCategoryDB
                .FirstOrDefault(x => x.Id == subCatId)
                .ClassifiedAds.Count;
            return val;
        }

        public int ClassifiedAdViewIncrement(int Id, string userId, bool increment)
        {
            // fetch obj then increment and save                        
            var o = CurrentDbContext.AdViewsDB.SingleOrDefault(a => a.ClassifiedAdId == Id);
            if (o != null)
            {
                if (o.UserId.ToString() != userId && increment)
                {
                    CurrentDbContext.Entry(o).CurrentValues.SetValues(++o.Count);
                    CurrentDbContext.SaveChanges();
                }
                return o.Count;
            }
            return 0;
        }

        public void SubCatFix()
        {
            var all = CurrentDbContext.SubCategoryDB.Include("ClassifiedAds");
            foreach (var i in all)
            {
                i.ClassifiedAdsCount = i.ClassifiedAds.Where(x => x.Status == 0).Count();
            }
            CurrentDbContext.SaveChanges();
        }

        public virtual void Dispose()
        {
            if (_currentDbContext != null)
            {
                _currentDbContext.Dispose();
                _currentDbContext = null;
            }
            if (_userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }
            if (_currentUser != null)
            {
                _currentUser = null;
            }
        }

    }
}