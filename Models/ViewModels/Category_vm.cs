using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Trinbago_MVC5.Managers;

namespace Trinbago_MVC5.Models
{
    public class CategoryBase
    {
        public int Id { get; set; }
    }

    public class DropDownCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class SearchBarCategory : CategoryBase
    {
        public int CategoryId { get; set; }
    }

    public class CategorySeoEditForm : CategoryBase
    {
        public string Name { get; set; }
        [StringLength(50), Display(Name = "Meta Keywords")]
        public string MetaKeywords { get; set; }
        [StringLength(60), Display(Name = "Meta Title")]
        public string MetaTitle { get; set; }
        [StringLength(maximumLength: 150, MinimumLength = 80), Display(Name = "Meta Description")]
        public string MetaDescription { get; set; }
    }

    public class CategorySeoEdit : CategoryBase
    {
        [StringLength(50)]
        public string MetaKeywords { get; set; }
        [StringLength(60)]
        public string MetaTitle { get; set; }
        [StringLength(maximumLength: 150, MinimumLength = 80)]
        public string MetaDescription { get; set; }
    }

    public class CategoryLucene : CategoryBase
    {
        public string Name { get; set; }
        public string SeoName { get; set; }
    }

    public class CategoryList : CategoryBase
    {
        public string Name { get; set; }
        public string SeoName { get; set; }
        public ICollection<SubCategoryList> SubCategories { get; set; }
        public int TotalClassifiedAdsCount { get; set; }
    }

    public class CategorySelectAdList
    {
        public IEnumerable<CategoryList> CategoryList { get; set; }
        public ICollection<SelectListForm> SelectLists { get; set; }

        public CategorySelectAdList(int countryId, int regionId, int catId, int subCatId)
        {
            var Manager = new BaseApplicationManager();

                // Get All Categories Based on current catId
            CategoryList = Manager.GetCategoryList();
            var SubCategory = CategoryList.Where(c => c.SubCategories != null && c.SubCategories.Count > 0).SelectMany(x => x.SubCategories).FirstOrDefault(f => f.Id == subCatId);
            // For search Bar
            IEnumerable<DropDownCategory> searchBarCat = from cats in CategoryList
                                                         select new DropDownCategory()
                                                         {
                                                             Id = cats.Id.ToString(),
                                                             Name = cats.Name
                                                         };
            IEnumerable<DropDownCategory> searchBarCategoryList;
            if (SubCategory != null)
            {
                searchBarCategoryList = searchBarCat.Concat(
                    new List<DropDownCategory>(){
                            new DropDownCategory() { Id = SubCategory.StringId, Name = SubCategory.Name }
                    });
            }
            else
            {
                searchBarCategoryList = searchBarCat;
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
                SelectLists.Add(new SelectListForm() { Name = "RegionList", List = new SelectList(new List<RegionBase>() { new RegionBase() { Id = 0, Name = "-Region-" } }, "Id", "Name", regionId) });
            }
        }
    }
}