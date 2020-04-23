using System.Web.Mvc;
using Trinbago_MVC5.Managers;

namespace Trinbago_MVC5.Controllers
{
    [RoutePrefix("Seo")]
    [Route("{action}")]
    public class SeoController : Controller
    {
        protected SeoManager _managerSeo;

        public SeoManager ManagerSeo
        {
            get { return _managerSeo ?? new SeoManager(); }
            set { _managerSeo = value; }
        }

        [HttpGet]
        public JsonResult FetchSeoLocationName(int cid, int rid = 0)
        {
            return Json(ManagerSeo.GetSeoLocation(cid, rid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult FetchSeoCategoryName(int catId = 0, int scatId = 0)
        {
            return Json(ManagerSeo.GetSeoCategory(catId, scatId), JsonRequestBehavior.AllowGet);
        }
    }
}