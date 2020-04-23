using System.ComponentModel.DataAnnotations;

namespace Trinbago_MVC5.Models
{
    public class RegionBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RegionLucene : RegionBase
    {
        public string SeoName { get; set; }
    }

    public class RegionCoordinate : RegionBase
    {
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public int Zoom { get; set; }
    }

    public class RegionList
    {
        [Display(Name="Region Name")]
        public int Name { get; set; }

        [Display(Name="Country Name")]
        public string CountryName { get; set; }
    }
}