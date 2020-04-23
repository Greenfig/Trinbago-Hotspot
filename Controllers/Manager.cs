using AutoMapper;
using HtmlAgilityPack;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Controllers
{
    public class Manager
    {
        protected ApplicationUserManager _userManager;
        protected ApplicationUser _currentUser;

        public Manager() {
        }

        internal ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        internal ApplicationUser CurrentUser
        {
            get
            {
                return _currentUser ?? HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(HttpContext.Current.User.Identity.GetUserId());
            }
            private set
            {
                _currentUser = value;
            }
        }
                
        // Literally does nothing -- needed for task
        public bool AbortCallback()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Templated conversion type in this case Country
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal IEnumerable<T> CountryGetAll<T>()
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                IEnumerable<Country> val = new List<Country>();
                val = newthreadcontext.CountryDB.Include("Regions");
                var config = new MapperConfiguration(r => r.CreateMap<Country, T>());
                IMapper mapper = config.CreateMapper();

                return (val == null) ? null : mapper.Map<IEnumerable<T>>(val);
            }
        }

        internal CountryWithDetail CountryGetById(int id)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var config = new MapperConfiguration(r =>
                {
                    r.CreateMap<Country, CountryWithDetail>();
                    r.CreateMap<Trinbago_MVC5.Models.Region, RegionBase>();
                });

                IMapper mapper = config.CreateMapper();

                var val = newthreadcontext.CountryDB.Include("Regions").SingleOrDefault(a => a.Id == id);
                val.Regions = val.Regions.OrderBy(a => a.Name).ToList();
                return (val == null) ? null : mapper.Map<CountryWithDetail>(val);
            }
        }

        internal IEnumerable<T> TypeGetAll<T>()
        {
            IEnumerable<MiscInfo> val;
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.MiscInfoDB.Where(x => x.Descriptor.Equals("AdType"));
                var config = new MapperConfiguration(r => r.CreateMap<MiscInfo, T>());
                IMapper mapper = config.CreateMapper();
                var retval = mapper.Map<IEnumerable<T>>(val);
                return (retval == null) ? null : retval;
            }

        }

        internal IEnumerable<T> RegionGetAll<T>(int Id)
        {
            IEnumerable<Trinbago_MVC5.Models.Region> val = new List<Trinbago_MVC5.Models.Region>();

            // spin up a new context for every thread making it's way through the code
            // refer to http://devproconnections.com/development/solving-net-scalability-problem

            using (ApplicationDbContext newthreadcontext =
                new ApplicationDbContext())
            {

                val = newthreadcontext.CountryDB.Include("Regions").SingleOrDefault(a => a.Id == Id).Regions.OrderBy(x => x.Name);
            }

            var config = new MapperConfiguration(r => r.CreateMap<Trinbago_MVC5.Models.Region, T>());
            IMapper mapper = config.CreateMapper();
            return (val == null) ? null : mapper.Map<IEnumerable<T>>(val);
        }

        internal T RegionGetById<T>(int id) where T : class
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var val = newthreadcontext.RegionDB.Include("Country").SingleOrDefault(a => a.Id == id);
                var config = new MapperConfiguration(r => r.CreateMap<Trinbago_MVC5.Models.Region, T>());
                IMapper mapper = config.CreateMapper();

                return (val == null) ? null : mapper.Map<T>(val);
            }
        }

        // Generate StringId
        public string GetGen()
        {
            long i = 1;
            string str;

            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            str = string.Format("{0:x}", i - DateTime.Now.Ticks);

            return str;
        }

        /**
         * 
         * USER METHODS
         * 
         **/
        
        internal void ClassifiedAdViewIncrement(string stringId, string Username)
        {
            // fetch obj then increment and save
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.ClassifiedDB.Include("UserCreator.PremiumUserData").SingleOrDefault(a => a.StringId == stringId);
                if (o != null)
                {
                    if (!o.UserCreator.UserName.Equals(Username))
                    {
                        newthreadcontext.Entry(o).CurrentValues.SetValues(++o.Views);
                        if (o.UserCreator.PremiumUserData != null)
                            o.UserCreator.PremiumUserData.ClassifiedAdsViewCount++;
                        newthreadcontext.SaveChanges();
                    }
                }
            }
        }

        internal IEnumerable<MiscInfo> PriceInfoGetAll()
        {
            IEnumerable<MiscInfo> val;
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.MiscInfoDB.Where(x => x.Descriptor.Equals("PriceInfo"));
                return (val == null) ? null : val.ToList();
            }
        }


        internal IEnumerable<Category> CategoryGetAll()
        {
            ICollection<Category> val = new List<Category>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.CategoryDB.Include("SubCategories").ToList();

                foreach (var c in val)
                {
                    var last = c.SubCategories.SingleOrDefault(x => x.Name.Equals("Other"));
                    c.SubCategories.ToList().RemoveAll(x => x == null);
                    c.SubCategories.Remove(last);
                    c.SubCategories = c.SubCategories.OrderBy(a => a.Name).ToList();
                    if (last != null)
                    {
                        c.SubCategories.Add(last);
                    }
                }

            }
            return (val == null) ? null : val;
        }

        internal IEnumerable<DropDownCategory> CategoryOnlyGetAll(string stringId = null)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var val = newthreadcontext.CategoryDB.ToList();
                var config = new MapperConfiguration(r => 
                {
                    r.CreateMap<SubCategory, DropDownCategory>()
                        .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.stringId));
                    r.CreateMap<Category, DropDownCategory>();
                });
                IMapper mapper = config.CreateMapper();
                ICollection<DropDownCategory> toret = mapper.Map<ICollection<DropDownCategory>>(val);
                if (!String.IsNullOrEmpty(stringId))
                {
                    var subcatinsert = newthreadcontext.SubCategoryDB.SingleOrDefault(x => x.stringId.Equals(stringId));
                    if(subcatinsert != null)
                        toret.Add(mapper.Map<DropDownCategory>(subcatinsert));
                }
                return (toret == null) ? null : toret;
            }
        }

        // get subcategory using string id
        internal SubCategory SubCatGetWithCatAndClass(string p)
        {
            SubCategory val = new SubCategory();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.SubCategoryDB.Include("Category").Include("ClassifiedAds").SingleOrDefault(x => x.stringId == p);
            }
            return val;
        }

        // get subcategory using string id
        internal SubCategory SubCatGetWithCat(string p)
        {
            SubCategory val = new SubCategory();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.SubCategoryDB.Include("Category").SingleOrDefault(x => x.stringId == p);
            }
            return val;
        }

        // get category using id
        internal Category CatGetById(int p)
        {
            Category val = new Category();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.CategoryDB.Include("SubCategories").SingleOrDefault(x => x.Id == p);
            }
            return val;
        }

        // get subcategory using string id
        internal SubCategory SubCatGetWithCatID(int p)
        {
            SubCategory val = new SubCategory();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.SubCategoryDB.Include("Category").SingleOrDefault(x => x.Id == p);
            }
            return val;
        }

        internal int SubCatAdCount(string p)
        {
            int val;
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.SubCategoryDB
                    .FirstOrDefault(x => x.stringId == p)
                    .ClassifiedAds.Count;
            }
            return val;
        }

        internal bool CheckEmailDuplicate(string p)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var user = UserManager.FindByEmail(p);
                if (user != null)
                    return true;
            }
            return false;
        }

        internal bool ApplyToJob(ClassifiedAdApplyTo msg)
        {
            var message = new MailMessage();
            try
            {
                var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p><p>Related Ad:</p><p>{3}</p>";
                message.To.Add(new MailAddress(msg.To));
                message.CC.Add(new MailAddress(msg.From));
                message.From = new MailAddress("DoNotReply@trinbagohotspot.com", "TrinbagoHotspot.com");
                message.Subject = "Job Application Message From " + msg.Name + " via 'TrinbagoHotSpot.com'";
                message.Body = string.Format(body, msg.Name, msg.From, msg.Message, msg.ItemUrl);
                message.IsBodyHtml = true;

                string fileName = Path.GetFileName(msg.FileUpload.FileName);
                var attachment = new Attachment(msg.FileUpload.InputStream, fileName);

                message.Attachments.Add(attachment);

                using (var smtp = new SmtpClient("trinbagohotspot.com", 26))
                {
                    var credentials = new NetworkCredential("DoNotReply@trinbagohotspot.com", "ZSE$aw3Q@1!q2AW#zse4");
                    smtp.Credentials = credentials;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 5000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal bool SendMessage(ClassifiedAdQ msg)
        {
            var message = new MailMessage();
            try
            {                
                message.To.Add(new MailAddress(msg.To));
                message.From = new MailAddress("DoNotReply@trinbagohotspot.com", "TrinbagoHotspot.com");
                message.ReplyToList.Add(new MailAddress(msg.From));
                message.Subject = "Message from " + msg.Name + " via 'TrinbagoHotSpot.com'";
                message.Body = MessageInterface.ReplyToAdMessage(msg);
                message.IsBodyHtml = true;
                
                using (var smtp = new SmtpClient("trinbagohotspot.com", 26))
                {
                    var credentials = new NetworkCredential("DoNotReply@trinbagohotspot.com", "ZSE$aw3Q@1!q2AW#zse4");
                    smtp.Credentials = credentials;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 5000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal bool SendForgotPassMessage(string username, string email, string passkey)
        {
            var message = new MailMessage();
            try
            {
                var body = "<p>Hello ({0}),</p><br/><p>To reset your account password go to the following link:</p><section>{1}</section><br/>If clicking the link does not work, copy & paste this url into your browser:<section>{2}</section><br/><p>Thank you,</p><br/><p>TrinbagoHotspot.com</p>";
                var confirmationUrl = "https://trinbagohotspot.com/Account/ForgottenPassChange?passresetkey="
                    + HttpUtility.UrlEncode(passkey);
                var confirmationLink = String.Format("<a href='{0}'>"+confirmationUrl+"</a>",
                    confirmationUrl);
                message.To.Add(new MailAddress(email));
                message.From = new MailAddress("DoNotReply@trinbagohotspot.com", "TrinbagoHotspot.com password recovery");
                message.Subject = "TrinbagoHotSpot.com - Password recovery!";
                message.Body = MessageInterface.ForgotPasswordMessage(username, confirmationLink, confirmationUrl);
                message.IsBodyHtml = true;
                
                using (var smtp = new SmtpClient("trinbagohotspot.com", 26))
                {
                    var credentials = new NetworkCredential("DoNotReply@trinbagohotspot.com", "ZSE$aw3Q@1!q2AW#zse4");
                    smtp.Credentials = credentials;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 5000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal bool SendVerificationMessage(string email, string name, string returnUrl, string token)
        {
            var message = new MailMessage();
            try
            {
                var body = "<p>Hello ({0}),</p><br/><p>You are one step away from validating your account. To validate your account click on the following link:</p><section>{1}</section><br/>If clicking the link does not work, copy & paste this url into your browser:</p><section>{2}</section><br/><p>Thank you,</p><br/><p>TrinbagoHotspot.com</p>";
                var confirmationUrl = "https://trinbagohotspot.com/Account/ConfirmAccount?confirmation="
                    + HttpUtility.UrlEncode(token)+(!String.IsNullOrEmpty(returnUrl) ? "&returnUrl="+returnUrl : null);
                var confirmationLink = String.Format("<a href='{0}'>"+confirmationUrl+"</a>",
                    confirmationUrl);
                message.To.Add(new MailAddress(email));
                message.From = new MailAddress("DoNotReply@trinbagohotspot.com", "TrinbagoHotspot.com");
                message.Subject = "TrinbagoHotSpot.com - Validate your email address!";
                message.Body = string.Format(body, name, confirmationLink, confirmationUrl);
                message.IsBodyHtml = true;


                using (var smtp = new SmtpClient("trinbagohotspot.com", 26))
                {
                    var credentials = new NetworkCredential("DoNotReply@trinbagohotspot.com", "ZSE$aw3Q@1!q2AW#zse4");
                    smtp.Credentials = credentials;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 5000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal IEnumerable<InfoForm> TemplateInfoGetOne(int catid, string subcatid)
        {
            SubCategory val;
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.SubCategoryDB.Include("AdInfoTemplate.RecommendedInfo")                    
                    .SingleOrDefault(x => x.stringId.Equals(subcatid));
            }
            var config = new MapperConfiguration(r => 
            {
                r.CreateMap<AdInfoString, InfoForm>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(x => x.Name));
            });
            IMapper mapper = config.CreateMapper();
            return (val.AdInfoTemplate == null) ? null : mapper.Map<IEnumerable<InfoForm>>(val.AdInfoTemplate.RecommendedInfo);
        }

        internal IEnumerable<MiscInfo> MakeGetAll()
        {
            IEnumerable<MiscInfo> val = new List<MiscInfo>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleMake")).OrderByDescending(y => y.Value);
                return (val == null) ? null : val.ToList();
            }
        }

        internal IEnumerable<MiscInfo> BodyTypeGetAll()
        {
            IEnumerable<MiscInfo> val = new List<MiscInfo>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleBodyType"));
                return (val == null) ? null : val.ToList();
            }
        }

        internal bool Favourite(string stringId)
        {
            var fav = CurrentUser.Favourites.SingleOrDefault(x => x.StringId.Equals(stringId));
            if(fav == null)
            {
                using(ApplicationDbContext newthreadcontex = new ApplicationDbContext())
                {
                    fav = newthreadcontex.ClassifiedDB.SingleOrDefault(x => x.StringId.Equals(stringId));
                }
                CurrentUser.Favourites.Add(fav);
                return true;
            }
            else
            {
                CurrentUser.Favourites.Remove(fav);
                return false;
            }
           
        }

        internal IEnumerable<MiscInfo> TransmissionGetAll()
        {
            IEnumerable<MiscInfo> val = new List<MiscInfo>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleTransmission"));
                return (val == null) ? null : val.ToList();
            }
        }

        internal IEnumerable<MiscInfo> FuelTypeGetAll()
        {
            IEnumerable<MiscInfo> val = new List<MiscInfo>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleFuelType"));
                return (val == null) ? null : val.ToList();
            }
        }

        internal IEnumerable<MiscInfo> ConditionGetAll()
        {
            IEnumerable<MiscInfo> val = new List<MiscInfo>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleCondition"));
                return (val == null) ? null : val.ToList();
            }
        }

        internal IEnumerable<MiscInfo> DrivetrainGetAll()
        {
            IEnumerable<MiscInfo> val = new List<MiscInfo>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                val = newthreadcontext.MiscInfoDB.Where(x => x.Descriptor.Equals("VehicleDrivetrain"));
                return (val == null) ? null : val.ToList();
            }
        }

        internal void SetContanctInfo(ref string UserContactName, ref string UserContactEmail, ref string UserContactPhone)
        {
            UserContactEmail = (String.IsNullOrEmpty(CurrentUser.Email)) ? null : CurrentUser.Email.ToString();
            UserContactName = (String.IsNullOrEmpty(CurrentUser.UserName)) ? null : CurrentUser.UserName.ToString();
            UserContactPhone = (String.IsNullOrEmpty(CurrentUser.PhoneNumber)) ? null : CurrentUser.PhoneNumber.ToString();
        }

        internal IEnumerable<ClassifiedAdFeaturedIndex> ClassifiedAdGetFeaturedIndex()
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                if (!newthreadcontext.ClassifiedDB.Any(x => x.FeaturedAdStatus == true))
                    return null;
                IQueryable<ClassifiedAd> val;
                val = newthreadcontext.ClassifiedDB.Include("AdPromotions").Include("AdPhotos").Where(x => x.Status == 0 && x.FeaturedAdStatus == true);
                // Construct view models for the AdList view
                var config = new MapperConfiguration(r =>
                {
                    r.CreateMap<ClassifiedAd, ClassifiedAdFeaturedIndex>()
                        .ForMember(dest => dest.AdPhoto, opt => opt.MapFrom(x => x.AdPhotos.FirstOrDefault(a => a.SetThumbnail == true)));
                    r.CreateMap<Photo, PhotoBase>();
                });
                IMapper mapper = config.CreateMapper();
                
                return mapper.Map<IEnumerable<ClassifiedAdFeaturedIndex>>(val);
            }
        }

        internal IEnumerable<ClassifiedAdFeaturedIndex> ClassifiedAdGetFeaturedIndexWithSearch(int categoryId, string subCategoryId, string AdType = "ALL")
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                IQueryable<ClassifiedAd> items = null;
                if (AdType.Equals("ALL"))
                {
                    if (categoryId != -100 && string.IsNullOrEmpty(subCategoryId))
                    {
                        items = newthreadcontext.ClassifiedDB.Include("AdPhotos").Where(x => x.Status == 0 && x.FeaturedAdStatus == true && x.Category.Id.Equals(categoryId));
                    }
                    else if (!string.IsNullOrEmpty(subCategoryId))
                    {
                        items = newthreadcontext.ClassifiedDB.Include("AdPhotos").Where(x => x.Status == 0 && x.FeaturedAdStatus == true && x.SubCategory.stringId.Equals(subCategoryId));
                    }
                    else
                    {
                        items = newthreadcontext.ClassifiedDB.Include("AdPhotos").Where(x => x.Status == 0 && x.FeaturedAdStatus == true);
                    }
                }
                else
                {
                    if (categoryId != -100 && string.IsNullOrEmpty(subCategoryId))
                    {
                        items = newthreadcontext.ClassifiedDB.Include("AdPhotos").Where(x => x.Status == 0 && x.FeaturedAdStatus == true && x.AdType.Equals(AdType) && x.Category.Id.Equals(categoryId));
                    }
                    else if (!string.IsNullOrEmpty(subCategoryId))
                    {
                        items = newthreadcontext.ClassifiedDB.Include("AdPhotos").Where(x => x.Status == 0 && x.FeaturedAdStatus == true && x.AdType.Equals(AdType) && x.SubCategory.stringId.Equals(subCategoryId));
                    }
                    else
                    {
                        items = newthreadcontext.ClassifiedDB.Include("AdPhotos").Where(x => x.Status == 0 && x.FeaturedAdStatus == true && x.AdType.Equals(AdType));
                    }
                }
                var config = new MapperConfiguration(r =>
                    {
                        r.CreateMap<ClassifiedAd, ClassifiedAdFeaturedIndex>()
                            .ForMember(dest => dest.AdPhoto, opt => opt.MapFrom(x => x.AdPhotos.FirstOrDefault(a => a.SetThumbnail == true)));
                        r.CreateMap<Photo, PhotoBase>();
                    });
                IMapper mapper = config.CreateMapper();

                return mapper.Map<IEnumerable<ClassifiedAdFeaturedIndex>>(items);
            }
        }

        internal bool MyAdClose(string adstringid)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                // get existing
                try
                {
                    var obj = newthreadcontext.ClassifiedDB.Include("UserCreator").Include("Category").Include("SubCategory").SingleOrDefault(x => x.StringId.Equals(adstringid));
                    if (CurrentUser != null && obj.UserCreator.Id.Equals(CurrentUser.Id))
                    {
                        obj.Status = 1;
                        obj.Category.TotalClassifiedAdsCount--;
                        obj.SubCategory.ClassifiedAdsCount--;
                        newthreadcontext.SaveChanges();
                        // Remove from Lucene
                        LuceneSearch.ClearLuceneIndexRecord(obj.StringId);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        internal MyProfile GetCurrentUserProfile()
        {            
            var config = new MapperConfiguration(r => r.CreateMap<ApplicationUser, MyProfile>());
            IMapper mapper = config.CreateMapper();
            return (CurrentUser == null) ? null : mapper.Map<MyProfile>(CurrentUser);
        }

        internal ContactInfo EditContactInfo(ContactInfo model)
        {
            CurrentUser.UserName = model.ContactName;
            CurrentUser.PhoneNumber = (!String.IsNullOrEmpty(model.ContactNumber) ? (model.ContactNumber.Contains("-") ? model.ContactNumber : model.ContactNumber.Insert(3, "-").ToString()) : model.ContactNumber);              
            return new ContactInfo() { ContactName = CurrentUser.UserName, ContactNumber = CurrentUser.PhoneNumber };            
        }

        internal ClassifiedAdReportForm ClassifiedAdReportDetails(string p, string url)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.ClassifiedDB.SingleOrDefault(x => x.StringId.Equals(p));
                if (obj != null)
                {
                    var config = new MapperConfiguration(r => r.CreateMap<ClassifiedAd, ClassifiedAdReportForm>());
                    IMapper mapper = config.CreateMapper();
                    var ret = mapper.Map<ClassifiedAdReportForm>(obj);
                    return ret;
                }
                return new ClassifiedAdReportForm();
            }
        }

        internal bool ClassifiedAdReportAd(ClassifiedAdReportPost rep)
        {
            //get assocciated ad
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.ClassifiedDB.SingleOrDefault(x => x.StringId.Equals(rep.StringId));
                if (obj != null)
                {
                    var config = new MapperConfiguration(r => r.CreateMap<ClassifiedAdReportPost, ClassifiedAdReport>());
                    IMapper mapper = config.CreateMapper();
                    var newitem = mapper.Map<ClassifiedAdReport>(rep);
                    newitem.ClassifiedAd = obj;
                    newitem.CreatedDate = DateTime.Now;
                    newthreadcontext.ReportDB.Add(newitem);
                    newthreadcontext.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }

        internal bool GenericMessageAdd(GenericMessage newmsg)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                newmsg.PostDate = DateTime.Now;
                var o = newthreadcontext.GenMessageDB.Add(newmsg);
                newthreadcontext.SaveChanges();
                return (o == null) ? false : true;
            }
        }

        internal IEnumerable<CategoryListSlim> GetCategoryListSlim()
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.CategoryDB.Include("SubCategories");
                var config = new MapperConfiguration(r =>
                {
                    r.CreateMap<Category, CategoryListSlim>();
                    r.CreateMap<SubCategory, SubCategoryListSlim>();
                });
                IMapper mapper = config.CreateMapper();
                Parallel.ForEach(obj, (currentitem) =>
                {
                    var other = (currentitem).SubCategories.SingleOrDefault(x => x.Name.Equals("Other"));
                    (currentitem).SubCategories.Remove(other);
                    (currentitem).SubCategories = (currentitem).SubCategories.OrderBy(x => x.Name).ToList();
                    (currentitem).SubCategories.Add(other);
                });

                return mapper.Map<IEnumerable<CategoryListSlim>>(obj);
            }
        }

        internal CategoryListSlim GetCategoryListSlimById(int id)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.CategoryDB.Include("SubCategories").SingleOrDefault(x => x.Id == id);
                var config = new MapperConfiguration(r =>
                {
                    r.CreateMap<Category, CategoryListSlim>();
                    r.CreateMap<SubCategory, SubCategoryListSlim>();
                });
                IMapper mapper = config.CreateMapper();
                
                var other = obj.SubCategories.SingleOrDefault(x => x.Name.Equals("Other"));
                obj.SubCategories.Remove(other);
                obj.SubCategories = obj.SubCategories.OrderBy(x => x.Name).ToList();
                obj.SubCategories.Add(other);                

                return mapper.Map<CategoryListSlim>(obj);
            }
        }

        internal SubCategoryListWithCatSlim GetSubCategoryListSlim(string stringid)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.SubCategoryDB.Include("Category").SingleOrDefault(x => x.stringId.Equals(stringid));
                var config = new MapperConfiguration(r => r.CreateMap<SubCategory, SubCategoryListWithCatSlim>());
                IMapper mapper = config.CreateMapper();
                return (obj == null) ? null : mapper.Map<SubCategoryListWithCatSlim>(obj);
            }
        }

        internal int CompressPhoto(string dir, int counter, HttpPostedFileBase PhotoUpload, Photo photothumb)
        {
            string filerename;
            string path;

            filerename = GetGen();
            path = Path.Combine(dir, filerename);

            Image source = Image.FromStream(PhotoUpload.InputStream);
            double widthRatio = ((photothumb.SetThumbnail == true ? (double)310 : (photothumb.AdListThumbnail == true ? (double)450 : (double)450))) / source.Width;
            double heightRatio = ((photothumb.SetThumbnail == true ? (double)310 : (photothumb.AdListThumbnail == true ? (double)450 : (double)450))) / source.Height;

            double ratio = (widthRatio < heightRatio) ? widthRatio : heightRatio;
            Image thumbnail = source.GetThumbnailImage((int)(source.Width * ratio), (int)(source.Height * ratio), AbortCallback, IntPtr.Zero);

            using (var memory = new MemoryStream())
            {
                thumbnail.Save(memory, source.RawFormat);
                using (var imageFile = new FileStream(path, FileMode.Create))
                {
                    imageFile.Write(memory.ToArray(), 0, memory.ToArray().Length);
                }
            }

            photothumb.FileName = filerename;
            photothumb.ContentType = PhotoUpload.ContentType;
            photothumb.CountNum = counter++;

            photothumb.ClassifiedAd.AdPhotos.Add(photothumb);

            return counter;
        }

        internal void CompressPhotoEdit(string dir, int counter, HttpPostedFileBase PhotoUpload, Photo photothumb)
        {
            string filerename;
            string path;

            filerename = GetGen();
            path = Path.Combine(dir, filerename);

            Image source = Image.FromStream(PhotoUpload.InputStream);
            double widthRatio = ((photothumb.SetThumbnail == true ? (double)310 : (photothumb.AdListThumbnail == true ? (double)450 : (double)450))) / source.Width;
            double heightRatio = ((photothumb.SetThumbnail == true ? (double)310 : (photothumb.AdListThumbnail == true ? (double)450 : (double)450))) / source.Height;
            double ratio = (widthRatio < heightRatio) ? widthRatio : heightRatio;
            Image thumbnail = source.GetThumbnailImage((int)(source.Width * ratio), (int)(source.Height * ratio), AbortCallback, IntPtr.Zero);

            using (var memory = new MemoryStream())
            {
                thumbnail.Save(memory, source.RawFormat);
                using (var imageFile = new FileStream(path, FileMode.Create))
                {
                    imageFile.Write(memory.ToArray(), 0, memory.ToArray().Length);
                }
            }
            photothumb.FileName = filerename;
            photothumb.ContentType = PhotoUpload.ContentType;
            photothumb.CountNum = counter;
        }

        //http://bobcravens.com/2009/10/image-compression-in-c-for-asp-net-mvc/
        internal int DefaultCompressionPng(string dir, int counter, HttpPostedFileBase PhotoUpload, Photo photo)
        {
            string filerename;
            string path;

            filerename = GetGen();
            path = Path.Combine(dir, filerename);

            var getImg = Image.FromStream(PhotoUpload.InputStream, true, true);
            Bitmap original = new Bitmap(getImg);
            getImg.Dispose();
            ImageCodecInfo jpgEncoder = null;
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    jpgEncoder = codec;
                    break;
                }
            }
            if (jpgEncoder != null)
            {
                System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters encoderParameters = new EncoderParameters(1);

                long quality = 75;

                EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                encoderParameters.Param[0] = encoderParameter;

                FileStream ms = new FileStream(path, FileMode.Create, FileAccess.Write);
                original.Save(ms, jpgEncoder, encoderParameters);
                ms.Flush();
                ms.Close();
            }

            photo.FileName = filerename;
            photo.ContentType = PhotoUpload.ContentType;
            photo.CountNum = counter++;
            photo.ClassifiedAd.AdPhotos.Add(photo);
            return counter;
        }

        internal void DefaultCompressionPngEdit(string dir, int counter, HttpPostedFileBase PhotoUpload, Photo photo)
        {
            string filerename;
            string path;

            filerename = GetGen();
            path = Path.Combine(dir, filerename);

            var getImg = Image.FromStream(PhotoUpload.InputStream, true, true);
            Bitmap original = new Bitmap(getImg);
            getImg.Dispose();
            ImageCodecInfo jpgEncoder = null;
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    jpgEncoder = codec;
                    break;
                }
            }
            if (jpgEncoder != null)
            {
                System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters encoderParameters = new EncoderParameters(1);

                long quality = 75;

                EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                encoderParameters.Param[0] = encoderParameter;

                FileStream ms = new FileStream(path, FileMode.Create, FileAccess.Write);
                original.Save(ms, jpgEncoder, encoderParameters);
                ms.Flush();
                ms.Close();
            }

            photo.FileName = filerename;
            photo.ContentType = PhotoUpload.ContentType;
            photo.CountNum = counter;
        }

        internal PhotoBase GetFullPhotoById(string stringId)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                //r.CreateMap<Photo, PhotoBase>();
            }
            return null;
        }

        internal PUserPage GetPremiumUserList()
        {
            throw new NotImplementedException();
        }

        internal PUserPage GetPremiumUserByString(string id)
        {
            /*var config = new MapperConfiguration(r =>
            {
                r.CreateMap<PremiumUserData, PUserPage>();
                r.CreateMap<PremiumUserPhoto, PremiumUserPhotoBase>();
                r.CreateMap<ClassifiedAd, PUserClassifiedAdList>()
                    .ForMember(dest => dest.AdPhoto, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(y => y.AdListThumbnail == true && y.CountNum == 11)))
                    .AfterMap((src,dest) => dest.AdInfo = dest.AdInfo.GroupBy(x => x.Name).Select(g => g.First()).ToList());
                r.CreateMap<PremiumUserInfo, PremiumUserInfoBase>();
                r.CreateMap<PremiumUserReview, PremiumUserReviewBase>();
                r.CreateMap<Country, CountryBase>();
                r.CreateMap<Info, InfoForm>();
                r.CreateMap<Photo, PhotoBase>();
            });
            IMapper mapper = config.CreateMapper();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.PremUserDataDB
                    .Include("UserProfile.ClassifiedAds.AdInfo")
                    .Include("UserProfile.ClassifiedAds.AdPhotos")
                    .Include("PremiumUserPhotos")
                    .Include("UserReviews")
                    .Include("PremiumUserInfos")
                    .SingleOrDefault(x => x.UrlName.Equals(id));
                if (obj == null)
                {
                    obj = newthreadcontext.PremUserDataDB
                    .Include("UserProfile.ClassifiedAds.AdInfo")
                    .Include("UserProfile.ClassifiedAds.AdPhotos")
                    .Include("PremiumUserPhotos")
                    .Include("UserReviews")
                    .Include("PremiumUserInfos")
                    .SingleOrDefault(x => x..UserProfileStringId.Equals(id));

                    if (obj == null)
                        return null;
                }
                var item = mapper.Map<PUserPage>(obj);
                var ads = mapper.Map<IEnumerable<PUserClassifiedAdList>>(obj.UserProfile.ClassifiedAds);
                item.ClassifiedAds = new PagedList<PUserClassifiedAdList>(ads, 1, RecordsPerPage.recordsPerPage);
                return item;
            }*/
            return null;
        }

        /// <summary>
        /// Used for premium user editing
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        internal PUserProfilePage GetPremiumUserByUserIdandName(string username, int userid)
        {
            /*using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.PremUserDataDB
                    .Include("PremiumUserPhotos")
                    .Include("PremiumUserInfos")
                    .Include("UserProfile")
                    .Include("UserProfile.ClassifiedAds")
                    .SingleOrDefault(x => x.UserProfile.isSeller == true && x.UserProfile.UserId == userid && x.UserProfile.UserName.Equals(username));
                var config = new MapperConfiguration(r =>
                {
                    r.CreateMap<PremiumUserData, PUserProfilePage>();
                    r.CreateMap<PremiumUserPhoto, PremiumUserPhotoBase>();
                    r.CreateMap<PremiumUserInfo, PremiumUserInfoBase>();
                });
                IMapper mapper = config.CreateMapper();
                return (obj == null) ? null : mapper.Map<PUserProfilePage>(obj);
            }*/
            return null;
        }

        internal PUserProfileEditForm GetPremiumUserEditByUserIdandName(string name, int id)
        {
            /*using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.PremUserDataDB
                    .Include("PremiumUserPhotos")
                    .Include("PremiumUserInfos")
                    .Include("UserProfile")
                    .SingleOrDefault(x => x.UserProfile.isSeller == true && x.UserProfile.UserId == id && x.UserProfile.UserName.Equals(name));
                var config = new MapperConfiguration(r =>
                {
                    r.CreateMap<PremiumUserData, PUserProfileEditForm>()
                        .ForMember(dest => dest.PhotoUpload1, opt => opt.MapFrom(src => src.PremiumUserPhotos.SingleOrDefault(x => x.Position == 0).FileName))
                        .ForMember(dest => dest.PhotoUpload2, opt => opt.MapFrom(src => src.PremiumUserPhotos.SingleOrDefault(x => x.Position == 1).FileName))
                        .ForMember(dest => dest.PhotoUpload3, opt => opt.MapFrom(src => src.PremiumUserPhotos.SingleOrDefault(x => x.Position == 2).FileName));

                    r.CreateMap<PremiumUserPhoto, PremiumUserPhotoBase>()
                        .ForMember(dest => dest.FileName, opt => opt.ResolveUsing(src => new PremiumUserPhotoBase(){ FileName = src.FileName, Position = src.Position }));
                    r.CreateMap<PremiumUserInfo, PremiumUserInfoBase>();
                });
                IMapper mapper = config.CreateMapper();
                return (obj == null) ? null : mapper.Map<PUserProfileEditForm>(obj);
            }*/
            return null;
        }

        internal PremiumUserData PrimiumUserEdit(PUserProfileEdit editItem, HttpServerUtilityBase Server)
        {
            /*var dir = Server.MapPath("~/Photos/" + editItem.UserProfilestringId.Substring(2, 4) + "/" + editItem.UserProfilestringId.Substring(0, 4));

            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.PremUserDataDB
                    .Include("PremiumUserPhotos")
                    .Include("PremiumUserInfos")
                    .Include("UserProfile")
                    .SingleOrDefault(x => x.UserProfile.isSeller == true && x.UserProfileStringId == editItem.UserProfilestringId);

                if (obj.UrlName == null)
                {
                    obj.UrlName = editItem.UrlName;
                }
                else
                {
                    if (!obj.UrlName.Equals(editItem.UrlName))
                    {
                        obj.UrlName = editItem.UrlName;
                    }
                }

                if (obj.PremiumUserInfos != null)
                {
                    if (obj.PremiumUserInfos.Count() == 0)
                        obj.PremiumUserInfos.Add(new PremiumUserInfo() { Name = "Address", Description = editItem.PUserAddress });
                    else
                        obj.PremiumUserInfos.SingleOrDefault(x => x.Name == "Address").Description = editItem.PUserAddress;
                }
                else
                {
                    obj.PremiumUserInfos.Add(new PremiumUserInfo() { Name = "Address", Description = editItem.PUserAddress });
                }

                if (obj.PremiumUserName != null)
                {
                    if (!obj.PremiumUserName.Equals(editItem.PremiumUserName))
                    {
                        obj.PremiumUserName = editItem.PremiumUserName;
                    }
                }
                else
                {
                    obj.PremiumUserName = editItem.PremiumUserName;
                }


                if (obj.PremiumUserPhotos == null)
                {
                    obj.PremiumUserPhotos = new List<PremiumUserPhoto>();
                }

                Directory.CreateDirectory(dir);
                string path;
                if (editItem.PhotoUpload1 == null && editItem.PhotoUpload1bool)
                {
                    var temp = obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 0);
                    if (temp != null)
                    {
                        path = Path.Combine(dir, temp.FileName);
                        File.Delete(path);
                    }
                }
                else if (editItem.PhotoUpload1 != null && obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 0) == null && !editItem.PhotoUpload1bool)
                {
                    var temp = new PremiumUserPhoto()
                    {
                        ContentType = editItem.PhotoUpload1.ContentType,
                        FileName = GetGen(),
                        Position = 0,
                        UserProfile = obj.UserProfile
                    };
                    path = Path.Combine(dir, temp.FileName);
                    editItem.PhotoUpload1.SaveAs(path);
                    obj.PremiumUserPhotos.Add(temp);
                }
                else if (editItem.PhotoUpload1 != null && obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 0) != null && !editItem.PhotoUpload1bool)
                {
                    var temp = obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 0);
                    path = Path.Combine(dir, temp.FileName);
                    File.Delete(path);

                    temp.FileName = GetGen();
                    path = Path.Combine(dir, temp.FileName);
                    temp.ContentType = editItem.PhotoUpload1.ContentType;
                    editItem.PhotoUpload1.SaveAs(path);
                }

                if (editItem.PhotoUpload2 == null && editItem.PhotoUpload2bool)
                {
                    var temp = obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 1);
                    if (temp != null)
                    {
                        path = Path.Combine(dir, temp.FileName);
                        File.Delete(path);
                    }
                }
                else if (editItem.PhotoUpload2 != null && obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 1) == null && !editItem.PhotoUpload2bool)
                {
                    var temp = new PremiumUserPhoto()
                    {
                        ContentType = editItem.PhotoUpload2.ContentType,
                        FileName = GetGen(),
                        Position = 1,
                        UserProfile = obj.UserProfile
                    };
                    path = Path.Combine(dir, temp.FileName);
                    editItem.PhotoUpload2.SaveAs(path);
                    obj.PremiumUserPhotos.Add(temp);
                }
                else if (editItem.PhotoUpload2 != null && obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 1) != null && !editItem.PhotoUpload2bool)
                {
                    var temp = obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 1);
                    path = Path.Combine(dir, temp.FileName);
                    File.Delete(path);

                    temp.FileName = GetGen();
                    path = Path.Combine(dir, temp.FileName);
                    temp.ContentType = editItem.PhotoUpload2.ContentType;
                    editItem.PhotoUpload2.SaveAs(path);
                }

                if (editItem.PhotoUpload3 == null && editItem.PhotoUpload3bool)
                {
                    var temp = obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 2);
                    if (temp != null)
                    {
                        path = Path.Combine(dir, temp.FileName);
                        File.Delete(path);
                    }
                }
                else if (editItem.PhotoUpload3 != null && obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 2) == null && !editItem.PhotoUpload3bool)
                {
                    var temp = new PremiumUserPhoto()
                    {
                        ContentType = editItem.PhotoUpload3.ContentType,
                        FileName = GetGen(),
                        Position = 2,
                        UserProfile = obj.UserProfile
                    };
                    path = Path.Combine(dir, temp.FileName);
                    editItem.PhotoUpload3.SaveAs(path);
                    obj.PremiumUserPhotos.Add(temp);
                }
                else if (editItem.PhotoUpload3 != null && obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 2) != null && !editItem.PhotoUpload3bool)
                {
                    var temp = obj.PremiumUserPhotos.SingleOrDefault(x => x.Position == 2);
                    path = Path.Combine(dir, temp.FileName);
                    File.Delete(path);

                    temp.FileName = GetGen();
                    path = Path.Combine(dir, temp.FileName);
                    temp.ContentType = editItem.PhotoUpload3.ContentType;
                    editItem.PhotoUpload3.SaveAs(path);
                }


                newthreadcontext.SaveChanges();
                return obj;
            }*/
            return null;
        }

        internal Category GetCategoryByName(string name)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.CategoryDB.SingleOrDefault(x => x.Name.Contains(name));
                return (obj == null) ? null : obj;
            }
        }

        internal SubCategory GetSubCategoryByName(string name)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.SubCategoryDB.Include("Category").SingleOrDefault(x => x.Name.Contains(name));
                return (obj == null) ? null : obj;
            }
        }

        internal bool IsUrlNameTaken(string urlname, string stringId)
        {
            /*var userId = WebSecurity.CurrentUserId;
            var userName = WebSecurity.CurrentUserName;
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.PremUserDataDB.Include("UserProfile")
                    .SingleOrDefault(x => x.UrlName == urlname);
                // name exist
                if (obj != null)
                {
                    // check if it belongs to current user
                    if (obj.UserProfile.UserId == userId && obj.UserProfile.UserName == userName && obj.UserProfileStringId == stringId)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }*/
            return true;
        }

        internal bool MyAdRequestOpen(string adstringid)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.ReportDB.Include("ClassifiedAd").Where(x => x.ClassifiedAd.StringId.Equals(adstringid));
                if (obj != null)
                {
                    foreach (var o in obj)
                    {
                        o.OpenRequest = true;
                    }
                    newthreadcontext.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }

        internal SubCategory GetSubCategoryById(string stringId)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var obj = newthreadcontext.SubCategoryDB.SingleOrDefault(x => x.stringId.Equals(stringId));
                return (obj == null) ? null : obj;
            }
        }

        //http://stackoverflow.com/questions/12787449/html-agility-pack-removing-unwanted-tags-without-removing-content
        internal static string RemoveUnwantedTags(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            var document = new HtmlDocument();
            document.LoadHtml(data);

            var acceptableTags = new String[] { };

            var nodes = new Queue<HtmlNode>(document.DocumentNode.SelectNodes("./*|./text()"));
            while (nodes.Count > 0)
            {
                //block-elements - convert to line-breaks
                if (document.DocumentNode.SelectNodes("//li") != null)
                    foreach (HtmlNode n in document.DocumentNode.SelectNodes("//li")) //you could add more tags here
                    {
                        //we add a "\n" ONLY if the node contains some plain text as "direct" child
                        //meaning - text is not nested inside children, but only one-level deep

                        //use XPath to find direct "text" in element
                        var txtNode = n.SelectSingleNode("text()");

                        //no "direct" text - NOT ADDDING the \n !!!!
                        if (txtNode == null || txtNode.InnerHtml.Trim() == "") continue;

                        //"surround" the node with line breaks
                        n.ParentNode.InsertAfter(document.CreateTextNode("\r"), n);
                    }

                //todo: might need to replace multiple "\n\n" into one here, I'm still testing...

                //now BR tags - simply replace with "\n" and forget
                if (document.DocumentNode.SelectNodes("//br") != null)
                    foreach (HtmlNode n in document.DocumentNode.SelectNodes("//br"))
                        n.ParentNode.ReplaceChild(document.CreateTextNode("\r"), n);

                // a tags
                if (document.DocumentNode.SelectNodes("//a") != null)
                    foreach (HtmlNode n in document.DocumentNode.SelectNodes("//a"))
                        n.ParentNode.InsertBefore(document.CreateTextNode("\r"), n);

                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                if (!acceptableTags.Contains(node.Name) && node.Name != "#text")
                {
                    var childNodes = node.SelectNodes("./*|./text()");

                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);                            
                            parentNode.InsertBefore(child, node);
                        }
                    }
                    parentNode.RemoveChild(node);
                }
            }

            return document.DocumentNode.InnerHtml;
        }

        internal int GetLuceneAdCount()
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var items = newthreadcontext.CategoryDB;
                int counter = 0;
                foreach (var num in items)
                {
                    counter += num.TotalClassifiedAdsCount;
                }
                return counter;
            }
        }
    }
}