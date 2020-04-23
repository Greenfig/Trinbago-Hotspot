using AutoMapper;
using AutoMapper.QueryableExtensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using Trinbago_MVC5.Areas.Admin.Models;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Areas.User.Models;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Models;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Areas.ClassifiedAd.Managers;
using Microsoft.AspNet.Identity;
using Trinbago_MVC5.Areas.Promotion.Managers;
using Trinbago_MVC5.Areas.Promotion.Models;
using System.Web.WebPages;
using ClassifiedAdAlias = Trinbago_MVC5.Models.ClassifiedAd;
using Trinbago_MVC5.Controllers;
using Hangfire;

namespace Trinbago_MVC5.Areas.Admin.Managers
{
    public class AdministratorManager : BaseApplicationManager
    {
        private ClassifiedAdManager _classifiedManager;

        private PromotionManager _promoManager;

        public ClassifiedAdManager ClassifiedManager
        {
            get
            {
                return _classifiedManager ?? new ClassifiedAdManager();
            }
            private set
            {
                _classifiedManager = value;
            }
        }

        private PromotionManager PromoManager
        {
            get
            {
                return _promoManager ?? new PromotionManager();
            }
            set
            {
                _promoManager = value;
            }
        }

        public CountryBase AddCountry(CountryAdd newItem)
        {
            if (!CurrentDbContext.CountryDB.Any(a => a.Name.Contains(newItem.Name)))
            {
                var addedItem = CurrentDbContext.CountryDB.Add(Mapper.Map<Country>(newItem));
                addedItem.SeoName = SeoManager.GetSeoLocation(addedItem.Name);
                CurrentDbContext.SaveChanges();
                return (addedItem == null) ? null : Mapper.Map<CountryBase>(addedItem);
            }
            return null;
        }

        public RegionBase AddRegion(RegionAdd newItem)
        {
            // get associated object
            var o = CurrentDbContext.CountryDB.Include("Regions").SingleOrDefault(a => a.Id == newItem.CountryId);

            if (o == null)
            {
                return (Mapper.Map<RegionBase>(newItem));
            }

            // check for duplicate region
            var dup = o.Regions.SingleOrDefault(a => a.Name == newItem.Name);

            if (!o.Regions.Any(a => a.Name == newItem.Name))
            {
                var addedItem = CurrentDbContext.RegionDB.Add(Mapper.Map<Region>(newItem));
                addedItem.Country = o;
                addedItem.SeoName = SeoManager.GetSeoLocation(addedItem.Country.Name, addedItem.Name);
                o.RegionCount++;
                CurrentDbContext.SaveChanges();
                return (addedItem == null) ? null : Mapper.Map<RegionBase>(addedItem);

            }
            return (Mapper.Map<RegionBase>(newItem));
        }

        public RegionBase EditRegion(RegionEdit editItem)
        {
            var o = CurrentDbContext.RegionDB.Include("Country").SingleOrDefault(r => r.Id == editItem.Id);

            if (o != null)
            {
                o.Name = editItem.Name;
                o.Lat = editItem.Lat;
                o.Lng = editItem.Lng;
                o.Zoom = editItem.Zoom;
                o.SeoName = SeoManager.GetSeoLocation(o.Country.Name, o.Name);
                CurrentDbContext.SaveChanges();
            }

            return (o == null) ? null : Mapper.Map<RegionBase>(editItem);
        }

