using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Controllers
{
    public class PremiumUserReviewListDetail
    {
        public string IntededAdTitle { get; set; }
        public string PremiumUserName { get; set; }
        [Required]
        public PremiumUserReview ReplyedToUserReview { get; set; }
        //public UserProfile IntendedUser { get; set; }
        public ClassifiedAd IntededAd { get; set; }
        //public UserProfile UserThatReplied { get; set; }
        [Range(0, 5)]
        public double Rating { get; set; }
        [StringLength(300, MinimumLength = 10)]
        public string Description { get; set; }
    }

    public class PremiumUserReviewAddForm
    {
        [Required]
        public string Title { get; set; }
        public PremiumUserReview ReplyedToUserReview { get; set; }
        [Range(0, 5)]
        public double Rating { get; set; }
        [StringLength(300, MinimumLength = 10), Required]
        public string Description { get; set; }
    }

    public class PremiumUserReviewAdd
    {
        public string Title { get; set; }
        public PremiumUserReview ReplyedToUserReview { get; set; }
        //[Required]
        //public UserProfile IntendedUser { get; set; }
        [Required]
        public ClassifiedAd IntededAd { get; set; }
        //[Required]
        //public UserProfile UserThatReplied { get; set; }
        [Range(0, 5)]
        public double Rating { get; set; }
        [StringLength(300, MinimumLength = 10), Required]
        public string Description { get; set; }
    }
}
