using System.ComponentModel.DataAnnotations;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Models
{
    public class ClassifiedAdEmailUserPost
    {
        public string AdTitle { get; set; }

        [Required]
        public string AdContactName { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string ItemUrl { get; set; }

        [Required]
        public string StringId { get; set; }

        public string To { get; set; }

        [Required, EmailAddress]
        public string From { get; set; }

        [Required, StringLength(600)]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
    }

    public class ClassifiedAdEmailUserForm
    {
        [Required]
        public string AdContactName { get; set; }

        [Required, Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string ItemUrl { get; set; }

        [Required]
        public string StringId { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string From { get; set; }

        [Required, StringLength(600)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Message")]
        public string Message { get; set; }
    }
}