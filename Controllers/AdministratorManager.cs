using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Controllers
{
    public class AdministratorManager : ManagerClassifiedAd
    {
        protected ApplicationRoleManager _roleManager;
 
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


        internal CountryBase CountryAdd(CountryAdd newItem)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var config = new MapperConfiguration(r => r.CreateMap<CountryAdd, Country>());
                IMapper mapper = config.CreateMapper();

                var dup = newthreadcontext.CountryDB.SingleOrDefault(a => a.Name.Contains(newItem.Name));

                // check for duplicate
                if (dup != null)
                    return null;

                var addedItem = newthreadcontext.CountryDB.Add(mapper.Map<Country>(newItem));

                newthreadcontext.SaveChanges();

                config = new MapperConfiguration(r => r.CreateMap<Country, CountryBase>());
                mapper = config.CreateMapper();

                return (addedItem == null) ? null : mapper.Map<CountryBase>(addedItem);
            }
        }

        internal RegionBase RegionAdd(RegionAdd newItem)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var config = new MapperConfiguration(r => r.CreateMap<RegionAdd, RegionBase>());
                IMapper mapper = config.CreateMapper();
                // get associated object
                var o = newthreadcontext.CountryDB.Include("Regions").SingleOrDefault(a => a.Id == newItem.CountryId);

                if (o == null)
                {
                    return (mapper.Map<RegionBase>(newItem));
                }

                // check for duplicate region
                var dup = o.Regions.SingleOrDefault(a => a.Name == newItem.Name);

                if (dup != null)
                {
                    return (mapper.Map<RegionBase>(newItem));
                }

                config = new MapperConfiguration(r => r.CreateMap<RegionAdd, Models.Region>());
                mapper = config.CreateMapper();

                var addedItem = newthreadcontext.RegionDB.Add(mapper.Map<Models.Region>(newItem));

                addedItem.Country = o;
                o.RegionCount++;

                newthreadcontext.SaveChanges();

                config = new MapperConfiguration(r => r.CreateMap<Models.Region, RegionBase>());
                mapper = config.CreateMapper();

                return (addedItem == null) ? null : mapper.Map<RegionBase>(addedItem);
            }
        }

        internal RegionBase RegionEdit(RegionEdit editItem)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.RegionDB.Include("Country").SingleOrDefault(r => r.Id == editItem.Id);

                if (o != null)
                {
                    o.Name = editItem.Name;

                    newthreadcontext.SaveChanges();
                }

                var config = new MapperConfiguration(r => r.CreateMap<RegionEdit, RegionBase>());
                IMapper mapper = config.CreateMapper();

                return (o == null) ? null : mapper.Map<RegionBase>(editItem);
            }
        }

        internal IEnumerable<UserProfileBase> MemberGetAll()
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {

                var o = newthreadcontext.Users;
                var config = new MapperConfiguration(r => r.CreateMap<ApplicationUser, UserProfileBase>());
                IMapper mapper = config.CreateMapper();

                return (o == null) ? null : mapper.Map<IEnumerable<UserProfileBase>>(o);
            }
        }

        internal IEnumerable<Category> CatGetAll()
        {
            IEnumerable<Category> var = new List<Category>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var = newthreadcontext.CategoryDB.ToList();
            }
            return (var == null) ? null : var;
        }

        internal Category CatGetOneById(int id)
        {
            Category var = new Category();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var = newthreadcontext.CategoryDB.Include("SubCategories").SingleOrDefault(c => c.Id == id);
                var last = var.SubCategories.SingleOrDefault(x => x.Name.Equals("Other"));
                var.SubCategories.Remove(last);
                var.SubCategories = var.SubCategories.OrderBy(a => a.Name).ToList();
                var.SubCategories.Add(last);
            }
            return (var == null) ? null : var;
        }

        internal Category CategoryEdit(Category item)
        {
            Category var = new Category();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var = newthreadcontext.CategoryDB.SingleOrDefault(c => c.Id == item.Id);
                var.Name = item.Name;
                newthreadcontext.SaveChanges();
            }
            return var;
        }

        internal SubCategory SubCatGetOneById(int id)
        {
            SubCategory var = new SubCategory();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var = newthreadcontext.SubCategoryDB.SingleOrDefault(s => s.Id == id);
            }
            return var;
        }

        internal SubCategory SubCatEdit(SubCategoryEdit item)
        {
            SubCategory var = new SubCategory();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var = newthreadcontext.SubCategoryDB.SingleOrDefault(s => s.Id == item.Id);
                var.Name = item.Name;
                newthreadcontext.SaveChanges();
            }
            return var;
        }

        internal Category CategoryAdd(Category newitem)
        {
            Category addeditem = new Category();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                addeditem = newthreadcontext.CategoryDB.Add(newitem);
                addeditem.SubCategories = null;
                newthreadcontext.SaveChanges();
            }
            return addeditem;
        }

        internal SubCategory SubCategoryAdd(SubCategoryAdd newitem)
        {
            SubCategory addeditem = new SubCategory();
            addeditem.stringId = GetGen();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var ao = newthreadcontext.CategoryDB.Include("SubCategories").SingleOrDefault(x => x.Id == newitem.CategoryId);
                ao.SubCategories.ToList().RemoveAll(x => x == null);
                addeditem.Name = newitem.Name;
                addeditem = newthreadcontext.SubCategoryDB.Add(addeditem);
                addeditem.Category = ao;
                newthreadcontext.SaveChanges();
            }
            return addeditem;
        }

        /*internal bool LoadDbData()
        {
            bool done = false;

            if (!Roles.RoleExists("Admin"))
                Roles.CreateRole("Admin");
            if (!Roles.RoleExists("Moderator"))
                Roles.CreateRole("Moderator");
            if (!Roles.RoleExists("User"))
                Roles.CreateRole("User");
            if (!Roles.RoleExists("Banned"))
                Roles.CreateRole("Banned");
            if (!Roles.RoleExists("Premium"))
                Roles.CreateRole("Premium");


            if (ds.TypesDB.Count() == 0)
            {
                ICollection<AdType> t1 = new List<AdType>() { 
                    new AdType() { Name = "All Ads", Value = "ALL"},
                    new AdType() { Name = "Offering", Value = "SELL" },
                    new AdType() { Name = "Looking For", Value = "WANT" },
                    new AdType() { Name = "Trading", Value = "TRADE" }};
                ds.TypesDB.AddRange(t1);
                ds.SaveChanges();
            }

            if (ds.PriceInfoDB.Count() == 0)
            {
                ICollection<PriceInfo> p = new List<PriceInfo>() { 
                    new PriceInfo() { Name = "None", Id = 1},
                    new PriceInfo() { Name = "Please Contact", Id = 2},
                    new PriceInfo() { Name = "Negotiable", Id = 3 },
                    new PriceInfo() { Name = "Non-Negotiable", Id = 4 }
                };
                ds.PriceInfoDB.AddRange(p);
                ds.SaveChanges();
            }

            if (ds.CategoryDB.Count() == 0)
            {
                ICollection<Category> c = new List<Category>() {
                    new Category() { Name = "Vehicles" },
                    new Category() { Name = "Real Estate" },
                    new Category() { Name = "Pets" },
                    new Category() { Name = "Business Services" },
                    new Category() { Name = "Buy and Sell" },
                    new Category() { Name = "Jobs"}
                };
                ds.CategoryDB.AddRange(c);
                ds.SaveChanges();
            }

            if (ds.SubCategoryDB.Count() == 0)
            {
                var ve = ds.CategoryDB.SingleOrDefault(v => v.Name == "Vehicles");
                ICollection<SubCategory> one = new List<SubCategory>() { 
                    new SubCategory() { Name = "Automotive Parts", stringId = GetGen(), Category = ve },
                    new SubCategory() { Name = "Automotive Services", stringId = GetGen(), Category = ve },
                    new SubCategory() { Name = "Boat Parts", stringId = GetGen(), Category = ve },
                    new SubCategory() { Name = "Boat Services", stringId = GetGen(), Category = ve },
                    new SubCategory() { Name = "Boats", stringId = GetGen(), Category = ve },
                    new SubCategory() { Name = "Cars/Trucks", stringId = GetGen(), Category = ve },
                    new SubCategory() { Name = "Motorcycles/ATVs", stringId = GetGen(), Category = ve },
                    new SubCategory() { Name = "Other", stringId = GetGen(), Category = ve }
                };
                ds.SubCategoryDB.AddRange(one);
                ds.SaveChanges();

                var re = ds.CategoryDB.SingleOrDefault(r => r.Name == "Real Estate");
                ICollection<SubCategory> two = new List<SubCategory>() {
                    new SubCategory() { Name = "Apartments/Condos Rental", stringId = GetGen(), Category = re },
                    new SubCategory() { Name = "House Rental", stringId = GetGen(), Category = re },
                    new SubCategory() { Name = "Room Rental", stringId = GetGen(), Category = re },
                    new SubCategory() { Name = "Commercial Office Space", stringId = GetGen(), Category = re },
                    new SubCategory() { Name = "Apartments/Condos For Sale", stringId = GetGen(), Category = re},
                    new SubCategory() { Name = "House For Sale", stringId = GetGen(), Category = re },
                    new SubCategory() { Name = "Other", stringId = GetGen(), Category = re}
                };
                ds.SubCategoryDB.AddRange(two);
                ds.SaveChanges();

                var pe = ds.CategoryDB.SingleOrDefault(p => p.Name == "Pets");
                ICollection<SubCategory> three = new List<SubCategory>(){
                    new SubCategory() { Name = "Pet Services", stringId = GetGen(), Category = pe},
                    new SubCategory() { Name = "Pet Hub", stringId = GetGen(), Category = pe},
                    new SubCategory() { Name = "Pet Accessories", stringId = GetGen(), Category = pe},
                    new SubCategory() { Name = "Pet Adoption", stringId = GetGen(), Category = pe},
                    new SubCategory() { Name = "Lost Pet", stringId = GetGen(), Category = pe},
                    new SubCategory() { Name = "Other", stringId = GetGen(), Category = pe}
                };
                ds.SubCategoryDB.AddRange(three);
                ds.SaveChanges();

                var se = ds.CategoryDB.SingleOrDefault(s => s.Name == "Business Services");
                ICollection<SubCategory> four = new List<SubCategory>(){
                    new SubCategory() { Name = "Child Care", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Cleaning", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Entertainment", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Catering", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Beauty", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Lessons", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Plumbing", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Carpentry", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Welding", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Tiling", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Wedding Planning", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Electrical", stringId = GetGen(), Category = se},
                    new SubCategory() { Name = "Other", stringId = GetGen(), Category = se}
                };
                ds.SubCategoryDB.AddRange(four);
                ds.SaveChanges();

                var ge = ds.CategoryDB.SingleOrDefault(g => g.Name == "Buy and Sell");
                ICollection<SubCategory> five = new List<SubCategory>(){
                    new SubCategory() { Name = "Electronics", stringId = GetGen(), Category = ge},
                    new SubCategory() { Name = "Computers/Laptops", stringId = GetGen(), Category = ge},
                    new SubCategory() { Name = "Computer/Laptop Accessories", stringId = GetGen(), Category = ge},
                    new SubCategory() { Name = "Appliances", stringId = GetGen(), Category = ge},
                    new SubCategory() { Name = "Apparel", stringId = GetGen(), Category = ge},
                    new SubCategory() { Name = "Appliance Parts", stringId = GetGen(), Category = ge},
                    new SubCategory() { Name = "Phones", stringId = GetGen(), Category = ge},
                    new SubCategory() { Name = "Phone Accessories", stringId = GetGen(), Category = ge},
                    new SubCategory() { Name = "Cameras", stringId = GetGen(), Category = ge},
                    new SubCategory() { Name = "Camera Accessories", stringId = GetGen(), Category = ge},
                    new SubCategory() { Name = "Other", stringId = GetGen(), Category = ge}
                };
                ds.SubCategoryDB.AddRange(five);
                ds.SaveChanges();

                var jo = ds.CategoryDB.SingleOrDefault(j => j.Name == "Jobs");
                ICollection<SubCategory> six = new List<SubCategory>(){
                    new SubCategory() { Name = "Accounting", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Actuary", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Administration & Office Support", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Agriculture, Animals & Conservation", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Architecture & Design", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Banking & Financial Services", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Communications, Advertising, Arts & Media", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Community Services", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Construction", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Customer Service & Call Centre", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Defence & Protective Services", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Education & Training", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Engineering", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Executive & General Management", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Health & Medical", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Hospitality & Tourism", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Human Resources & Recruitment", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Information & Communication Technology(ICT)", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Insurance & Superannuation", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Legal", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Manufacturing", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Mining & Energy", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Real Estate & Property", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Retail", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Sales", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Science", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Sport & Recreation", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Trades & Services", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Transport & Logistics", stringId = GetGen(), Category = jo},
                    new SubCategory() { Name = "Other", stringId = GetGen(), Category = jo}
                };
                ds.SubCategoryDB.AddRange(six);
                ds.SaveChanges();

            }

            if (ds.CountryDB.Count() == 0)
            {
                ICollection<Country> cun = new List<Country>() {
                    new Country() { Name = "Trinidad"},
                    new Country() { Name = "Tobago"}
                };
                ds.CountryDB.AddRange(cun);
                ds.SaveChanges();
            }

            if (ds.RegionDB.Count() == 0)
            {
                var tr = ds.CountryDB.SingleOrDefault(tri => tri.Name == "Trinidad");
                ICollection<Trinbago_MVC5.Models.Region> trr = new List<Trinbago_MVC5.Models.Region>(){
                    new Trinbago_MVC5.Models.Region() { Name = "Diego Martin", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Port of Spain", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "San Juan", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Laventille", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Tunapuna-Piarco", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Arima", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Sangre Grande", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Chaguanas", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Couva", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Tabaquite", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Talparo", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Rio Claro", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Mayaro", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Princes Town", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Penal", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Debe", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Siparia", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "Point Fortin", Country = tr},
                    new Trinbago_MVC5.Models.Region() { Name = "San Fernando", Country = tr}
                };
                tr.RegionCount = trr.Count;
                ds.RegionDB.AddRange(trr);
                ds.SaveChanges();
                var to = ds.CountryDB.SingleOrDefault(tob => tob.Name == "Tobago");
                ICollection<Trinbago_MVC5.Models.Region> toba = new List<Trinbago_MVC5.Models.Region>(){
                    new Trinbago_MVC5.Models.Region() { Name = "Charlotteville", Country = to},
                    new Trinbago_MVC5.Models.Region() { Name = "Roxborough", Country = to},
                    new Trinbago_MVC5.Models.Region() { Name = "Scarborough", Country = to},
                    new Trinbago_MVC5.Models.Region() { Name = "Canaan", Country = to},
                    new Trinbago_MVC5.Models.Region() { Name = "Plymouth", Country = to},
                    new Trinbago_MVC5.Models.Region() { Name = "Moriah", Country = to}
                };
                to.RegionCount = toba.Count;
                ds.RegionDB.AddRange(toba);
                ds.SaveChanges();
            }

            if (ds.TemplatesDB.Count() == 0)
            {
                ICollection<AdInfoTemplate> temp = new List<AdInfoTemplate>(){
                    new AdInfoTemplate() { TemplateName = "CT" },
                    new AdInfoTemplate() { TemplateName = "MA" },
                    new AdInfoTemplate() { TemplateName = "RE" },
                    new AdInfoTemplate() { TemplateName = "RECOS"},
                    new AdInfoTemplate() { TemplateName = "JOB" },
                    new AdInfoTemplate() { TemplateName = "AMP" },
                    new AdInfoTemplate() { TemplateName = "PET" },
                    new AdInfoTemplate() { TemplateName = "PETSERV" }
                };
                ds.TemplatesDB.AddRange(temp);
                ds.SaveChanges();

                ICollection<AdInfoString> _t1 = new List<AdInfoString>() { 
                           new AdInfoString() { Name ="Make"},
                           new AdInfoString() { Name ="Model"},
                           new AdInfoString() { Name ="Mileage"},
                           new AdInfoString() { Name ="Year"},
                           new AdInfoString() { Name ="Condition"},
                           new AdInfoString() { Name ="Body Type"},
                           new AdInfoString() { Name ="Drivetrain"},
                           new AdInfoString() { Name ="Engine Size"},
                           new AdInfoString() { Name ="Transmission"},
                           new AdInfoString() { Name ="Colour"},
                           new AdInfoString() { Name ="Fuel Type"}
                        };
                ds.AdInfoStringDB.AddRange(_t1);
                ds.SaveChanges();

                var t1 = ds.TemplatesDB.SingleOrDefault(x => x.TemplateName.Equals("CT"));
                t1.RecommendedInfo = _t1;
                ds.SaveChanges();
                ds.SubCategoryDB.SingleOrDefault(x => x.Name.Equals("Cars/Trucks")).AdInfoTemplate = t1;
                ds.SaveChanges();

                ICollection<AdInfoString> _t2 = new List<AdInfoString>() { 
                            new AdInfoString() { Name ="Make"},
                            new AdInfoString() { Name ="Model"},
                            new AdInfoString() { Name ="Mileage"},
                            new AdInfoString() { Name ="Year"},
                            new AdInfoString() { Name ="Engine Size"},
                            new AdInfoString() { Name ="Condition"},
                            new AdInfoString() { Name ="Colour"},
                            new AdInfoString() { Name ="Fuel Type"}
                        };
                ds.AdInfoStringDB.AddRange(_t2);
                ds.SaveChanges();

                var t2 = ds.TemplatesDB.SingleOrDefault(x => x.TemplateName.Equals("MA"));
                t2.RecommendedInfo = _t2;
                ds.SaveChanges();
                ds.SubCategoryDB.SingleOrDefault(x => x.Name.Equals("Motorcycles/ATVs")).AdInfoTemplate = t2;
                ds.SaveChanges();


                ICollection<AdInfoString> _t3 = new List<AdInfoString>(){
                            new AdInfoString() { Name ="Bedrooms"},
                            new AdInfoString() { Name ="Bathrooms"},
                            new AdInfoString() { Name ="Size"},
                            new AdInfoString() { Name ="Furnished"}
                        };
                ds.AdInfoStringDB.AddRange(_t3);
                ds.SaveChanges();

                var t3 = ds.TemplatesDB.SingleOrDefault(x => x.TemplateName.Equals("RE"));
                t3.RecommendedInfo = _t3;
                ds.SaveChanges();
                foreach (var x in ds.SubCategoryDB.Include("Category").Where(x => 
                    x.Category.Name.Equals("Real Estate") && x.Name.Equals("Apartments/Condos Rental") || 
                    x.Category.Name.Equals("Real Estate") && x.Name.Equals("Apartments/Condos For Sale") || 
                    x.Category.Name.Equals("Real Estate") && x.Name.Equals("House For Sale") ||
                    x.Category.Name.Equals("Real Estate") && x.Name.Equals("House Rental")
                    ))
                {
                    x.AdInfoTemplate = t3;
                }
                ds.SaveChanges();

                ICollection<AdInfoString> _t4 = new List<AdInfoString>(){
                    new AdInfoString() { Name ="Size"},
                    new AdInfoString() { Name ="Furnished"}
                };
                ds.AdInfoStringDB.AddRange(_t4);
                ds.SaveChanges();

                var t4 = ds.TemplatesDB.SingleOrDefault(x => x.TemplateName.Equals("RECOS"));
                t4.RecommendedInfo = _t4;
                ds.SaveChanges();
                ds.SubCategoryDB.Include("Category").SingleOrDefault(x => x.Category.Name.Equals("Real Estate") && x.Name.Equals("Commercial Office Space")).AdInfoTemplate = t4;
                ds.SaveChanges();

                ICollection<AdInfoString> _t5 = new List<AdInfoString>(){
                    new AdInfoString() { Name ="Company Name"},
                    new AdInfoString() { Name ="Job Type"},
                    new AdInfoString() { Name ="Salary Type"}
                };
                ds.AdInfoStringDB.AddRange(_t5);
                ds.SaveChanges();

                var t5 = ds.TemplatesDB.SingleOrDefault(x => x.TemplateName.Equals("JOB"));
                t5.RecommendedInfo = _t5;
                ds.SaveChanges();
                foreach(var x in ds.SubCategoryDB.Include("Category").Where(x => x.Category.Name.Equals("Jobs"))){
                    x.AdInfoTemplate = t5;
                }
                ds.SaveChanges();


                ICollection<AdInfoString> _t6 = new List<AdInfoString>() { 
                            new AdInfoString() { Name ="Make"},
                            new AdInfoString() { Name ="Model"},
                            new AdInfoString() { Name ="Year"},
                            new AdInfoString() { Name ="Condition"}
                        };
                ds.AdInfoStringDB.AddRange(_t6);
                ds.SaveChanges();

                var t6 = ds.TemplatesDB.SingleOrDefault(x => x.TemplateName.Equals("AMP"));
                t6.RecommendedInfo = _t6;
                ds.SaveChanges();
                ds.SubCategoryDB.SingleOrDefault(x => x.Name.Equals("Automotive Parts")).AdInfoTemplate = t6;
                ds.SaveChanges();

                // Pets
                ICollection<AdInfoString> _t7 = new List<AdInfoString>() { 
                            new AdInfoString() { Name ="Species"},
                            new AdInfoString() { Name ="Breed"},
                            new AdInfoString() { Name ="Gender"},
                            new AdInfoString() { Name ="Pet's Name"},
                            new AdInfoString() { Name ="Colour"},
                            new AdInfoString() { Name ="Age"}
                        };
                ds.AdInfoStringDB.AddRange(_t7);
                ds.SaveChanges();

                var t7 = ds.TemplatesDB.SingleOrDefault(x => x.TemplateName.Equals("PET"));
                t7.RecommendedInfo = _t7;
                ds.SaveChanges();
                foreach (var x in ds.SubCategoryDB.Include("Category").Where(x =>
                    x.Category.Name.Equals("Pets") && x.Name.Equals("Lost Pet") ||
                    x.Category.Name.Equals("Pets") && x.Name.Equals("Pet Adoption") ||
                    x.Category.Name.Equals("Pets") && x.Name.Equals("Pet Hub")
                    ))
                {
                    x.AdInfoTemplate = t7;
                }
                ds.SaveChanges();

                ICollection<AdInfoString> _t8 = new List<AdInfoString>() { 
                            new AdInfoString() { Name ="Species"}
                        };
                ds.AdInfoStringDB.AddRange(_t8);
                ds.SaveChanges();

                var t8 = ds.TemplatesDB.SingleOrDefault(x => x.TemplateName.Equals("PETSERV"));
                t8.RecommendedInfo = _t8;
                ds.SaveChanges();
                foreach (var x in ds.SubCategoryDB.Include("Category").Where(x => x.Category.Name.Equals("Pets") && x.Name.Equals("Pet Services") || x.Category.Name.Equals("Pets") && x.Name.Equals("Pet Accessories")))
                {
                    x.AdInfoTemplate = t8;
                }
                ds.SaveChanges();

                ds.TemplatesDB.Add(new AdInfoTemplate() { TemplateName = "REL" });
                ds.SaveChanges();

                ICollection<AdInfoString> _t9 = new List<AdInfoString>(){
                    new AdInfoString() { Name ="Size"}
                };
                ds.AdInfoStringDB.AddRange(_t9);
                ds.SaveChanges();

                var t9 = ds.TemplatesDB.SingleOrDefault(x => x.TemplateName.Equals("REL"));
                t4.RecommendedInfo = _t9;
                ds.SaveChanges();
                ds.SubCategoryDB.Include("Category").SingleOrDefault(x => x.Category.Name.Equals("Real Estate") && x.Name.Equals("Land Rental/Leasing")).AdInfoTemplate = t9;
                ds.SubCategoryDB.Include("Category").SingleOrDefault(x => x.Category.Name.Equals("Real Estate") && x.Name.Equals("Land For Sale")).AdInfoTemplate = t9;
                ds.SaveChanges();

            }

            // Breed
            if (ds.BreedDB.Count() == 0)
            {
                ICollection<Breed> breed = new List<Breed>(){
                    new Breed() { Name = "Affenpinscher (Dog)" },
                    new Breed() { Name = "Afghan Hound (Dog)" },
                    new Breed() { Name = "Airedale Terrier (Dog)" },
                    new Breed() { Name = "Akita (Dog)"  },
                    new Breed() { Name = "Alaskan Malamute (Dog)" },
                    new Breed() { Name = "American English Coonhound (Dog)" },
                    new Breed() { Name = "American Eskimo (Dog)"  },
                    new Breed() { Name = "American Eskimo (Dog)"  },
                    new Breed() { Name = "American Eskimo (Dog)"  },
                    new Breed() { Name = "American Foxhound (Dog)" },
                    new Breed() { Name = "American Hairless Terrier (Dog)" },
                    new Breed() { Name = "American Staffordshire Terrier (Dog)" },
                    new Breed() { Name = "American Water Spaniel (Dog)" },
                    new Breed() { Name = "Anatolian Shepherd (Dog)" },
                    new Breed() { Name = "Australian Cattle (Dog)" },
                    new Breed() { Name = "Australian Shepherd (Dog)" },
                    new Breed() { Name = "Australian Terrier (Dog)" },
                    new Breed() { Name = "Basenji (Dog) (Dog)" },
                    new Breed() { Name = "Basset Hound (Dog) (Dog)" },
                    new Breed() { Name = "Beagle (Dog)" },
                    new Breed() { Name = "Bearded Collie (Dog)" },
                    new Breed() { Name = "Beauceron (Dog)" },
                    new Breed() { Name = "Bedlington Terrier (Dog)" },
                    new Breed() { Name = "Belgian Malinois (Dog)" },
                    new Breed() { Name = "Belgian Sheep (Dog)" },
                    new Breed() { Name = "Belgian Tervuren (Dog)" },
                    new Breed() { Name = "Bergamasco Shepherd (Dog)"  },
                    new Breed() { Name = "Berger Picard (Dog)" },
                    new Breed() { Name = "Bernese Mountain (Dog)" },
                    new Breed() { Name = "Bichon Frisé (Dog)" },
                    new Breed() { Name = "Black and Tan Coonhound (Dog)" },
                    new Breed() { Name = "Black Russian Terrier (Dog)" },
                    new Breed() { Name = "Bloodhound (Dog)" },
                    new Breed() { Name = "Bluetick Coonhound (Dog)" },
                    new Breed() { Name = "Boerboel (Dog)" },
                    new Breed() { Name = "Border Collie (Dog)" },
                    new Breed() { Name = "Border Terrier (Dog)" },
                    new Breed() { Name = "Borzoi (Dog)" },
                    new Breed() { Name = "Boston Terrier (Dog)" },
                    new Breed() { Name = "Bouvier des Flandres (Dog)" },
                    new Breed() { Name = "Boxer (Dog)"  },
                    new Breed() { Name = "Boykin Spaniel (Dog)" },
                    new Breed() { Name = "Briard (Dog)" },
                    new Breed() { Name = "Brittany (Dog)"  },
                    new Breed() { Name = "Griffon Bruxellois (Dog)"  },
                    new Breed() { Name = "Bull Terrier (Dog)" },
                    new Breed() { Name = "Bull Terrier (Miniature) (Dog)" },
                    new Breed() { Name = "Bull (Dog)" },
                    new Breed() { Name = "Bullmastiff (Dog)" },
                    new Breed() { Name = "Cairn Terrier (Dog)" },
                    new Breed() { Name = "Canaan (Dog)" },
                    new Breed() { Name = "Cane Corso (Dog)" },
                    new Breed() { Name = "Cardigan Welsh Corgi (Dog)" },
                    new Breed() { Name = "Cavalier King Charles Spaniel (Dog)" },
                    new Breed() { Name = "Cesky Terrier (Dog)" },
                    new Breed() { Name = "Chesapeake Bay Retriever (Dog)" },
                    new Breed() { Name = "Chihuahua (Dog)"  },
                    new Breed() { Name = "Chinese Crested (Dog)" },
                    new Breed() { Name = "Shar Pei (Dog)"  },
                    new Breed() { Name = "Chinook (Dog)"  },
                    new Breed() { Name = "Chow Chow (Dog)" },
                    new Breed() { Name = "Cirneco dell'Etna (Dog)" },
                    new Breed() { Name = "Clumber Spaniel (Dog)" },
                    new Breed() { Name = "American Cocker Spaniel (Dog)"  },
                    new Breed() { Name = "Rough Collie (Dog)"  },
                    new Breed() { Name = "Coton de Tulear (Dog)" },
                    new Breed() { Name = "Curly-Coated Retriever (Dog)" },
                    new Breed() { Name = "Dachshund (Dog)" },
                    new Breed() { Name = "Dalmatian (Dog)"  },
                    new Breed() { Name = "Dandie Dinmont Terrier (Dog)" },
                    new Breed() { Name = "Doberman Pinscher (Dog)" },
                    new Breed() { Name = "Dogue de Bordeaux (Dog)" },
                    new Breed() { Name = "English Cocker Spaniel (Dog)" },
                    new Breed() { Name = "English Foxhound (Dog)" },
                    new Breed() { Name = "English Setter (Dog)" },
                    new Breed() { Name = "English Springer Spaniel (Dog)" },
                    new Breed() { Name = "King Charles Spaniel (Dog)"  },
                    new Breed() { Name = "Entlebucher Mountain (Dog)" },
                    new Breed() { Name = "Field Spaniel (Dog)" },
                    new Breed() { Name = "Finnish Lapphund (Dog)" },
                    new Breed() { Name = "Finnish Spitz (Dog)" },
                    new Breed() { Name = "Flat-Coated Retriever (Dog)" },
                    new Breed() { Name = "French Bull (Dog)" },
                    new Breed() { Name = "German Pinscher (Dog)" },
                    new Breed() { Name = "German Shepherd (Dog)" },
                    new Breed() { Name = "German Shorthaired Pointer (Dog)" },
                    new Breed() { Name = "German Wirehaired Pointer (Dog)" },
                    new Breed() { Name = "Giant Schnauzer (Dog)" },
                    new Breed() { Name = "Glen of Imaal Terrier (Dog)" },
                    new Breed() { Name = "Golden Retriever (Dog)" },
                    new Breed() { Name = "Gordon Setter (Dog)" },
                    new Breed() { Name = "Great Dane (Dog)" },
                    new Breed() { Name = "Great Pyrenees (Dog)" },
                    new Breed() { Name = "Greater Swiss Mountain (Dog)" },
                    new Breed() { Name = "Greyhound (Dog)" },
                    new Breed() { Name = "Harrier (Dog)"  },
                    new Breed() { Name = "Havanese (Dog)" },
                    new Breed() { Name = "Ibizan Hound (Dog)" },
                    new Breed() { Name = "Icelandic Sheep (Dog)" },
                    new Breed() { Name = "Irish Red and White Setter (Dog)" },
                    new Breed() { Name = "Irish Setter (Dog)" },
                    new Breed() { Name = "Irish Terrier (Dog)" },
                    new Breed() { Name = "Irish Water Spaniel (Dog)" },
                    new Breed() { Name = "Irish Wolfhound (Dog)" },
                    new Breed() { Name = "Italian Greyhound (Dog)" },
                    new Breed() { Name = "Japanese Chin (Dog)" },
                    new Breed() { Name = "Keeshond (Dog)" },
                    new Breed() { Name = "Kerry Blue Terrier (Dog)" },
                    new Breed() { Name = "Komondor (Dog)" },
                    new Breed() { Name = "Kuvasz (Dog)" },
                    new Breed() { Name = "Labrador Retriever (Dog)" },
                    new Breed() { Name = "Lagotto Romagnolo (Dog)" },
                    new Breed() { Name = "Lakeland Terrier (Dog)" },
                    new Breed() { Name = "Leonberger (Dog)" },
                    new Breed() { Name = "Lhasa Apso (Dog)" },
                    new Breed() { Name = "Löwchen (Dog)" },
                    new Breed() { Name = "Maltese (Dog)"  },
                    new Breed() { Name = "Manchester Terrier (Dog)" },
                    new Breed() { Name = "English Mastiff (Dog)"  },
                    new Breed() { Name = "Miniature American Shepherd (Dog)" },
                    new Breed() { Name = "Miniature Bull Terrier (Dog)" },
                    new Breed() { Name = "Miniature Pinscher (Dog)" },
                    new Breed() { Name = "Miniature Schnauzer (Dog)" },
                    new Breed() { Name = "Neapolitan Mastiff (Dog)" },
                    new Breed() { Name = "Newfoundland (Dog)"  },
                    new Breed() { Name = "Norfolk Terrier (Dog)" },
                    new Breed() { Name = "Norwegian Buhund (Dog)" },
                    new Breed() { Name = "Norwegian Elkhound (Dog)" },
                    new Breed() { Name = "Norwegian Lundehund (Dog)" },
                    new Breed() { Name = "Norwich Terrier (Dog)" },
                    new Breed() { Name = "Nova Scotia Duck-Tolling Retriever (Dog)" },
                    new Breed() { Name = "Old English Sheep (Dog)" },
                    new Breed() { Name = "Otterhound (Dog)" },
                    new Breed() { Name = "Papillon (Dog)"  },
                    new Breed() { Name = "Parson Russell Terrier (Dog)" },
                    new Breed() { Name = "Pekingese (Dog)" },
                    new Breed() { Name = "Pembroke Welsh Corgi (Dog)" },
                    new Breed() { Name = "Petit Basset Griffon Vendéen (Dog)" },
                    new Breed() { Name = "Pharaoh Hound (Dog)" },
                    new Breed() { Name = "Plott (Dog)" },
                    new Breed() { Name = "Pointer (dog breed) (Dog)"  },
                    new Breed() { Name = "Polish Lowland Sheep (Dog)" },
                    new Breed() { Name = "Pomeranian (Dog)"  },
                    new Breed() { Name = "Poodle (Dog)" },
                    new Breed() { Name = "Portuguese Podengo (Dog)" },
                    new Breed() { Name = "Portuguese Water (Dog)" },
                    new Breed() { Name = "Pug (Dog)" },
                    new Breed() { Name = "Puli (Dog)" },
                    new Breed() { Name = "Pyrenean Shepherd (Dog)" },
                    new Breed() { Name = "Rat Terrier (Dog)" },
                    new Breed() { Name = "Redbone Coonhound (Dog)" },
                    new Breed() { Name = "Rhodesian Ridgeback (Dog)" },
                    new Breed() { Name = "Rottweiler (Dog)" },
                    new Breed() { Name = "Russell Terrier (Dog)" },
                    new Breed() { Name = "St. Bernard (Dog)"  },
                    new Breed() { Name = "Saluki (Dog)" },
                    new Breed() { Name = "Samoyed (Dog)"  },
                    new Breed() { Name = "Schipperke (Dog)" },
                    new Breed() { Name = "Scottish Deerhound (Dog)" },
                    new Breed() { Name = "Scottish Terrier (Dog)" },
                    new Breed() { Name = "Sealyham Terrier (Dog)" },
                    new Breed() { Name = "Shetland Sheep (Dog)" },
                    new Breed() { Name = "Shiba Inu (Dog)" },
                    new Breed() { Name = "Shih Tzu (Dog)" },
                    new Breed() { Name = "Siberian Husky (Dog)" },
                    new Breed() { Name = "Australian Silky Terrier (Dog)"  },
                    new Breed() { Name = "Skye Terrier (Dog)" },
                    new Breed() { Name = "Sloughi (Dog)" },
                    new Breed() { Name = "Smooth Fox Terrier (Dog)" },
                    new Breed() { Name = "Soft-Coated Wheaten Terrier (Dog)" },
                    new Breed() { Name = "Spanish Water (Dog)" },
                    new Breed() { Name = "Spinone Italiano (Dog)" },
                    new Breed() { Name = "Staffordshire Bull Terrier (Dog)" },
                    new Breed() { Name = "Standard Schnauzer (Dog)" },
                    new Breed() { Name = "Sussex Spaniel (Dog)" },
                    new Breed() { Name = "Swedish Vallhund (Dog)" },
                    new Breed() { Name = "Tibetan Mastiff (Dog)" },
                    new Breed() { Name = "Tibetan Spaniel (Dog)" },
                    new Breed() { Name = "Tibetan Terrier (Dog)" },
                    new Breed() { Name = "Toy Fox Terrier (Dog)" },
                    new Breed() { Name = "Treeing Walker Coonhound (Dog)" },
                    new Breed() { Name = "Hungarian Vizsla (Dog)"  },
                    new Breed() { Name = "Weimaraner (Dog)" },
                    new Breed() { Name = "Welsh Springer Spaniel (Dog)" },
                    new Breed() { Name = "Welsh Terrier (Dog)" },
                    new Breed() { Name = "West Highland White Terrier (Dog)" },
                    new Breed() { Name = "Whippet (Dog)" },
                    new Breed() { Name = "Wire Fox Terrier (Dog)" },
                    new Breed() { Name = "Wirehaired Pointing Griffon (Dog)" },
                    new Breed() { Name = "Wirehaired Vizsla (Dog)" },
                    new Breed() { Name = "Xoloitzcuintli (Dog)" },
                    new Breed() { Name = "Yorkshire Terrier (Dog)" },
                    new Breed() { Name = "Abyssinian (Cat)" },
                    new Breed() { Name = "Aegean (Cat)" },
                    new Breed() { Name = "American Curl (Cat)" },
                    new Breed() { Name = "American Bobtail (Cat)" },
                    new Breed() { Name = "American Shorthair (Cat)" },
                    new Breed() { Name = "American Wirehair (Cat)" },
                    new Breed() { Name = "Arabian Mau (Cat)" },
                    new Breed() { Name = "Australian Mist (Cat)" },
                    new Breed() { Name = "Asian (Cat)" },
                    new Breed() { Name = "Asian Semi-longhair (Cat)" },
                    new Breed() { Name = "Balinese (Cat)" },
                    new Breed() { Name = "Bambino (Cat)" },
                    new Breed() { Name = "Bengal (Cat)" },
                    new Breed() { Name = "Birman (Cat)" },
                    new Breed() { Name = "Bombay (Cat)" },
                    new Breed() { Name = "Brazilian Shorthair (Cat)" },
                    new Breed() { Name = "British Longhair (Cat)" },
                    new Breed() { Name = "British Shorthair (Cat)" },
                    new Breed() { Name = "British Longhair (Cat)" },
                    new Breed() { Name = "Burmese (Cat)" },
                    new Breed() { Name = "Burmilla (Cat)" },
                    new Breed() { Name = "California Spangled (Cat)" },
                    new Breed() { Name = "Chantilly-Tiffany (Cat)" },
                    new Breed() { Name = "Chartreux (Cat)" },
                    new Breed() { Name = "Chausie (Cat)" },
                    new Breed() { Name = "Cheetoh (Cat)" },
                    new Breed() { Name = "Colorpoint Shorthair (Cat)" },
                    new Breed() { Name = "Cornish Rex (Cat)" },
                    new Breed() { Name = "Cymric (Cat)" },
                    new Breed() { Name = "Cyprus (Cat)" },
                    new Breed() { Name = "Devon Rex (Cat)" },
                    new Breed() { Name = "Donskoy (Cat)" },
                    new Breed() { Name = "Dragon Li (Cat)" },
                    new Breed() { Name = "Dwarf cat (Cat)" },
                    new Breed() { Name = "Egyptian Mau (Cat)" },
                    new Breed() { Name = "European Shorthair (Cat)" },
                    new Breed() { Name = "Exotic Shorthair (Cat)" },
                    new Breed() { Name = "Foldex (Cat)" },
                    new Breed() { Name = "German Rex (Cat)" },
                    new Breed() { Name = "Havana Brown (Cat)" },
                    new Breed() { Name = "Highlander (Cat)" },
                    new Breed() { Name = "Himalayan (Cat)" },
                    new Breed() { Name = "Japanese Bobtail (Cat)" },
                    new Breed() { Name = "Javanese (Cat)" },
                    new Breed() { Name = "Kurilian Bobtail (Cat)" },
                    new Breed() { Name = "Khao Manee (Cat)" },
                    new Breed() { Name = "Korat (Cat)" },
                    new Breed() { Name = "Korean Bobtail (Cat)" },
                    new Breed() { Name = "Korn Ja (Cat)" },
                    new Breed() { Name = "Kurilian Bobtail (Cat)" },
                    new Breed() { Name = "LaPerm (Cat)" },
                    new Breed() { Name = "Lykoi (Cat)" },
                    new Breed() { Name = "Maine Coon (Cat)" },
                    new Breed() { Name = "Manx (Cat)" },
                    new Breed() { Name = "Mekong Bobtail (Cat)" },
                    new Breed() { Name = "Minskin (Cat)" },
                    new Breed() { Name = "Munchkin (Cat)" },
                    new Breed() { Name = "Nebelung (Cat)" },
                    new Breed() { Name = "Napoleon (Cat)" },
                    new Breed() { Name = "Norwegian Forest cat (Cat)" },
                    new Breed() { Name = "Ocicat (Cat)" },
                    new Breed() { Name = "Ojos Azules (Cat)" },
                    new Breed() { Name = "Oregon Rex (Cat)" },
                    new Breed() { Name = "Oriental Bicolor (Cat)" },
                    new Breed() { Name = "Oriental Shorthair (Cat)" },
                    new Breed() { Name = "Oriental Longhair (Cat)" },
                    new Breed() { Name = "Persian (Cat)" },
                    new Breed() { Name = "Traditional Persian (Cat)" },
                    new Breed() { Name = "Peterbald (Cat)" },
                    new Breed() { Name = "Pixie-bob (Cat)" },
                    new Breed() { Name = "Raas (Cat)" },
                    new Breed() { Name = "Ragamuffin (Cat)" },
                    new Breed() { Name = "Ragdoll (Cat)" },
                    new Breed() { Name = "Russian Blue (Cat)" },
                    new Breed() { Name = "Russian White, Black and Tabby (Cat)" },
                    new Breed() { Name = "Sam Sawet (Cat)" },
                    new Breed() { Name = "Savannah (Cat)" },
                    new Breed() { Name = "Scottish Fold (Cat)" },
                    new Breed() { Name = "Selkirk Rex (Cat)" },
                    new Breed() { Name = "Serengeti (Cat)" },
                    new Breed() { Name = "Serrade petit (Cat)" },
                    new Breed() { Name = "Siamese (Cat)" },
                    new Breed() { Name = "Siberian (Cat)" },
                    new Breed() { Name = "Singapura (Cat)" },
                    new Breed() { Name = "Snowshoe (Cat)" },
                    new Breed() { Name = "Sokoke (Cat)" },
                    new Breed() { Name = "Somali (Cat)" },
                    new Breed() { Name = "Sphynx (Cat)" },
                    new Breed() { Name = "Suphalak (Cat)" },
                    new Breed() { Name = "Thai (Cat)" },
                    new Breed() { Name = "Thai Lilac (Cat)" },
                    new Breed() { Name = "Tonkinese (Cat)" },
                    new Breed() { Name = "Toyger (Cat)" },
                    new Breed() { Name = "Turkish Angora (Cat)" },
                    new Breed() { Name = "Turkish Van (Cat)" },
                    new Breed() { Name = "Ukrainian Levkoy (Cat)" },
                    new Breed() { Name = "Mixed" },
                    new Breed() { Name = "Other" }
                };
                ds.BreedDB.AddRange(breed);
                ds.SaveChanges();
            }

            // Make
            if (ds.MakeDB.Count() == 0)
            {
                ICollection<Make> mak = new List<Make>(){
                    new Make() { Name = "Acura" },
                    new Make() { Name = "Alfa Romeo" },
                    new Make() { Name = "Audi" },
                    new Make() { Name = "Austin" },
                    new Make() { Name = "Aston Martin" },
                    new Make() { Name = "Bajaj"},
                    new Make() { Name = "Bentley" },
                    new Make() { Name = "BMW" },
                    new Make() { Name = "Buick" },
                    new Make() { Name = "Cadillac" },
                    new Make() { Name = "Can-Am" },
                    new Make() { Name = "Caterpillar" },
                    new Make() { Name = "Chrysler" },
                    new Make() { Name = "Citroen" },
                    new Make() { Name = "Chevrolet" },
                    new Make() { Name = "Daihatsu" },
                    new Make() { Name = "Datsun" },
                    new Make() { Name = "Dodge" },
                    new Make() { Name = "Dongfeng" },
                    new Make() { Name = "Ferrari" },
                    new Make() { Name = "Fiat" },
                    new Make() { Name = "Ford" },
                    new Make() { Name = "Foton" },
                    new Make() { Name = "Great Wall" },
                    new Make() { Name = "Hafei" },
                    new Make() { Name = "Harley Davidson" },
                    new Make() { Name = "Hillman" },
                    new Make() { Name = "Hino" },
                    new Make() { Name = "Honda" },
                    new Make() { Name = "Hummer" },
                    new Make() { Name = "Hyundai" },
                    new Make() { Name = "Infiniti" },
                    new Make() { Name = "Isuzu" },
                    new Make() { Name = "JAC" },
                    new Make() { Name = "Jaguar" },
                    new Make() { Name = "Jeep" },
                    new Make() { Name = "John Deere" },
                    new Make() { Name = "Kawasaki" },
                    new Make() { Name = "Kandi" },
                    new Make() { Name = "Kia" },
                    new Make() { Name = "Lada" },
                    new Make() { Name = "Lamborghini" },
                    new Make() { Name = "Land Rover" },
                    new Make() { Name = "Lexus" },
                    new Make() { Name = "Layland" },                    
                    new Make() { Name = "Lincoln" },
                    new Make() { Name = "Lotus" },
                    new Make() { Name = "Mazda" },                    
                    new Make() { Name = "Mercedes-Benz" },
                    new Make() { Name = "Mitsubishi" },
                    new Make() { Name = "Nissan" },
                    new Make() { Name = "Oldsmobile" },
                    new Make() { Name = "Opel" },
                    new Make() { Name = "Peugeot" },
                    new Make() { Name = "Plymouth" },
                    new Make() { Name = "Pontiac" },
                    new Make() { Name = "Porsche" },
                    new Make() { Name = "Proton" },
                    new Make() { Name = "Quantum" },
                    new Make() { Name = "Range Rover" },
                    new Make() { Name = "Renault" },
                    new Make() { Name = "Rover" },
                    new Make() { Name = "Saab" },
                    new Make() { Name = "Scooter" },
                    new Make() { Name = "Sea-doo" },
                    new Make() { Name = "Skoda" },
                    new Make() { Name = "SsangYong" },
                    new Make() { Name = "Subaru" },
                    new Make() { Name = "Suzuki" },
                    new Make() { Name = "Tesla" },
                    new Make() { Name = "Toyota" },
                    new Make() { Name = "Volkswagen" },
                    new Make() { Name = "Volvo" },
                    new Make() { Name = "Yamaha" },
                    new Make() { Name = "Yuejin" },
                    new Make() { Name = "Zongshen" },
                    new Make() { Name = "Other" }
                };
                ds.MakeDB.AddRange(mak);
                ds.SaveChanges();

                // body type
                if (ds.BodyTypeDB.Count() == 0)
                {
                    ICollection<BodyType> bt = new List<BodyType>(){
                        new BodyType() { Type = "Convertible" },
                        new BodyType() { Type = "Coupe (2 door)" },
                        new BodyType() { Type = "Hatchback" },
                        new BodyType() { Type = "Minivan" },
                        new BodyType() { Type = "Van" },
                        new BodyType() { Type = "Pickup Truck" },
                        new BodyType() { Type = "Sedan" },
                        new BodyType() { Type = "SUV/Crossover" },
                        new BodyType() { Type = "Wagon" }
                    };
                    ds.BodyTypeDB.AddRange(bt);
                    ds.SaveChanges();
                }

                // Transmission
                if (ds.TransmissionDB.Count() == 0)
                {
                    ICollection<Transmission> tm = new List<Transmission>(){
                        new Transmission() { Name = "Automatic" },
                        new Transmission() { Name = "Manual" },
                        new Transmission() { Name = "Semi-Automatic (paddle-shift/tiptronic)" },
                        new Transmission() { Name = "CVT" }
                    };
                    ds.TransmissionDB.AddRange(tm);
                    ds.SaveChanges();
                }

                // Fuel Type
                if (ds.FuelDB.Count() == 0)
                {
                    ICollection<FuelType> ft = new List<FuelType>(){
                        new FuelType() { Name = "Diesel" },
                        new FuelType() { Name = "Gasoline" },
                        new FuelType() { Name = "Hybrid-Electric" },
                        new FuelType() { Name = "Electric" }
                    };
                    ds.FuelDB.AddRange(ft);
                    ds.SaveChanges();
                }

                // Condition
                if (ds.ConditionDB.Count() == 0)
                {
                    ICollection<Condition> condi = new List<Condition>(){
                        new Condition() { Name = "Damaged" },
                        new Condition() { Name = "New" },
                        new Condition() { Name = "Used" }
                    };
                    ds.ConditionDB.AddRange(condi);
                    ds.SaveChanges();
                }

                // Drivetrain
                if (ds.DrivetrainDB.Count() == 0)
                {
                    ICollection<Drivetrain> dritr = new List<Drivetrain>(){
                        new Drivetrain() { Name = "4 x 4" },
                        new Drivetrain() { Name = "All Wheel Drive (AWD)" },
                        new Drivetrain() { Name = "Front Wheel Drive (FWD)" },
                        new Drivetrain() { Name = "Rear Wheel Drive (RWD)" }
                    };
                    ds.DrivetrainDB.AddRange(dritr);
                    ds.SaveChanges();
                }
            }


            return done;
        }*/

        internal IEnumerable<ClassifiedAdReportList> GetAllReports()
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.ReportDB.Include("ClassifiedAd").ToList();
                obj.Sort((x, y) => DateTime.Compare(y.CreatedDate, x.CreatedDate));
                if (obj != null)
                    if (obj.Count() > 0)
                    {
                        var config = new MapperConfiguration(r => r.CreateMap<ClassifiedAdReport, ClassifiedAdReportList>());
                        IMapper mapper = config.CreateMapper();
                        return mapper.Map<IEnumerable<ClassifiedAdReportList>>(obj);
                    }
                return new List<ClassifiedAdReportList>();
            }
        }

        internal bool ReportAdClose(string sId)
        {
            return false;
        }

        // Get one ad with details
        internal AdminAdListDetail AdminGetClassifiedAdWithDetails(string stringId)
        {
            // Check for session id
            ClassifiedAd item;
            IEnumerable<Photo> pho = new List<Photo>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                item = newthreadcontext.ClassifiedDB.Include("AdPhotos").Include("AdInfo")
               .Include("Country").Include("Region").Include("UserCreator").Include("Category").Include("SubCategory")
               .SingleOrDefault(a => a.StringId == stringId);
                // crashes when user creates ad with no photo
                if (item != null)
                {
                    if (item.AdPhotos != null)
                        if (item.AdPhotos.Count > 0)
                            pho = item.AdPhotos.Where(a => a.SetThumbnail == false);
                }
                else
                    return new AdminAdListDetail();
                item.AdPhotos = pho.ToArray();
            }

            var config = new MapperConfiguration(r =>
            {
                r.CreateMap<ClassifiedAd, AdminAdListDetail>()
                    .ForMember(dest => dest.AdInfo, opt => opt.MapFrom(x => x.AdInfo.Where(a => a.Description != null)));
                r.CreateMap<Photo, PhotoBase>();
                r.CreateMap<Country, CountryBase>();
                r.CreateMap<Models.Region, RegionBase>();
                r.CreateMap<ApplicationUser, UserProfileContact>();
                r.CreateMap<Info, InfoForm>();
            });

            IMapper mapper = config.CreateMapper();

            return (item == null) ? new AdminAdListDetail() : mapper.Map<AdminAdListDetail>(item);
        }


        // Close a user posted ad
        internal bool AdminCloseAd(string adstringid)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // get existing
                try
                {
                    var obj = newthreadcontext.ClassifiedDB.Include("Category").Include("SubCategory").Include("UserCreator").SingleOrDefault(x => x.StringId.Equals(adstringid));
                    if (obj.Status != 1)
                    {
                        obj.Status = 1;
                        obj.Category.TotalClassifiedAdsCount--;
                        obj.SubCategory.ClassifiedAdsCount--;
                        newthreadcontext.SaveChanges();
                        // Remove old Lucene
                        LuceneSearch.ClearLuceneIndexRecord(obj.StringId);
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
        }

        internal bool AdminOpenAd(string adstringid)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // get existing
                try
                {
                    var obj = newthreadcontext.ClassifiedDB.Include("AdPhotos")
                        .Include("Country")
                        .Include("Region")
                        .Include("UserCreator")
                        .Include("AdInfo")
                        .Include("Category")
                        .Include("SubCategory")
                        .SingleOrDefault(x => x.StringId.Equals(adstringid));

                    if (obj.Status == -1)
                    {
                        var obj2 = newthreadcontext.ReportDB.Include("ClassifiedAd").Where(x => x.ClassifiedAd.StringId.Equals(adstringid));
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
                        newthreadcontext.SaveChanges();
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
        }

        // Suspend ad
        internal bool AdminSuspendAd(string adstringid)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // get existing
                try
                {
                    var obj = newthreadcontext.ClassifiedDB.Include("Category").Include("SubCategory").Include("UserCreator").SingleOrDefault(x => x.StringId.Equals(adstringid));
                    if (obj.Status != -1)
                    {
                        obj.Status = -1;
                        obj.Category.TotalClassifiedAdsCount--;
                        obj.SubCategory.ClassifiedAdsCount--;
                        newthreadcontext.SaveChanges();
                        // Remove old Lucene
                        LuceneSearch.ClearLuceneIndexRecord(obj.StringId);
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
        }

        // Close a classified ad report
        internal bool AdminCloseReport(int repId)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // get existing
                try
                {
                    var obj = newthreadcontext.ReportDB.SingleOrDefault(x => x.Id == repId);
                    obj.Status = 1;
                    newthreadcontext.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        internal void AdminRoleChange(string useremail, string oldrole, string newrole)
        {

            if (oldrole.Contains("User") && newrole.Contains("Premium"))
            {
                AdminAddPremiumRole(useremail);

            }
            else if (oldrole.Contains("Premium") && newrole.Contains("User"))
            {
                AdminRemovePremiumRole(useremail);

            }
            else
            {
                using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
                {
                    // 1. Get user and add to 'Premium' role
                    var user = UserManager.FindByEmail(useremail);
                    UserManager.RemoveFromRole(user.Id.ToString(), oldrole);
                    UserManager.AddToRole(user.Id, (newrole == "Unbanned") ? "User" : newrole);                

                    if (newrole == "Banned" || newrole == "Unbanned")
                    {
                        foreach (var ad in user.ClassifiedAds)
                        {
                            ad.Status = (newrole == "Banned" ? 1 : (newrole == "Unbanned" ? 0 : 0));
                            foreach (var rep in ad.Reports)
                            {
                                rep.Status = (newrole == "Banned" ? 1 : (newrole == "Unbanned" ? 0 : 0));
                            }
                            if (newrole == "Banned")
                            {
                                ad.Category.TotalClassifiedAdsCount--;
                                ad.SubCategory.ClassifiedAdsCount--;
                            }
                            else if (newrole == "Unbanned")
                            {
                                ad.Category.TotalClassifiedAdsCount++;
                                ad.SubCategory.ClassifiedAdsCount++;
                            }
                        }
                        newthreadcontext.SaveChanges();                        
                    }
                }
            }
        }

        internal void AdminAddPremiumRole(string useremail)
        {            
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // 1. Get user and add to 'Premium' role
                var user = UserManager.FindByEmail(useremail);
                UserManager.AddToRole(user.Id, "Premium");
                
                /*
                var dat = newthreadcontext.PremUserDataDB.SingleOrDefault(x => x..Equals(obj.stringId));
                if (dat == null)
                {
                    var init = new PremiumUserData()
                    {
                        UserProfileStringId = obj.stringId,
                        UserProfile = obj,
                        PremiumUserInfos = new List<PremiumUserInfo>(),
                        PremiumUserPhotos = new List<PremiumUserPhoto>(),
                        UserReviews = new List<PremiumUserReview>()
                    };
                    init.PremiumUserName = obj.UserName;
                    init.UrlName = obj.stringId;
                    obj.PremiumUserData = init;
                }
                else
                {
                    dat.UserProfile = obj;
                    obj.PremiumUserData = dat;
                }
                newthreadcontext.SaveChanges();*/
            }
        }

        internal void AdminRemovePremiumRole(string useremail)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // 1. Get user and remove from 'Premium' role
                var user = UserManager.FindByEmail(useremail);
                UserManager.RemoveFromRole(user.Id, "Premium");
                
                //var dat = newthreadcontext.PremUserDataDB.SingleOrDefault(x => x(obj.stringId));
                //dat.UserProfile = null;
                //newthreadcontext.SaveChanges();
            }
        }

        internal AdminMemberDetails AdminMemberDetails(int id)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // 1. Get user
                var user = UserManager.FindById(id.ToString());
                var config = new MapperConfiguration(r =>
                {
                    r.CreateMap<ApplicationUser, AdminMemberDetails>();
                    r.CreateMap<ClassifiedAd, ClassifiedAdBase>();
                });
                IMapper mapper = config.CreateMapper();
                return (user != null) ? mapper.Map<AdminMemberDetails>(user) : null;
            }
        }

        internal IEnumerable<GenericMessageQueue> AdminGetMessageQueue()
        {
            var config = new MapperConfiguration(r => r.CreateMap<GenericMessage, GenericMessageQueue>());
            IMapper mapper = config.CreateMapper();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.GenMessageDB;
                return (obj == null) ? new List<GenericMessageQueue>() : mapper.Map<IEnumerable<GenericMessageQueue>>(obj);
            }
        }

        // Close a user posted message
        internal bool AdminMessageClose(int msgId)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // get existing
                try
                {
                    var obj = newthreadcontext.GenMessageDB.SingleOrDefault(x => x.Id == msgId);
                    if (obj == null) return false;                    
                    obj.Status = 1;
                    newthreadcontext.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        // Delete a user posted message
        internal bool AdminMessageDelete(int msgId)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // get existing
                try
                {
                    var obj = newthreadcontext.GenMessageDB.SingleOrDefault(x => x.Id == msgId);
                    if (obj == null) return false;                    
                    newthreadcontext.GenMessageDB.Remove(obj);
                    newthreadcontext.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        internal bool AdminDeleteReportAd(int id)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                try
                {
                    var obj = newthreadcontext.ReportDB.Include("ClassifiedAd.Reports").SingleOrDefault(r => r.Id == id && r.Status == 1);
                    if (obj == null) return false;
                    obj.ClassifiedAd.Reports.Remove(obj);
                    obj.ClassifiedAd = null;
                    newthreadcontext.ReportDB.Remove(obj);
                    newthreadcontext.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        internal bool AdminDeleteAllClosedReportAds()
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                try
                {
                    var objs = newthreadcontext.ReportDB.Include("ClassifiedAd.Reports").Where(r => r.Status == 1);
                    if (objs == null) return false;
                    foreach (var obj in objs)
                    {
                        obj.ClassifiedAd.Reports.Remove(obj);
                        obj.ClassifiedAd = null;
                        newthreadcontext.ReportDB.Remove(obj);
                    }
                    newthreadcontext.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        internal ClassifiedAdBase AdminClassifiedAdEdit(AdminClassifiedAdEdit editItem, HttpServerUtilityBase Server)
        {
            // 1. Ensure user credibility
            // 2. Attempt to fetch obj
            // 3. Set last edited time
            // 4. Pull existing item
            // 5. Compare photos for changes and add/remove
            // 6. Save changes                       
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.ClassifiedDB
                        .Include("Country")
                        .Include("Region")
                        .Include("UserCreator")
                        .Include("AdInfo")
                        .Include("Category")
                        .Include("SubCategory")
                        .SingleOrDefault(n => n.StringId == editItem.StringId);

                if (o == null)
                {
                    return null;
                }
                else
                {
                    // convert string descriptions to int before we pull from db
                    var convertInfo_Mileage = o.AdInfo.SingleOrDefault(x => x.Name.Equals("Mileage"));
                    if (convertInfo_Mileage != null)
                        if (!String.IsNullOrEmpty(convertInfo_Mileage.Description))
                            convertInfo_Mileage.IntDescription = Convert.ToInt32(convertInfo_Mileage.Description.Replace(",", ""));

                    var convertInfo_Size = o.AdInfo.SingleOrDefault(x => x.Name.Equals("Size"));
                    if (convertInfo_Size != null)
                        if (!String.IsNullOrEmpty(convertInfo_Size.Description))
                            convertInfo_Size.IntDescription = Convert.ToInt32(convertInfo_Size.Description.Replace(",", ""));

                    // year
                    // delete old years
                    var deleteyears = o.AdInfo.Where(x => x.Name.Equals("Year")).ToList();
                    if (deleteyears != null && deleteyears.Count > 0)
                    {
                        foreach (var dy in deleteyears)
                        {
                            if (deleteyears.FirstOrDefault() != dy)
                            {
                                dy.ClassifiedAd = null;
                                o.AdInfo.Remove(dy);
                                newthreadcontext.InfosDB.Remove(dy);
                            }
                            else
                            {
                                dy.Description = null;
                                dy.IntDescription = 0;
                            }
                        }

                        var convertInfo_Year = new Info() { Description = editItem.AdInfo.SingleOrDefault(x => x.Name.Equals("Year")).Description, Name = "Year" };
                        if (editItem.SubCategoryName.Equals("Automotive Parts"))
                        {
                            if (!String.IsNullOrEmpty(convertInfo_Year.Description))
                            {
                                if (convertInfo_Year.Description.Contains("-"))
                                {
                                    var years = convertInfo_Year.Description.Split('-');
                                    convertInfo_Year.Description = years[0];
                                    convertInfo_Year.IntDescription = Convert.ToInt32(years[0]);
                                    var updatey = o.AdInfo.FirstOrDefault(x => x.Name.Equals("Year"));
                                    updatey.Description = convertInfo_Year.Description;
                                    updatey.IntDescription = convertInfo_Year.IntDescription;
                                    if (years.Length > 1)
                                    {
                                        for (int i = Convert.ToInt32(years[0]); i <= Convert.ToInt32(years[1]); i++)
                                        {
                                            o.AdInfo.Add(new Info() { Name = "Year", Description = i.ToString(), IntDescription = i, ClassifiedAd = o });
                                        }
                                    }
                                }
                                else
                                {
                                    convertInfo_Year.IntDescription = Convert.ToInt32(convertInfo_Year.Description);
                                    var updatey = o.AdInfo.FirstOrDefault(x => x.Name.Equals("Year"));
                                    updatey.Description = convertInfo_Year.Description;
                                    updatey.IntDescription = convertInfo_Year.IntDescription;
                                }
                            }
                        }
                        else
                        {
                            if (convertInfo_Year != null)
                                if (!String.IsNullOrEmpty(convertInfo_Year.Description))
                                    convertInfo_Year.IntDescription = Convert.ToInt32(convertInfo_Year.Description);
                            var updatey = o.AdInfo.FirstOrDefault(x => x.Name.Equals("Year"));
                            updatey.Description = convertInfo_Year.Description;
                            updatey.IntDescription = convertInfo_Year.IntDescription;
                        }
                    }

                    // pet age
                    var deleteages = o.AdInfo.Where(x => x.Name.Equals("Age")).ToList();
                    if (deleteages != null && deleteages.Count > 0)
                    {
                        foreach (var da in deleteages)
                        {
                            da.ClassifiedAd = null;
                            o.AdInfo.Remove(da);
                            newthreadcontext.InfosDB.Remove(da);
                        }
                        var convertAge = new Info() { Description = editItem.AdInfo.SingleOrDefault(x => x.Name.Equals("Age")).Description, Name = "Age" };
                        if (convertAge != null)
                        {                                
                            if (!String.IsNullOrEmpty(convertAge.Description))
                            {
                                convertAge.IntDescription = Convert.ToInt32(convertAge.Description);
                                convertAge.Description += " " + editItem.AgeType;
                                o.AdInfo.Add(convertAge);
                                if (!String.IsNullOrEmpty(editItem.AgeType))
                                {
                                    if (editItem.AgeType == "Days" && convertAge.IntDescription > 7)
                                    {
                                        double _weeks = (double)convertAge.IntDescription / 7.0;
                                        double _months = (double)convertAge.IntDescription / 31;
                                        double _year = (double)convertAge.IntDescription / 365;
                                        int weeks = (int)Math.Floor(_weeks);
                                        int months = (int)Math.Floor(_months);
                                        int years = (int)Math.Floor(_year);

                                        if (weeks > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = weeks + " " + "Weeks", IntDescription = weeks, ClassifiedAd = o });
                                        if (months > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = months + " " + "Months", IntDescription = months, ClassifiedAd = o });
                                        if (years > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = years + " " + "Years", IntDescription = years, ClassifiedAd = o });
                                    }
                                    else if (editItem.AgeType == "Weeks" && convertAge.IntDescription > 1)
                                    {
                                        double _days = ((double)convertAge.IntDescription * 7);
                                        double _months = (double)convertAge.IntDescription / 4;
                                        double _year = (double)convertAge.IntDescription / 48;
                                        int days = (int)Math.Floor(_days);
                                        int months = (int)Math.Floor(_months);
                                        int years = (int)Math.Floor(_year);

                                        if (days > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = days + " " + "Days", IntDescription = days, ClassifiedAd = o });
                                        if (months > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = months + " " + "Months", IntDescription = months, ClassifiedAd = o });
                                        if (years > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = years + " " + "Years", IntDescription = years, ClassifiedAd = o });
                                    }
                                    else if (editItem.AgeType == "Months" && convertAge.IntDescription > 1)
                                    {
                                        double _days = (double)convertAge.IntDescription * 31;
                                        double _weeks = (double)convertAge.IntDescription * 4;
                                        double _year = (double)convertAge.IntDescription / 12;
                                        int days = (int)Math.Floor(_days);
                                        int weeks = (int)Math.Floor(_weeks);
                                        int years = (int)Math.Floor(_year);

                                        if (days > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = days + " " + "Days", IntDescription = days, ClassifiedAd = o });
                                        if (weeks > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = weeks + " " + "Weeks", IntDescription = weeks, ClassifiedAd = o });
                                        if (years > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = years + " " + "Years", IntDescription = years, ClassifiedAd = o });
                                    }
                                    else if (editItem.AgeType == "Years" && convertAge.IntDescription > 1)
                                    {
                                        double _days = (double)convertAge.IntDescription * 365;
                                        double _weeks = (double)convertAge.IntDescription * 12 * 4;
                                        double _months = (double)convertAge.IntDescription * 12;
                                        int days = (int)Math.Floor(_days);
                                        int weeks = (int)Math.Floor(_weeks);
                                        int months = (int)Math.Floor(_months);

                                        if (days > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = days + " " + "Days", IntDescription = days, ClassifiedAd = o });
                                        if (weeks > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = weeks + " " + "Weeks", IntDescription = weeks, ClassifiedAd = o });
                                        if (months > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = months + " " + "Months", IntDescription = months, ClassifiedAd = o });
                                    }
                                }
                            }
                            else
                            {
                                o.AdInfo.Add(convertAge);
                            }
                        }
                    }

                    // format adinfo
                    var format_EngineSize = o.AdInfo.SingleOrDefault(x => x.Name.Equals("Engine Size"));
                    if (format_EngineSize != null)
                        if (!String.IsNullOrEmpty(format_EngineSize.Description))
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
                        var country = newthreadcontext.CountryDB.Include("Regions").Include("Regions.ClassifiedAds").SingleOrDefault(a => a.Id == editItem.CountryId);
                        o.Country = country;
                    }
                    if (o.Region.Id != editItem.RegionId)
                    {
                        var country = newthreadcontext.CountryDB.Include("Regions").Include("Regions.ClassifiedAds").SingleOrDefault(a => a.Id == editItem.CountryId);
                        country.Regions.SingleOrDefault(a => a.Id == editItem.RegionId).ClassifiedAds.Add(o);
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

                    if (!editItem.UserContactName.Equals(o.UserContactName))
                        o.UserContactName = editItem.UserContactName;

                    if (!editItem.UserContactPhone.Equals(o.UserContactPhone))
                    {
                        editItem.UserContactPhone = (editItem.UserContactPhone.Contains("-") ? editItem.UserContactPhone : editItem.UserContactPhone.Insert(3, "-").ToString());
                        o.UserContactPhone = editItem.UserContactPhone;
                    }
                    if (!String.IsNullOrEmpty(editItem.UserContactPhone2))
                    {
                        if (!editItem.UserContactPhone2.Equals(o.UserContactPhone2))
                        {
                            editItem.UserContactPhone2 = (editItem.UserContactPhone2.Contains("-") ? editItem.UserContactPhone2 : editItem.UserContactPhone2.Insert(3, "-").ToString());
                            o.UserContactPhone2 = editItem.UserContactPhone2;
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(o.UserContactPhone2))
                            o.UserContactPhone2 = null;
                    }
                    if (!String.IsNullOrEmpty(editItem.UserContactPhone3))
                    {
                        if (!editItem.UserContactPhone3.Equals(o.UserContactPhone3))
                        {
                            editItem.UserContactPhone3 = (editItem.UserContactPhone3.Contains("-") ? editItem.UserContactPhone3 : editItem.UserContactPhone3.Insert(3, "-").ToString());
                            o.UserContactPhone3 = editItem.UserContactPhone3;
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(o.UserContactPhone3))
                            o.UserContactPhone3 = null;
                    }

                    if (!editItem.UserContactEmail.Equals(o.UserContactEmail))
                        o.UserContactEmail = editItem.UserContactEmail;

                    // Update photos
                    var dir = Server.MapPath("~/Photos/" + editItem.StringId.Substring(2, 4) + "/" + editItem.StringId.Substring(0, 4));
                    string path;

                    // replace existing photo1
                    if (editItem.PhotoUpload1 != null && !editItem.PhotoUpload1bool)
                    {
                        // Delete thumbnail
                        var thumb = o.AdPhotos.SingleOrDefault(a => a.SetThumbnail == true);
                        try
                        {
                            path = Path.Combine(dir, thumb.FileName);
                            // delete physical
                            File.Delete(path);
                        }
                        catch (Exception) { Directory.CreateDirectory(dir); } // create directory if failed
                        //convert first img into thumbnail
                        if (thumb != null)
                            CompressPhotoEdit(dir, 10, editItem.PhotoUpload1, thumb);
                        else
                            CompressPhoto(dir, 10, editItem.PhotoUpload1, new Photo() { SetThumbnail = true, ClassifiedAd = o });

                        // Delete Ad List Tumbnail
                        var first_t = o.AdPhotos.SingleOrDefault(a => a.CountNum == 11 && a.AdListThumbnail == true && a.SetThumbnail == false);

                        if (first_t != null)
                        {
                            path = Path.Combine(dir, first_t.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 11, editItem.PhotoUpload1, first_t);
                        }
                        else
                        {
                            CompressPhoto(dir, 11, editItem.PhotoUpload1, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }

                        // Delete Photo
                        var first = o.AdPhotos.SingleOrDefault(a => a.CountNum == 11 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (first != null)
                        {
                            path = Path.Combine(dir, first.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 11, editItem.PhotoUpload1, first);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 11, editItem.PhotoUpload1, new Photo() { ClassifiedAd = o });
                        }

                    }
                    else if (editItem.PhotoUpload1bool)
                    {
                        // REMOVE IMG
                        // delete thumbnail
                        Photo _dThumb = o.AdPhotos.SingleOrDefault(x => x.CountNum == 10 && x.AdListThumbnail == false && x.SetThumbnail == true);
                        if (_dThumb != null)
                        {
                            path = Path.Combine(dir, _dThumb.FileName);
                            File.Delete(path);
                            _dThumb.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dThumb);
                            o.AdPhotos.Remove(_dThumb);
                        }
                        // delete first
                        Photo _dFirst = o.AdPhotos.SingleOrDefault(x => x.CountNum == 11 && x.SetThumbnail == false && x.AdListThumbnail == false);
                        if (_dFirst != null)
                        {
                            path = Path.Combine(dir, _dFirst.FileName);
                            File.Delete(path);
                            _dFirst.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFirst);
                            o.AdPhotos.Remove(_dFirst);
                        }
                        // delete first ad list tumb
                        Photo _dFirstALT = o.AdPhotos.SingleOrDefault(x => x.CountNum == 11 && x.SetThumbnail == false && x.AdListThumbnail == true);
                        if (_dFirstALT != null)
                        {
                            path = Path.Combine(dir, _dFirstALT.FileName);
                            File.Delete(path);
                            _dFirstALT.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFirstALT);
                            o.AdPhotos.Remove(_dFirstALT);
                        }
                    }

                    // replace photo 2
                    if (editItem.PhotoUpload2 != null && !editItem.PhotoUpload2bool)
                    {
                        // replace regular photo 2
                        var second = o.AdPhotos.SingleOrDefault(a => a.CountNum == 12 && a.SetThumbnail == false && a.AdListThumbnail == false);
                        if (second != null)
                        {
                            path = Path.Combine(dir, second.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 12, editItem.PhotoUpload2, second);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 12, editItem.PhotoUpload2, new Photo() { ClassifiedAd = o });
                        }

                        // remove adlist tumb photo 2
                        var second_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 12 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (second_ != null)
                        {
                            path = Path.Combine(dir, second_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 12, editItem.PhotoUpload2, second_);
                        }
                        else
                        {
                            CompressPhoto(dir, 12, editItem.PhotoUpload2, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload2bool)
                    {
                        // delete second
                        Photo _dSecond = o.AdPhotos.SingleOrDefault(x => x.CountNum == 12 && x.SetThumbnail == false && x.AdListThumbnail == false);
                        if (_dSecond != null)
                        {
                            path = Path.Combine(dir, _dSecond.FileName);
                            File.Delete(path);
                            _dSecond.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSecond);
                            o.AdPhotos.Remove(_dSecond);
                        }
                        // delete second alt
                        Photo _dSecond_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 12 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dSecond_ != null)
                        {
                            path = Path.Combine(dir, _dSecond_.FileName);
                            File.Delete(path);
                            _dSecond_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSecond_);
                            o.AdPhotos.Remove(_dSecond_);
                        }
                    }

                    // replace third photo
                    if (editItem.PhotoUpload3 != null && !editItem.PhotoUpload3bool)
                    {
                        // replace regular photo
                        var third = o.AdPhotos.SingleOrDefault(a => a.CountNum == 13 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (third != null)
                        {
                            path = Path.Combine(dir, third.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 13, editItem.PhotoUpload3, third);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 13, editItem.PhotoUpload3, new Photo() { ClassifiedAd = o });
                        }

                        // replace adlist thumbnail photo
                        var third_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 13 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (third_ != null)
                        {
                            path = Path.Combine(dir, third_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 13, editItem.PhotoUpload3, third_);
                        }
                        else
                        {
                            CompressPhoto(dir, 13, editItem.PhotoUpload3, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }

                    }
                    else if (editItem.PhotoUpload3bool)
                    {
                        // delete third
                        Photo _dThird = o.AdPhotos.SingleOrDefault(x => x.CountNum == 13 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dThird != null)
                        {
                            path = Path.Combine(dir, _dThird.FileName);
                            File.Delete(path);
                            _dThird.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dThird);
                            o.AdPhotos.Remove(_dThird);
                        }
                        // delete third alt
                        Photo _dThird_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 13 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dThird_ != null)
                        {
                            path = Path.Combine(dir, _dThird_.FileName);
                            File.Delete(path);
                            _dThird_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dThird_);
                            o.AdPhotos.Remove(_dThird_);
                        }
                    }

                    // replace fourth photo
                    if (editItem.PhotoUpload4 != null && !editItem.PhotoUpload4bool)
                    {
                        // replace fourth regular
                        var fourth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 14 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (fourth != null)
                        {
                            path = Path.Combine(dir, fourth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 14, editItem.PhotoUpload4, fourth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 14, editItem.PhotoUpload4, new Photo() { ClassifiedAd = o });
                        }

                        // replace forth adlist photo
                        var forth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 14 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (forth_ != null)
                        {
                            path = Path.Combine(dir, forth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 14, editItem.PhotoUpload4, forth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 14, editItem.PhotoUpload4, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload4bool)
                    {
                        // delete fourth
                        Photo _dFourth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 14 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dFourth != null)
                        {
                            path = Path.Combine(dir, _dFourth.FileName);
                            File.Delete(path);
                            _dFourth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFourth);
                            o.AdPhotos.Remove(_dFourth);
                        }
                        // delete third alt
                        Photo _dFourth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 14 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dFourth_ != null)
                        {
                            path = Path.Combine(dir, _dFourth_.FileName);
                            File.Delete(path);
                            _dFourth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFourth_);
                            o.AdPhotos.Remove(_dFourth_);
                        }
                    }

                    if (editItem.PhotoUpload5 != null && !editItem.PhotoUpload5bool)
                    {
                        //replace fifth photo regular
                        var fifth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 15 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (fifth != null)
                        {
                            path = Path.Combine(dir, fifth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 15, editItem.PhotoUpload5, fifth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 15, editItem.PhotoUpload5, new Photo() { ClassifiedAd = o });
                        }

                        // replace fifth photo adlist thumb
                        var fifth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 15 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (fifth_ != null)
                        {
                            path = Path.Combine(dir, fifth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 15, editItem.PhotoUpload5, fifth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 15, editItem.PhotoUpload5, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload5bool)
                    {
                        // delete fifth
                        Photo _dFifth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 15 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dFifth != null)
                        {
                            path = Path.Combine(dir, _dFifth.FileName);
                            File.Delete(path);
                            _dFifth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFifth);
                            o.AdPhotos.Remove(_dFifth);
                        }
                        // delete fifth alt
                        Photo _dFifth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 15 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dFifth_ != null)
                        {
                            path = Path.Combine(dir, _dFifth_.FileName);
                            File.Delete(path);
                            _dFifth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFifth_);
                            o.AdPhotos.Remove(_dFifth_);
                        }
                    }

                    // replace sith photo
                    if (editItem.PhotoUpload6 != null && !editItem.PhotoUpload6bool)
                    {
                        // replace sith photo regular
                        var sixth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 16 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (sixth != null)
                        {
                            path = Path.Combine(dir, sixth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 16, editItem.PhotoUpload6, sixth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 16, editItem.PhotoUpload6, new Photo() { ClassifiedAd = o });
                        }
                        var sixth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 16 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (sixth_ != null)
                        {
                            path = Path.Combine(dir, sixth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 16, editItem.PhotoUpload6, sixth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 16, editItem.PhotoUpload6, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload6bool)
                    {
                        // delete sixth
                        Photo _dSixth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 16 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dSixth != null)
                        {
                            path = Path.Combine(dir, _dSixth.FileName);
                            File.Delete(path);
                            _dSixth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSixth);
                            o.AdPhotos.Remove(_dSixth);
                        }
                        // delete fifth alt
                        Photo _dSixth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 16 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dSixth_ != null)
                        {
                            path = Path.Combine(dir, _dSixth_.FileName);
                            File.Delete(path);
                            _dSixth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSixth_);
                            o.AdPhotos.Remove(_dSixth_);
                        }
                    }

                    // replace seventh photo
                    if (editItem.PhotoUpload7 != null && !editItem.PhotoUpload7bool)
                    {
                        // replace seventh photo regular
                        var seventh = o.AdPhotos.SingleOrDefault(a => a.CountNum == 17 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (seventh != null)
                        {
                            path = Path.Combine(dir, seventh.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 17, editItem.PhotoUpload7, seventh);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 17, editItem.PhotoUpload7, new Photo() { ClassifiedAd = o });
                        }
                        var seventh_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 17 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (seventh_ != null)
                        {
                            path = Path.Combine(dir, seventh_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 17, editItem.PhotoUpload7, seventh_);
                        }
                        else
                        {
                            CompressPhoto(dir, 17, editItem.PhotoUpload7, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload7bool)
                    {
                        // delete seventh
                        Photo _dSeventh = o.AdPhotos.SingleOrDefault(x => x.CountNum == 17 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dSeventh != null)
                        {
                            path = Path.Combine(dir, _dSeventh.FileName);
                            File.Delete(path);
                            _dSeventh.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSeventh);
                            o.AdPhotos.Remove(_dSeventh);
                        }
                        // delete seventh alt
                        Photo _dSeventh_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 17 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dSeventh_ != null)
                        {
                            path = Path.Combine(dir, _dSeventh_.FileName);
                            File.Delete(path);
                            _dSeventh_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSeventh_);
                            o.AdPhotos.Remove(_dSeventh_);
                        }
                    }

                    // replace eighth photo
                    if (editItem.PhotoUpload8 != null && !editItem.PhotoUpload8bool)
                    {
                        // replace eighth photo regular
                        var eighth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 18 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (eighth != null)
                        {
                            path = Path.Combine(dir, eighth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 18, editItem.PhotoUpload8, eighth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 18, editItem.PhotoUpload8, new Photo() { ClassifiedAd = o });
                        }
                        var eighth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 18 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (eighth_ != null)
                        {
                            path = Path.Combine(dir, eighth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 18, editItem.PhotoUpload8, eighth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 18, editItem.PhotoUpload8, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload8bool)
                    {
                        // delete eighth
                        Photo _dEighth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 18 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dEighth != null)
                        {
                            path = Path.Combine(dir, _dEighth.FileName);
                            File.Delete(path);
                            _dEighth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dEighth);
                            o.AdPhotos.Remove(_dEighth);
                        }
                        // delete eighth alt
                        Photo _dEighth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 18 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dEighth_ != null)
                        {
                            path = Path.Combine(dir, _dEighth_.FileName);
                            File.Delete(path);
                            _dEighth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dEighth_);
                            o.AdPhotos.Remove(_dEighth_);
                        }
                    }

                    // replace ninth photo
                    if (editItem.PhotoUpload9 != null && !editItem.PhotoUpload9bool)
                    {
                        // replace ninth photo regular
                        var ninth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 19 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (ninth != null)
                        {
                            path = Path.Combine(dir, ninth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 19, editItem.PhotoUpload9, ninth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 19, editItem.PhotoUpload9, new Photo() { ClassifiedAd = o });
                        }
                        var ninth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 19 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (ninth_ != null)
                        {
                            path = Path.Combine(dir, ninth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 19, editItem.PhotoUpload9, ninth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 19, editItem.PhotoUpload9, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload9bool)
                    {
                        // delete ninth
                        Photo _dNinth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 19 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dNinth != null)
                        {
                            path = Path.Combine(dir, _dNinth.FileName);
                            File.Delete(path);
                            _dNinth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dNinth);
                            o.AdPhotos.Remove(_dNinth);
                        }
                        // delete ninth alt
                        Photo _dNinth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 19 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dNinth_ != null)
                        {
                            path = Path.Combine(dir, _dNinth_.FileName);
                            File.Delete(path);
                            _dNinth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dNinth_);
                            o.AdPhotos.Remove(_dNinth_);
                        }
                    }

                    // replace tenth photo
                    if (editItem.PhotoUpload10 != null && !editItem.PhotoUpload10bool)
                    {
                        // replace tenth photo regular
                        var tenth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 20 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (tenth != null)
                        {
                            path = Path.Combine(dir, tenth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 20, editItem.PhotoUpload10, tenth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 20, editItem.PhotoUpload10, new Photo() { ClassifiedAd = o });
                        }
                        var tenth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 20 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (tenth_ != null)
                        {
                            path = Path.Combine(dir, tenth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 20, editItem.PhotoUpload10, tenth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 20, editItem.PhotoUpload10, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload10bool)
                    {
                        // delete tenth
                        Photo _dTenth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 20 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dTenth != null)
                        {
                            path = Path.Combine(dir, _dTenth.FileName);
                            File.Delete(path);
                            _dTenth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dTenth);
                            o.AdPhotos.Remove(_dTenth);
                        }
                        // delete tenth alt
                        Photo _dTenth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 20 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dTenth_ != null)
                        {
                            path = Path.Combine(dir, _dTenth_.FileName);
                            File.Delete(path);
                            _dTenth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dTenth_);
                            o.AdPhotos.Remove(_dTenth_);
                        }
                    }

                    // replace eleventh photo
                    if (editItem.PhotoUpload11 != null && !editItem.PhotoUpload11bool)
                    {
                        // replace eleventh photo regular
                        var eleventh = o.AdPhotos.SingleOrDefault(a => a.CountNum == 21 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (eleventh != null)
                        {
                            path = Path.Combine(dir, eleventh.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 21, editItem.PhotoUpload11, eleventh);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 21, editItem.PhotoUpload11, new Photo() { ClassifiedAd = o });
                        }
                        var eleventh_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 21 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (eleventh_ != null)
                        {
                            path = Path.Combine(dir, eleventh_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 21, editItem.PhotoUpload11, eleventh_);
                        }
                        else
                        {
                            CompressPhoto(dir, 21, editItem.PhotoUpload11, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload11bool)
                    {
                        // delete eleventh
                        Photo _dEleventh = o.AdPhotos.SingleOrDefault(x => x.CountNum == 21 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dEleventh != null)
                        {
                            path = Path.Combine(dir, _dEleventh.FileName);
                            File.Delete(path);
                            _dEleventh.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dEleventh);
                            o.AdPhotos.Remove(_dEleventh);
                        }
                        // delete eleventh alt
                        Photo _dEleventh_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 21 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dEleventh_ != null)
                        {
                            path = Path.Combine(dir, _dEleventh_.FileName);
                            File.Delete(path);
                            _dEleventh_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dEleventh_);
                            o.AdPhotos.Remove(_dEleventh_);
                        }
                    }

                    // replace twelfth photo
                    if (editItem.PhotoUpload12 != null && !editItem.PhotoUpload12bool)
                    {
                        // replace eleventh photo regular
                        var twelfth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 22 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (twelfth != null)
                        {
                            path = Path.Combine(dir, twelfth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 22, editItem.PhotoUpload12, twelfth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 22, editItem.PhotoUpload12, new Photo() { ClassifiedAd = o });
                        }
                        var twelfth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 22 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (twelfth_ != null)
                        {
                            path = Path.Combine(dir, twelfth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 22, editItem.PhotoUpload12, twelfth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 22, editItem.PhotoUpload12, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload12bool)
                    {
                        // delete eleventh
                        Photo _dTwenfth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 22 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dTwenfth != null)
                        {
                            path = Path.Combine(dir, _dTwenfth.FileName);
                            File.Delete(path);
                            _dTwenfth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dTwenfth);
                            o.AdPhotos.Remove(_dTwenfth);
                        }
                        // delete eleventh alt
                        Photo _dTwelfth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 22 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dTwelfth_ != null)
                        {
                            path = Path.Combine(dir, _dTwelfth_.FileName);
                            File.Delete(path);
                            _dTwelfth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dTwelfth_);
                            o.AdPhotos.Remove(_dTwelfth_);
                        }
                    }

                    newthreadcontext.SaveChanges();
                    // Remove old Lucene
                    LuceneSearch.ClearLuceneIndexRecord(o.StringId);
                    // Add to Lucene
                    LuceneSearch.AddUpdateLuceneIndex(o);
                }
                var config = new MapperConfiguration(r => r.CreateMap<ClassifiedAd, ClassifiedAdBase>());
                IMapper mapper = config.CreateMapper();
                return (o == null) ? null : mapper.Map<ClassifiedAdBase>(o);
            }           
        }

        // Get edit ad
        internal AdminClassifiedAdEditForm AdminGetClassifiedAdWithAll(string stringId)
        {
            ClassifiedAd item;
            IEnumerable<Photo> pho = new List<Photo>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                item = newthreadcontext.ClassifiedDB
               .Include("AdPhotos")
               .Include("AdInfo")
               .Include("Country")
               .Include("Region")
               .Include("UserCreator")
               .Include("Category")
               .Include("SubCategory")
               .SingleOrDefault(a => a.StringId == stringId);
            }

            if (item == null)
                return null;

            var config = new MapperConfiguration(r =>
            {
                r.CreateMap<ClassifiedAd, AdminClassifiedAdEditForm>()
                    .ForMember(dest => dest.PhotoUpload1, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 11).FileName))
                    .ForMember(dest => dest.PhotoUpload2, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 12).FileName))
                    .ForMember(dest => dest.PhotoUpload3, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 13).FileName))
                    .ForMember(dest => dest.PhotoUpload4, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 14).FileName))
                    .ForMember(dest => dest.PhotoUpload5, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 15).FileName))
                    .ForMember(dest => dest.PhotoUpload6, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 16).FileName))
                    .ForMember(dest => dest.PhotoUpload7, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 17).FileName))
                    .ForMember(dest => dest.PhotoUpload8, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 18).FileName))
                    .ForMember(dest => dest.PhotoUpload9, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 19).FileName))
                    .ForMember(dest => dest.PhotoUpload10, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 20).FileName))
                    .ForMember(dest => dest.PhotoUpload11, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 21).FileName))
                    .ForMember(dest => dest.PhotoUpload12, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 22).FileName));

                r.CreateMap<Photo, PhotoBase>();
                r.CreateMap<Country, CountryBase>();
                r.CreateMap<Models.Region, RegionBase>();
                r.CreateMap<ApplicationUser, UserProfileContact>();
                r.CreateMap<Info, InfoForm>();
            });

            IMapper mapper = config.CreateMapper();

            var newit = mapper.Map<AdminClassifiedAdEditForm>(item);

            return (item == null) ? null : newit;
        }

        internal void AdminAdCategoryChange(string adstringid, int catid, string substringid)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.ClassifiedDB
                    .Include("Category")
                    .Include("SubCategory")
                    .Include("AdInfo")
                    .SingleOrDefault(a => a.StringId==adstringid);

                if (o == null) return;

                if (o.Category.Id == catid && o.SubCategory.stringId == substringid) return;

                // remove all associated ad info
                var ail = o.AdInfo.ToList();
                foreach (var ai in ail)
                {
                    ai.ClassifiedAd = null;
                    o.AdInfo.Remove(ai);
                    newthreadcontext.InfosDB.Remove(ai);
                }
                // change associated category and subcategory
                // get the cat then decriment counter and remove ad
                var tempcat = newthreadcontext.CategoryDB.SingleOrDefault(a => a.Id == o.Category.Id);
                o.Category = null;
                tempcat.TotalClassifiedAdsCount--;
                
                var tempsubcat = newthreadcontext.SubCategoryDB.Include("ClassifiedAds").SingleOrDefault(a => a.stringId == o.SubCategory.stringId);
                tempsubcat.ClassifiedAdsCount--;
                o.SubCategory = null;
                tempsubcat.ClassifiedAds.Remove(o);

                // add to new category
                var newtempcat = newthreadcontext.CategoryDB.SingleOrDefault(a => a.Id == catid);
                o.Category = newtempcat;
                newtempcat.TotalClassifiedAdsCount++;

                // add to new subcategory
                var newtempsub = newthreadcontext.SubCategoryDB.Include("ClassifiedAds").SingleOrDefault(a => a.stringId == substringid);
                o.SubCategory = newtempsub;
                newtempsub.ClassifiedAds.Add(o);
                newtempsub.ClassifiedAdsCount++;          

                // add associated info
                var getinfo = new Manager().TemplateInfoGetOne(catid, substringid);
                if (getinfo != null)
                    if (getinfo.Count() > 0)
                    {
                        foreach (var nai in getinfo)
                            o.AdInfo.Add(new Info() { Name = nai.Name, ClassifiedAd = o });
                    } 


                newthreadcontext.SaveChanges();
                // Remove old Lucene
                LuceneSearch.ClearLuceneIndexRecord(o.StringId);
                // Add to Lucene
                LuceneSearch.AddUpdateLuceneIndex(o);
            }
        }

        internal void AdminRemoveUser(int userId, string userStringId, HttpServerUtilityBase Server)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // 1. Get user
                var user = UserManager.FindById(userId.ToString());
                // 2. Remove ads
                foreach (var ad in user.ClassifiedAds)
                {
                    ClassifiedAdRemove(ad, Server);
                }
                // 3. Remove user from roles
                var userroles = UserManager.GetRoles(user.Id);
                foreach(var role in userroles.ToList())
                    UserManager.RemoveFromRole(user.Id,role);
                // 4. Remove user from database
                UserManager.Delete(user);

                newthreadcontext.SaveChanges();
            }
        }

        internal void AdminRemoveUserAds(int userId, string userStringId, HttpServerUtilityBase Server)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // 1. Get user
                var user = UserManager.FindById(userId.ToString());
                // 2. Remove ads
                foreach (var ad in user.ClassifiedAds)
                {
                    ClassifiedAdRemove(ad, Server);
                }
            }
        }
        
        internal void LoadDbAfter()
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {

                //Delete breed
                var breeddelete = newthreadcontext.ClassifiedDB.Include("Category").Include("SubCategory").Include("AdInfo").Where(x => x.Category.Name.Equals("Pets"));
                var info = newthreadcontext.InfosDB;
                foreach (var i in breeddelete)
                {
                    if(i.AdInfo != null)
                        if (i.AdInfo.Count > 0)
                        {
                            var a = i.AdInfo.SingleOrDefault(x => x.Name.Equals("Breed"));
                            var b = i.AdInfo.SingleOrDefault(x => x.Name.Equals("Pet's Name"));
                            var c = i.AdInfo.SingleOrDefault(x => x.Name.Equals("Colour"));
                            var d = i.AdInfo.SingleOrDefault(x => x.Name.Equals("Gender"));
                            var e = i.AdInfo.Where(x => x.Name.Equals("Age"));
                            if (a != null)
                            {
                                a.ClassifiedAd = null;
                                info.Remove(a);
                            }
                            if (b != null)
                            {
                                b.ClassifiedAd = null;
                                info.Remove(b);
                            }
                            if (c != null)
                            {
                                c.ClassifiedAd = null;
                                info.Remove(c);
                            }
                            if (d != null)
                            {
                                d.ClassifiedAd = null;
                                info.Remove(d);
                            }
                            if (e != null)
                            {
                                if (i.SubCategory.Name.Equals("Lost Pet"))
                                {
                                    var el = e.ToList();
                                    foreach (var x in el)
                                    {
                                        x.ClassifiedAd = null;
                                        info.Remove(x);
                                    }
                                }
                            }
                        }
                }
                newthreadcontext.SaveChanges();


                // Delete
                /*var t7 = newthreadcontext.TemplatesDB.Include("RecommendedInfo").SingleOrDefault(x => x.TemplateName.Equals("PET"));
                var all = newthreadcontext.AdInfoStringDB;
                var list = t7.RecommendedInfo.ToList();
                foreach (var ais in list)
                {
                    if(ais.Name.Equals("Breed"))
                        all.Remove(ais);
                    else if (ais.Name.Equals("Pet's Name"))
                        all.Remove(ais);
                    else if (ais.Name.Equals("Colour"))
                        all.Remove(ais);
                    else if (ais.Name.Equals("Gender"))
                        all.Remove(ais);
                }
                newthreadcontext.SaveChanges();

                var ps = newthreadcontext.TemplatesDB.SingleOrDefault(x => x.TemplateName.Equals("PETSERV"));
                var ss = newthreadcontext.SubCategoryDB.Include("Category").SingleOrDefault(x =>
                    x.Category.Name.Equals("Pets") && x.Name.Equals("Lost Pet"));
                ss.AdInfoTemplate = ps;
                newthreadcontext.SaveChanges();*/
            }

            /*using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.CountryDB.Include("Regions").SingleOrDefault(x => x.Name == "Trinidad");

                // Port of Spain
                // San Fernando
                // Chaguanas
                // Arima
                // Point Fortin
                // Couva-Tabaquite-Talparo
                // Diego Martin
                // Penal-Debe
                // Princes Town
                // Rio Claro-Mayaro
                // San Juan-Laventille
                // Sangre Grande
                // Siparia
                // Tunapuna Piarco                
                var TP_newregions = new List<Trinbago_MVC5.Models.Region>(){
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Blanchisseuse"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Brasso Seco Village"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Maracas/St. Joseph"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Ancono Village"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "El Dorado"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Caura"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Kanadahar"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Lopinot Village"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "La Laja"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Heights of Gunapo"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Wallerfield"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Arima Heights/Temple Village"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "Valley View"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = "St.John's Village"},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""},
                    new Trinbago_MVC5.Models.Region() { Country = o, Name = ""}
                };

                newthreadcontext.SaveChanges();
            }*/
        }
    }
}