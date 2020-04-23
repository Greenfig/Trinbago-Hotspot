using System.Web.Mvc;

/// <summary>
/// Used in more than one view model
/// </summary>
namespace Trinbago_MVC5.Models
{
    public class SelectListForm
    {
        public string Name { get; set; }
        public SelectList List { get; set; }
    }

    public class InfoForm
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}