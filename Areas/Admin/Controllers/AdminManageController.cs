using System.Web.Mvc;
using Trinbago_MVC5.Areas.Admin.Models;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [RoutePrefix("Manage")]
    [Route("{action}")]
    [Authorize(Roles = "Admin")]
    public class AdminManageController : AdminBaseController
    {
        [HttpGet]
        public ActionResult CategoryAdd()
        {
            Category newform = new Category();
            return View(newform);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryAdd(Category newItem)
        {
            if (!ModelState.IsValid)
            {
                return View(newItem);
            }

            var addedItem = AdminManager.AddCategory(newItem);

            if (addedItem == null)
            {
                return View(newItem);
            }
            else
            {
                return RedirectToAction("CategoryList");
            }
        }

        [HttpGet]
        public ActionResult CategoryEdit(int id)
        {
            var val = AdminManager.GetOneCatById(id);
            return View(val);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryEdit(int id, Category edititem)
        {
            if (!ModelState.IsValid)
            {
                return View(edititem);
            }

            var addedItem = AdminManager.EditCategory(edititem);

            if (addedItem == null)
            {
                return View(edititem);
            }
            else
            {
                CacheHelper.RemoveFromCache(string.Format("getcatlist-{0}", 0));
                CacheHelper.RemoveFromCache(string.Format("getcatlist-{0}", addedItem.Id));
                CacheHelper.RemoveFromCache("getnavibarcatlist");
                return RedirectToAction("CategoryList");
            }
        }

        [HttpGet]
        public ActionResult CategorySeoEdit(int id)
        {
            var val = AdminManager.GetOneCatSeoById(id);
            return View(val);
        }

        [HttpGet]
        public ActionResult SubCategoryAdd(int id)
        {
            SubCategoryAddForm newform = new SubCategoryAddForm();
            newform.CategoryId = id;
            return View(newform);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubCategoryAdd(SubCategoryAdd newItem)
        {
            if (!ModelState.IsValid)
            {
                return View(newItem);
            }

            var addedItem = AdminManager.AddSubCategory(newItem);

            if (addedItem == null)
            {
                return View(newItem);
            }
            else
            {
                CacheHelper.RemoveFromCache(string.Format("getcatlist-{0}", addedItem.Category.Id));
                CacheHelper.RemoveFromCache("getnavibarcatlist");
                return RedirectToAction("CategoryDetails", new { id = newItem.CategoryId });
            }
        }

        [HttpGet]
        public ActionResult SubCategoryEdit(string id, int catid)
        {
            var val = AdminManager.GetOneSubCatById(id);
            val.CategoryId = catid;
            return View(val);
        }

        [HttpPost]
        public ActionResult SubCategoryEdit(SubCategoryEdit edititem)
        {
            if (!ModelState.IsValid)
            {
                return View(edititem);
            }

            var addedItem = AdminManager.EditSubCat(edititem);

            if (addedItem == null)
            {
                return View(edititem);
            }
            else
            {
                CacheHelper.RemoveFromCache(string.Format("getcatlist-{0}", addedItem.Id));
                CacheHelper.RemoveFromCache("getnavibarcatlist");
                return RedirectToAction("CategoryDetails", new { id = edititem.CategoryId });
            }
        }

        //
        // GET: /Country/Create
        [HttpGet]
        public ActionResult CreateCountry()
        {
            CountryAddForm newItem = new CountryAddForm();
            return View(newItem);
        }

        //
        // POST: /Country/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCountry(CountryAdd newItem)
        {
            if (!ModelState.IsValid)
            {
                return View(newItem);
            }

            var addedItem = AdminManager.AddCountry(newItem);

            if (addedItem == null)
            {
                return View(newItem);
            }
            else
            {
                return RedirectToAction("Country");
            }

        }

        [HttpGet]
        public ActionResult RegionAdd(int id)
        {
            // Attempt to get the matching object
            var o = AdminManager.GetCountryById(id);

            if (o == null)
            {
                return HttpNotFound();
            }
            else
            {
                // Create a form
                var form = new RegionAddForm();
                // Configure its values
                form.CountryId = o.Id;

                // Pass the object to the view
                return View(form);
            }
        }


        // POST: Properties/5/AddPhoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegionAdd(int id, RegionAdd newItem)
        {
            // Validate the input
            // Two conditions must be checked
            if (!ModelState.IsValid && id == newItem.CountryId)
            {
                return View(newItem);
            }

            // Process the input
            var addedItem = AdminManager.AddRegion(newItem);

            if (addedItem == null)
            {
                return View(newItem);
            }
            else
            {
                return RedirectToAction("CountryDetails", new { id = id });
            }
        }

        [HttpGet]
        [Route("RegionEdit/{id:int}")]
        public ActionResult RegionEdit(int id)
        {
            RegionEditForm reg = AdminManager.GetRegionById<RegionEditForm>(id);

            return View(reg);
        }

        [HttpPost]
        [Route("RegionEdit/{id:int}")]
        [ValidateAntiForgeryToken]
        public ActionResult RegionEdit(int id, RegionEdit editItem)
        {
            // Validate the input
            // Two conditions must be checked
            if (!ModelState.IsValid)
            {
                return View(editItem);
            }

            // Process the input
            var editedItem = AdminManager.EditRegion(editItem);

            if (editItem == null)
            {
                return View(editItem);
            }
            else
            {
                return RedirectToAction("CountryDetails", new { id = editItem.CountryId });
            }
        }


        public ActionResult AdminTools()
        {
            return View();
        }

        public ActionResult AdminCountAds()
        {
            AdminManager.AdminCountAds();
            return RedirectToAction("AdminTools");
        }

        public ActionResult AdminRemoveUser(string userid, string userStringId)
        {
            AdminManager.AdminRemoveUser(userid);
            return RedirectToAction("MemberList", "Admin");
        }
        
        public ActionResult AdminRemoveUserAds(string userid)
        {
            AdminManager.AdminRemoveUserAds(userid);
            return RedirectToAction("MemberDetails", "Admin", new { id = userid });
        }

        public ActionResult AdminUpdateLuceneIndex()
        {
            AdminManager.AdminUpdateLuceneSearchEngine();
            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        //
        // GET: /Country/
        [HttpGet]
        public ActionResult CountryList()
        {
            return View(AdminManager.GetAllCountriesWithRegionCount());
        }

        //
        // GET: /Country/Details/5
        [HttpGet]
        public ActionResult CountryDetails(int id)
        {
            return View(AdminManager.GetCountryById(id));
        }

        [HttpGet]
        public ActionResult CategoryList()
        {
            return View(AdminManager.GetAllCatNameOnly());
        }

        [HttpGet]
        public ActionResult CategoryDetails(int id)
        {
            return View(AdminManager.GetOneCatById(id));
        }
    }
}