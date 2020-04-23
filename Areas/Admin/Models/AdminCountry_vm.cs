using System.ComponentModel.DataAnnotations;

namespace Trinbago_MVC5.Areas.Admin.Models
{
    public class CountryAddForm
    {
        [Required, Display(Name = "Island Name")]
        [StringLength(40)]
        public string Name { get; set; }
    }

    public class CountryAdd
    {
        public string Name { get; set; }
    }
}