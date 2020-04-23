using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Trinbago_MVC5.Models
{
    public class MigrationDesignModels
    {
        [Table("UserProfile")]
        public class UserProfile
        {
            [Key]
            [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
            public int UserId { get; set; }
            public string StringId { get; set; }
            public bool isSeller { get; set; }
            public string UserName { get; set; }
            [Display(Name = "Contact Name")]
            public string ContactName { get; set; }
            [Display(Name = "Contact Number")]
            [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
            public string ContactNumber { get; set; }
            public string Email { get; set; }
            public PremiumUserDataOld PremiumUserData { get; set; }
            public ICollection<ClassifiedAdOld> ClassifiedAds { get; set; }
        }

        // All models related to Ads
        [Table("CategoryOld")]
        public class CategoryOld
        {
            public CategoryOld()
            {
                SubCategories = new List<SubCategoryOld>();
            }

            public int Id { get; set; }

            [StringLength(30)]
            public string Name { get; set; }

            public int TotalClassifiedAdsCount { get; set; }

            public ICollection<SubCategoryOld> SubCategories { get; set; }
        }

        [Table("SubCategoryOld")]
        public class SubCategoryOld
        {
            public SubCategoryOld()
            {
                ClassifiedAds = new List<ClassifiedAdOld>();
                ClassifiedAdsCount = ClassifiedAds.Count;
            }

            public int Id { get; set; }

            [StringLength(25)]
            public string stringId { get; set; }

            [StringLength(100)]
            public string Name { get; set; }

            public CategoryOld Category { get; set; }

            public int ClassifiedAdsCount { get; set; }

            public AdInfoTemplateOld AdInfoTemplate { get; set; }
            public ICollection<ClassifiedAdOld> ClassifiedAds { get; set; }
        }

        [Table("InfoOld")]
        public class InfoOld
        {
            public int Id { get; set; }

            [Required, StringLength(40), Index]
            public string Name { get; set; }

            [StringLength(40), Index]
            public string Description { get; set; }

            public int IntDescription { get; set; }

            [Required]
            public ClassifiedAdOld ClassifiedAd { get; set; }
        }

        [Table("AdInfoTemplateOld")]
        public class AdInfoTemplateOld
        {
            public int Id { get; set; }

            [Required, StringLength(20)]
            public string TemplateName { get; set; }
            [Required]
            public ICollection<AdInfoStringOld> RecommendedInfo { get; set; }
        }

        [Table("AdInfoStringOld")]
        public class AdInfoStringOld
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Table("PhotoOld")]
        public class PhotoOld
        {
            public PhotoOld()
            {
                SetThumbnail = false;
                AdListThumbnail = false;
                long i = 1;
                foreach (byte b in Guid.NewGuid().ToByteArray())
                {
                    i *= ((int)b + 1);
                }
                StringId = string.Format("{0:x}", i - DateTime.Now.Ticks);
            }

            public int Id { get; set; }

            public int CountNum { get; set; }

            public string StringId { get; set; }

            public string ContentType { get; set; }

            public string FileName { get; set; }

            // User selected thumbnail
            public bool SetThumbnail { get; set; }
            // Ad List compressed image
            public bool AdListThumbnail { get; set; }

            [Required]
            public ClassifiedAdOld ClassifiedAd { get; set; }
        }

        [Table("ClassifiedAdOld")]
        public class ClassifiedAdOld
        {
            public ClassifiedAdOld()
            {
                TimeStamp = DateTime.Now;
                EditTimeStamp = DateTime.Now;
                Status = 0;
                EditCount = 0;
                AdInfo = new List<InfoOld>();
                AdPhotos = new List<PhotoOld>();
                AdPromotions = new List<AdPromotionOld>();
                StringId = null;
                TopAdStatus = false;
                UrgentAdStatus = false;
                FeaturedAdStatus = false;
                SocialAdStatus = false;
                ContactPrivacy = false;
            }

            // All Ads contain these
            public int Id { get; set; }

            [StringLength(25)]
            public string StringId { get; set; }

            [Required, StringLength(100)]
            public string Title { get; set; }

            public int Price { get; set; }

            public string PriceInfo { get; set; }

            public int EditCount { get; set; }

            public bool ContactPrivacy { get; set; }

            [Required]
            public string UserContactName { get; set; }

            [Required]
            public string UserContactPhone { get; set; }

            public string UserContactPhone2 { get; set; }

            public string UserContactPhone3 { get; set; }

            [Required]
            public string UserContactEmail { get; set; }

            [Required]
            public UserProfile UserCreator { get; set; }

            [Required]
            public DateTime TimeStamp { get; set; }

            public DateTime EditTimeStamp { get; set; }

            public int MyProperty { get; set; }

            public string Description { get; set; }

            public string HtmlFreeDescription { get; set; }

            // 0 = Display Ad; 1 = Add closed by user; -1 = Closed(add closed by moderator)
            [Required, Range(-1, 1)]
            public int Status { get; set; }  

            public int Views { get; set; }

            public CountryOld Country { get; set; }

            public RegionOld Region { get; set; }

            public CategoryOld Category { get; set; }

            public SubCategoryOld SubCategory { get; set; }

            [Required]
            public string AdType { get; set; }

            //
            // Ad Promotion
            public ICollection<AdPromotionOld> AdPromotions { get; set; }

            public bool TopAdStatus { get; set; }

            public bool UrgentAdStatus { get; set; }

            public bool FeaturedAdStatus { get; set; }

            public bool SocialAdStatus { get; set; }

            // ---------
            // Photos
            public ICollection<PhotoOld> AdPhotos { get; set; }

            // ---------
            // Info
            public ICollection<InfoOld> AdInfo { get; set; }

            // ---------
            // Reports
            public ICollection<ClassifiedAdReportOld> Reports { get; set; }

        }

        [Table("AdPromotionOld")]
        // Ad Promotion
        public class AdPromotionOld
        {
            public AdPromotionOld()
            {
                StartDate = new DateTime();
                IsActive = false;
            }
            public int Id { get; set; }

            public string Name { get; set; }

            public int CurrentDuration { get; set; }

            public int MaxDuration { get; set; }

            public bool IsActive { get; set; }

            public DateTime StartDate { get; set; }
        }

        [Table("ClassifiedAdReportOld")]
        // Ad reporting class
        public class ClassifiedAdReportOld
        {
            public ClassifiedAdReportOld()
            {
                CreatedDate = new DateTime();
                OpenRequest = false;
                Status = 0;
            }
            public int Id { get; set; }

            public ClassifiedAdOld ClassifiedAd { get; set; }

            public string ReportingUser { get; set; }

            [Required, StringLength(20)]
            public string ReasonTitle { get; set; }

            [Required, StringLength(250)]
            public string ReasonDescription { get; set; }

            public DateTime CreatedDate { get; set; }

            public bool OpenRequest { get; set; }

            public int Status { get; set; }
        }

        [Table("CountryOld")]
        // Country
        public class CountryOld
        {
            public CountryOld()
            {
                Regions = new List<RegionOld>();
                RegionCount = Regions.Count();
            }
            public int Id { get; set; }

            public string Name { get; set; }

            public int RegionCount { get; set; }

            public ICollection<RegionOld> Regions { get; set; }
        }

        [Table("RegionOld")]
        // Region
        public class RegionOld
        {
            public RegionOld()
            {
                ClassifiedAds = new List<ClassifiedAdOld>();
            }

            public int Id { get; set; }

            public string Name { get; set; }

            [Required]
            public CountryOld Country { get; set; }
            public ICollection<ClassifiedAdOld> ClassifiedAds { get; set; }
        }

        [Table("AdType")]
        public class AdTypeOld
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        [Table("PriceInfoOld")]
        public class PriceInfoOld
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Table("Make")]
        // Make
        public class MakeOld
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Table("BodyType")]
        // Body Type
        public class BodyTypeOld
        {
            public int Id { get; set; }
            public string Type { get; set; }
        }
        [Table("Transmission")]
        // Transmission
        public class TransmissionOld
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Table("FuelType")]
        // Fuel Type
        public class FuelTypeOld
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Table("ConditionOld")]
        // Condition
        public class ConditionOld
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Table("Drivetrain")]
        // Drivetrain
        public class DrivetrainOld
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Table("SponsoredAdOld")]
        //---------------------------------
        // Sponsored Ads
        public class SponsoredAdOld
        {
            public SponsoredAdOld()
            {
                long i = 1;
                foreach (byte b in Guid.NewGuid().ToByteArray())
                {
                    i *= ((int)b + 1);
                }
                StringId = string.Format("{0:x}", i - DateTime.Now.Ticks);
            }
            public int Id { get; set; }
            public string StringId { get; set; }
            public string Name { get; set; }
            public ICollection<SponsoredPhotoOld> SponsoredPhotos { get; set; }
        }

        [Table("SponsoredPhotoOld")]
        public class SponsoredPhotoOld
        {
            public SponsoredPhotoOld()
            {
                long i = 1;
                foreach (byte b in Guid.NewGuid().ToByteArray())
                {
                    i *= ((int)b + 1);
                }
                StringId = string.Format("{0:x}", i - DateTime.Now.Ticks);
                Start = new DateTime();
            }

            public int Id { get; set; }
            public string PageName { get; set; }
            public int Duriation { get; set; }
            public DateTime Start { get; set; }
            public int CountNum { get; set; }
            public string StringId { get; set; }
            public string ContentType { get; set; }
            public string FileName { get; set; }
            [Required]
            public SponsoredAdOld SponsoredAd { get; set; }
        }

        [Table("GenericMessage")]
        // Generic msg used for - Contact us/feedback ..ect
        public class GenericMessageOld
        {
            public GenericMessageOld()
            {
                Status = 0;
                PostDate = new DateTime();
            }
            public int Id { get; set; }
            [Range(0, 1)]
            public int Status { get; set; }
            [Required]
            public string ReturnTo { get; set; }
            [StringLength(60), Required]
            public string Title { get; set; }
            [StringLength(400), Required]
            public string Description { get; set; }
            public DateTime PostDate { get; set; }

        }

        [Table("PremiumUserDataOld")]
        //Premium User Data
        public class PremiumUserDataOld
        {
            public int Id { get; set; }
            public string UserProfileStringId { get; set; }
            [Required]
            public string PremiumUserName { get; set; }
            public int ClassifiedAdsViewCount { get; set; }
            [Required]
            public UserProfile UserProfile { get; set; }
            [Range(0, 5)]
            public double AverageRating { get; set; }
            public string UrlName { get; set; }
            public ICollection<PremiumUserPhotoOld> PremiumUserPhotos { get; set; }
            public ICollection<PremiumUserReviewOld> UserReviews { get; set; }
            public ICollection<PremiumUserInfoOld> PremiumUserInfos { get; set; }
        }

        [Table("PremiumUserInfoOld")]
        //prem user info
        public class PremiumUserInfoOld
        {
            public int Id { get; set; }
            [StringLength(20)]
            public string Name { get; set; }
            [StringLength(150)]
            public string Description { get; set; }
        }

        [Table("PremiumUserReviewOld")]
        //User review
        public class PremiumUserReviewOld
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public PremiumUserReviewOld ReplyedToUserReview { get; set; }
            [Required]
            public PremiumUserDataOld IntendedUser { get; set; }
            [Required]
            public UserProfile UserThatReplied { get; set; }
            [Range(0, 5)]
            public double Rating { get; set; }
            [StringLength(300, MinimumLength = 10), Required]
            public string Description { get; set; }
        }

        [Table("PremiumUserPhotoOld")]
        //user photo
        public class PremiumUserPhotoOld
        {
            public PremiumUserPhotoOld()
            {
                long i = 1;
                foreach (byte b in Guid.NewGuid().ToByteArray())
                {
                    i *= ((int)b + 1);
                }
                StringId = string.Format("{0:x}", i - DateTime.Now.Ticks);
            }

            public int Id { get; set; }
            public string StringId { get; set; }
            public string ContentType { get; set; }
            public string FileName { get; set; }
            public int Position { get; set; }
            [Required]
            public UserProfile UserProfile { get; set; }
        }
    }
}