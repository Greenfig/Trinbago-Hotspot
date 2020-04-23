using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;

namespace Trinbago_MVC5.Areas.Admin.Models
{
    public class AdminMemberDetails
    {
        public string UserId { get; set; }
        public string StringId { get; set; }
        public string PosterName { get; set; }
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public IEnumerable<ClassifiedAdAdminBase> ClassifiedAds { get; set; }
        public bool IsUser { get; set; }
    }
}