using System.ComponentModel.DataAnnotations;
using System.Web;
using Trinbago_MVC5.Extensions.AttributeClasses;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Models
{
    public class ClassifiedAdApplyToForm
    {
        [Required, Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        public string ItemUrl { get; set; }

        [Required]
        public string stringId { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string From { get; set; }

        [Required, StringLength(600)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Resume/CV")]
        [DataType(DataType.Upload), Required]
        public string FileUpload { get; set; }
    }

    public class ClassifiedAdApplyTo
    {
        public string To { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string stringId { get; set; }

        [Required]
        public string ItemUrl { get; set; }

        [Required, EmailAddress]
        public string From { get; set; }

        [Required, StringLength(600)]
        public string Message { get; set; }

        [FileBaseValidate(methodname: "ApplyTo")]
        public HttpPostedFileBase FileUpload { get; set; }
    }
}
