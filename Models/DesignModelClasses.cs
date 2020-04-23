using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Trinbago_MVC5.Extensions;

namespace Trinbago_MVC5.Models
{
    public class SeoFormat
    {
        public string SeoName { get; set; }
        //[StringLength(50)]
        //public string MetaKeywords { get; set; }
        //[StringLength(60)]
        //public string MetaTitle { get; set; }
        //[StringLength(maximumLength: 150, MinimumLength = 80)]
        //public string MetaDescription { get; set; }
    }
    [Table("Category")]
    // All models related to Ads
    public class Category : SeoFormat
    {
        public Category()
        {
            SubCategories = new List<SubCategory>();
        }

        public int Id { get; set; }
        [StringLength(30)]
        public string Name { get; set; }
        public int TotalClassifiedAdsCount { get; set; }
        public virtual ICollection<SubCategory> SubCategories { get; set; }
    }

    [Table("SubCategory")]
    public class SubCategory : SeoFormat
    {
        public SubCategory()
        {
            ClassifiedAds = new List<ClassifiedAd>();
            ClassifiedAdsCount = ClassifiedAds.Count;
        }
        public int Id { get; set; }
        [StringLength(25)]
        public string StringId { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        public Category Category { get; set; }
        public int ClassifiedAdsCount { get; set; }
        public virtual AdInfoTemplate AdInfoTemplate { get; set; }
        public ICollection<ClassifiedAd> ClassifiedAds { get; set; }
    }

    // Country
    [Table("Country")]
    public class Country
    {
        public Country()
        {
            Regions = new List<Region>();
            RegionCount = Regions.Count();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string SeoName { get; set; }
        public int RegionCount { get; set; }
        public ICollection<Region> Regions { get; set; }
    }

    // Region
    [Table("Region_MVC5")]
    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SeoName { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public int Zoom { get; set; }
        [Required]
        public Country Country { get; set; }
    }

    [Table("Info")]
    public class Info
    {
        public int Id { get; set; }
        [Required, StringLength(40), Index]
        public string Name { get; set; }
        [StringLength(40), Index]
        public string Description { get; set; }
        [Required]
        public ClassifiedAd ClassifiedAd { get; set; }
    }

    [Table("AdInfoTemplate")]
    public class AdInfoTemplate
    {
        public int Id { get; set; }
        [Required, StringLength(20)]
        public string TemplateName { get; set; }
        public virtual ICollection<AdInfoString> RecommendedInfo { get; set; }
    }

    [Table("AdInfoString")]
    public class AdInfoString
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Table("ClassifiedAdPhoto")]
    public class ClassifiedAdPhoto
    {
        public int Id { get; set; }
        public int PhotoLayoutIndex { get; set; }
        public string ContentType { get; set; }
        public string Raw_FileName { get; set; }
        public string AdDetails_FileName { get; set; }
        public string AdList_FileName { get; set; }
        public string Original_FileName { get; set; }
        [Required]
        public ClassifiedAd ClassifiedAd { get; set; }
    }

    [Table("ClassifiedAd")]
    public class ClassifiedAd
    {
        public ClassifiedAd()
        {
            TimeStamp = DateTime.Now;
            EditTimeStamp = DateTime.Now;
            Status = 0;
            EditCount = 0;
            AdInfo = new List<Info>();
            Photos = new List<ClassifiedAdPhoto>();
            Reports = new List<ClassifiedAdReport>();
            StringId = MySecurity.GetGen();
            ContactPrivacy = false;
            NeedApproval = true;
        }
        // All Ads contain these
        public int Id { get; set; }
        [StringLength(25)]
        public string StringId { get; set; }
        [Required]
        public string Title { get; set; }
        public int Price { get; set; }
        public string PriceInfo { get; set; }
        public int EditCount { get; set; }
        public bool ContactPrivacy { get; set; }
        [Required]
        public string AdContactName { get; set; }
        [Required]
        public string AdContactPhone { get; set; }
        public string AdContactPhone2 { get; set; }
        public string AdContactPhone3 { get; set; }
        [Required]
        public string AdContactEmail { get; set; }
        public virtual ApplicationUser Poster { get; set; }
        [Required]
        public DateTime TimeStamp { get; set; }
        public DateTime EditTimeStamp { get; set; }
        public DateTime? ExpiryTimeStamp { get; set; }
        public string Description { get; set; }
        public string HtmlFreeDescription { get; set; }
        /// <summary>
        /// -2 = PENDING(waiting to be added); -1 = SUSPENDED(ad closed by moderator); 0 = OPEN(Display Ad); 1 = SOLD(Ad closed by user); 2 = RENDTED(Ad closed by user)  3 = DELETE(Ad needs to be removed)
        /// </summary>
        [Required, Range(-2, 3)]
        public int Status { get; set; }
        public virtual AdViews AdViews { get; set; }
        public virtual Country Country { get; set; }
        public virtual Region Region { get; set; }
        public virtual Category Category { get; set; }
        public virtual SubCategory SubCategory { get; set; }
        public bool NeedApproval { get; set; }
        [Required]
        public string AdType { get; set; }
        public virtual AdPromotion AdPromotion { get; set; }
        // -----
        // Photo
        public virtual ICollection<ClassifiedAdPhoto> Photos { get; set; }
        // ----
        // Info
        public virtual ICollection<Info> AdInfo { get; set; }
        // -------
        // Reports
        public ICollection<ClassifiedAdReport> Reports { get; set; }
    }

