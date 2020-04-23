using System;
using System.Linq;
using System.Text.RegularExpressions;
using Trinbago_MVC5.Controllers;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Managers
{
    public class SeoManager : BaseApplicationManager
    {

        // Slug generation taken from http://stackoverflow.com/questions/2920744/url-slugify-algorithm-in-c

        private string RemoveAccent(string text)
        {
            text = Regex.Replace(text, @"[/]", " / ");
            text = Regex.Replace(text, @"[&]", " & ");
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(text);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        private string CategorySlug(ref int CatId, ref int SubCatId)
        {
            // Check cache
            string returnUrl = null;
            string CatName = null;
            string SubCatName = null;
            int catId = CatId;
            int subCatId = SubCatId;

            if (catId == 0 && subCatId == 0)
            {
                CatName = "Classified";
                SubCatName = "Ads";
            }
            else
            {
                // There is no names given
                // Get values from Db
                if (SubCatId > 0)
                {
                    var subcat = CurrentDbContext.SubCategoryDB.Include("Category").Select(s => new { Id = s.Id, CatId = s.Category.Id, CategoryName = s.Category.Name, SubCatName = s.Name }).FirstOrDefault(x => x.Id == subCatId);
                    if (subcat == null) return null;
                    CatName = subcat.CategoryName;
                    SubCatName = subcat.SubCatName;
                    CatId = subcat.CatId;
                }
                else if (CatId > 0)
                {
                    var cat = CurrentDbContext.CategoryDB.Include("Category").Select(s => new { CategoryId = s.Id, CategoryName = s.Name }).FirstOrDefault(x => x.CategoryId == catId);
                    if (cat == null) return null;
                    CatName = cat.CategoryName;
                }
            } 
            int len = 80;
            string phrase = SubCatName != null ? SubCatName.Contains("Services") ? string.Format("{0}", SubCatName) : string.Format("{0}-{1}", CatName, SubCatName) : string.Format("{0}", CatName);
            returnUrl = RemoveAccent(phrase).ToLower();
            // invalid chars
            returnUrl = Regex.Replace(returnUrl, @"(\s-\s)", " ");
            returnUrl = Regex.Replace(returnUrl, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            returnUrl = Regex.Replace(returnUrl, @"\s+", " ").Trim();
            // cut and trim 
            returnUrl = returnUrl.Substring(0, returnUrl.Length <= len ? returnUrl.Length : len).Trim();
            returnUrl = Regex.Replace(returnUrl, @"\s", "-"); // hyphens   
            
            return returnUrl;
        }

        private string CategorySlug(int CatId, int SubCatId, string CatName = null, string SubCatName = null)
        {
            // Check cache
            var key = string.Format("c-{0}-{1}", CatId, SubCatId);
            var urlCache = CacheHelper.GetFromCache<string>(key);
            if (urlCache != null) return urlCache;

            if(CatId == 0 && SubCatId == 0)
            {
                CatName = "Classified";
                SubCatName = "Ads";
            }
            else if(SubCatName == null  && CatName == null)
            {
                // There is no names given
                // Get values from Db
                if (SubCatId > 0)
                {   
                    var subcat = CurrentDbContext.SubCategoryDB.Include("Category").Select(s => new { Id = s.Id, CategoryName = s.Category.Name, SubCatName = s.Name }).FirstOrDefault(x => x.Id == SubCatId);
                    if (subcat == null) return null;
                    CatName = subcat.CategoryName;
                    SubCatName = subcat.SubCatName;                    
                }
                else if (CatId > 0)
                {                    
                    var cat = CurrentDbContext.CategoryDB.Include("Category").Select(s => new { CategoryId = s.Id, CategoryName = s.Name }).FirstOrDefault(x => x.CategoryId == CatId);
                    if (cat == null) return null;
                    CatName = cat.CategoryName;                    
                }
            }

            int len = 80;
            string phrase = SubCatName != null ? SubCatName.Contains("Services") ? string.Format("{0}", SubCatName) : string.Format("{0}-{1}", CatName, SubCatName) : string.Format("{0}", CatName);
            string str = RemoveAccent(phrase).ToLower();
            // invalid chars
            str = Regex.Replace(str, @"(\s-\s)", " ");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= len ? str.Length : len).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            
            // Save to server cache
            CacheHelper.SaveTocache(key, str, DateTime.Now.AddHours(1));
            return str;
        }

        private string LocationSlug(string CountryName, string RegionName = null)
        {
            int len = 80;
            string phrase = RegionName != null ? string.Format("{0}-{1}", CountryName, RegionName) : string.Format("{0}", CountryName);
            string str = RemoveAccent(phrase).ToLower();
            // invalid chars
            str = Regex.Replace(str, @"(\s-\s)", " ");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= len ? str.Length : len).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens
            return str;
        }

        private string TitleSlug(string Title)
        {
            int len = 80;
            string phrase = Title.Trim();

            string str = RemoveAccent(phrase).ToLower();
            // invalid chars
            str = Regex.Replace(str, @"(\s-\s)", " ");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= len ? str.Length : len).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   

            return str;
        }

        private string LocationSlug(ref int CountryId, ref int RegionId)
        {
            string returnUrl = null;
            string CountryName = null;
            string RegionName = null;
            int countryId = CountryId;
            int regionId = RegionId;

            if (countryId == 0 && regionId == 0)
            {
                CountryName = "Trinidad";
                RegionName = "Tobago";
            }
            else
            {
                if (RegionId > 0)
                {
                    var region = CurrentDbContext.RegionDB.Include("Country").Select(s => new { CountryId = s.Country.Id, RegionId = s.Id, CountryName = s.Country.Name, RegionName = s.Name }).FirstOrDefault(x => x.RegionId == regionId);
                    if (region == null) return null;
                    CountryName = region.CountryName;
                    RegionName = region.RegionName;
                    CountryId = region.CountryId;
                }
                else if (CountryId > 0)
                {
                    var country = CurrentDbContext.CountryDB.Select(s => new { CountryId = s.Id, CountryName = s.Name }).FirstOrDefault(x => x.CountryId == countryId);
                    if (country == null) return null;
                    CountryName = country.CountryName;
                }
            }

            int len = 80;
            string phrase = RegionName != null ? string.Format("{0}-{1}", CountryName, RegionName) : string.Format("{0}", CountryName);
            returnUrl = RemoveAccent(phrase).ToLower();
            // invalid chars
            returnUrl = Regex.Replace(returnUrl, @"(\s-\s)", " ");
            returnUrl = Regex.Replace(returnUrl, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            returnUrl = Regex.Replace(returnUrl, @"\s+", " ").Trim();
            // cut and trim 
            returnUrl = returnUrl.Substring(0, returnUrl.Length <= len ? returnUrl.Length : len).Trim();
            returnUrl = Regex.Replace(returnUrl, @"\s", "-"); // hyphens

            return returnUrl;
        }

        private string LocationSlug(int CountryId, int RegionId = 0)
        {
            var key = string.Format("loc-{0}-{1}", CountryId, RegionId);
            var urlCache = CacheHelper.GetFromCache<string>(key);
            if (urlCache != null) return urlCache;
            string location = null;
            if (CountryId == 0 && RegionId == 0)
            {
                location = "trinidad-tobago";
            }
            else
            {
                if (RegionId > 0)
                {
                    var region = CurrentDbContext.RegionDB.Include("Country").Select(s => new { RegionId = s.Id, RegionSeoName = s.SeoName }).FirstOrDefault(x => x.RegionId == RegionId);
                    if (region == null) return null;
                    location = region.RegionSeoName;                    
                }
                else if (CountryId > 0)
                {
                    var country = CurrentDbContext.CountryDB.Select(s => new { CountryId = s.Id, CountrySeoName = s.SeoName }).FirstOrDefault(x => x.CountryId == CountryId);
                    if (country == null) return null;
                    location = country.CountrySeoName;                    
                }
            }

            CacheHelper.SaveTocache(key, location, DateTime.Now.AddHours(1));
            return location;
        }

        public string GenerateAdListOldUrl(int catId, string subCatId)
        {
            var sc = CurrentDbContext.SubCategoryDB.Select(x => new { Id = x.Id, StringId = x.StringId }).FirstOrDefault(f => f.StringId == subCatId);
            if (sc == null) return null;
            var url = GenerateAdListUrl(catId, sc.Id);
            if (url == null) return null;
            return (url);            
        }

        public string GenerateListDetailsOldUrl(string stringId)
        {
            var id = CurrentDbContext.ClassifiedDB.Select(x => new { Id = x.Id, StringId = x.StringId }).FirstOrDefault(f => f.StringId == stringId);
            if (id == null) return null;
            var url = GenerateListDetailsUrl(id.Id);
            return url;            
        }

        public string CategorySlug(string CatName, string SubCatName = null)
        {
            
            int len = 80;
            string phrase = SubCatName != null ? string.Format("{0}-{1}", CatName, SubCatName) : string.Format("{0}", CatName);
            string str = RemoveAccent(phrase).ToLower();
            // invalid chars
            str = Regex.Replace(str, @"(\s-\s)", " ");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= len ? str.Length : len).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        /// <summary>
        /// Generate Ad Detail Url From LUCENE
        /// </summary>
        /// <param name="stringId"></param>
        /// <returns></returns>
        public string GenerateAdDetailUrl(int Id)
        {
            var key = string.Format("ld-{0}", Id);

            var urlCache = CacheHelper.GetFromCache<string>(key);
            if (urlCache != null) return urlCache;

            var format = "/cld-{0}/{1}/{2}/{3}";
            string returnUrl;
            var ad = SearchEngineManager.GetClassifiedAdWithDetails(Id);
            if (ad == null) return null;
            returnUrl = string.Format(format, CategorySlug(ad.Category.Id, ad.SubCategory.Id, ad.Category.Name, ad.SubCategory.Name), LocationSlug(ad.Country.Id, ad.Region.Id), ad.Id, TitleSlug(ad.Title));
            CacheHelper.SaveTocache(key, returnUrl, DateTime.Now.AddHours(1));
            return returnUrl;            
        }

        /// <summary>
        /// Generate from DB
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public string GenerateListDetailsUrl(int Id)
        {
            var key = string.Format("ld-{0}", Id);

            var urlCache = CacheHelper.GetFromCache<string>(key);
            if (urlCache != null) return urlCache;

            var format = "/cld-{0}/{1}/{2}/{3}";
            string returnUrl;
            var ad = SearchEngineManager.GetClassifiedAdWithDetailsFromDb(Id);
            if (ad == null) return null;
            returnUrl = string.Format(format, CategorySlug(ad.Category.Id, ad.SubCategory.Id, ad.Category.Name, ad.SubCategory.Name), LocationSlug(ad.Country.Id, ad.Region.Id), ad.Id, TitleSlug(ad.Title));
            CacheHelper.SaveTocache(key, returnUrl, DateTime.Now.AddHours(1));
            return returnUrl;
        }

        public bool IsAdSuspended(int Id)
        {            
            return SearchEngineManager.IsAdSuspended(Id);
        }        

        /// <summary>
        /// Generate Url
        /// </summary>
        /// <param name="CatId"></param>
        /// <param name="SubCatId"></param>
        /// <param name="CountryId"></param>
        /// <param name="RegionId"></param>
        /// <returns></returns>
        public string GenerateAdListUrl(int CatId, int SubCatId, int CountryId = 0, int RegionId = 0)
        {
            // Check cache
            var key = string.Format("{0}-{1}-{2}-{3}", CatId, SubCatId, CountryId, RegionId);

            var urlCache = CacheHelper.GetFromCache<string>(key);
            if (urlCache != null) return urlCache;

            string returnUrl;
            var format = "/{0}/{1}/lc{2}lr{3}-c{4}sc{5}";
            var category = CategorySlug(CatId, SubCatId);
            var location = LocationSlug(CountryId, RegionId);
            if (category == null || location == null) return null;

            returnUrl = string.Format(format, category, location, CountryId, RegionId, CatId, SubCatId);
            
            // Save to server cache
            CacheHelper.SaveTocache(key, returnUrl, DateTime.Now.AddHours(1));
            return returnUrl;
            
        }

        /// <summary>
        /// Generate Ad List Url and update parameters sent
        /// </summary>
        /// <param name="CatId"></param>
        /// <param name="SubCatId"></param>
        /// <param name="CountryId"></param>
        /// <param name="RegionId"></param>
        /// <returns></returns>
        public string GenerateAdListUrl(ref int CatId, ref int SubCatId, ref int CountryId, ref int RegionId)
        {
            // Check cache
            var key = string.Format("{0}-{1}-{2}-{3}", CatId, SubCatId, CountryId, RegionId);

            string returnUrl = CacheHelper.GetFromCache<string>(key);
            if (returnUrl == null)
            {
                var format = "/{0}/{1}/lc{2}lr{3}-c{4}sc{5}";
                var category = CategorySlug(ref CatId, ref SubCatId);
                var location = LocationSlug(ref CountryId, ref RegionId);
                if (category == null || location == null) return null;

                returnUrl = string.Format(format, category, location, CountryId, RegionId, CatId, SubCatId);

                // Save to server cache
                CacheHelper.SaveTocache(key, returnUrl, DateTime.Now.AddMinutes(20));
            }
            return returnUrl;

        }

        /// <summary>
        /// Cache location from database
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        public string GetSeoLocation(int cid, int rid)
        {
            return LocationSlug(cid, rid);
        }

        /// <summary>
        /// Get location from names
        /// </summary>
        /// <param name="countryName"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
        public string GetSeoLocation(string countryName, string regionName = null)
        {
            return LocationSlug(countryName, regionName);
        }

        /// <summary>
        /// Cache category name from database
        /// </summary>
        /// <param name="catid"></param>
        /// <param name="scatid"></param>
        /// <returns></returns>
        public string GetSeoCategory(int catid, int scatid)
        {
            return CategorySlug(catid, scatid);
        }

        /// <summary>
        /// Generate category name with out cache
        /// </summary>
        /// <param name="catName"></param>
        /// <param name="scatName"></param>
        /// <returns></returns>
        public string GetSeoCategory(string catName, string scatName = null)
        {
            return CategorySlug(catName, scatName);
        }

        public string GetSeoTitle(string title)
        {
            return TitleSlug(title);
        }

        ///// <summary>
        ///// Edits a Category's Seo strings
        ///// </summary>
        ///// <param name="item"></param>
        //public void EditCategorySeo(CategorySeoEdit item)
        //{
        //      var cat = CurrentDbContext.CategoryDB.First(x => x.Id == item.Id);
        //      if (cat == null) return;
        //      cat.MetaDescription = item.MetaDescription;
        //      cat.MetaTitle = item.MetaTitle;
        //      cat.MetaKeywords = item.MetaKeywords;
        //      CurrentDbContext.SaveChanges();
        //}

        ///// <summary>
        ///// Edits a Subcategory's Seo strings
        ///// </summary>
        ///// <param name="item"></param>
        //public void EditSubCategorySeo(SubCategorySeoEdit item)
        //{
        //      var sub = CurrentDbContext.SubCategoryDB.First(x => x.Id == item.Id);
        //      if (sub == null) return;
        //      sub.MetaDescription = item.MetaDescription;
        //      sub.MetaTitle = item.MetaTitle;
        //      sub.MetaKeywords = item.MetaKeywords;
        //      CurrentDbContext.SaveChanges();
        //}
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}