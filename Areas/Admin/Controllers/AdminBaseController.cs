using System.Web.Mvc;
using Trinbago_MVC5.Areas.Admin.Managers;
using Trinbago_MVC5.Areas.Promotion.Managers;

namespace Trinbago_MVC5.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, Moderator")]
    public class AdminBaseController : Controller
    {
        protected AdministratorManager _adminManager;

        protected PromotionManager _promotionManager;

        public PromotionManager PromotionManager
        {
            get { return _promotionManager ?? new PromotionManager(); }
            set { _promotionManager = value; }
        }

        public AdministratorManager AdminManager
        {
            get
            {
                return _adminManager ?? new AdministratorManager();
            }
            set
            {
                _adminManager = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if(_adminManager != null)
            {
                _adminManager.Dispose();
                _adminManager = null;
            }
            base.Dispose(disposing);
        }
    }
}