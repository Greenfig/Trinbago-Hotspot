using System.Web.Mvc;
using Trinbago_MVC5.Areas.ClassifiedAd.Managers;
using Trinbago_MVC5.Controllers;
using Trinbago_MVC5.Managers;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Controllers
{
    public class ClassifiedAdBaseController : Controller
    {
        private ClassifiedAdManager _classifiedAdManager;
        
        private SeoManager _seoManager;

        protected ClassifiedAdManager ClassifiedAdManager
        {
            get { return _classifiedAdManager ?? new ClassifiedAdManager(); }
            set { _classifiedAdManager = value; }
        }

        protected SeoManager SeoManager
        {
            get { return _seoManager ?? new SeoManager(); }
            set { _seoManager = value; }
        }

        protected override void Dispose(bool disposing)
        {
            if (_classifiedAdManager != null)
            {
                _classifiedAdManager.Dispose();
                _classifiedAdManager = null;
            }
            if (_seoManager != null)
            {
                _seoManager.Dispose();
                _seoManager = null;
            }
            base.Dispose(disposing);
        }
    }
}