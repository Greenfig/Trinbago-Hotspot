using System.Web.Mvc;

namespace Trinbago_MVC5.Controllers
{
    [RoutePrefix("Errors")]
    [Route("{action}")]
    public class ErrorsController : Controller
    {
        // GET: 401 / 403
        public ActionResult Unauthorized()
        {
            HttpContext.Response.Clear();
            HttpContext.Response.StatusCode = 403;
            return View();
        }
        // GET: 404
        public ActionResult ResourceNotFound()
        {
            HttpContext.Response.Clear();
            HttpContext.Response.StatusCode = 404;
            return View();
        }
        // GET: 405
        public ActionResult MethodNotAllowed()
        {
            HttpContext.Response.Clear();
            HttpContext.Response.StatusCode = 405;
            return View();
        }
        // GET: 406
        public ActionResult NotAcceptable()
        {
            HttpContext.Response.Clear();
            HttpContext.Response.StatusCode = 406;
            return View();
        }
        // GET: 410
        public ActionResult PageGone()
        {
            HttpContext.Response.Clear();
            HttpContext.Response.StatusCode = 410;
            return View("ResourceNotFound");
        }
        // GET: 412
        public ActionResult PreconditionFailed()
        {
            HttpContext.Response.Clear();
            HttpContext.Response.StatusCode = 412;
            return View();
        }
        // GET: 500
        public ActionResult InternalServerError()
        {
            HttpContext.Response.Clear();
            HttpContext.Response.StatusCode = 500;
            return View();
        }
        // GET: 501
        public ActionResult NotImplemented()
        {
            HttpContext.Response.Clear();
            HttpContext.Response.StatusCode = 501;
            return View();
        }
        // GET: 502
        public ActionResult BadGateway()
        {
            HttpContext.Response.Clear();
            HttpContext.Response.StatusCode = 502;
            return View();
        }
        // GET: AdSuspended
        public ActionResult AdSuspended()
        {
            return View();
        }
        // GET: Generic
        public ActionResult ErrorPage()
        {
            return View();
        }
    }
}