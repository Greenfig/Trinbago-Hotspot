using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.Admin.Models
{
    public class SubCategorySeoEditForm : SubCategoryBase
    {
        public string Name { get; set; }
        [StringLength(50), Display(Name = "Meta Keywords")]
        public string MetaKeywords { get; set; }
        [StringLength(60), Display(Name = "Meta Title")]
        public string MetaTitle { get; set; }
        [StringLength(maximumLength: 150, MinimumLength = 80), Display(Name = "Meta Description")]
        public string MetaDescription { get; set; }
    }

    public class SubCategorySeoEdit : SubCategoryBase
    {
        [StringLength(50)]
        public string MetaKeywords { get; set; }
        [StringLength(60)]
        public string MetaTitle { get; set; }
        [StringLength(maximumLength: 150, MinimumLength = 80)]
        public string MetaDescription { get; set; }
    }
    public class SubCategoryAddForm
    {
        [Required, HiddenInput]
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class SubCategoryAdd
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class SubCategoryEditForm
    {
        [Required, HiddenInput]
        public string StringId { get; set; }
        [Required, HiddenInput]
        public int CategoryId { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class SubCategoryEdit
    {
        [Required]
        public string StringId { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public string Name { get; set; }
    }

}