        public IEnumerable<UserProfileBase> GetAllMembers()
        {
            IEnumerable<UserProfileBase> members =
                from user in CurrentDbContext.Users
                join role in CurrentDbContext.Roles on user.Roles.FirstOrDefault().RoleId equals role.Id into _RoleNames
                orderby user.RegisterDate descending
                select new UserProfileBase() { Id = user.Id, PosterName = user.PosterName, Email = user.Email, RoleNames = _RoleNames.Select(x => x.Name).ToList() };
            return members.ToList();
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CategoryList> GetAllCatNameOnly()
        {
            var var = CurrentDbContext.CategoryDB.Select(x => new CategoryList() { Id = x.Id, Name = x.Name }).ToList();
            return var ?? null;
        }

        /// <summary>
        /// Get one category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CategoryList GetOneCatById(int id)
        {
            var var = CurrentDbContext.CategoryDB.Select(x => new CategoryList() { Id = x.Id, SubCategories = x.SubCategories.Select(s => new SubCategoryList() { Id = s.Id, StringId = s.StringId, Name = s.Name }).ToList() }).FirstOrDefault(c => c.Id == id);
            return var ?? null;
        }

        public CategorySeoEditForm GetOneCatSeoById(int id)
        {
            var var = CurrentDbContext.CategoryDB.ProjectTo<CategorySeoEditForm>().FirstOrDefault(x => x.Id == id);
            return (var == null) ? null : Mapper.Map<CategorySeoEditForm>(var);
        }

        public Category EditCategory(Category item)
        {
            Category var = CurrentDbContext.CategoryDB.SingleOrDefault(c => c.Id == item.Id);
            var.Name = item.Name;
            var.SeoName = SeoManager.GetSeoCategory(var.Name);
            CurrentDbContext.SaveChanges();
            return var;
        }

        /// <summary>
        /// Get one subcategory by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SubCategoryEditForm GetOneSubCatById(string id)
        {
            return CurrentDbContext.SubCategoryDB.ProjectTo<SubCategoryEditForm>().SingleOrDefault(s => s.StringId == id);
        }

        public SubCategory EditSubCat(SubCategoryEdit item)
        {
            SubCategory var = CurrentDbContext.SubCategoryDB.Include("Category").SingleOrDefault(s => s.StringId == item.StringId);
            var.Name = item.Name;
            var.SeoName = SeoManager.GetSeoCategory(var.Category.Name, var.Name);
            CurrentDbContext.SaveChanges();

            return var;
        }

        public Category AddCategory(Category newItem)
        {
            var addeditem = CurrentDbContext.CategoryDB.Add(newItem);
            addeditem.SubCategories = null;
            addeditem.SeoName = SeoManager.GetSeoCategory(addeditem.Name);
            CurrentDbContext.SaveChanges();
            return addeditem;
        }

        public SubCategory AddSubCategory(SubCategoryAdd newItem)
        {
            SubCategory addeditem = Mapper.Map<SubCategory>(newItem);
            addeditem.StringId = MySecurity.GetGen();
            var cat = CurrentDbContext.CategoryDB.Include("SubCategories").SingleOrDefault(x => x.Id == newItem.CategoryId);
            cat.SubCategories.Add(addeditem);
            addeditem.Category = cat;
            CurrentDbContext.SaveChanges();
            if (addeditem.Category.Name == "Business Services")
            {
                addeditem.AdInfoTemplate = CurrentDbContext.TemplateDB.FirstOrDefault(x => x.TemplateName == "BUSSERV");
            }
            addeditem.SeoName = SeoManager.GetSeoCategory(addeditem.Category.Name, addeditem.Name);
            CurrentDbContext.SaveChanges();

            return addeditem;
        }

        public IEnumerable<ClassifiedAdReportList> GetAllReports()
        {
            var obj = CurrentDbContext.ReportDB.Include("ClassifiedAd").OrderByDescending(x => x.CreatedDate).ProjectTo<ClassifiedAdReportList>().ToList();
            return obj ?? new List<ClassifiedAdReportList>();
        }

        public bool CloseReportAd(string sId)
        {
            return false;
        }

        // Get one ad with details
        public AdminAdListDetail AdminGetClassifiedAdWithDetails(string stringId)
        {
            AdminAdListDetail item = CurrentDbContext.ClassifiedDB.Include("Photos").Include("AdInfo")
               .Include("Country").Include("Region").Include("Poster").Include("Category").Include("SubCategory").ProjectTo<AdminAdListDetail>()
               .SingleOrDefault(a => a.StringId == stringId);
            item.Price = item.Price.Equals("0") && item.PriceInfo.Equals("Please Contact") ? "Please Contact" : string.Format("{0:C0}", item.Price.AsFloat());
            return item ?? new AdminAdListDetail();
        }

        // Close a user posted ad
        public bool AdminCloseAd(int Id, string closingOptions, HttpServerUtilityBase server)
        {
            // get existing
            try
            {
                ClassifiedAdAlias obj = new ClassifiedAdAlias();

                switch (closingOptions)
                {
                    case "None":
                        ClassifiedManager.RemoveClassifiedAd(Id);
                        break;
                    case "Sold":
                        obj = CurrentDbContext.ClassifiedDB.Include("Poster").Include("Category").Include("SubCategory").SingleOrDefault(x => x.Id == Id && x.Status != 1 && x.Status != 2);
                        if (obj == null) return false;
                        obj.Status = 1;
                        obj.ExpiryTimeStamp = DateTime.Now.AddDays(30);
                        CurrentDbContext.SaveChanges();
                        goto default;
                    case "Rented":
                        obj = CurrentDbContext.ClassifiedDB.Include("Poster").Include("Category").Include("SubCategory").SingleOrDefault(x => x.Id == Id && x.Status != 1 && x.Status != 2);
                        if (obj == null) return false;
                        obj.Status = 2;
                        obj.ExpiryTimeStamp = DateTime.Now.AddDays(30);
                        CurrentDbContext.SaveChanges();
                        goto default;
                    default:
                        LuceneSearch.ClearLuceneIndexRecord(obj.Id);
                        LuceneSearch.AddUpdateLuceneIndex(obj);
                        break;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AdminOpenAd(int Id)
        {
            // get existing
            try
            {
                var obj = CurrentDbContext.ClassifiedDB.Include("Photos")
                    .Include("Country")
                    .Include("Region")
                    .Include("Poster")
                    .Include("AdInfo")
                    .Include("Category")
                    .Include("SubCategory")
                    .SingleOrDefault(x => x.Id == Id);

                if (obj.Status == -1)
                {
                    var obj2 = CurrentDbContext.ReportDB.Include("ClassifiedAd").Where(x => x.ClassifiedAd.Id == Id);
                    if (obj2 != null)
                    {
                        foreach (var o in obj2)
                        {
                            o.OpenRequest = false;
                        }
                    }
                }

                if (obj.Status != 0)
                {
                    obj.Status = 0;
                    obj.Category.TotalClassifiedAdsCount++;
                    obj.SubCategory.ClassifiedAdsCount++;
                    CurrentDbContext.SaveChanges();
                    // Add to Lucene
                    LuceneSearch.AddUpdateLuceneIndex(obj);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Suspend ad
        public bool AdminSuspendAd(int Id)
        {
            // get existing
            try
            {
                var obj = CurrentDbContext.ClassifiedDB.Include("Category").Include("SubCategory").Include("Poster").Include("Photos").SingleOrDefault(x => x.Id == Id);
                if (obj.Status != -1)
                {
                    obj.Status = -1;
                    obj.NeedApproval = true;
                    obj.Category.TotalClassifiedAdsCount--;
                    obj.SubCategory.ClassifiedAdsCount--;
                    CurrentDbContext.SaveChanges();
                    // Remove old Lucene
                    LuceneSearch.ClearLuceneIndexRecord(obj.Id, obj.Photos);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Close a classified ad report
        public bool AdminCloseReport(int repId)
        {
            // get existing
            try
            {
                var obj = CurrentDbContext.ReportDB.SingleOrDefault(x => x.Id == repId);
                obj.Status = 1;
                CurrentDbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void AdminRoleChange(string userEmail, string oldRole, string newRole)
        {
            if (oldRole == newRole) return;
            if (oldRole.Contains("User") && newRole.Contains("Premium"))
            {
                AdminAddPremiumRole(userEmail);

            }
            else if (oldRole.Contains("Premium") && newRole.Contains("User"))
            {
                AdminRemovePremiumRole(userEmail);
            }
            else
            {
                // 1. Get user
                var user = UserManager.FindByEmailWithAds(userEmail);
                if (user == null) return;
                UserManager.AccountManager.RemoveFromRole(user.Id, oldRole);
                UserManager.AccountManager.AddToRole(user.Id, (newRole == "Unbanned") ? "User" : newRole);

                if (newRole == "Banned" || newRole == "Unbanned")
                {
                    foreach (var ad in user.ClassifiedAds)
                    {
                        foreach (var rep in ad.Reports)
                        {
                            rep.Status = (newRole == "Banned" ? 1 : (newRole == "Unbanned" ? 0 : 0));
                        }
                        if (newRole == "Banned" && ad.Status == 1)
                        {
                            ad.Category.TotalClassifiedAdsCount--;
                            ad.SubCategory.ClassifiedAdsCount--;
                            // Remove from Lucene
                            LuceneSearch.ClearLuceneIndexRecord(ad.Id, ad.Photos);
                        }
                        else if (newRole == "Unbanned")
                        {
                            ad.Category.TotalClassifiedAdsCount++;
                            ad.SubCategory.ClassifiedAdsCount++;
                            // Add to Lucene
                            LuceneSearch.AddUpdateLuceneIndex(ad);
                        }
                        ad.Status = (newRole == "Banned" ? 1 : (newRole == "Unbanned" ? 0 : 0));
                    }
                }
                CurrentDbContext.SaveChanges();
            }
        }

        public void AdminAddPremiumRole(string userEmail)
        {
            // 1. Get user and add to 'Premium' role
            var user = UserManager.AccountManager.FindByEmail(userEmail);
            UserManager.AccountManager.AddToRole(user.Id, "Premium");

            var dat = CurrentDbContext.PremUserDataDB.SingleOrDefault(x => x.UserProfileId == user.Id);
            if (dat == null)
            {
                var init = new PremiumUserData()
                {
                    UserProfileId = user.Id,
                    PremiumUserInfos = new List<PremiumUserInfo>(),
                    PremiumUserPhotos = new List<PremiumUserPhoto>(),
                    UserReviews = new List<PremiumUserReview>()
                };
            }
            else
            {
                user.PremiumUserData = dat;
            }
            CurrentDbContext.SaveChanges();
        }

        public void AdminRemovePremiumRole(string userEmail)
        {
            // 1. Get user and remove from 'Premium' role
            var user = UserManager.AccountManager.FindByEmail(userEmail);
            UserManager.AccountManager.RemoveFromRole(user.Id, "Premium");

            var dat = CurrentDbContext.PremUserDataDB.SingleOrDefault(x => x.UserProfileId == user.Id);
            user.PremiumUserData = null;
            CurrentDbContext.SaveChanges();
        }

        public AdminMemberDetails AdminMemberDetails(string id)
        {
            // 1. Get user            
            var userinfo = UserManager.AccountManager.Users
                .Include(u => u.ClassifiedAds)
                .Include(u => u.ClassifiedAds.Select(x => x.Reports))
                .ProjectTo<AdminMemberDetails>()
                .FirstOrDefault(u => u.UserId == id);
            if(userinfo != null)
            {
                userinfo.IsUser = UserManager.AccountManager.IsInRole(userinfo.UserId, "User");
                return userinfo;
            }
            return null;
        }

        public IEnumerable<GenericMessageQueue> AdminGetMessageQueue()
        {
            var obj = CurrentDbContext.GenMessageDB;
            return (obj == null) ? new List<GenericMessageQueue>() : Mapper.Map<IEnumerable<GenericMessageQueue>>(obj);
        }

        // Close a user posted message
        public bool AdminMessageClose(int msgId)
        {
            // get existing
            try
            {
                var obj = CurrentDbContext.GenMessageDB.SingleOrDefault(x => x.Id == msgId);
                if (obj == null) return false;
                obj.Status = 1;
                CurrentDbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Delete a user posted message
        public bool AdminMessageDelete(int msgId)
        {
            // get existing
            try
            {
                var obj = CurrentDbContext.GenMessageDB.SingleOrDefault(x => x.Id == msgId);
                if (obj == null) return false;
                CurrentDbContext.GenMessageDB.Remove(obj);
                CurrentDbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AdminDeleteReportAd(int id)
        {
            try
            {
                var obj = CurrentDbContext.ReportDB.Include("ClassifiedAd.Reports").SingleOrDefault(r => r.Id == id && r.Status == 1);
                if (obj == null) return false;
                obj.ClassifiedAd.Reports.Remove(obj);
                obj.ClassifiedAd = null;
                CurrentDbContext.ReportDB.Remove(obj);
                CurrentDbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void AdminUpdateLuceneSearchEngine()
        {
            IEnumerable<ClassifiedAdLucene> ads = CurrentDbContext.ClassifiedDB.ProjectTo<ClassifiedAdLucene>().Where(x => x.Status == 0 || x.Status == 1 || x.Status == 2).ToList();

            if (ads != null && ads.Count() > 0)
            {
                LuceneSearch.ClearAllLuceneIndexRecords();
                LuceneSearch.AdminCreateLuceneIndex(ads);
            }
        }

        /// <summary>
        /// Delete all closed ad reports
        /// </summary>
        /// <returns></returns>
        public bool AdminDeleteAllClosedReportAds()
        {
            try
            {
                var objs = CurrentDbContext.ReportDB.Include("ClassifiedAd.Reports").Where(r => r.Status == 1);
                if (objs == null) return false;
                foreach (var obj in objs)
                {
                    obj.ClassifiedAd.Reports.Remove(obj);
                    obj.ClassifiedAd = null;
                    CurrentDbContext.ReportDB.Remove(obj);
                }
                CurrentDbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Edit user ads
        /// </summary>
        /// <param name="editItem"></param>
        /// <param name="Server"></param>
        /// <returns></returns>
        public ClassifiedAdBase AdminClassifiedAdEdit(AdminClassifiedAdEdit editItem)
        {
            // 1. Ensure user credibility
            // 2. Attempt to fetch obj
            // 3. Set last edited time
            // 4. Pull existing item
            // 5. Compare photos for changes and add/remove
            // 6. Save changes                       

            var o = CurrentDbContext.ClassifiedDB.Include("Poster").SingleOrDefault(n => n.StringId == editItem.StringId);

            if (o == null)
            {
                return null;
            }
            else
            {
                // format adinfo
                var format_EngineSize = o.AdInfo.SingleOrDefault(x => x.Name.Equals("Engine Size"));
                if (format_EngineSize != null)
                    if (!string.IsNullOrEmpty(format_EngineSize.Description))
                        format_EngineSize.Description.Replace(" ", "");

                o.EditTimeStamp = DateTime.Now;

                if (o.ContactPrivacy != editItem.ContactPrivacy)
                {
                    o.ContactPrivacy = editItem.ContactPrivacy;
                }

                /// 
                /// Upadate Country/Region
                if (o.Country.Id != editItem.CountryId)
                {
                    var country = CurrentDbContext.CountryDB.Include("Regions").Include("Regions.ClassifiedAds").SingleOrDefault(a => a.Id == editItem.CountryId);
                    o.Country = country;
                }

                if (editItem.SubCategoryName.Equals("Lost Pet"))
                {
                    editItem.PriceInfo = editItem.Price = "Please Contact";
                }

                if (editItem.Price == "Please Contact" && editItem.PriceInfo != "Please Contact")
                    editItem.PriceInfo = "Please Contact";

                // update price
                var pri = Convert.ToInt32(editItem.Price.Equals("Please Contact") ? "0" : editItem.Price.Replace(",", ""));
                if (o.Price != pri)
                    o.Price = pri;

                // update desc
                if (o.Description != editItem.Description)
                    o.Description = editItem.Description;

                // update title
                if (o.Title != editItem.Title)
                    o.Title = editItem.Title;

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
                            if (!ai.Name.Equals("Age") && !ai.Name.Equals("Year"))
                            {
                                var currentai = o.AdInfo.SingleOrDefault(x => x.Name.Equals(ai.Name));
                                if (currentai.Description != null)
                                {
                                    if (!currentai.Description.Equals(ai.Description))
                                    {
                                        currentai.Description = ai.Description;
                                    }
                                }
                                else
                                {
                                    currentai.Description = ai.Description;
                                }
                            }
                        }
                    }
                }

                if (!editItem.AdType.Equals(o.AdType))
                    o.AdType = editItem.AdType;

                if (!editItem.AdContactName.Equals(o.AdContactName))
                    o.AdContactName = editItem.AdContactName;

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

                if (o.Photos != editItem.Photos)
                {
                    var oldp = o.Photos.ToList();
                    LuceneSearch.ClearLuceneIndexRecord(o.Id, oldp);
                    CurrentDbContext.AdPhotosDB.RemoveRange(oldp);
                    foreach (var newpho in Mapper.Map<IEnumerable<ClassifiedAdPhoto>>(editItem.Photos))
                    {
                        o.Photos.Add(newpho);
                    }
                }
                else
                {
                    LuceneSearch.ClearLuceneIndexRecord(o.Id);
                }

                CurrentDbContext.SaveChanges();
                PhotoFileManager.DeleteTempPhotos(o.Id, o.StringId);
                CacheHelper.RemoveFromCache(string.Format("ld-{0}", o.Id));
                // Add to Lucene

                LuceneSearch.AddUpdateLuceneIndex(o);

                return Mapper.Map<ClassifiedAdBase>(o);
            }
        }

        public void AdminCountAds()
        {
            var i = CurrentDbContext.SubCategoryDB.Include("ClassifiedAds");

            foreach (var x in i)
            {
                x.ClassifiedAdsCount = x.ClassifiedAds.Where(u => u.Status == 0).Count();
            }
            CurrentDbContext.SaveChanges();
            var a = CurrentDbContext.CategoryDB.Include("SubCategories");
            foreach (var x in a)
            {
                x.TotalClassifiedAdsCount = 0;
                foreach (var z in x.SubCategories)
                {
                    x.TotalClassifiedAdsCount += z.ClassifiedAdsCount;
                }
            }
            CurrentDbContext.SaveChanges();
        }

        public void AdQueueApproveAd(int adId)
        {
            var ad = CurrentDbContext.ClassifiedDB
                .Include("Poster")
                .SingleOrDefault(x => x.Id == adId);
            if (ad != null)
            {
                ad.Status = 0;
                ad.NeedApproval = false;
                CurrentDbContext.SaveChanges();
                // Add to Lucene
                LuceneSearch.AddUpdateLuceneIndex(ad);
            }
        }

        public bool AdQueueRemoveAd(int Id)
        {
            return ClassifiedManager.RemoveClassifiedAd(Id);
        }

        public void AdQueueUpdateAd(int adId)
        {
            var ad = CurrentDbContext.ClassifiedDB
                        .Include("Poster")
                        .SingleOrDefault(x => x.Id == adId);
            if (ad != null)
            {
                ad.NeedApproval = false;
                if (ad.Status == 1 || ad.Status == 2)
                {
                    var dtn = DateTime.Now;
                    ad.ExpiryTimeStamp = dtn.AddMonths(1);
                    HangfireManager.ScheduleRemoval(ad.Id, dtn, ad.ExpiryTimeStamp.Value);
                }
                CurrentDbContext.SaveChanges();
                LuceneSearch.ClearLuceneIndexRecord(ad.Id, ad.Photos);
                // Update Lucene
                LuceneSearch.AddUpdateLuceneIndex(ad);
            }
        }
        // get user posted ads
        public IPagedList<ClassifiedAdQueueList> GetAdQueue(int pageNumber = 1)
        {
            var val = CurrentDbContext.ClassifiedDB.Where(a => a.NeedApproval && (a.Status == -2 || a.Status == 1 || a.Status == 2 || a.Status == 3)).OrderByDescending(x => x.TimeStamp).ProjectTo<ClassifiedAdQueueList>().ToList();
            return val != null ? new PagedList<ClassifiedAdQueueList>(val, pageNumber, RecordsPerPage.Records) : new List<ClassifiedAdQueueList>().ToPagedList(pageNumber, RecordsPerPage.Records);
        }

        // Get edit ad
        public AdminClassifiedAdEditForm AdminGetClassifiedAdWithAll(int Id)
        {
            var val = CurrentDbContext.ClassifiedDB.FirstOrDefault(a => a.Id == Id);
            return val != null ? Mapper.Map<AdminClassifiedAdEditForm>(val) : null;
        }

        public void AdminAdCategoryChange(int adId, int subCatId)
        {
            var o = CurrentDbContext.ClassifiedDB
                .Include("Category")
                .Include("SubCategory")
                .Include("AdInfo")
                .Include("Country")
                .Include("Region")
                .SingleOrDefault(a => a.Id == adId);

            if (o == null) return;

            if (o.SubCategory.Id == subCatId) return;

            // remove all associated ad info
            var ail = o.AdInfo.ToList();
            foreach (var ai in ail)
            {
                ai.ClassifiedAd = null;
                o.AdInfo.Remove(ai);
                CurrentDbContext.InfosDB.Remove(ai);
            }
            // change associated category and subcategory
            // get the cat then decriment counter and remove ad
            var tempcat = CurrentDbContext.CategoryDB.SingleOrDefault(a => a.Id == o.Category.Id);
            o.Category = null;
            tempcat.TotalClassifiedAdsCount--;

            var tempsubcat = CurrentDbContext.SubCategoryDB.Include("ClassifiedAds").SingleOrDefault(a => a.StringId == o.SubCategory.StringId);
            tempsubcat.ClassifiedAdsCount--;
            o.SubCategory = null;
            tempsubcat.ClassifiedAds.Remove(o);

            // add to new subcategory
            var newtempsub = CurrentDbContext.SubCategoryDB.Include("Category").Include("ClassifiedAds").SingleOrDefault(a => a.Id == subCatId);
            o.SubCategory = newtempsub;
            newtempsub.ClassifiedAds.Add(o);
            newtempsub.ClassifiedAdsCount++;

            // add to new category
            var newtempcat = newtempsub.Category;
            o.Category = newtempcat;
            newtempcat.TotalClassifiedAdsCount++;
            
            // add associated info
            var getinfo = GetOneTemplateInfo(newtempcat.Id, newtempsub.StringId);
            if (getinfo != null)
            {
                if (getinfo.Count() > 0)
                {
                    foreach (var nai in getinfo)
                        o.AdInfo.Add(new Info() { Name = nai.Name, ClassifiedAd = o });
                }
            }

            CurrentDbContext.SaveChanges();
            // Remove old Lucene
            LuceneSearch.ClearLuceneIndexRecord(o.Id);
            // Add to Lucene
            LuceneSearch.AddUpdateLuceneIndex(o);
        }

        public bool AdminRemoveUser(string userId)
        {
            // Remove user ads
            if (AdminRemoveUserAds(userId))
            {
                // Remove user
                var userroles = UserManager.AccountManager.GetRoles(userId);
                foreach (var role in userroles.ToList())
                    UserManager.AccountManager.RemoveFromRole(userId, role);
                UserManager.AccountManager.Delete(UserManager.AccountManager.FindById(userId));

                CurrentDbContext.SaveChanges();
                return true;
            }
            return false;
        }

        public bool AdminRemoveUserAds(string userId)
        {
            // 1. Get user
            var user = CurrentDbContext.Users
                .Include("ClassifiedAds").Select(u => new { uid = u.Id, adids = u.ClassifiedAds.Select(s => s.Id) })
                .FirstOrDefault(x => x.uid == userId);
            // 2. Remove ads
            foreach (var sti in user.adids)
            {
                // remove from cart
                if (PromoManager.IsAdInCart(sti)) if (!PromoManager.RemoveFromShoppingCart(sti)) return false;
                if (PromoManager.IsAdPromoted(user.uid, sti)) if (!PromoManager.RemoveFromPromotedList(user.uid, sti)) return false;
                if (!ClassifiedManager.RemoveClassifiedAd(sti))
                {
                    return false;
                }
            }
            return true;
        }

        public override void Dispose()
        {
            if(_classifiedManager != null)
            {
                _classifiedManager.Dispose();
                _classifiedManager = null;
            }
            if(_promoManager != null)
            {
                _promoManager.Dispose();
                _promoManager = null;
            }
            base.Dispose();
        }
    }
}