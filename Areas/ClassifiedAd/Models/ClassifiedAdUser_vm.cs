using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Trinbago_MVC5.Models;
using ClassifiedAdAlias = Trinbago_MVC5.Models.ClassifiedAd;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Models
{
    public class ClassifiedAdMyAdRenew
    {
        public int Id { get; set; }

        public string StringId { get; set; }

        public string AdPhoto { get; set; }

        public string Title { get; set; }

        public DateTime ExpiryTimeStamp { get; set; }
    }

    public class ClassifiedAdQueueList
    {
        public int Id { get; set; }

        public string StringId { get; set; }

        public DateTime TimeStamp { get; set; }

        [Display(Name = "Ad Title")]
        public string Title { get; set; }

        public int Status { get; set; }
    }
    
    /**
     * Display specified user's list
     * */
    public class ClassifiedAdMyList : ClassifiedAdQueueList
    {
        public string AdPhoto { get; set; }

        public bool NeedApproval { get; set; }

        public string AdType { get; set; }

        public int AdViewsCount { get; set; }

        public DateTime EditTimeStamp { get; set; }

        public DateTime ExpiryTimeStamp { get; set; }

        public int UsersQuestionCount { get; set; }
    }

    public class MyAdList
    {
        public MyAdList()
        {
            SelectLists = new List<SelectListForm>(){
                new SelectListForm() { Name = "searchType", List = new SelectList(new List<string>() { "All", "Open", "Renewable", "Sold", "Rented", "Pending", "Suspended" })}
            };
        }

        public List<SelectListForm> SelectLists { get; set; }

        public IPagedList<ClassifiedAdMyList> MyAds { get; set; }
    }

    public class UserAdList
    {
        public string Id { get; set; }
        public ICollection<ClassifiedAdMyList> ClassifiedAds { get; set; }
    }

    public class UserAdListPending : UserAdList { }
    public class UserAdListSuspended : UserAdList { }
    public class UserAdListOpen : UserAdList { }
    public class UserAdListSold : UserAdList { }
    public class UserAdListRented : UserAdList { }

    public class UserComparer : IEqualityComparer<ClassifiedAdAlias>
    {
        public bool Equals(ClassifiedAdAlias x, ClassifiedAdAlias y)
        {
            //Check whether the compared objects reference the same data.
            if (x.StringId == y.StringId) return true;
            return false;
        }

        public int GetHashCode(ClassifiedAdAlias obj)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(obj, null)) return 0;
            //Get hash code for the Name field if it is not null.
            int hashProductName = obj.StringId == null ? 0 : obj.StringId.GetHashCode();
            //Calculate the hash code for the product.
            return hashProductName;
        }
    }
}