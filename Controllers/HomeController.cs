using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trinbago_MVC5.Extensions.AttributeClasses;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Managers;
using Trinbago_MVC5.Models;
using System;
using Hangfire;
using Trinbago_MVC5.Areas.Account.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Trinbago_MVC5.Areas.User.Managers;

namespace Trinbago_MVC5.Controllers
{
    [RoutePrefix("Home")]
    [Route("{action}")]
    public class HomeController : Controller
    {
        protected BaseApplicationManager _baseManager;

        public BaseApplicationManager BaseManager
        {
            get
            {
                return _baseManager ?? new BaseApplicationManager();
            }
            private set
            {
                _baseManager = value;
            }
        }
        
        [Route("~/")]
        public ActionResult Index()
        {
            //Set Temp Data
            int cid = 0;
            int rid = 0;
            int catid = 0;
            int scatid = 0;
            TempData.Clear();

            var item = new IndexPage(cid, rid, catid, scatid);
            ViewBag.ImgOpenGraphMeta = new HtmlString("<meta property='og:image' content='https://trinbagohotspot.com/images/category/HomePage.jpg' />");
            ViewBag.ImgLinkOverride = new HtmlString("<link rel='img_src' href='https://trinbagohotspot.com/images/category/HomePage.jpg' />");
            ViewBag.DesOpenGraphMeta = new HtmlString("<meta property='og:description' content='Local classified ads in Trinidad and Tobago. FIND or POST FREE classified ads for real estate, vehicles, pets, business services and more in Trinidad and Tobago.' />");
            ViewBag.MetaDesc = new HtmlString("<meta name='description' content='Local classified ads in Trinidad and Tobago. FIND or POST FREE classified ads for real estate, vehicles, pets, business services and more in Trinidad and Tobago.' />");
            ViewBag.TitleOpenGraphMeta = new HtmlString("<meta property='og:title' content='Free Classifieds in Trinidad & Tobago! Buy, Sell, Rent, Lease & Trade!' />");
            ViewBag.Title = "Free Classifieds in Trinidad & Tobago! Buy, Sell, Rent, Lease & Trade!";
            return View(item);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";
            ViewBag.Canonical = new HtmlString("<link rel=\"canonical\" href=\"" + Request.Url.AbsoluteUri + "\">");
            return View();
        }

        [HttpGet]
        [NoCache]
        public ActionResult Contact()
        {
            ViewBag.Canonical = new HtmlString("<link rel=\"canonical\" href=\"" + Request.Url.AbsoluteUri + "\">");
            return View(new ContactUsForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateRecaptcha2(ErrorMessage = @"Recaptcha is required!")]
        public ActionResult Contact(ContactUs newmsg)
        {
            ViewBag.Canonical = new HtmlString("<link rel=\"canonical\" href=\"" + Request.Url.AbsoluteUri + " \">");
            if (!ModelState.IsValid)
            {
                return View(Mapper.Map<ContactUsForm>(newmsg));
            }

            // add to db
            if (BaseManager.AddGenericMessage(Mapper.Map<GenericMessage>(newmsg)))
            {
                string sendTo;
                if (newmsg.Title.Equals("Feedback/Suggestions"))
                {
                    sendTo = "feedback@trinbagohotspot.com";
                }
                else if (newmsg.Title.Equals("Marketing") || newmsg.Title.Equals("How can i advertize on your site"))
                {
                    sendTo = "marketing@trinbagohotspot.com";
                }
                else
                {
                    sendTo = "support@trinbagohotspot.com";
                }
                EmailMessenger.SendContactUsMessage(newmsg, sendTo);
                ViewBag.Message = "Your message sent successfully. We will respond, if necessary, within 24hrs. Thank you.";
                ModelState.Clear();
                return View();
            }
            else
            {
                ViewBag.Message = "Message failed to send. Try again or use a different method.";
                return View();
            }
        }

        public ActionResult Guides()
        {
            ViewBag.Canonical = new HtmlString("<link rel=\"canonical\" href=\"" + Request.Url.AbsoluteUri + "\">");
            return View();
        }

        public ActionResult Terms()
        {
            ViewBag.Canonical = new HtmlString("<link rel=\"canonical\" href=\"" + Request.Url.AbsoluteUri + "\">");
            return View();
        }

        public ActionResult Privacy()
        {
            ViewBag.Canonical = new HtmlString("<link rel=\"canonical\" href=\"" + Request.Url.AbsoluteUri + "\">");
            return View();
        }

        public ActionResult Pricing()
        {
            ViewBag.Canonical = new HtmlString("<link rel=\"canonical\" href=\"" + Request.Url.AbsoluteUri + " \">");
            return View();
        }

        [Route("~/Robots.txt")]
        public ActionResult Robots()
        {
            Response.Clear();
            Response.ContentType = "text/plain";
            return View();
        }

        [Route("~/ads.txt")]
        public ActionResult AdSense()
        {
            Response.Clear();
            Response.ContentType = "text/plain";
            return View();
        }

        [Route("~/tbhs-fc-sitemap.xml")]
        public ContentResult SiteMap()
        {
            return Content(BaseManager.GenerateSiteMap().ToString(), "text/xml");
        }
        
        [Authorize(Roles = "Admin")]
        public ActionResult GenericFixerMethod()
        {
            //using (var newthreadcontext = new ApplicationDbContext())
            //{
            //    var regions = newthreadcontext.RegionDB;
            //    foreach(var r in regions)
            //    {
            //        r.SeoName = new SeoManager().GetSeoTitle(r.Name);
            //    }

            //    newthreadcontext.SaveChanges();
            //}
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (_baseManager != null)
            {
                _baseManager.Dispose();
                _baseManager = null;
            }
            base.Dispose(disposing);
        }
    }
}