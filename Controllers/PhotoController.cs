using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.UI;

namespace Trinbago_MVC5.Controllers
{
    [RoutePrefix("Photo")]
    [Route("{action}")]
    [SessionState(SessionStateBehavior.Disabled)]
    public class PhotoController : Controller
    {
        protected PhotoManager _managerPhoto;

        public PhotoManager ManagerPhoto { get { return _managerPhoto ?? new PhotoManager(); } set { _managerPhoto = value; } }

        [Route("~/Images/{adId:int}/{FileName}")]
        [OutputCache(Duration = 60 * 60 * 24 * 30 * 2, Location = OutputCacheLocation.Client)]
        public FilePathResult LoadLucenePhoto(int adId, string FileName)
        {
            var img = ManagerPhoto.GetAdDetailImageBytes(adId, FileName);
            if (img == null) { Response.StatusCode = 404; return null; }
            return File(img.FilePath, img.ContentType);
        }

        protected sealed override void Dispose(bool disposing)
        {
            if (_managerPhoto != null)
            {
                _managerPhoto.Dispose();
                _managerPhoto = null;
            }
            base.Dispose(disposing);
        }
    }
}