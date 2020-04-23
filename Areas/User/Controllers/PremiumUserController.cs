using System.Web.Mvc;
using Trinbago_MVC5.Areas.User.Models;

namespace Trinbago_MVC5.Areas.User.Controllers
{
    [Authorize(Roles = "Admin, Moderator, Premium")]
    [RoutePrefix("PremiumUser")]
    [RouteArea("PremiumUser")]
    [Route("{action}")]
    public class PremiumUserController : Controller
    {
        //
        // GET: /Premium/

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult PUser(string premiumUserNameorId)
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult PUserSearch(string premiumUserNameorId, int pageNumber = 1)
        {
            return null;
        }

        public ActionResult PUserProfile(string stringId)
        {
            //var id = WebSecurity.GetUserId(User.Identity.Name);
            //var name = WebSecurity.CurrentUserName;

            //if (User.IsInRole("Premium"))
            //{
            //    var obj = new Manager().GetPremiumUserByUserIdandName(name, id);
            //    if (obj == null)
            //    {
            //        return RedirectToAction("Index", "Home", new { Area = "" });
            //    }
            //    else
            //    {
            //        obj.UrlName = (obj.UrlName == null) ? obj.UserProfilestringId : obj.UrlName;
            //        return View(obj);
            //    }
            //}

            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        public ActionResult PUserProfileEdit()
        {
            //var id = WebSecurity.GetUserId(User.Identity.Name);
            //var name = WebSecurity.CurrentUserName;

            //if (User.IsInRole("Premium"))
            //{
            //    var obj = new Manager().GetPremiumUserEditByUserIdandName(name, id);
            //    if (obj == null)
            //    {
            //        return RedirectToAction("Index", "Home", new { Area = "" });
            //    }
            //    else
            //    {
            //        obj.UrlName = (obj.UrlName == null) ? obj.UserProfilestringId : obj.UrlName;
            //        return View(obj);
            //    }
            //}
            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        [HttpPost]
        public ActionResult PUserProfileEdit(PremiumUserProfileEdit editItem)
        {
            //if (Roles.IsUserInRole("Premium"))
            //{
            //    // initial check
            //    // check if url name exists
            //    if (editItem.UrlName.ToUpper() == "CAR" || editItem.UrlName.ToUpper() == "TRUCK" ||
            //        editItem.UrlName.ToUpper() == "HOUSEFORRENT" || editItem.UrlName.ToUpper() == "HOUSEFORSALE" ||
            //        editItem.UrlName.ToUpper() == "CONDOFORSALE" || editItem.UrlName.ToUpper() == "APARTMENTFORSALE" ||
            //        editItem.UrlName.ToUpper() == "APARTMENTFORRENT" || editItem.UrlName.ToUpper() == "CONDOFORRENT" ||
            //        editItem.UrlName.ToUpper() == "ROOMFORRENT" || editItem.UrlName.ToUpper() == "PETS" ||
            //        editItem.UrlName.ToUpper() == "JOBS" || editItem.UrlName.ToUpper() == "ELECTRONICS" ||
            //        editItem.UrlName.ToUpper() == "PHONES" || editItem.UrlName.ToUpper() == "ComLapAccess" ||
            //        editItem.UrlName.ToUpper() == "CompLap")
            //    {
            //        ModelState.AddModelError("UrlName", "Name is already in use. Please choose a different name.");
            //    }
            //    else
            //    {
            //        //check if the name put it belongs to user already
            //        if (new Manager().IsUrlNameTaken(editItem.UrlName, editItem.UserProfilestringId))
            //        {
            //            ModelState.AddModelError("UrlName", "Name is already in use. Please choose a different name.");
            //        }
            //    }

            //    // check photos
            //    List<bool> pu = new List<bool>();
            //    // check all images
            //    if (editItem.PhotoUpload1 != null)
            //    {
            //        //file check
            //        pu.Add(AllowedFileTypes.AllowedFileTypesValidation(editItem.PhotoUpload1, "CreateAd"));
            //    }
            //    else
            //        pu.Add(true);
            //    if (editItem.PhotoUpload2 != null)
            //    {
            //        //file check
            //        pu.Add(AllowedFileTypes.AllowedFileTypesValidation(editItem.PhotoUpload2, "CreateAd"));
            //    }
            //    else
            //        pu.Add(true);
            //    if (editItem.PhotoUpload3 != null)
            //    {
            //        //file check                
            //        pu.Add(AllowedFileTypes.AllowedFileTypesValidation(editItem.PhotoUpload3, "CreateAd"));
            //    }
            //    else
            //        pu.Add(true);

            //    int counter = 1;
            //    string errormessage = null;
            //    foreach (bool pub in pu)
            //    {
            //        if (!pub)
            //        {
            //            string filename = null;
            //            if (counter == 1) { filename = editItem.PhotoUpload1.FileName; }
            //            if (counter == 2) { filename = editItem.PhotoUpload2.FileName; }
            //            if (counter == 3) { filename = editItem.PhotoUpload3.FileName; }

            //            errormessage += "File: " + filename + " is not the required format!" + '\n';
            //            ModelState.AddModelError("error", "error");
            //        }
            //        counter++;
            //    }
            //    ViewBag.PhotoUploadMessage = errormessage;

            //    if (!ModelState.IsValid)
            //    {
            //        //return form
            //        var config = new MapperConfiguration(r => r.CreateMap<PUserProfileEdit, PUserProfileEditForm>()
            //            .ForMember(x => x.PhotoUpload1, opt => opt.Ignore())
            //            .ForMember(x => x.PhotoUpload2, opt => opt.Ignore())
            //            .ForMember(x => x.PhotoUpload3, opt => opt.Ignore()));
            //        IMapper mapper = config.CreateMapper();

            //        return View(mapper.Map<PUserProfileEditForm>(editItem));
            //    }

            //    var editeditem = new Manager().PrimiumUserEdit(editItem, Server);

            //    if (editeditem == null)
            //    {
            //        return View(editItem);
            //    }
            //    else
            //    {
            //        TempData["isPostedCreate"] = editeditem.UserProfileStringId;
            //        TempData["PrevPage"] = editeditem.UserProfileStringId;
            //        return RedirectToAction("PUserProfile", "Premium", new { stringId = editeditem.UserProfileStringId });
            //    }
            //}
            return RedirectToAction("Index", "Home", new { Area = "" });
        }
    }
}
