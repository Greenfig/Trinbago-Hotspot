using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Trinbago_MVC5.Models
{
    public class CountryBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CountryLucene : CountryBase
    {
        public string SeoName { get; set; }
    }
 
    public class CountryList
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Island Name")]
        public string Name { get; set; }

        [Display(Name="Number of Regions")]
        public int RegionCount { get; set; }
    }

    public class CountryWithDetail : CountryList
    {
        public CountryWithDetail()
        {
            Regions = new List<RegionBase>();
        }
        public ICollection<RegionBase> Regions { get; set; }
    }

    public class CountryAdList : CountryLucene
    {
        public ICollection<RegionLucene> Regions { get; set; }
    }
}