using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Trinbago_MVC5.Models
{
    public class ContactUs
    {
        [EmailAddress,Required]
        public string ReturnTo { get; set; }

        [Required,StringLength(60)]
        public string Title { get; set; }

        [StringLength(maximumLength: 400, MinimumLength = 10)]
        [DataType(DataType.MultilineText),Required]
        public string Description { get; set; }
    }

    public class ContactUsForm
    {
        [Required,Display(Name="Email"),EmailAddress]
        public string ReturnTo { get; set; }

        [Required,StringLength(60)]
        public string Title { get; set; }

        [StringLength(maximumLength:400,MinimumLength=10)]
        [DataType(DataType.MultilineText), Required]
        public string Description { get; set; }
    }

    // Admin
    public class GenericMessageQueue
    {
        [HiddenInput]
        public int Id { get; set; }
        public int Status { get; set; }
        public string ReturnTo { get; set; }
        [HiddenInput]
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PostDate { get; set; }
    }

    // Hangfire
    public class HangfireMessage
    {
        public string To { get; set; }
        public string UserName { get; set; }
        public string AdUrl { get; set; }
        public string ExtendUrl { get; set; }
        public string DateTime { get; set; }
    }
}
