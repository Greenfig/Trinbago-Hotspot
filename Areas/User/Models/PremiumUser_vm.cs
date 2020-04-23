using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Web;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.User.Models
{
    public class PremiumUserPhotoBase
    {
        public string StringId { get; set; }
        public string FileName { get; set; }
        public int Position { get; set; }
    }

    // List Detail Page
    public class PremiumUserListDetail
    {
        public int ClassifiedAdsViewCount { get; set; }
        public string UserProfileId { get; set; }
        public double AverageRating { get; set; }
        public string UrlName { get; set; }
        public int UserReviewsCount { get; set; }
        public PremiumUserPhotoBase PremiumUserPhoto { get; set; }
        public ICollection<PremiumUserInfoBase> PremiumUserInfos { get; set; }
    }

    // PREMIUM USERS
    public class PremiumUserProfilePage
    {
        public string UserProfilestringId { get; set; }
        [Display(Name = "User Name")]
        public string UserProfileUserName { get; set; }
        public string UserProfileId { get; set; }
        [Display(Name = "User Email")]
        public string UserProfileEmail { get; set; }
        [Display(Name = "Contact Name")]
        public string UserProfileContactName { get; set; }
        [Display(Name = "Contact Phone")]
        public string UserProfileContactNumber { get; set; }
        [Display(Name = "Url Route Name")]
        public string UrlName { get; set; }
        [Display(Name = "Premium User Name")]
        public string PremiumUserName { get; set; }
        [Display(Name = "Average Ratings")]
        public int AverageRating { get; set; }
        public int PremiumPhotosCount { get; set; }
        [Display(Name = "Total Reviews")]
        public int UserReviewsCount { get; set; }
        [Display(Name = "Total Views")]
        public int UserProfileClassifiedAdsViews { get; set; }
        public ICollection<PremiumUserInfoBase> PremiumUserInfos { get; set; }
    }

    public class PremiumUserPage
    {
        public string UserProfilestringId { get; set; }
        public string UserProfileUserName { get; set; }
        public string UserProfileId { get; set; }
        public string UserProfileEmail { get; set; }
        public string UserProfileContactName { get; set; }
        public string UserProfileContactNumber { get; set; }
        public string PremiumUserName { get; set; }
        [Display(Name = "Url Route Name")]
        public string UrlName { get; set; }
        public int AverageRating { get; set; }
        public int ClassifiedAdsViewCount { get; set; }
        public ICollection<PremiumUserPhotoBase> PremiumUserPhotos { get; set; }
        public IPagedList<PremiumUserClassifiedAdList> ClassifiedAds { get; set; }
        public ICollection<PremiumUserInfoBase> PremiumUserInfos { get; set; }
        public ICollection<PremiumUserReviewBase> UserReviews { get; set; }
    }


    // Edit Form
    public class PremiumUserProfileEditForm
    {
        public string UserProfilestringId { get; set; }
        [Required(ErrorMessage="Url name is required")]
        public string UrlName { get; set; }
        [Required(ErrorMessage="The primium username field is required")]
        public string PremiumUserName { get; set; }
        [StringLength(150),DataType(DataType.MultilineText)]
        public string PUserAddress { get; set; }
        public string PhotoUpload1 { get; set; }
        public string PhotoUpload2 { get; set; }
        public string PhotoUpload3 { get; set; }
        public bool PhotoUpload1bool { get; set; }
        public bool PhotoUpload2bool { get; set; }
        public bool PhotoUpload3bool { get; set; }

    }

    // Edit
    public class PremiumUserProfileEdit
    {
        public string UserProfilestringId { get; set; }
        [Required]
        public string UrlName { get; set; }
        [Required]
        public string PremiumUserName { get; set; }
        [StringLength(150), DataType(DataType.MultilineText)]
        public string PUserAddress { get; set; }
        public HttpPostedFileBase PhotoUpload1 { get; set; }
        public HttpPostedFileBase PhotoUpload2 { get; set; }
        public HttpPostedFileBase PhotoUpload3 { get; set; }
        public bool PhotoUpload1bool { get; set; }
        public bool PhotoUpload2bool { get; set; }
        public bool PhotoUpload3bool { get; set; }
    }

    public class PremiumUserReviewBase
    {
        public int Id { get; set; }
        [Required]
        public PremiumUserInfoBase ReplyedToUserReview { get; set; }
        public UserProfileBase IntendedUser { get; set; }
        public ClassifiedAdList IntededAd { get; set; }
        public UserProfileBase UserThatReplied { get; set; }
        [Range(0, 5)]
        public double Rating { get; set; }
    }

    public class PremiumUserClassifiedAdList
    {
        public PremiumUserClassifiedAdList()
        {
            AdInfo = new List<InfoForm>();
        }

        // Slug generation taken from http://stackoverflow.com/questions/2920744/url-slugify-algorithm-in-c
        public string GenerateSlug()
        {
            string phrase = string.Format("{0}-{1}", StringId, Title);

            string str = RemoveAccent(phrase).ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        private string RemoveAccent(string text)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(text);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public string StringId { get; set; }

        public string Title { get; set; }

        public string Price { get; set; }

        public string PriceInfo { get; set; }

        [Display(Name = "Posting Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}")]
        public DateTime TimeStamp { get; set; }

        public int Views { get; set; }

        public CountryBase Country { get; set; }

        public PhotoBase AdPhoto { get; set; }

        public bool TopAdStatus { get; set; }

        public bool UrgentAdStatus { get; set; }

        public bool FeaturedAdStatus { get; set; }

        public bool SocialAdStatus { get; set; }

        public ICollection<InfoForm> AdInfo { get; set; }
    }

    // Associated 

    public class PremiumUserInfoBase
    {
        [StringLength(20)]
        public string Name { get; set; }
        [StringLength(150)]
        public string Description { get; set; }
    }
}
