using PagedList;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Models
{
    public class AdlistPage : SearchBar
    {
        public IEnumerable<ClassifiedAdList> FeaturedAds { get; set; }
        
        public IPagedList<ClassifiedAdList> ClassifiedAdList { get; set; }

        public IEnumerable<CountryAdList> CountryAdList { get; set; }

        public CategoryList Category { get; set; }

        public int CountryId { get; set; }

        public int RegionId { get; set; }

        public int Pages { get; set; }

        public AdlistPage(int countryId, int regionId, int catId, int subCatId) : base(countryId, regionId, catId, subCatId)
        {
            CountryId = countryId;
            RegionId = regionId;
            using (BaseApplicationManager Manager = new BaseApplicationManager())
            {
                SelectLists.Add(new SelectListForm { Name = "AdType", List = new SelectList(Manager.GetAllAdTypes(), "Name", "Value", "All Ads") });
                SelectLists.Add(new SelectListForm
                {
                    Name = "Search Only",
                    List = new SelectList(new List<string>(){
                        "All Ads",
                        "Urgent Ads"
                    }, "All Ads")
                });

                // Get All Categories Based on current catId     

                Category = Categories.FirstOrDefault(x => x.Id == catId);
                // =======================
                if (Category != null)
                {
                    if (Category.Name.Equals("Jobs"))
                    {
                        var list = SelectLists.SingleOrDefault(x => x.Name.Equals("AdType")).List.ToList();
                        list.Remove(list.SingleOrDefault(x => x.Text == "TRADE"));
                        // remove old list
                        SelectLists.Remove(SelectLists.SingleOrDefault(x => x.Name.Equals("AdType")));
                        // recreate new list
                        SelectLists.Add(new SelectListForm() { Name = "AdType", List = new SelectList(list, "Value", "Text", "All Ads") });

                        SelectLists.Add(new SelectListForm()
                        {
                            Name = "JobTypeList",
                            List = new SelectList(new List<string>(){
                            "Full-time",
                            "Permanent Full-time",
                            "Part-time",
                            "Permanent Part-time",
                            "Temporary",
                            "Contract",
                            "Internship",
                            "Project",
                            "Please Contact"
                        })
                        });
                        SelectLists.Add(new SelectListForm()
                        {
                            Name = "SalaryInfoList",
                            List = new SelectList(new List<string>(){
                            "Hourly",
                            "Daily",
                            "Weekly",
                            "Fortnightly",
                            "Monthly",
                            "Yearly"
                        })
                        });
                    }
                }

                // =======================
                if (SubCategory != null)
                {
                    if (SubCategory.Name.Equals("Cars/Trucks"))
                    {
                        SelectLists.Add(new SelectListForm() { Name = "MakeList", List = new SelectList(Manager.GetAllMake(), "Value", "Value") });
                        SelectLists.Add(new SelectListForm() { Name = "BodyTypeList", List = new SelectList(Manager.BodyTypeGetAll(), "Value", "Value") });
                        SelectLists.Add(new SelectListForm() { Name = "TransmissionList", List = new SelectList(Manager.GetAllTransmission(), "Value", "Value") });
                        SelectLists.Add(new SelectListForm() { Name = "FuelTypeList", List = new SelectList(Manager.GetAllFuelType(), "Value", "Value") });
                        SelectLists.Add(new SelectListForm() { Name = "ConditionList", List = new SelectList(Manager.GetAllCondition(), "Value", "Value") });
                        SelectLists.Add(new SelectListForm() { Name = "DrivetrainList", List = new SelectList(Manager.GetAllDrivetrain(), "Value", "Value") });
                    }
                    else if (SubCategory.Name.Equals("Motorcycles/ATVs"))
                    {
                        SelectLists.Add(new SelectListForm() { Name = "MakeList", List = new SelectList(Manager.GetAllMake(), "Value", "Value") });
                        SelectLists.Add(new SelectListForm() { Name = "FuelTypeList", List = new SelectList(Manager.GetAllFuelType(), "Value", "Value") });
                        SelectLists.Add(new SelectListForm() { Name = "ConditionList", List = new SelectList(Manager.GetAllCondition(), "Value", "Value") });
                    }
                    else if (SubCategory.Name.Equals("Automotive Parts"))
                    {
                        SelectLists.Add(new SelectListForm() { Name = "MakeList", List = new SelectList(Manager.GetAllMake(), "Value", "Value") });
                        SelectLists.Add(new SelectListForm() { Name = "ConditionList", List = new SelectList(Manager.GetAllCondition(), "Value", "Value") });

                    }
                    else if (SubCategory.Name.Contains("Apartments/Condos") || SubCategory.Name.Contains("House"))
                    {

                        SelectLists.Add(new SelectListForm()
                        {
                            Name = "BedroomList",
                            List = new SelectList(new List<string>() {
                            "Bachelor / Studio",
                            "1 bedroom",
                            "2 bedrooms",
                            "3 bedrooms",
                            "4 bedrooms",
                            "5 or more bedrooms"
                        })
                        });
                        SelectLists.Add(new SelectListForm()
                        {
                            Name = "BathroomList",
                            List = new SelectList(new List<string>(){
                            "1 bathroom",
                            "2 bathrooms",
                            "3 bathrooms",
                            "4 bathrooms",
                            "5 or more bathrooms"
                        })
                        });
                        SelectLists.Add(new SelectListForm()
                        {
                            Name = "FurnishedList",
                            List = new SelectList(new List<string>(){
                            "Fully-Furnished",
                            "Semi-Furnished",
                            "Unfurnished"
                        })
                        });
                    }
                    else if (SubCategory.Name.Equals("Commercial Office Space"))
                    {
                        SelectLists.Add(new SelectListForm()
                        {
                            Name = "FurnishedList",
                            List = new SelectList(new List<string>(){
                            "Fully-Furnished",
                            "Semi-Furnished",
                            "Unfurnished"
                        })
                        });
                    }

                    if (SubCategory.Name.Equals("Apartments/Condos Rental") || SubCategory.Name.Equals("House Rental") || SubCategory.Name.Equals("Land Rental/Leasing"))
                    {
                        SelectLists.Add(new SelectListForm()
                        {
                            Name = "RentalType",
                            List = new SelectList(new List<string>() {
                            "Rental Only",
                            "Rent To Own"
                        })
                        });
                    }

                    if (SubCategory.Name.Equals("Lost Pet") || SubCategory.Name.Equals("Pet Accessories") ||
                        SubCategory.Name.Equals("Pet Adoption") || SubCategory.Name.Equals("Pet Hub"))
                    {
                        SelectLists.Add(new SelectListForm()
                        {
                            Name = "SpeciesList",
                            List = new SelectList(new List<string>(){
                            "Bird",
                            "Cat",
                            "Dog",
                            "Fish",
                            "Other"
                        })
                        });


                        if (SubCategory.Name.Equals("Pet Adoption") || SubCategory.Name.Equals("Pet Hub"))
                        {
                            SelectLists.Add(new SelectListForm()
                            {
                                Name = "AgeTypeList",
                                List = new SelectList(new List<string>(){
                            "Days",
                            "Weeks",
                            "Months",
                            "Years"
                            })
                            });
                        }
                        if (SubCategory.Name.Equals("Lost Pet"))
                        {
                            var list = SelectLists.SingleOrDefault(x => x.Name.Equals("AdType")).List.ToList();
                            list.Remove(list.SingleOrDefault(x => x.Text == "TRADE"));
                            list.SingleOrDefault(x => x.Text == "SELL").Value = "Found";
                            list.SingleOrDefault(x => x.Text == "WANT").Value = "Missing";
                            // remove old list
                            SelectLists.Remove(SelectLists.SingleOrDefault(x => x.Name.Equals("AdType")));
                            // recreate new list
                            SelectLists.Add(new SelectListForm() { Name = "AdType", List = new SelectList(list, "Value", "Text", "All Ads") });

                        }
                    }
                    else if (SubCategory.Name.Equals("Pet Services"))
                    {
                        SelectLists.Add(new SelectListForm()
                        {
                            Name = "SpeciesList",
                            List = new SelectList(new List<string>(){
                            "All",
                            "Bird",
                            "Cat",
                            "Dog",
                            "Fish",
                            "Other"
                        })
                        });
                    }
                }
                CountryAdList = new List<CountryAdList>();
                CountryAdList = Manager.GetCountryAdLists(CountryId);
            }
        }
    }

    public class AdListDetailParent : SearchBar
    {
        public ClassifiedAdWithDetail Model1 { get; set; }
        public ClassifiedAdEmailUserForm Model2 { get; set; }
        public ClassifiedAdReportForm Model3 { get; set; }
        public ClassifiedAdApplyToForm Model4 { get; set; }
        public IEnumerable<ClassifiedAdMinimal> RelatedAds { get; set; }
        public AdListDetailParent(int catId, int subCatId, int countryId = 0, int regionId = 0) : base(countryId, regionId, catId, subCatId) { }
    }
}