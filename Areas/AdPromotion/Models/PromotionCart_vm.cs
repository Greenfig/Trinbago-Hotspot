using PagedList;
using System.Collections.Generic;

namespace Trinbago_MVC5.Areas.Promotion.Models
{
    public class CartItemList
    {
        public string ClassifiedAdStringId { get; set; }
        public string ClassifiedAdTitle { get; set; }
        public int ClassifiedAdId { get; set; }
        public string ClassifiedAdAdPhoto { get; set; }
        public double? BumpAdPIPrice { get; set; }
        public int? BumpAdPIInterval { get; set; }
        public int? BumpAdPIDays { get; set; }
        public double? FeaturedAdPIPrice { get; set; }
        public int? FeaturedAdPIDays { get; set; }
        public double? TopAdPIPrice { get; set; }
        public int? TopAdPIDays { get; set; }
        public double? UrgentAdPIPrice { get; set; }
        public int? UrgentAdPIDays { get; set; }

    }

    public class ClassifiedAdPromoted : CartItemList
    {
        public string StringId { get; set; }
        public string Title { get; set; }
        public string AdPhoto { get; set; }
    }

    public class ClassifiedAdPromotionList
    {
        public string StringId { get; set; }
        public string Title { get; set; }
        public string AdPhoto { get; set; }
    }

    public class ClassifiedAdPromotionListRemoveCart
    {
        public int Id { get; set; }
        public int? PageNumber { get; set; }
    }

    public class ClassifiedAdPromotionListAddToCart
    {
        public int Id { get; set; }
        public int? BumpAd { get; set; }
        public bool UrgentAd { get; set; }
        public int? TopAd { get; set; }
        public int? FeaturedAd { get; set; }
        public int? PageNumber { get; set; }
    }

    // Promote Ad
    public class PromoteAd
    {
        public IPagedList<ClassifiedAdPromotionList> MyOpenAds { get; set; }
        public IEnumerable<CartItemList> CartItems { get; set; }
    }

    public class UserCartBase
    {
        public string Id { get; set; }
    }

    public class UserCartItemSlim : UserCartBase
    {
        public ICollection<CartItemList> PromotionCart { get; set; }
    }

    public class UserPromotedList
    {
        public string Id { get; set; }
        public ICollection<ClassifiedAdPromoted> PromotedList { get; set; }
    }

    public class UserPromoteAdPage
    {
        public string Id { get; set; }
        public ICollection<ClassifiedAdPromoted> PromotedList { get; set; }
        public ICollection<ClassifiedAdPromotionList> MyAdList { get; set; }
    }

    public class UserAdListSlim : UserCartBase
    {
        public ICollection<ClassifiedAdPromotionList> MyAdList { get; set; }
    }

}