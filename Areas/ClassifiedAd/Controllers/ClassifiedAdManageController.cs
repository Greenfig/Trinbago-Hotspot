using AutoMapper;
using Ganss.XSS;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Trinbago_MVC5.Areas.ClassifiedAd.Managers;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Extensions.AttributeClasses;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Models;
using System;
using System.Web.Hosting;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Controllers
{
    [RouteArea("ClassifiedAd")]
    [RoutePrefix("Manage")]
    [Route("{action}")]
    [AuthorizeEmailConfirm(Roles = "Admin, Moderator, User")]
    public class ClassifiedAdManageController : ClassifiedAdBaseController
    {
        //
        // Get: /ClassifiedAd/MyList
        public ActionResult MyAdList()
        {
            ViewBag.AdRenewPopupMessage = TempData["AdRenewPopupMessage"];
            TempData.Clear();
            return View(new MyAdList() { MyAds = ClassifiedAdManager.GetUserAdList() });
        }

        [HttpGet]
        public ActionResult MyAdListPagination(string searchType = "All", int pageNumber = 1)
        {
            ViewBag.sTyp = searchType;
            return PartialView("_MyAdsEditList", ClassifiedAdManager.GetUserAdList(searchType, pageNumber));
        }

        //
        // Post: /ClassifiedAd/MyAdClose
        [HttpPost]
        public HtmlString MyAdClose(int adId, string closingOption)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (ClassifiedAdManager.MyAdClose(adId, closingOption, Server))
                {
                    return new HtmlString(
                        "<div class='modal-dialog'>" +
                            "<div class='modal-content'>" +
                                "<div class='modal-body'>" +
                                    "<span>Your Ad will be closed within 24 hours</span>" +
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
                                    "<span>Failed to close ad</span>" +
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

        [HttpPost]
        public HtmlString MyAdRequestOpen(int adId)
        {
            if (ClassifiedAdManager.OpenRequestMyAd(adId))
            {
                return new HtmlString(
                    "<div class='modal-dialog'>" +
                        "<div class='modal-content'>" +
                            "<div class='modal-body'>" +
                                "<span>Open Ad request sent to administrator!<br/>Please allow 1 to 5 hrs for your ad to be reviewed and reopened.</span>" +
                            "</div>" +
                            "<div class='modal-footer'>" +
                                "<button type='button' class='btn btn-primary' data-dismiss='modal'>OK</button>" +
                            "</div>" +
                        "</div>" +
                    "</div>");
            }
            else
            {
                return new HtmlString(
                    "<div class='modal-dialog'>" +
                        "<div class='modal-content'>" +
                            "<div class='modal-body'>" +
                                "<span>Request failed to send!<br/>Please contact Support at support@trinbagohotspot.com for more details.</span>" +
                            "</div>" +
                            "<div class='modal-footer'>" +
                                "<button type='button' class='btn btn-primary' data-dismiss='modal'>OK</button>" +
                            "</div>" +
                        "</div>" +
                    "</div>");
            }
        }

        [AllowAnonymous]
        public ActionResult AdClosedPage()
        {
            if (TempData["isAdClosed"] != null)
            {
                if (TempData["isAdClosed"].ToString() == "true")
                {
                    return View();
                }
            }
            return RedirectToAction("AdList", "ClassifiedAd");
        }

        //
        // GET: /ClassifiedAd/CategoryPicker
        public ActionResult CategorySelectCreateAd()
        {
            var cats = ClassifiedAdManager.GetCategoryListSlim();
            cats.SingleOrDefault(x => x.Name == "Pets").SubCategories.SingleOrDefault(s => s.Name == "Pet Hub").Name = "Pet Hub(BUY & SELL)";
            return View(cats);
        }

        //
        // GET: /ClassifiedAd/Create
        [HttpGet]
        [NoCache]
        [Route("CreateAd/{subCatId?}")]
        public ActionResult CreateAd(int? subCatId)
        {
            if (Request.UrlReferrer == null || Request.UrlReferrer != null && (Request.UrlReferrer.Host == Request.Url.Host && !subCatId.HasValue || Request.UrlReferrer.Host != Request.Url.Host && subCatId.HasValue || Request.UrlReferrer.Host != Request.Url.Host && !subCatId.HasValue))
            {
                TempData.Clear();
                return RedirectToAction("CategorySelectCreateAd");
            }

            // check if user pressed back button after posting
            if (TempData["isPostedCreate"] != null || ViewBag.isPostedCreate != null)
            {
                return RedirectToAction("CategorySelectCreateAd");
            }

            var f = ClassifiedAdManager.GetSubCatWithCat(subCatId.Value);

            // Create form
            var defaultForm = new ClassifiedAdAddForm
            {
                SubCategoryId = f.Id,
                SubCategoryName = f.Name,
                CategoryName = f.CategoryName
            };
            defaultForm.ConfigureForm(ClassifiedAdManager);

            var getinfo = f.AdInfoTemplate?.RecommendedInfo;
            if (getinfo != null)
                if (getinfo.Count() > 0)
                {
                    defaultForm.AdInfo = new List<InfoForm>();
                    defaultForm.AdInfo = getinfo.ToList();
                }
            return View(defaultForm);
        }

        //
        // POST: /ClassifiedAd/CreateAd
        [NoCache]
        [HttpPost]
        [ValidateAntiForgeryToken, ValidateAdInfoFilter]
        [Route("CreateAd/{subCatId:int}")]
        public ActionResult CreateAd(ClassifiedAdAdd newItem)
        {
            if (!ModelState.IsValid)
            {
                var retItem = Mapper.Map<ClassifiedAdAddForm>(newItem);
                var f = ClassifiedAdManager.GetSubCatWithCat(newItem.SubCatId);

                // Create form
                retItem.SubCategoryId = f.Id;
                retItem.SubCategoryName = f.Name;
                retItem.CategoryName = f.CategoryName;
                retItem.ConfigureForm(ClassifiedAdManager);
                retItem.AdInfo = null;

                var getinfo = f.AdInfoTemplate?.RecommendedInfo;
                if (getinfo != null)
                    if (getinfo.Count() > 0)
                    {
                        retItem.AdInfo = new List<InfoForm>();
                        retItem.AdInfo = getinfo.ToList();
                    }

                return View(retItem);
            }
            // Prevent double post
            if (TempData["hasBeenPosted"] == null)
            {
                TempData["hasBeenPosted"] = true;
                // Sanitize description
                newItem.Description = new HtmlSanitizer().Sanitize(newItem.Description);

                // Process the input
                var addedItem = ClassifiedAdManager.AddClassifiedAd(newItem);

                if (addedItem == null)
                {
                    return View(newItem);
                }
                else
                {
                    // Prevent user going back after posting ad
                    TempData["isPostedCreate"] = true;
                    return RedirectToAction("MyAdPreview", new { adId = addedItem.Id });
                }
            }

            return RedirectToAction("MyAdList");
        }

        //
        // GET: /ClassifiedAdEdit/Edit/5
        [HttpGet]
        [NoCache]
        [Route("MyAdEdit/{adId:int}")]
        public ActionResult MyAdEdit(int adId)
        {
            // get the obj
            var edititem = ClassifiedAdManager.GetClassifiedAdWithAll(adId);
            if (edititem == null) return RedirectToAction("Index", "Home", new { Area = "" });
            edititem.ConfigureForm(ClassifiedAdManager);
            PhotoFileManager.CreateTempPhotos(edititem.Id, edititem.StringId, edititem.Photos);
            return View(edititem);
        }

        //
        // POST: /ClassifiedAdSell/Edit/5
        [HttpPost]
        [NoCache]
        [ValidateAntiForgeryToken, ValidateAdInfoFilter]
        [Route("MyAdEdit/{adId:int}")]
        public ActionResult MyAdEdit(int adId, ClassifiedAdEdit editItem)
        {
            if (!ModelState.IsValid)
            {
                var retItem = ClassifiedAdManager.GetClassifiedAdWithAll(adId); ;
                retItem.ConfigureForm(ClassifiedAdManager);
                PhotoFileManager.CreateTempPhotos(retItem.Id, retItem.StringId, retItem.Photos);
                return View(retItem);
            }

            // Sanitize description            
            editItem.Description = new HtmlSanitizer().Sanitize(editItem.Description);

            // Process the input
            var editedItem = ClassifiedAdManager.EditClassifiedAd(editItem);

            if (editedItem == null)
            {
                var retItem = ClassifiedAdManager.GetClassifiedAdWithAll(adId);
                retItem.ConfigureForm(ClassifiedAdManager);
                PhotoFileManager.CreateTempPhotos(retItem.Id, retItem.StringId, retItem.Photos);
                return View(retItem);
            }
            return RedirectToAction("MyAdPreview", new { adId = editedItem.Id });

        }

        [HttpGet]
        [Route("MyAdRenew/{adId:int}")]
        public ActionResult MyAdRenew(int adId)
        {
            var ad = ClassifiedAdManager.GetClassifiedAdRenew(adId);
            if (ad == null)
            {
                TempData["AdRenewPopupMessage"] = new HtmlString("$(function(){ <script> alert('No Ad found with that Id');</script> });");
                return RedirectToAction("MyAdList");
            }
            return View(ad);
        }

        [HttpPost]
        public ActionResult FinalizeMyAdRenew(int adId)
        {
            if (ClassifiedAdManager.RenewMyAd(adId))
            {
                TempData["AdRenewPopupMessage"] = new HtmlString("$(function(){ <script> alert('Ad Renewal Succeeded. Thank You!');</script> });");
            }
            else
            {
                TempData["AdRenewPopupMessage"] = new HtmlString("$(function(){ <script> alert('Failed to Renew Ad');</script> });");
            }
            return RedirectToAction("MyAdList");
        }

        [Route("AdPreview/{adId:int}")]
        public ActionResult MyAdPreview(int adId)
        {
            var map = ClassifiedAdManager.MyAdPreview(adId);
            if (map == null) return new HttpStatusCodeResult(404);
            return View(map);
        }

        /// <summary>
        /// Converts files and adds them ONLY to file system
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadPhotoAdd(FormDataUpload form)
        {
            if (Response.IsClientConnected)
            {
                if (form.Photos == null || form.StringId == null) return Json(new { success = false });
                var dir = HostingEnvironment.MapPath("~/Photos/" + form.StringId.Substring(2, 4) + "/" + form.StringId.Substring(0, 4));
                Directory.CreateDirectory(dir);
                List<UploadPhoto> phos = new List<UploadPhoto>();
                int _curr = form.CurrentPhotoCount;
                int _max = form.MaxPhotoCount;
                for (int i = 0; i < form.Photos.Count && form.CurrentPhotoCount <= form.MaxPhotoCount && i < _max - _curr; i++, form.CurrentPhotoCount++)
                {
                    HttpPostedFileBase photo = form.Photos.ElementAt(i);
                    if (AllowedFileTypes.AllowedFileTypesValidation(photo, "CreateAd"))
                    {
                        var p = new UploadPhoto() { Original_FileName = photo.FileName };
                        // Compress image for ad list thumbnail
                        PhotoEditing.CompressUploadPhoto(dir, true, false, photo, ref p);
                        // Compress image for ad details thumbnail
                        PhotoEditing.CompressUploadPhoto(dir, false, true, photo, ref p);
                        // Compress imgae
                        PhotoEditing.DefaultCompressionJpegUpload(dir, photo, ref p);
                        // Set display src
                        p.Src = "/Images/" + form.StringId.Substring(2, 4) + "/" + form.StringId.Substring(0, 4) + "/" + p.AdList_FileName;
                        phos.Add(p);
                    }
                }

                return Json(new { success = true, photocount = new { current = form.CurrentPhotoCount }, photos = JsonConvert.SerializeObject(phos) });
            }
            return Json(new { success = false });
        }

        /// <summary>
        /// Deletes files ONLY from file system
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadPhotoDelete(FormDataDelete form)
        {
            // Delete items
            var dir = HostingEnvironment.MapPath("~/Photos/" + form.StringId.Substring(2, 4) + "/" + form.StringId.Substring(0, 4));
            try
            {
                var path1 = Path.Combine(dir, form.AdList_FileName);
                var path2 = Path.Combine(dir, form.Raw_FileName);
                var path3 = Path.Combine(dir, form.AdDetails_FileName);
                // delete physical
                System.IO.File.Delete(path1);
                System.IO.File.Delete(path2);
                System.IO.File.Delete(path3);
                return Json(new { success = true, photocount = new { current = --form.CurrentPhotoCount } });
            }
            catch (Exception)
            {
                // failed to delete
                return Json(new { success = false, message = "Failed To Delete!" });
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}