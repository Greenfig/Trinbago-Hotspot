using System.Web;
using System.Web.Mvc;
using Trinbago_MVC5.Areas.ClassifiedAd.Managers;
using Trinbago_MVC5.Areas.User.Managers;
using Trinbago_MVC5.Areas.User.Models;

namespace Trinbago_MVC5.Areas.User.Controllers
{
    [RouteArea("User")]
    [Route("{action}")]
    [Authorize(Roles = "Admin, Moderator, User")]
    public class UserController : Controller
    {
        private UserManager _userManager;

        private ClassifiedAdManager _classifiedAdManager;

        private UserManager UserManager
        {
            get
            {
                return _userManager ?? new UserManager();
            }
            set
            {
                _userManager = value;
            }
        }

        private ClassifiedAdManager ClassifiedManager
        {
            get
            {
                return _classifiedAdManager ?? new ClassifiedAdManager();
            }
            set
            {
                _classifiedAdManager = value;
            }
        }

        //
        // Get: /Account/Account        
        public ActionResult MyProfile()
        {
            if (User.IsInRole("Premium"))
            {
                return RedirectToAction("PUserProfile", "Premium");
            }            
            return View(UserManager.GetCurrentUserProfile());
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult MyFavourites()
        {
            return View(new MyFavouriteList() { MyFavourites = UserManager.GetMyFavouritedAds() });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult MyFavouritesListPagination(int pageNumber = 1)
        {
            return PartialView("_MyFavouriteList", UserManager.GetMyFavouritedAds(pageNumber));
        }

        //
        // GET: /Account/ContactInfo
        public ActionResult ContactInfo()
        {
            return View(UserManager.GetContactInfo());
        }

        //
        // POST: /Account/ContactInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactInfo(ContactInfo model)
        {
            ViewBag.ReturnUrl = Url.Action("ContactInfo");

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else
            {
                var addedItem = UserManager.EditContactInfo(model);

                if (addedItem == null)
                {
                    return View(model);
                }
                else
                {
                    return RedirectToAction("MyProfile");
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult Favourite(int adId)
        {
            if (UserManager.Favourite(adId))
            {
                return Json(new { success = true, isFavourited = true });
            }
            else
            {
                return Json(new { success = true, isFavourited = false });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult RemoveFavourite(int adId, int pageNumber = 1)
        {
            UserManager.Favourite(adId);
            return PartialView("_MyFavouriteList", UserManager.GetMyFavouritedAds(pageNumber));
        }

        protected override void Dispose(bool disposing)
        {
            if(_userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }
            if(_classifiedAdManager != null)
            {
                _classifiedAdManager.Dispose();
                _classifiedAdManager = null;
            }
            base.Dispose(disposing);
        }
    }
}
