using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using Trinbago_MVC5.Areas.User.Models;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Models
{
    /**
     * Classified Ad base
     **/

    public class ClassifiedAdBase
    {
        public int Id { get; set; }
        public string StringId { get; set; }
        public int Status { get; set; }  // 0 = Display Ad; 1 = Add closed by user; 2 = Closed(add closed by moderator)
    }

    public class ClassifiedAdListPhoto
    {
        public string FilePath { get; set; }
        public string ContentType { get; set; }
    }
    
    public class ClassifiedAdAdminBase : ClassifiedAdBase
    {
        public string Title { get; set; }
        public int EditCount { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime EditTimeStamp { get; set; }
        public int ReportsCount { get; set; }
    }

    /**
    * Classified Ad Inxed
    **/
    public class ClassifiedAdTitle
    {
        public int Id { get; set; }

        public string Price { get; set; }

        public string Title { get; set; }

        public bool IsNew { get; set; }

        public string SubCategoryName { get; set; }
        // Seo
        public string SeoCategory { get; set; }

        public string SeoLocation { get; set; }

        public string SeoTitle { get; set; }
    }

    public class ClassifiedAdMinimal
    {
        public int Id { get; set; }
        
        public bool FeaturedAdStatus { get; set; }

        public bool UrgentAdStatus { get; set; }

        public string Title { get; set; }

        public string Price { get; set; }

        // SEO img alt
        public string ModelName { get; set; }

        // has photo
        public bool AdPhoto { get; set; }

        public string AdList_FileName { get; set; }

        public string SubCategoryName { get; set; }
        // Seo
        public string SeoCategory { get; set; }

        public string SeoLocation { get; set; }

        public string SeoTitle { get; set; }
        //
    }

    /**
     * Displays ClassifiedAd Sell List
     **/
    public class ClassifiedAdListBase
    {
        // Seo
        public string SeoCategory { get; set; }

        public string SeoLocation { get; set; }

        public string SeoTitle { get; set; }

        public int Id { get; set; }

        public string PriceInfo { get; set; }

        // For Images
        public string StringId { get; set; }

        public string Title { get; set; }

        public int Status { get; set; }

        public string Price { get; set; }

        public string AdType { get; set; }

        [Display(Name = "Posting Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}")]
        public DateTime TimeStamp { get; set; }

        public PremiumUserPhotoBase PosterPremiumUserDataPremiumUserPhoto { get; set; }

        public bool UrgentAdStatus { get; set; }

        public bool TopAdStatus { get; set; }

        public string SubCategoryName { get; set; }

        public string CategoryName { get; set; }

        public string HtmlFreeDescription { get; set; }        
    }
    
    public class ClassifiedAdList : ClassifiedAdListBase
    {
        public bool AdPhoto { get; set; }

        public string AdList_FileName { get; set; }

        // only for vehicles
        public string ModelName { get; set; }

        public bool IsFavourited { get; set; }
    }

    public class ClassifiedAdLucene
    {
        public int Id { get; set; }
        public string StringId { get; set; }
        public string Title { get; set; }
        public int Price { get; set; }
        public string PriceInfo { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime EditTimeStamp { get; set; }
        public string Description { get; set; }
        public string HtmlFreeDescription { get; set; }
        public int Status { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string CountrySeoName { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public string RegionSeoName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategorySeoName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string SubCategorySeoName { get; set; }
        public string AdType { get; set; }
        public bool AdPromotionFeaturedAdStatus { get; set; }
        public bool AdPromotionTopAdStatus { get; set; }
        public bool AdPromotionUrgentAdStatus { get; set; }
        public string PosterId { get; set; }
        public string PosterName { get; set; }
        public string AdContactName { get; set; }
        public string AdContactPhone { get; set; }
        public string AdContactPhone2 { get; set; }
        public string AdContactPhone3 { get; set; }
        public string AdContactEmail { get; set; }
        public bool ContactPrivacy { get; set; }
        public ICollection<UploadPhoto> Photos { get; set; }
        public ICollection<InfoForm> AdInfo { get; set; }
    }

    public class ClassifiedAdWithDetail
    {
        public ClassifiedAdWithDetail()
        {
            AdInfo = new List<InfoForm>();
            Photos = new List<ClassifiedAdPhoto>();
        }

        public int Id { get; set; }

        public string StringId { get; set; }        

        public string AdType { get; set; }

        public string Title { get; set; }

        public string Price { get; set; }

        // Seo Img alt
        public string ModelName { get; set; }

        [Display(Name = "Posting Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}")]
        public DateTime TimeStamp { get; set; }

        public int AdViewsCount { get; set; }

        public CountryBase Country { get; set; }

        public RegionBase Region { get; set; }

        public string SeoLocation { get; set; }

        public string Description { get; set; }

        public string HtmlFreeDescription { get; set; }

        public CategoryList Category { get; set; }

        public SubCategoryList SubCategory { get; set; }

        public string SeoCategory { get; set; }

        public bool ContactPrivacy { get; set; }

        [Display(Name = "Contact Name")]
        public string AdContactName { get; set; }

        [Display(Name = "Contact Phone")]
        public string AdContactPhone { get; set; }

        public string AdContactPhone2 { get; set; }

        public string AdContactPhone3 { get; set; }

        public string AdContactEmail { get; set; }

        public string PriceInfo { get; set; }

        public int Status { get; set; }
        
        // ---------
        // Owner flag
        public bool IsOwner { get; set; }

        // ---------
        // Favourited
        public bool IsFavourited { get; set; }

        // ---------
        // Photos
        public IEnumerable<ClassifiedAdPhoto> Photos { get; set; }

        // ---------
        // Info
        public ICollection<InfoForm> AdInfo { get; set; }

        // ------------
        // PUser
        public PremiumUserListDetail PremiumUser { get; set; }
    }

    //
    // Form Base
    public class ClassifiedAdFormBase : IClassifiedAdForm
    {
        public int Id { get; set; }
        public ClassifiedAdFormBase()
        {
            StringId = MySecurity.GetGen();
        }

        [Required]
        public string StringId { get; set; }

        [Required, StringLength(75,MinimumLength = 6)]
        public string Title { get; set; }

        [Required, Display(Name = "Price")]
        [RegularExpression(@"^([1-9]{1}[0-9]{0,2}(\,[0-9]{3})?(\,[0-9]{3}))$|^([1-9]{1}[0-9]{0,8})$|^[P][l][e][a][s][e]\s[C][o][n][t][a][c][t]$", ErrorMessage = "Invalid format. Example (1234 OR 1,234)")]
        public string Price { get; set; }

        public string PriceInfo { get; set; }

        [Required, StringLength(10000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required, Key, Column(Order = 1), Display(Name = "Island")]
        public int? CountryId { get; set; }

        [Required, Key, Column(Order = 2), Display(Name = "Region")]
        public int? RegionId { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        public string CategoryName { get; set; }

        public string SubCategoryName { get; set; }

        public bool ContactPrivacy { get; set; }

        [Required, Display(Name = "Contact Name")]
        public string AdContactName { get; set; }

        [Required, Display(Name = "Contact Phone")]
        [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
        public string AdContactPhone { get; set; }

        [Display(Name = "Contact Phone 2")]
        [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
        public string AdContactPhone2 { get; set; }

        [Display(Name = "Contact Phone 3")]
        [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
        public string AdContactPhone3 { get; set; }

        [Required, EmailAddress, Display(Name = "Contact Email")]
        public string AdContactEmail { get; set; }

        // Pet
        public string AgeType { get; set; }

        // Info
        public ICollection<InfoForm> AdInfo { get; set; }

        // Select list
        public ICollection<SelectListForm> SelectListForm { get; set; }

        public UploadPhoto[] Photos { get; set; }

        public void ConfigureForm(BaseApplicationManager m)
        {
            if (SelectListForm == null) SelectListForm = new List<SelectListForm>();

            // Set Price Info (Saved values set in view)
            SelectListForm.Add(new SelectListForm() { Name = "PriceInfoList", List = new SelectList(m.GetAllPriceInfo(), "Value", "Value", (PriceInfo ?? null)) });

            // populate select lists (Saved values set in view)
            SelectListForm.Add(new SelectListForm() { Name = "CountryList", List = new SelectList(m.GetAllCountries(), "Id", "Name", CountryId) });

            if (CountryId.HasValue)
            {
                var regions = m.GetAllRegionsByCountryId(CountryId.Value);
                SelectListForm.Add(new SelectListForm() { Name = "RegionList", List = new SelectList(regions, "Id", "Name", 0) });
            }
            else
            {
                SelectListForm.Add(new SelectListForm() { Name = "RegionList", List = new SelectList(Enumerable.Empty<SelectListItem>()) });
            }

            // =======================
            if (SubCategoryName.Equals("Cars/Trucks"))
            {
                SelectListForm.Add(new SelectListForm() { Name = "MakeList", List = new SelectList(m.GetAllMake(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Make")).Description : null) });
                SelectListForm.Add(new SelectListForm() { Name = "BodyTypeList", List = new SelectList(m.BodyTypeGetAll(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Body Type")).Description : null) });
                SelectListForm.Add(new SelectListForm() { Name = "TransmissionList", List = new SelectList(m.GetAllTransmission(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Transmission")).Description : null) });
                SelectListForm.Add(new SelectListForm() { Name = "FuelTypeList", List = new SelectList(m.GetAllFuelType(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Fuel Type")).Description : null) });
                SelectListForm.Add(new SelectListForm() { Name = "ConditionList", List = new SelectList(m.GetAllCondition(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Condition")).Description : null) });
                SelectListForm.Add(new SelectListForm() { Name = "DrivetrainList", List = new SelectList(m.GetAllDrivetrain(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Drivetrain")).Description : null) });          
            }
            else if (SubCategoryName.Equals("Motorcycles/ATVs"))
            {
                SelectListForm.Add(new SelectListForm() { Name = "MakeList", List = new SelectList(m.GetAllMake(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Make")).Description : null) });
                SelectListForm.Add(new SelectListForm() { Name = "FuelTypeList", List = new SelectList(m.GetAllFuelType(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Fuel Type")).Description : null) });
                SelectListForm.Add(new SelectListForm() { Name = "ConditionList", List = new SelectList(m.GetAllCondition(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Condition")).Description : null) });
            }
            else if (SubCategoryName.Equals("Automotive Parts"))
            {
                SelectListForm.Add(new SelectListForm() { Name = "MakeList", List = new SelectList(m.GetAllMake(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Make")).Description : null) });
                SelectListForm.Add(new SelectListForm() { Name = "ConditionList", List = new SelectList(m.GetAllCondition(), "Value", "Value", (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Condition")).Description : null) });                
            }
            else if (CategoryName.Equals("Real Estate"))
            {
                if(SubCategoryName.Equals("Apartments/Condos Rental") || SubCategoryName.Equals("House Rental") || SubCategoryName.Equals("Land Rental/Leasing"))
                {
                    SelectListForm.Add(new SelectListForm()
                    {
                        Name = "RentalType",
                        List = new SelectList(new List<string>() {
                            "Rental Only",
                            "Rent To Own"
                        }, (AdInfo != null) ? AdInfo.DefaultIfEmpty(null).SingleOrDefault(x => x.Name.Equals("Rental Type")).Description : null)
                    });
                }

                if (SubCategoryName.Contains("Apartments/Condos") || SubCategoryName.Contains("House"))
                {
                    SelectListForm.Add(new SelectListForm() { 
                        Name = "BedroomList", 
                        List = new SelectList(new List<string>() {
                            "Bachelor / Studio",
                            "1 bedroom",
                            "2 bedrooms",
                            "3 bedrooms",
                            "4 bedrooms",
                            "5 or more bedrooms"
                        }, (AdInfo != null) ? AdInfo.DefaultIfEmpty(null).SingleOrDefault(x => x.Name.Equals("Bedrooms")).Description : null)
                    }); 
                    SelectListForm.Add(new SelectListForm() { 
                        Name = "BathroomList", 
                        List = new SelectList(new List<string>(){
                            "1 bathroom",
                            "2 bathrooms",
                            "3 bathrooms",
                            "4 bathrooms",
                            "5 or more bathrooms"
                        }, (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Bathrooms")).Description : null)
                    });
                    SelectListForm.Add(new SelectListForm() { 
                        Name = "FurnishedList", 
                        List = new SelectList(new List<string>(){
                            "Unfurnished",
                            "Semi-Furnished",
                            "Fully-Furnished"                           
                        }, (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Furnished")).Description : null)
                    });
                }
                else if (SubCategoryName.Equals("Commercial Office Space"))
                {
                    SelectListForm.Add(new SelectListForm() { 
                        Name = "FurnishedList", 
                        List = new SelectList(new List<string>(){
                            "Unfurnished",
                            "Semi-Furnished",
                            "Fully-Furnished"
                        }, (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Furnished")).Description : null)
                    });
                }
            } 
            else if (CategoryName.Equals("Jobs"))
            {
                SelectListForm.Add(new SelectListForm(){
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
                    }, (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Job Type")).Description : null)
                });
                SelectListForm.Add(new SelectListForm(){
                    Name = "SalaryInfoList",
                    List = new SelectList(new List<string>(){
                        "Hourly",
                        "Daily",
                        "Weekly",
                        "Fortnightly",
                        "Monthly",
                        "Yearly"
                    }, (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Salary Type")).Description : null)
                });
            }
            else if (CategoryName.Equals("Pets"))
            {
                if(SubCategoryName.Equals("Lost Pet") || SubCategoryName.Equals("Pet Adoption") || SubCategoryName.Equals("Pet Hub") || SubCategoryName.Equals("Pet Services") || SubCategoryName.Equals("Pet Accessories") || SubCategoryName.Equals("Pet & Animal Services"))
                {
                    SelectListForm.Add(new SelectListForm() {
                        Name = "SpeciesList",
                        List = new SelectList(new List<string>(){
                            "Bird",
                            "Cat",
                            "Dog",
                            "Fish",
                            "Other"
                        }, (AdInfo != null) ? AdInfo.SingleOrDefault(x => x.Name.Equals("Species")).Description : null)
                    });
                }              
            }
            // Handles Create ad photo init
            if (Photos == null)
            {
                if (CategoryName.Equals("Jobs"))
                {
                    Photos = new UploadPhoto[1];
                }
                else if (SubCategoryName.Contains("Apartments/Condos") || SubCategoryName.Contains("House") || SubCategoryName.Contains("Office") || SubCategoryName.Contains("Cars/Trucks"))
                {
                    Photos = new UploadPhoto[12];
                }
                else
                {
                    Photos = new UploadPhoto[6];
                }
            }
        }
    }

    //
    // Create Ad form
    public class ClassifiedAdAddForm : ClassifiedAdFormBase
    {
        [Required, Display(Name = "Select Ad Type")]
        public string AdType { get; set; }
        
        public new void ConfigureForm(BaseApplicationManager m)
        {
            base.ConfigureForm(m);

            // Set Contact info from datastore
            string ucn = "", uce = "", ucp = "", ucp2 = "", ucp3 = "";
            m.SetContanctInfo(ref ucn, ref uce, ref ucp, ref ucp2, ref ucp3);
            AdContactName = ucn;
            AdContactEmail = uce;
            AdContactPhone = ucp;
            AdContactPhone2 = ucp2;
            AdContactPhone3 = ucp3;
            IEnumerable<MiscInfoNoId> tl;
            // Set radiobutton Ad Type
            if (CategoryName.Equals("Real Estate") || CategoryName.Equals("Jobs") || SubCategoryName.Equals("Pet Adoption") || 
                SubCategoryName.Equals("Pet & Animal Services") || SubCategoryName.Equals("Lost Pet") || CategoryName.Equals("Business Services"))
            {
                tl = m.GetAllAdTypes().Where(x => !x.Value.Equals("ALL") && !x.Value.Equals("TRADE")).ToList();
                // Rename
                if (SubCategoryName.Equals("Lost Pet"))
                {
                    tl.SingleOrDefault(x => x.Value.Equals("SELL")).Name = "Found";
                    tl.SingleOrDefault(x => x.Value.Equals("WANT")).Name = "Missing";
                }
                else if(SubCategoryName.Equals("Pet Adoption"))
                {
                    tl.SingleOrDefault(x => x.Value.Equals("SELL")).Name = "Offering (I am Offering)";
                    tl.SingleOrDefault(x => x.Value.Equals("WANT")).Name = "Looking for (I am Looking For)";
                }
                else if (CategoryName.Equals("Jobs"))
                {
                    tl.SingleOrDefault(x => x.Value.Equals("SELL")).Name = "Hiring (I am Hiring)";
                    tl.SingleOrDefault(x => x.Value.Equals("WANT")).Name = "Looking for (I am Looking For)";
                }
                else
                {
                    tl.SingleOrDefault(x => x.Value.Equals("SELL")).Name = "Offering (I am Selling)";
                    tl.SingleOrDefault(x => x.Value.Equals("WANT")).Name = "Looking For (I am Looking for)";
                }
            }
            else
            {
                tl = m.GetAllAdTypes().Where(x => !x.Value.Equals("ALL")).ToList();
                tl.SingleOrDefault(x => x.Value.Equals("SELL")).Name = "Offering (I am Selling)";
                tl.SingleOrDefault(x => x.Value.Equals("WANT")).Name = "Looking For (I am Looking for)";
                tl.SingleOrDefault(x => x.Value.Equals("TRADE")).Name = "Trading (I am Trading)";
            }
            SelectListForm.Add(new SelectListForm() { Name = "TypeList", List = new SelectList(tl, "Name", "Value") });
        }
    }

    public class ClassifiedAdAdd
    {
        public ClassifiedAdAdd()
        {
            Status = 0;
        }

        [Required]
        public string StringId { get; set; }

        [Required, StringLength(75, MinimumLength = 6)]
        public string Title { get; set; }

        public string PriceInfo { get; set; }

        [Required]
        public string Price { get; set; }

        [AllowHtml]
        public string Description { get; set; }

        [Required]
        public int? CountryId { get; set; }

        [Required]
        public int? RegionId { get; set; }

        public ApplicationUser Poster { get; set; }

        public bool ContactPrivacy { get; set; }

        [Required]
        public string AdContactName { get; set; }

        [Required]
        [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
        public string AdContactPhone { get; set; }

        [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
        public string AdContactPhone2 { get; set; }

        [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
        public string AdContactPhone3 { get; set; }

        [Required, EmailAddress]
        public string AdContactEmail { get; set; }

        public int Status { get; set; }  // 0 = open; 1 = closed

        public string AdType { get; set; }

        public ICollection<InfoForm> AdInfo { get; set; }

        // Pet
        public string AgeType { get; set; }

        [Required]
        public int SubCatId { get; set; }

        public string SubCategoryName { get; set; }

        public int MaxPhotoCount { get; set; }

        public UploadPhoto[] Photos { get; set; }
    }

    public class ClassifiedAdEditForm : ClassifiedAdFormBase
    {
        public int Status { get; set; }
        
        public string PosterId { get; set; }

        public int CurrentPhotoCount { get; set; }
        
        // private helper for configform photo setup
        private UploadPhoto _getPhoto(int i, int length, UploadPhoto[] temp)
        {
            return i < length ? temp[i] : null;
        }

        public new void ConfigureForm(BaseApplicationManager m)
        {
            CurrentPhotoCount = Photos.Length;
            if(Photos != null)
            {
                var temp = Photos;
                var max = 0;
                if (CategoryName.Equals("Jobs"))
                {
                    max = 1;
                }
                else if (SubCategoryName.Contains("Apartments/Condos") || SubCategoryName.Contains("House") || SubCategoryName.Contains("Office") || SubCategoryName.Contains("Cars/Trucks"))
                {
                    max = 12;
                }
                else
                {
                    max = 6;
                }
                Photos = new UploadPhoto[max];
                for (int i = 0; i < max; i++)
                {
                    Photos[i] = _getPhoto(i, temp.Length, temp);
                }
            }
            base.ConfigureForm(m);
            try
            {
                var years = AdInfo.Where(x => x.Name.Equals("Year")).ToList();
                if (years != null && years.Count > 1)
                {
                    AdInfo.FirstOrDefault(x => x.Name.Equals("Year")).Description = years.FirstOrDefault().Description + "-" + years.LastOrDefault().Description;
                    foreach (var y in years)
                    {
                        if (y != years.FirstOrDefault())
                            AdInfo.Remove(y);
                    }
                }

                var ages = AdInfo.Where(x => x.Name.Equals("Age")).ToList();
                if (ages != null && ages.Count > 0)
                {
                    foreach (var a in ages)
                    {
                        if (a != ages.FirstOrDefault())
                            AdInfo.Remove(a);
                    }
                    if (ages.FirstOrDefault().Description != null)
                    {
                        AgeType = AdInfo.SingleOrDefault(x => x.Name.Equals("Age")).Description.Split(' ').Last();
                        AdInfo.SingleOrDefault(x => x.Name.Equals("Age")).Description = AdInfo.SingleOrDefault(x => x.Name.Equals("Age")).Description.Split(' ').First();
                    }
                }
                var ai = m.GetAdTemplate(SubCategoryId).Except(AdInfo, new CustomCompare()).ToList();
                AdInfo = AdInfo.Union(ai).ToList();
            }
            catch (Exception) { }        
        }
    }

    public class CustomCompare : IEqualityComparer<InfoForm>
    {
        public bool Equals(InfoForm x, InfoForm y)
        {
            //Check whether the compared objects reference the same data.
            if (x.Name == y.Name) return true;
            return false;
        }

        public int GetHashCode(InfoForm obj)
        {
            //Check whether the object is null
            if (ReferenceEquals(obj, null)) return 0;
            //Get hash code for the Name field if it is not null.
            int hashProductName = obj.Name == null ? 0 : obj.Name.GetHashCode();
            //Calculate the hash code for the product.
            return hashProductName;
        }
    }

    public class ClassifiedAdEditBase
    {
        [Required]
        public string StringId { get; set; }

        [Required, StringLength(75, MinimumLength = 6)]
        public string Title { get; set; }

        public string PriceInfo { get; set; }

        public string Price { get; set; }

        public string CategoryName { get; set; }

        public string SubCategoryName { get; set; }

        public bool ContactPrivacy { get; set; }

        public string AdContactEmail { get; set; }

        public string AdContactPhone { get; set; }

        public string AdContactPhone2 { get; set; }

        public string AdContactPhone3 { get; set; }

        public string AdContactName { get; set; }

        // Pet
        public string AgeType { get; set; }

        [AllowHtml]
        public string Description { get; set; }

        public int? CountryId { get; set; }

        public int? RegionId { get; set; }

        public ICollection<InfoForm> AdInfo { get; set; }

        public ICollection<UploadPhoto> Photos { get; set; }
    }

    public class ClassifiedAdEdit : ClassifiedAdEditBase { }

    public class ClassifiedAdReportForm
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required,StringLength(20), Display(Name = "Reason")]
        public string ReasonTitle { get; set; }

        [Required, EmailAddress, Display(Name="Email Address")]
        public string ReportingUser { get; set; }

        [Required, StringLength(250), Display(Name="Reason Description")]
        public string ReasonDescription { get; set; }
    }

    public class ClassifiedAdReportPost
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int PosterUserId { get; set; }

        [Required, StringLength(20)]
        public string ReasonTitle { get; set; }

        [Required, EmailAddress]
        public string ReportingUser { get; set; }

        [Required, StringLength(250)]
        public string ReasonDescription { get; set; }
    }

    public class ClassifiedAdReportList
    {
        public int Id { get; set; }

        public int ClassifiedAdId { get; set; }

        public string ClassifiedAdStringId { get; set; }

        public int ClassifiedAdStatus { get; set; }

        public string ReportingUser { get; set; }

        public string ReasonTitle { get; set; }

        public string ReasonDescription { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Status { get; set; }

        public bool OpenRequest { get; set; }
    }

    // Interface to be used in ConfigureFormBase() method
    public interface IClassifiedAdForm
    {
        void ConfigureForm(BaseApplicationManager m);
    }
}