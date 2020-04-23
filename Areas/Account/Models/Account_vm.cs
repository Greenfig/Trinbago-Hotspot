using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Trinbago_MVC5.Areas.Account.Models
{
    public class ExternalLoginConfirmation
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Poster Name (E.g. Posted By: Bob123, Bob's Cars Ltd.)")]
        [StringLength(250, MinimumLength = 3)]
        public string PosterName { get; set; }
    }

    public class ExternalLoginList
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCode
    {
        public string SelectedProvider { get; set; }
        public ICollection<SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCode
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class Forgot
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class Login
    {
        [Required]
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }

    public class BasePassword
    {
        [Required]
        //[RegularExpression(@"(?=.*[a-z])(?=.*[A-Z])(?=.*\d)([!""#$%&'()*+,\-.:;<=>?@[\\\]^_`{|}~])*[A-Za-z\d!""#$%&'()*+,\-.:;<=>?@[\\\]^_`{|}~]{8,}",
        //    ErrorMessage = "Password must contain at least 8 characters long and contain 1 letter and 1 number")]
        [RegularExpression(@"(?=.*[a-z])(?=.*\d)([!""#$%&'()*+,\-.:;\/<=>?@[\\\]^_`{|}~])*[A-Za-z\d!""#$%&'()*+,\-.:;\/<=>?@[\\\]^_`{|}~]{8,}",
            ErrorMessage = "Password must contain at least 8 characters long and contain 1 letter and 1 number.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class Register : BasePassword
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //
        // new
        [Display(Name = "Contact Name")]
        [StringLength(maximumLength: 50, MinimumLength = 2)]
        public string ContactName { get; set; }

        [Display(Name = "Contact Phone Number")]
        [RegularExpression(@"^[1-9]{1}[0-9]{2}((\-[0-9]{4})|([0-9]{4}))$", ErrorMessage = "Invalid phone number format. Example(123-1234 OR 1231234)")]
        public string ContactNumber { get; set; }

        [Required]
        [Display(Name = "Poster Name (E.g. Bob123, Bob's Cars Ltd.)")]        
        [StringLength(50, MinimumLength = 3)]
        public string PosterName { get; set; }
    }

    public class ResetPassword : BasePassword
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string Code { get; set; }
    }
    
    public class ForgotPass
    {
        [Display(Name = "Enter account email address")]
        [Required]
        public string Email { get; set; }
    }

    public class ForgotPassChange : BasePassword
    {
        public string Key { get; set; }
    }
}
