using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Trinbago_MVC5.Areas.ClassifiedAd.Managers;

namespace Trinbago_MVC5.Models
{
    public class SearchBar
    {

        public IEnumerable<CategoryList> Categories { get; set; }

        public ICollection<SelectListForm> SelectLists { get; set; }

        public SubCategoryList SubCategory { get; set; }

        public SearchBar(int countryId, int regionId, int catId, int subCatId)
        {
            if (SelectLists == null)
                SelectLists = new List<SelectListForm>();

            using (ClassifiedAdManager Manager = new ClassifiedAdManager())
            {

                // Get All Categories Based on current catId
                Categories = Manager.GetCategoryList(catId);
                SubCategory = Categories.Where(c => c.SubCategories != null && c.SubCategories.Count > 0).SelectMany(x => x.SubCategories).FirstOrDefault(f => f.Id == subCatId);
                // For search Bar
                IEnumerable<DropDownCategory> searchBarCat = from cats in Categories
                                                             select new DropDownCategory()
                                                             {
                                                                 Id = cats.Id.ToString(),
                                                                 Name = cats.Name
                                                             };
                IEnumerable<DropDownCategory> searchBarCategoryList = new List<DropDownCategory>() { new DropDownCategory() { Id = "0", Name = "All Categories" } }; ;
                if (SubCategory != null)
                {
                    searchBarCategoryList = searchBarCategoryList.Concat(searchBarCat.Concat(
                        new List<DropDownCategory>(){
                                new DropDownCategory() { Id = SubCategory.StringId, Name = SubCategory.Name }
                        }));
                }
                else
                {
                    searchBarCategoryList = searchBarCategoryList.Concat(searchBarCat);
                }
                //Categories dropdown
                SelectLists = new List<SelectListForm>();

                //Categories dropdown
                SelectLists.Add(new SelectListForm() { Name = "SearchBarCategories", List = new SelectList(searchBarCategoryList, "Id", "Name", subCatId > 0 ? searchBarCategoryList.Last().Id : catId.ToString()) });
                //Country dropdown
                SelectLists.Add(new SelectListForm { Name = "CountryList", List = new SelectList(Manager.GetAllCountriesWithDefault(), "Id", "Name", countryId) });
                //region
                if (countryId > 0)
                {
                    var regions = Manager.GetAllRegionsByCountryIdWithDefault(countryId);
                    SelectLists.Add(new SelectListForm() { Name = "RegionList", List = new SelectList(regions, "Id", "Name", regionId) });
                }
                else
                {
                    SelectLists.Add(new SelectListForm() { Name = "RegionList", List = new SelectList(new List<RegionBase>() { new RegionBase() { Id = 0, Name = "-No Country Set-" } }, "Id", "Name", regionId) });
                }
            }
        }
    }
}
