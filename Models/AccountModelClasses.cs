using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Controllers;
using Trinbago_MVC5.Extensions;

namespace Trinbago_MVC5.Models
{

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim("PosterName", this.PosterName));
            return userIdentity;
        }

        // Additional user details
        public ApplicationUser()
        {
            StringId = MySecurity.GetGen();
            ClassifiedAds = new List<ClassifiedAd>();
            Favourites = new List<Favourited>();
            OrderHistory = new List<Order_History>();
            PromotionCart = new List<CartItem>();
            MaximumAdCount = 5;
            RegisterDate = LastLogin = DateTime.Now;
        }
        public string StringId { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime LastLogin { get; set; }
        public string PosterName { get; set; }
        public string ContactName { get; set; }
        public string PhoneNumber2 { get; set; }
        public string PhoneNumber3 { get; set; }
        public int MaximumAdCount { get; set; }
        public ICollection<ClassifiedAd> ClassifiedAds { get; set; }
        public virtual ICollection<Favourited> Favourites { get; set; }
        public ICollection<Order_History> OrderHistory { get; set; }
        public virtual PremiumUserData PremiumUserData { get; set; }
        public ICollection<CartItem> PromotionCart { get; set; }
    }

    //Anonymous User
    [Table("AnonymousUser")]
    public class AnonymousUser
    {
        public AnonymousUser()
        {
            LastUsed = DateTime.Now;
            Favourites = new List<Favourited>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime LastUsed { get; set; }
        public virtual ICollection<Favourited> Favourites { get; set; }
    }
    
    [Table("Favourited")]
    public class Favourited
    {
        public int Id { get; set; }
        public int ClassifiedAdId { get; set; }
    }

    //Premium User Data
    [Table("PremiumUserData")]
    public class PremiumUserData
    {
        public int Id { get; set; }
        [Required]
        public string UserProfileId { get; set; }
        [Required]
        [Range(0, 5)]
        public double AverageRating { get; set; }
        public string UrlName { get; set; }
        public virtual ICollection<PremiumUserPhoto> PremiumUserPhotos { get; set; }
        public ICollection<PremiumUserReview> UserReviews { get; set; }
        public virtual ICollection<PremiumUserInfo> PremiumUserInfos { get; set; }
    }

    //prem user info
    [Table("PremiumUserInfo")]
    public class PremiumUserInfo
    {
        public int Id { get; set; }
        [StringLength(20)]
        public string Name { get; set; }
        [StringLength(150)]
        public string Description { get; set; }
    }

    //User review
    [Table("PremiumUserReview")]
    public class PremiumUserReview
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public PremiumUserReview ReplyedToUserReview { get; set; }
        [Required]
        public PremiumUserData IntendedUser { get; set; }
        [Required]
        public ApplicationUser UserThatReplied { get; set; }
        [Range(0, 5)]
        public double Rating { get; set; }
        [StringLength(300, MinimumLength = 10), Required]
        public string Description { get; set; }
    }

    //user photo
    [Table("PremiumUserPhoto")]
    public class PremiumUserPhoto
    {
        public PremiumUserPhoto()
        {
            StringId = MySecurity.GetGen();
        }

        public int Id { get; set; }
        public string StringId { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public int Position { get; set; }
        [Required]
        public ApplicationUser ApplicationUser { get; set; }
    }
}