    // Ad Views
    [Table("AdViews")]
    public class AdViews
    {
        public AdViews()
        {
            Count = 0;
        }
        public int Id { get; set; }
        [Index]
        public int ClassifiedAdId { get; set; }
        [Required, Index]
        public Guid UserId { get; set; }
        public int Count { get; set; }
    }
    // Ad Promotion
    [Table("AdPromotion")]
    public class AdPromotion
    {
        public int Id { get; set; }
        // --------------------
        // Ad Postition Counter
        public virtual BumpAd BumpAd { get; set; }
        public virtual FeaturedAd FeaturedAd { get; set; }
        public virtual TopAd TopAd { get; set; }
        public virtual UrgentAd UrgentAd { get; set; }
        [Required]
        public virtual ClassifiedAd ClassifiedAd { get; set; }
    }

    [Table("Promotion")]
    public abstract class Promotion
    {
        public int Id { get; set; }
        public int Duration { get; set; }
        public bool Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        public virtual AdPromotion AdPromotion { get; set; }
    }

    public class BumpAd : Promotion { }
    public class FeaturedAd : Promotion { }    
    public class TopAd : Promotion { }    
    public class UrgentAd : Promotion { }

    // Ad Promotion Info
    [Table("PromotionInfo")]
    public abstract class PromotionInfo
    {
        public int Id { get; set; }
        public double Price { get; set; }
        public int Interval { get; set; }
        public int Days { get; set; }
        public CartItem CartItem { get; set; }
    }

    public class BumpAdPI : PromotionInfo { }
    public class FeaturedAdPI : PromotionInfo { }
    public class TopAdPI : PromotionInfo { }
    public class UrgentAdPI : PromotionInfo { }

    [Table("Promotion_Cart")]
    public class CartItem
    {
        public int Id { get; set; }
        [Required]
        public ApplicationUser User { get; set; }
        [Required]
        public virtual ClassifiedAd ClassifiedAd { get; set; }
        public virtual BumpAdPI BumpAdPI { get; set; }
        public virtual FeaturedAdPI FeaturedAdPI { get; set; }
        public virtual TopAdPI TopAdPI { get; set; }
        public virtual UrgentAdPI UrgentAdPI { get; set; }
    }

    [Table("Order_History")]
    public class Order_History
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [StringLength(250)]
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }

    [Table("Mail_List")]
    public class Mail_List
    {
        public int Id { get; set; }
        [EmailAddress]
        public string Email { get; set; }
    }

    // Ad reporting class
    [Table("ClassifiedAdReport")]
    public class ClassifiedAdReport
    {
        public ClassifiedAdReport()
        {
            CreatedDate = new DateTime();
            OpenRequest = false;
            Status = 0;
        }
        public int Id { get; set; }
        public ClassifiedAd ClassifiedAd { get; set; }
        public string ReportingUser { get; set; }
        [Required, StringLength(20)]
        public string ReasonTitle { get; set; }
        [Required, StringLength(250)]
        public string ReasonDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool OpenRequest { get; set; }
        public int Status { get; set; }
    }

    [Table("MiscInfo")]
    public class MiscInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Descriptor { get; set; }
    }

    //---------------------------------
    // Sponsored Ads
    [Table("SponsoredAd")]
    public class SponsoredAd
    {
        public SponsoredAd()
        {
            StringId = MySecurity.GetGen();
        }
        public int Id { get; set; }
        public string StringId { get; set; }
        public string Name { get; set; }
        public ICollection<SponsoredPhoto> SponsoredPhotos { get; set; }
    }

    [Table("SponsoredPhoto")]
    public class SponsoredPhoto
    {
        public SponsoredPhoto()
        {
            StringId = MySecurity.GetGen();
            Start = new DateTime();
        }

        public int Id { get; set; }
        public string PageName { get; set; }
        public int Duriation { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int CountNum { get; set; }
        // Used for file location
        public string StringId { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        [Required]
        public SponsoredAd SponsoredAd { get; set; }
    }

    [Table("GenericMessage")]
    // Generic msg used for - Contact us/feedback ..ect
    public class GenericMessage
    {
        public GenericMessage()
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
}