using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Trinbago_MVC5.Models;
using static Trinbago_MVC5.Models.MigrationDesignModels;

namespace Trinbago_MVC5.Controllers
{
    public class MigrateDataBase
    {
        protected ApplicationUserManager _userManager;
        protected ApplicationRoleManager _roleManager;

        internal ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        internal ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.Current.Request.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public void fix()
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                using (MigrationDbContext migrateDB = new MigrationDbContext())
                {
                    // ad info
                    if (!newthreadcontext.TemplateDB.Any())
                    {
                        var adinfo = migrateDB.TemplatesDB.Include("RecommendedInfo");
                        foreach (var i in adinfo)
                        {
                            var t = newthreadcontext.TemplateDB.Add(new AdInfoTemplate() { TemplateName = i.TemplateName, RecommendedInfo = new List<AdInfoString>() });

                            foreach (var x in i.RecommendedInfo)
                            {
                                t.RecommendedInfo.Add(new AdInfoString() { Name = x.Name });
                            }
                        }
                        newthreadcontext.SaveChanges();
                    }
                }
            }
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                using (MigrationDbContext migrateDB = new MigrationDbContext())
                {
                    if (!newthreadcontext.MiscInfoDB.Any())
                    {
                        var make = migrateDB.MakeDB.ToList();
                        foreach (var i in make)
                        {
                            newthreadcontext.MiscInfoDB.Add(new MiscInfo() { Value = i.Name, Descriptor = "VehicleMake" });
                        }

                        var adtype = migrateDB.TypesDB.ToList();
                        foreach (var i in adtype)
                        {
                            newthreadcontext.MiscInfoDB.Add(new MiscInfo() { Value = i.Value, Name = i.Name, Descriptor = "AdType" });
                        }

                        var bodytype = migrateDB.BodyTypeDB.ToList();
                        foreach (var i in bodytype)
                        {
                            newthreadcontext.MiscInfoDB.Add(new MiscInfo() { Value = i.Type, Descriptor = "VehicleBodyType" });
                        }

                        var transmission = migrateDB.TransmissionDB.ToList();
                        foreach (var i in transmission)
                        {
                            newthreadcontext.MiscInfoDB.Add(new MiscInfo() { Value = i.Name, Descriptor = "VehicleTransmission" });
                        }

                        var drivetrain = migrateDB.DrivetrainDB.ToList();
                        foreach (var i in drivetrain)
                        {
                            newthreadcontext.MiscInfoDB.Add(new MiscInfo() { Value = i.Name, Descriptor = "VehicleDrivetrain" });
                        }

                        var condition = migrateDB.ConditionDB.ToList();
                        foreach (var i in condition)
                        {
                            newthreadcontext.MiscInfoDB.Add(new MiscInfo() { Value = i.Name, Descriptor = "VehicleCondition" });
                        }

                        var fueltype = migrateDB.FuelDB.ToList();
                        foreach (var i in fueltype)
                        {
                            newthreadcontext.MiscInfoDB.Add(new MiscInfo() { Value = i.Name, Descriptor = "VehicleFuelType" });
                        }

                        var priceinfo = migrateDB.PriceInfoDB.ToList();
                        foreach (var i in priceinfo)
                        {
                            newthreadcontext.MiscInfoDB.Add(new MiscInfo() { Value = i.Name, Descriptor = "PriceInfo" });
                        }
                        newthreadcontext.SaveChanges();

                    }
                }
            }
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                using (MigrationDbContext migrateDB = new MigrationDbContext())
                {
                    if (!newthreadcontext.CategoryDB.Any())
                    {
                        var cat = migrateDB.CategoryDB.Include("SubCategories.AdInfoTemplate").ToList();
                        foreach (var i in cat)
                        {
                            var currentcat = newthreadcontext.CategoryDB.Add(new Category() { Name = i.Name, TotalClassifiedAdsCount = i.TotalClassifiedAdsCount });
                            List<SubCategory> scl = new List<SubCategory>();
                            foreach (var j in i.SubCategories)
                            {
                                if (j.AdInfoTemplate != null)
                                {
                                    var template = newthreadcontext.TemplateDB.SingleOrDefault(x => x.TemplateName.Equals(j.AdInfoTemplate.TemplateName));
                                    scl.Add(new SubCategory() { Category = currentcat, Name = j.Name, stringId = j.stringId, AdInfoTemplate = template });
                                }
                                else
                                {
                                    scl.Add(new SubCategory() { Category = currentcat, Name = j.Name, stringId = j.stringId });
                                }
                            }
                            newthreadcontext.SubCategoryDB.AddRange(scl);
                        }
                        newthreadcontext.SaveChanges();
                    }
                }
            }

            if (!RoleManager.RoleExists("Admin"))
                RoleManager.Create(new IdentityRole() { Name = "Admin" });

            if (!RoleManager.RoleExists("Banned"))
                RoleManager.Create(new IdentityRole() { Name = "Banned" });

            if (!RoleManager.RoleExists("Moderator"))
                RoleManager.Create(new IdentityRole() { Name = "Moderator" });

            if (!RoleManager.RoleExists("Premium"))
                RoleManager.Create(new IdentityRole() { Name = "Premium" });

            if (!RoleManager.RoleExists("User"))
                RoleManager.Create(new IdentityRole() { Name = "User" });
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                using (MigrationDbContext migrateDB = new MigrationDbContext())
                {
                    if (!newthreadcontext.CountryDB.Any())
                    {
                        var trinidad = newthreadcontext.CountryDB.Add(new Country() { Name = "Trinidad" });
                        var tobago = newthreadcontext.CountryDB.Add(new Country() { Name = "Tobago" });
                        newthreadcontext.SaveChanges();
                    }

                    if (!newthreadcontext.RegionDB.Any())
                    {
                        var trinidad = newthreadcontext.CountryDB.SingleOrDefault(x => x.Name.Equals("Trinidad"));
                        List<Region> trinR = new List<Region>() {
                            new Region() { Country = trinidad, Name = "Port of Spain"},
                            new Region() { Country = trinidad, Name = "San Fernando"},
                            new Region() { Country = trinidad, Name = "Marabella"},
                            new Region() { Country = trinidad, Name = "Chaguanas"},
                            new Region() { Country = trinidad, Name = "Cunupia"},
                            new Region() { Country = trinidad, Name = "Endeavour"},
                            new Region() { Country = trinidad, Name = "Felicity"},
                            new Region() { Country = trinidad, Name = "Montrose"},
                            new Region() { Country = trinidad, Name = "Arima"},
                            new Region() { Country = trinidad, Name = "Point Fortin"},
                            new Region() { Country = trinidad, Name = "Guapo"},
                            new Region() { Country = trinidad, Name = "Techier"},
                            new Region() { Country = trinidad, Name = "Claxton Bay"},
                            new Region() { Country = trinidad, Name = "Diego Martin"},
                            new Region() { Country = trinidad, Name = "Maraval"},
                            new Region() { Country = trinidad, Name = "Westmoorings"},
                            new Region() { Country = trinidad, Name = "Penal"},
                            new Region() { Country = trinidad, Name = "Debe"},
                            new Region() { Country = trinidad, Name = "Moruga"},
                            new Region() { Country = trinidad, Name = "Princes Town"},
                            new Region() { Country = trinidad, Name = "Mayaro"},
                            new Region() { Country = trinidad, Name = "Rio Claro"},
                            new Region() { Country = trinidad, Name = "Guayaguayare"},
                            new Region() { Country = trinidad, Name = "Barataria"},
                            new Region() { Country = trinidad, Name = "Laventille"},
                            new Region() { Country = trinidad, Name = "Morvant"},
                            new Region() { Country = trinidad, Name = "St. Joseph"},
                            new Region() { Country = trinidad, Name = "San Juan"},
                            new Region() { Country = trinidad, Name = "Guaico"},
                            new Region() { Country = trinidad, Name = "Sangre Grande"},
                            new Region() { Country = trinidad, Name = "Toco"},
                            new Region() { Country = trinidad, Name = "Valencia"},
                            new Region() { Country = trinidad, Name = "Cedros"},
                            new Region() { Country = trinidad, Name = "Fyzabad"},
                            new Region() { Country = trinidad, Name = "La Brea"},
                            new Region() { Country = trinidad, Name = "Santa Flora"},
                            new Region() { Country = trinidad, Name = "Siparia"},
                            new Region() { Country = trinidad, Name = "Arouca"},
                            new Region() { Country = trinidad, Name = "Curepe"},
                            new Region() { Country = trinidad, Name = "Piarco"},
                            new Region() { Country = trinidad, Name = "St Augustine"},
                            new Region() { Country = trinidad, Name = "Trincity"},
                            new Region() { Country = trinidad, Name = "Tunapuna"},
                            new Region() { Country = trinidad, Name = "Couva" },
                            new Region() { Country = trinidad, Name = "Talparo" },
                            new Region() { Country = trinidad, Name = "Tabaquite" },
                            new Region() { Country = trinidad, Name = "Point Lisas" },
                            new Region() { Country = trinidad, Name = "Caroni" },
                            new Region() { Country = trinidad, Name = "Santa Cruz" },
                            new Region() { Country = trinidad, Name = "Freeport" }

                        };
                        trinidad.Regions = trinR;
                        trinidad.RegionCount = trinR.Count;
                        newthreadcontext.SaveChanges();

                        var tobago = newthreadcontext.CountryDB.SingleOrDefault(x => x.Name.Equals("Tobago"));
                        List<Region> tobaR = new List<Region>(){
                            new Region() { Country = tobago, Name = "Charlotteville"},
                            new Region() { Country = tobago, Name = "Roxborough"},
                            new Region() { Country = tobago, Name = "Scarborough"},
                            new Region() { Country = tobago, Name = "Canaan"},
                            new Region() { Country = tobago, Name = "Plymouth"},
                            new Region() { Country = tobago, Name = "Moriah"}
                        };
                        tobago.Regions = tobaR;
                        tobago.RegionCount = tobaR.Count;
                        newthreadcontext.SaveChanges();
                    }
                }
            }

            using (MigrationDbContext migrateDB = new MigrationDbContext())
            {
                if (!UserManager.Users.Any())
                {
                    var userwithad = migrateDB.UserProfiles.ToList();
                    foreach (var user in userwithad)
                    {
                        ApplicationUser newuser = new ApplicationUser() { Email = user.Email, UserName = !String.IsNullOrEmpty(user.UserName) ? user.Email.Split('@').First() : user.UserName, StringId = user.StringId, PhoneNumber = user.ContactNumber };
                        var result = UserManager.Create(newuser);
                        if (result.Succeeded)
                        {
                            string query = "SELECT green_kappakappa.webpages_Roles.RoleName FROM(green_kappakappa.webpages_Roles INNER JOIN green_kappakappa.webpages_UsersInRoles ON green_kappakappa.webpages_Roles.RoleId = green_kappakappa.webpages_UsersInRoles.RoleId) INNER JOIN UserProfile ON green_kappakappa.webpages_UsersInRoles.UserId = " + user.UserId;
                            var role = migrateDB.Database.SqlQuery<string>(query).ToList();
                            UserManager.AddToRole(newuser.Id, role.First());
                        }
                    }
                }
            }

            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                ICollection<ClassifiedAdOld> ads = new List<ClassifiedAdOld>();
                using (MigrationDbContext migrateDB = new MigrationDbContext())
                {
                    ads = migrateDB.ClassifiedDB.Include("UserCreator").Include("Country").Include("Region").Include("AdInfo").Include("AdPhotos").Include("Category").Include("SubCategory").Include("Reports").ToList();
                }

                if (!newthreadcontext.ClassifiedDB.Any())
                {
                    var config = new MapperConfiguration(r =>
                    {
                        r.CreateMap<CategoryOld, Category>();
                        r.CreateMap<SubCategoryOld, SubCategory>()
                        .ForMember(dest => dest.ClassifiedAds, opt => opt.Ignore());
                        r.CreateMap<InfoOld, Info>();
                        r.CreateMap<PhotoOld, Photo>();
                        r.CreateMap<AdPromotionOld, AdPromotion>();
                    });
                    IMapper mapper = config.CreateMapper();



                    foreach (var ad in ads)
                    {
                        var i = newthreadcontext.ClassifiedDB.Add(new ClassifiedAd());
                        i.AdType = ad.AdType;
                        i.StringId = ad.StringId;
                        i.ContactPrivacy = ad.ContactPrivacy;
                        i.Description = ad.Description;
                        i.EditCount = ad.EditCount;
                        i.EditTimeStamp = ad.EditTimeStamp;
                        i.FeaturedAdStatus = ad.FeaturedAdStatus;
                        i.HtmlFreeDescription = ad.HtmlFreeDescription;
                        i.MyProperty = ad.MyProperty;
                        i.Price = ad.Price;
                        i.PriceInfo = ad.PriceInfo;
                        i.TimeStamp = ad.TimeStamp;
                        i.Title = ad.Title;
                        i.UrgentAdStatus = ad.UrgentAdStatus;
                        i.UserContactEmail = ad.UserContactEmail;
                        i.UserContactName = ad.UserContactName;
                        i.UserContactPhone = ad.UserContactPhone;
                        i.UserContactPhone2 = ad.UserContactPhone2;
                        i.UserContactPhone3 = ad.UserContactPhone3;
                        i.Views = ad.Views;
                        var sc = newthreadcontext.SubCategoryDB.Include("Category").FirstOrDefault(x => x.stringId.Equals(ad.SubCategory.stringId));
                        i.SubCategory = sc;
                        i.Category = sc.Category;
                        var user = newthreadcontext.Users.FirstOrDefault(x => x.Email.Equals(ad.UserCreator.Email));
                        i.UserCreator = user;
                        var region = newthreadcontext.RegionDB.Include("Country").FirstOrDefault(x => ad.Region.Name.Contains(x.Name));
                        i.Region = region;
                        i.Country = region.Country;

                        foreach (var ap in ad.AdPhotos)
                        {
                            var pho = new Photo()
                            {
                                AdListThumbnail = ap.AdListThumbnail,
                                ClassifiedAd = i,
                                ContentType = ap.ContentType,
                                CountNum = ap.CountNum,
                                FileName = ap.FileName,
                                SetThumbnail = ap.SetThumbnail,
                                StringId = ap.StringId
                            };
                            pho.ClassifiedAd.AdPhotos.Add(pho);
                        }
                        foreach (var ai in ad.AdInfo)
                        {
                            var inf = new Info()
                            {
                                ClassifiedAd = i,
                                Description = ai.Description,
                                Name = ai.Name,
                                IntDescription = ai.IntDescription
                            };
                            inf.ClassifiedAd.AdInfo.Add(inf);
                        }
                        foreach (var rep in ad.Reports)
                        {
                            var crep = new ClassifiedAdReport()
                            {
                                ClassifiedAd = i,
                                CreatedDate = rep.CreatedDate,
                                OpenRequest = rep.OpenRequest,
                                ReasonDescription = rep.ReasonDescription,
                                ReasonTitle = rep.ReasonTitle,
                                ReportingUser = rep.ReportingUser,
                                Status = rep.Status
                            };
                            crep.ClassifiedAd.Reports.Add(crep);
                        }
                        if (ad.Status == 0)
                        {
                            i.SubCategory.ClassifiedAdsCount++;
                            i.Category.TotalClassifiedAdsCount++;
                        }
                        newthreadcontext.SaveChanges();

                    }
                }
            }

            using (MigrationDbContext migrateDB = new MigrationDbContext())
            {
                var query = "SELECT green_kappakappa.webpages_OAuthMembership.Provider, green_kappakappa.webpages_OAuthMembership.ProviderUserId, UserProfile.Email FROM UserProfile INNER JOIN green_kappakappa.webpages_OAuthMembership ON UserProfile.UserId = green_kappakappa.webpages_OAuthMembership.UserId";
                var logins = migrateDB.Database.SqlQuery<login>(query).ToList();

                foreach (var login in logins) {
                    var user = UserManager.FindByEmail(login.Email);
                    UserManager.AddLogin(user.Id, new UserLoginInfo(login.Provider, login.ProviderUserId));
                }
            }
        }
    }
}
public class login
{
    public string Provider { get; set; }
    public string ProviderUserId { get; set; }
    public string Email { get; set; }
}