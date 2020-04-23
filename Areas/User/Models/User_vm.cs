using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;

namespace Trinbago_MVC5.Areas.User.Models
{
    public class MyProfile
    {
        [Display(Name = "Poster Name")]
        public string PosterName { get; set; }
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }
        [Display(Name = "Contact Number 2")]
        public string ContactNumber2 { get; set; }
        [Display(Name = "Contact Number 3")]
        public string ContactNumber3 { get; set; }
        [Display(Name = "User Email")]
        public string Email { get; set; }
    }

    public class MyFavouriteList
    { 
        public IPagedList<ClassifiedAdFavouriteList> MyFavourites { get; set; }
    }

    public class ClassifiedAdFavouriteList
    {
        public int Id { get; set; }
        public string AdPhoto { get; set; }
        public string Title { get; set; }
        public string Price { get; set; }
        public DateTime EditTimeStamp { get; set; }
        public int Status { get; set; }
    }

    public class ContactInfo
    {
        [Display(Name = "Contact Name")]
        [StringLength(maximumLength: 50, MinimumLength = 2)]
        public string ContactName { get; set; }

        [Display(Name = "Contact Number 1")]
        [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
        public string ContactNumber { get; set; }

        [Display(Name = "Contact Number 2")]
        [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
        public string ContactNumber2 { get; set; }

        [Display(Name = "Contact Number 3")]
        [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
        public string ContactNumber3 { get; set; }

    }

    public class UserProfileBase
    {
        public UserProfileBase()
        {
            RoleNames = new List<string>();
        }
        public string Id { get; set; }
        public string PosterName { get; set; }
        public string Email { get; set; }
        public ICollection<string> RoleNames { get; set; }
    }

    public class UserProfileContact
    {
        
        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string Email { get; set; }
    }
}