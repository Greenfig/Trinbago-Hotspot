using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Trinbago_MVC5.Areas.Admin.Models
{
    public class RegionAddForm
    {
        [Key, HiddenInput]
        public int CountryId { get; set; }
        [Required, Display(Name = "Region")]
        public string Name { get; set; }
        [Display(Name = "Longitude (decimal)")]
        public decimal Lng { get; set; }
        [Display(Name = "Latitude (decimal)")]
        public decimal Lat { get; set; }
        [Display(Name = "Zoom (whole number)")]
        public int Zoom { get; set; }
    }

    public class RegionAdd
    {
        [Key]
        public int CountryId { get; set; }
        public string Name { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public int Zoom { get; set; }
    }

    public class RegionEditForm
    {
        public int Id { get; set; }
        [Required, HiddenInput]
        public int CountryId { get; set; }
        [Required, Display(Name = "Region")]
        public string Name { get; set; }
        [Display(Name = "Longitude")]
        public decimal Lng { get; set; }
        [Display(Name = "Latitude")]
        public decimal Lat { get; set; }
        [Display(Name = "Zoom")]
        public int Zoom { get; set; }
    }

    public class RegionEdit
    {
        public int Id { get; set; }
        [Required]
        public int CountryId { get; set; }
        [Required]
        public string Name { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public int Zoom { get; set; }
    }
}