using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Models
{
    public class ClassifiedAdQForm
    {
        [Required]
        public string CategoryName { get; set; }

        [Required, Display(Name="Name")]
        public string Name { get; set; }

        [Required]
        public string ItemUrl { get; set; }

        [Required]
        public string StringId { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string From { get; set; }

        [Required, StringLength(600)]
        [DataType(DataType.MultilineText)]
        [Display(Name= "Message")]
        public string Message { get; set; }

        [Display(Name = "Resume/CV")]
        [DataType(DataType.Upload)]
        public string FileUpload { get; set; }
    }

    public class ClassifiedAdQ
    {
        public string To { get; set; }
        
        public string Name { get; set; }

        public string ItemUrl { get; set; }

        [Required]
        public string StringId { get; set; }

        [Required]
        public string AdTitle { get; set; }

        public string From { get; set; }

        public string Message { get; set; }

        public HttpPostedFileBase FileUpload { get; set; }
    }
}