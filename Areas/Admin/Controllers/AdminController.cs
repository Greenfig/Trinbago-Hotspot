using AutoMapper;
using Ganss.XSS;
using System.Web;
using System.Web.Mvc;
using Trinbago_MVC5.Areas.Admin.Controllers;
using Trinbago_MVC5.Areas.Admin.Models;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Extensions.AttributeClasses;

/// <summary>
/// Admin and moderator views
/// </summary>
namespace Trinbago_MVC5.Controllers
{
    [RouteArea("Admin")]
    [Route("{action}")]
    public class AdminController : AdminBaseController
    {
        [Route("AdminUserAdDetails/{stringId}")]
        public ActionResult AdminUserAdDetails(string stringId)
        {
            var mod1 = AdminManager.AdminGetClassifiedAdWithDetails(stringId);
            if (mod1 == null)
                return RedirectToAction("AdList", "ClassifiedAd", new { Area = "ClassifiedAd" });
            return View(mod1);
        }

        //
        // GET: /Members/
        public ActionResult MemberList()
        {
            return View(AdminManager.GetAllMembers());
        }

        [HttpGet]
        public ActionResult MemberDetails(string id)
        {
            return View(AdminManager.AdminMemberDetails(id));
        }

        public ActionResult ReportQueue()
        {
            TempData.Clear();
            return View(AdminManager.GetAllReports());
        }

        public ActionResult MessageQueue()
        {
            return View(AdminManager.AdminGetMessageQueue());
        }

        //
        // Post: /ClassifiedAd/MyAdClose
        [HttpPost]
        public HtmlString AdminCloseReport(int repId)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin") || User.Identity.IsAuthenticated && User.IsInRole("Moderator"))
            {
                bool i = AdminManager.AdminCloseReport(repId);
                if (i)
                {
                    return new HtmlString(
                        "<div class='modal-dialog'>" +
                            "<div class='modal-content'>" +
                                "<div class='modal-body'>" +
                                    "<span>Report successfully closed</span>" +
                                "</div>" +
                                "<div class='modal-footer'>" +
                                    "<button id='okIdButtonSucceed' type='button' class='btn btn-primary' data-dismiss='modal'>OK</button>" +
                                "</div>" +
                            "</div>" +
                        "</div>"
                   );
                }
                else
                {
                    return new HtmlString(
                        "<div class='modal-dialog'>" +
                            "<div class='modal-content'>" +
                                "<div class='modal-body'>" +
                                    "<span>Failed to close report</span>" +
                                "</div>" +
                                "<div class='modal-footer'>" +
                                    "<button id='okIdButtonFailure' type='button' class='btn btn-primary' data-dismiss='modal'>OK</button>" +
                                "</div>" +
                            "</div>" +
                        "</div>"
                   );
                }
            }
            else
            {
                return null;
            }
        }

