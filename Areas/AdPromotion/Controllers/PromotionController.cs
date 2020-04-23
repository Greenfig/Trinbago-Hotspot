using System.Web.Mvc;
using Trinbago_MVC5.Areas.Promotion.Managers;
using Trinbago_MVC5.Areas.Promotion.Models;

namespace Trinbago_MVC5.Areas.Promotion.Controllers
{
    [Authorize]
    [RouteArea("Promotion")]
    [RoutePrefix("Promotion")]
    [Route("{action}")]
    public class PromotionController : Controller
    {

        protected PromotionManager _promotionManager;        

        public PromotionManager PromotionManager
        {
            get { return _promotionManager ?? new PromotionManager(); }
            set { _promotionManager = value; }
        }


        public ActionResult PromoteAd(int adId)
        {
            return null;
        }

        /// <summary>
        /// Gets the user's shopping cart if any
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CurrentPromotions()
        {
            return View(PromotionManager.GetUserPromotedAds());
        }

        /// <summary>
        /// Gets the user's open ads and promotion cart 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPromoteAds(int pageNumber = 1)
        {
            return View(new PromoteAd() { MyOpenAds = PromotionManager.GetUserNonPromotedAdList(pageNumber), CartItems = PromotionManager.GetUserShoppingCart() });
        }

        public ActionResult NonPromotedAdListPagination(int pageNumber = 1)
        {
            return PartialView("_MyOpenAdsPartial",PromotionManager.GetUserNonPromotedAdList(pageNumber));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPromotionToCart(ClassifiedAdPromotionListAddToCart newItem)
        {
            PromotionManager.AddPromotionToCart(newItem.Id, newItem.BumpAd, newItem.UrgentAd, newItem.TopAd, newItem.FeaturedAd);
            ModelState.Clear();
            return PartialView("_PromoteAdsContainer", new PromoteAd() { MyOpenAds = PromotionManager.GetUserNonPromotedAdList(newItem.PageNumber.Value), CartItems = PromotionManager.GetUserShoppingCart() });
        }
        
        public ActionResult RemovePromotionFromCart(ClassifiedAdPromotionListRemoveCart remItem)
        {
            PromotionManager.RemoveFromShoppingCart(remItem.Id);
            ModelState.Clear();
            return PartialView("_PromoteAdsContainer", new PromoteAd() { MyOpenAds = PromotionManager.GetUserNonPromotedAdList(remItem.PageNumber.Value), CartItems = PromotionManager.GetUserShoppingCart() });
        }

        /// <summary>
        /// Get the confim purchase... shows summary of items and different payment methods
        /// </summary>
        /// <returns></returns>
        public ActionResult ConfirmCartItemsPurchase()
        {
            return View(PromotionManager.GetUserShoppingCart());
        }

        public ActionResult PurchaseWithVISA()
        {
            return null;
            if (PromotionManager.AddPromotionsToClassifiedAd("VISA"))
            {
                return View("CurrentPromotions", PromotionManager.GetUserPromotedAds());
            }
            return View("PromoteAds", new PromoteAd() { MyOpenAds = PromotionManager.GetUserNonPromotedAdList(1), CartItems = PromotionManager.GetUserShoppingCart() });
        }

        public ActionResult OrderHistory()
        {
            return View(PromotionManager.GetUserOrderHistory());
        }

        protected override void Dispose(bool disposing)
        {
            if (_promotionManager != null)
            {
                _promotionManager.Dispose();
                _promotionManager = null;
            }
            base.Dispose(disposing);
        }
    }
}