using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.Admin.Models
{
    public class AdminAdListDetail
    {
        public AdminAdListDetail()
        {
            AdPhotos = new List<PhotoBase>();
        }

        public string StringId { get; set; }

        [HiddenInput]
        public string AdType { get; set; }

        public string Title { get; set; }

        public string Price { get; set; }

        [Display(Name = "Posting Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}")]
        public DateTime TimeStamp { get; set; }

        public DateTime EditTimeStamp { get; set; }

        public int Views { get; set; }

        public CountryBase Country { get; set; }

        public RegionBase Region { get; set; }

        public string Description { get; set; }

        public Category Category { get; set; }

        public SubCategory SubCategory { get; set; }

        public string PosterUserName { get; set; }

        [Display(Name = "Contact Name")]
        public string AdContactName { get; set; }

        [Display(Name = "Contact Phone")]
        public string AdContactPhone { get; set; }

        public string AdContactEmail { get; set; }

        public string PriceInfo { get; set; }

        // ---------
        // Photos
        public IEnumerable<PhotoBase> AdPhotos { get; set; }

        // ---------
        // Info
        public ICollection<InfoForm> AdInfo { get; set; }
    }

    public class AdminPromote
    {
        public IReadOnlyCollection<PromoDurationStruct> PromoDuration;
        public double UrgentAdPrice { get; set; }
        public double TopAdPrice { get; set; }
        public double FeaturedAdPrice { get; set; }
        public AdminPromoteClassifiedAd Ad { get; set; }
    }

    public class AdminPromoteClassifiedAd
    {
        public int Id { get; set; }
        public AdPromotion AdPromotion { get; set; }
    }

    public class AdminClassifiedAdEdit : ClassifiedAdEditBase
    {
        public string AdType { get; set; }
    }

    public class AdminClassifiedAdEditForm : ClassifiedAdEditForm
    {
        [Required, Display(Name = "Select Ad Type")]
        public string AdType { get; set; }

        public new void ConfigureForm(BaseApplicationManager m)
        {
            base.ConfigureForm(m);

            IEnumerable<MiscInfoNoId> tl;
            // Set radiobutton Ad Type
            if (CategoryName.Equals("Real Estate") || CategoryName.Equals("Jobs") || SubCategoryName.Equals("Pet Adoption") ||
                SubCategoryName.Equals("Pet & Animal Services") || SubCategoryName.Equals("Lost Pet") || CategoryName.Equals("Business Services"))
            {
                tl = m.GetAllAdTypes().Where(x => !x.Value.Equals("ALL") && !x.Value.Equals("TRADE")).ToList();
                // Rename
                if (SubCategoryName.Equals("Lost Pet"))
                {
                    tl.SingleOrDefault(x => x.Value.Equals("SELL")).Name = "Found";
                    tl.SingleOrDefault(x => x.Value.Equals("WANT")).Name = "Missing";
                }
                else if (CategoryName.Equals("Jobs"))
                {
                    tl.SingleOrDefault(x => x.Value.Equals("SELL")).Name = "Hiring (I am Hiring)";
                    tl.SingleOrDefault(x => x.Value.Equals("WANT")).Name = "Looking for (I am Looking For)";
                }
                else
                {
                    tl.SingleOrDefault(x => x.Value.Equals("SELL")).Name = "Offering (I am Selling)";
                    tl.SingleOrDefault(x => x.Value.Equals("WANT")).Name = "Looking For (I am Looking for)";
                }
            }
            else
            {
                tl = m.GetAllAdTypes().Where(x => !x.Value.Equals("ALL")).ToList();
                tl.SingleOrDefault(x => x.Value.Equals("SELL")).Name = "Offering (I am Selling)";
                tl.SingleOrDefault(x => x.Value.Equals("WANT")).Name = "Looking For (I am Looking for)";
                tl.SingleOrDefault(x => x.Value.Equals("TRADE")).Name = "Trading (I am Trading)";
            }
            SelectListForm.Add(new SelectListForm() { Name = "TypeList", List = new SelectList(tl, "Name", "Value") });
        }
    }
}