using System.Collections.Generic;
using Trinbago_MVC5.Areas.ClassifiedAd.Managers;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Areas.Sponsor.Models;
using Trinbago_MVC5.Controllers;

namespace Trinbago_MVC5.Models
{
    public class IndexPage : SearchBar
    {
        public int AdCount { get; set; }

        public IEnumerable<CategoryTile> CategoryTiles { get; set; }

        public IEnumerable<SponsoredAdBase> SponsoredAds { get; set; }

        public IEnumerable<ClassifiedAdList> ClassifiedAdListRecent { get; set; }

        public IEnumerable<ClassifiedAdList> ClassifiedAdRandomPicks { get; set; }

        public IndexPage(int countryId, int regionId, int catId, int subCatId) : base(countryId, regionId, catId, subCatId)
        {
            using (ClassifiedAdManager Manager = new ClassifiedAdManager())
            {
                CategoryTiles = Manager.GetCategoryTiles();

                foreach(var c in CategoryTiles)
                {
                    c.FeaturedAds = SearchEngineManager.GetCategoryTileFeaturedAds(c.Id);
                    c.RecentlyPosted = SearchEngineManager.GetRecentClassifiedAdIndex(c.Id);
                }

                // Ad Count
                AdCount = Manager.GetAdCount();
                
                // Recent Ad setup
                ClassifiedAdListRecent = SearchEngineManager.GetRecentClassifiedAdIndex();

                // Random Ad setup
                ClassifiedAdRandomPicks = SearchEngineManager.GetRandomPickedClassifiedAds();

                // Sponsored Ad setup            
            }
        }
    }

    public class CategoryTile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SeoName { get; set; }
        public IEnumerable<ClassifiedAdMinimal> FeaturedAds { get; set; }
        public IEnumerable<SubCategoryList> PopularSubcats { get; set; }
        public IEnumerable<ClassifiedAdTitle> RecentlyPosted { get; set; }
    }
}