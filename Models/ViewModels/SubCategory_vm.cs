using System.Collections.Generic;

namespace Trinbago_MVC5.Models
{
    public class SubCategoryBase
    {
        public int Id { get; set; }
        public string StringId { get; set; }
    }
    
    public class SubCategoryLucene : SubCategoryBase
    {
        public string Name { get; set; }
        public string SeoName { get; set; }
    }

    public class SubCategoryList : SubCategoryBase
    {
        public string SeoName { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public int ClassifiedAdsCount { get; set; }
    }

    public class AdInfoTemplateSlim : SubCategoryBase
    {
        public ICollection<InfoForm> RecommendedInfo { get; set; }
    }

    public class SubCategoryCreateAd : SubCategoryBase
    {
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public AdInfoTemplateSlim AdInfoTemplate { get; set; }
    }
}