        //
        // Get: /Admin/AdminCloseAd
        public ActionResult AdminCloseAd(int? adId, string closingOption)
        {

            if (adId.HasValue)
            {
                if (User.Identity.IsAuthenticated && User.IsInRole("Admin") || User.Identity.IsAuthenticated && User.IsInRole("Moderator"))
                {
                    bool i = AdminManager.AdminCloseAd(adId.Value, closingOption, Server);
                    if (i)
                    {
                        return RedirectToAction("ReportQueue", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("ReportQueue", "Admin");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        //
        // Get: /Admin/AdminCloseAd
        public ActionResult AdminOpenAd(int? adId)
        {
            if (adId.HasValue)
            {
                if (User.Identity.IsAuthenticated && User.IsInRole("Admin") || User.Identity.IsAuthenticated && User.IsInRole("Moderator"))
                {
                    bool i = AdminManager.AdminOpenAd(adId.Value);
                    if (i)
                    {
                        return RedirectToAction("ReportQueue", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("ReportQueue", "Admin");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        //
        // GET: /Admin/AdminSuspendAd/5
        public ActionResult AdminSuspendAd(int? adId)
        {
            if (adId.HasValue)
            {
                if (User.Identity.IsAuthenticated && User.IsInRole("Admin") || User.Identity.IsAuthenticated && User.IsInRole("Moderator"))
                {
                    bool i = AdminManager.AdminSuspendAd(adId.Value);
                    if (i)
                    {
                        return RedirectToAction("ReportQueue", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("ReportQueue", "Admin");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public string AdminRoleChange(string memname, string oldrole, string newrole)
        {
            if (!string.IsNullOrEmpty(newrole))
            {
                AdminManager.AdminRoleChange(memname, oldrole, newrole);
            }
            return newrole;
        }

        [HttpPost]
        public HtmlString AdminMessageClose(int msgId)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin") || User.Identity.IsAuthenticated && User.IsInRole("Moderator"))
            {
                bool i = AdminManager.AdminMessageClose(msgId);
                if (i)
                {
                    return new HtmlString(
                        "<div class='modal-dialog'>" +
                            "<div class='modal-content'>" +
                                "<div class='modal-body'>" +
                                    "<span>Message successfully closed</span>" +
                                "</div>" +
                                "<div class='modal-footer'>" +
                                    "<button id='okIdButtonSucceed' type='button' class='btn btn-primary' data-dismiss='modal'>OK</button>" +
                                "</div>" +
                            "</div>" +
                        "</div>"
                   );
                }
                else
                {
                    return new HtmlString(
                        "<div class='modal-dialog'>" +
                            "<div class='modal-content'>" +
                                "<div class='modal-body'>" +
                                    "<span>Failed to close report</span>" +
                                "</div>" +
                                "<div class='modal-footer'>" +
                                    "<button id='okIdButtonFailure' type='button' class='btn btn-primary' data-dismiss='modal'>OK</button>" +
                                "</div>" +
                            "</div>" +
                        "</div>"
                   );
                }
            }
            else
            {
                return null;
            }
        }

        public ActionResult AdminMessageDelete(int msgId)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin") || User.Identity.IsAuthenticated && User.IsInRole("Moderator"))
            {
                bool i = AdminManager.AdminMessageDelete(msgId);

                return RedirectToAction("MessageQueue");
            }
            return RedirectToAction("Login", "Account", null);
        }

        public ActionResult AdminDeleteReportAd(int id)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin") || User.Identity.IsAuthenticated && User.IsInRole("Moderator"))
            {
                bool i = AdminManager.AdminDeleteReportAd(id);

                return RedirectToAction("ReportQueue");
            }
            return RedirectToAction("Login", "Account", null);
        }

        public ActionResult AdminDeleteAllClosedReportAds()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin") || User.Identity.IsAuthenticated && User.IsInRole("Moderator"))
            {
                bool i = AdminManager.AdminDeleteAllClosedReportAds();

                return RedirectToAction("ReportQueue");
            }
            return RedirectToAction("Login", "Account", null);
        }

        [HttpGet]
        [NoCache]
        [Route("AdminEditUserAd/{adId:int}")]
        public ActionResult AdminEditUserAd(int adId)
        {
            // check if user pressed back button after posting
            if (TempData["isPostedEdit"] != null || ViewBag.isPosted != null)
            {
                ViewBag.isPostedEdit = TempData["isPostedEdit"].ToString();
                TempData["isPostedEdit"] = ViewBag.isPostedEdit;
                return RedirectToAction("ListDetails", new { stringId = ViewBag.isPostedEdit });
            }
            // get the obj
            var edititem = AdminManager.AdminGetClassifiedAdWithAll(adId);
            if (edititem == null) return RedirectToAction("Index","Home");
            edititem.ConfigureForm(AdminManager);
            PhotoFileManager.CreateTempPhotos(edititem.Id, edititem.StringId, edititem.Photos);
            return View(edititem);

        }

        [HttpPost]
        [ValidateAdInfoFilter]
        [Route("AdminEditUserAd/{adId:int}")]
        public ActionResult AdminEditUserAd(int adId, AdminClassifiedAdEdit editItem)
        {
            if (TempData["editUserAdPosted"] == null)
            {
                TempData["editUserAdPosted"] = true;
                if (!ModelState.IsValid)
                {
                    var retItem = AdminManager.ClassifiedManager.GetClassifiedAdWithAll(adId);
                    retItem.ConfigureForm(AdminManager);
                    PhotoFileManager.CreateTempPhotos(retItem.Id, retItem.StringId, retItem.Photos);
                    return View(retItem);
                }

                // Sanitize description
                editItem.Description = new HtmlSanitizer().Sanitize(editItem.Description);

                // Process the input
                var editedItem = AdminManager.AdminClassifiedAdEdit(editItem);

                if (editedItem == null)
                {
                    var retItem = AdminManager.ClassifiedManager.GetClassifiedAdWithAll(adId);
                    retItem.ConfigureForm(AdminManager);
                    PhotoFileManager.CreateTempPhotos(retItem.Id, retItem.StringId, retItem.Photos);
                    return View(editItem);
                }
                else
                {
                    return RedirectToAction("AdminUserAdDetails", "Admin", new { stringId = editedItem.StringId });
                }
            }
            return RedirectToAction("ReportQueue", "Admin");
        }

        [Route("AdminSelectAdCategory/{adId:int}")]
        public ActionResult AdminSelectAdCategory(int adId)
        {
            ViewBag.adId = adId;
            return View(AdminManager.GetCategoryListSlim());
        }
        
        public ActionResult AdminChangeAdCategory(int adId, int subCatId)
        {
            AdminManager.AdminAdCategoryChange(adId, subCatId);
            return RedirectToAction("ReportQueue", "Admin");
        }

        [Route("AdminRemoveUserAd/{adId:int}/{pageNumber:int}")]
        public ActionResult AdminRemoveUserAd(int adId, int pageNumber)
        {
            AdminManager.AdQueueRemoveAd(adId);
            TempData["pageNumber"] = pageNumber;
            return RedirectToAction("AdQueue");                    
        }

        [Route("AdminApproveUserAd/{adId:int}/{pageNumber}")]
        public ActionResult AdminApproveUserAd(int adId, int pageNumber)
        {
            AdminManager.AdQueueApproveAd(adId);
            TempData["pageNumber"] = pageNumber;
            return RedirectToAction("AdQueue");
        }

        public ActionResult AdminUpdateUserAd(int adId, int pageNumber)
        {
            AdminManager.AdQueueUpdateAd(adId);
            TempData["pageNumber"] = pageNumber;
            return RedirectToAction("AdQueue");
        }

        /// <summary>
        /// Display promote ad page
        /// </summary>
        /// <returns></returns>
        [Route("AdminPromoteUserAdPage/")]
        public ActionResult AdminPromoteUserAdPage()
        {
            return View();
        }

        /// <summary>
        /// Ajax call get user ad to be promoted
        /// </summary>
        /// <param name="adId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AdminFindPromoteAd(int? adId)
        {
            var promo = new AdminPromote()
            {
                Ad = adId != null ? PromotionManager.GetPromoteAdById<AdminPromoteClassifiedAd>(adId.Value) : null,
                PromoDuration = PromotionStaticInfo.PromotionDuration.DurationRange,
                UrgentAdPrice = PromotionStaticInfo.UrgentAdPaymentInfo.UrgentAdPrice.Price,
                TopAdPrice = PromotionStaticInfo.TopAdPaymentInfo.TopAdPrice.Price,
                FeaturedAdPrice = PromotionStaticInfo.FeaturedAdPaymentInfo.FeaturedAdPrice.Price
            };
            return View(promo);
        }

        /// <summary>
        /// Ajax call update ad promotion status
        /// </summary>
        /// <param name="ad"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AdminPromoteUserAd(AdminPromoteClassifiedAd ad)
        {
            var promo = PromotionManager.PromoteAd(ad);
            promo.PromoDuration = PromotionStaticInfo.PromotionDuration.DurationRange;
            promo.UrgentAdPrice = PromotionStaticInfo.UrgentAdPaymentInfo.UrgentAdPrice.Price;
            promo.TopAdPrice = PromotionStaticInfo.TopAdPaymentInfo.TopAdPrice.Price;
            promo.FeaturedAdPrice = PromotionStaticInfo.FeaturedAdPaymentInfo.FeaturedAdPrice.Price;
            return View("AdminFindPromoteAd", promo);
        }

        /// <summary>
        /// Ajax call demote ad
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AdminDemoteAd(int adId)
        {
            var demo = PromotionManager.RemovePromoteAd(adId);
            demo.PromoDuration = PromotionStaticInfo.PromotionDuration.DurationRange;
            demo.UrgentAdPrice = PromotionStaticInfo.UrgentAdPaymentInfo.UrgentAdPrice.Price;
            demo.TopAdPrice = PromotionStaticInfo.TopAdPaymentInfo.TopAdPrice.Price;
            demo.FeaturedAdPrice = PromotionStaticInfo.FeaturedAdPaymentInfo.FeaturedAdPrice.Price;
            return View("AdminFindPromoteAd", demo);
        }

        public ActionResult AdQueue()
        {
            return View(AdminManager.GetAdQueue());
        }

        [HttpGet]
        public ActionResult AdQueuePagination(int pageNumber = 1)
        {
            if (TempData["pageNumber"] != null)
            {
                int.TryParse(TempData["pageNumber"].ToString(), out pageNumber);
                TempData.Clear();
            }
            return PartialView("_AdQueueList", AdminManager.GetAdQueue(pageNumber));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
