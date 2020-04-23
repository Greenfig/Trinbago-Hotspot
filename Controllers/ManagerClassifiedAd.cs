using AutoMapper;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Trinbago_MVC5.Models;
namespace Trinbago_MVC5.Controllers
{
    public class ManagerClassifiedAd : Manager
    {
        // Add new Classified Ad
        internal ClassifiedAdBase ClassifiedAdAdd(ClassifiedAdAdd newItem, HttpServerUtilityBase Server)
        {
            // 1. Set time
            // 2. Add item to db
            // 3. Add photos to data store then to temporary collection
            // 4. Create thumbnail image
            // 5. Add Location
            // 6. Add collection to db item
            // 7. Save database 
            // set up automapper to map classified
            var config = new MapperConfiguration(r =>
            {
                r.CreateMap<ClassifiedAdAdd, ClassifiedAd>().ForMember(dest => dest.Price, opts => opts.MapFrom(src => Int32.Parse(src.Price.Replace(",", ""))));
                r.CreateMap<InfoForm, Info>();
                r.CreateMap<ClassifiedAd, ClassifiedAdBase>();
            });
            IMapper mapper = config.CreateMapper();

            newItem.TimeStamp = DateTime.Now;
            if (newItem.Price == "Please Contact" && newItem.PriceInfo != "Please Contact")
                newItem.PriceInfo = "Please Contact";
            newItem.Price = (newItem.Price == "Please Contact" ? "0" : newItem.Price);
            ClassifiedAd addedItem = new ClassifiedAd();

            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                addedItem = newthreadcontext.ClassifiedDB.Add(mapper.Map<ClassifiedAd>(newItem));

                // add html free string
                addedItem.HtmlFreeDescription = RemoveUnwantedTags(addedItem.Description);


                // convert string descriptions to int before we pull from db
                var convertInfo_Mileage = addedItem.AdInfo.SingleOrDefault(x => x.Name.Equals("Mileage"));
                if (convertInfo_Mileage != null)
                    if (!String.IsNullOrEmpty(convertInfo_Mileage.Description))
                        convertInfo_Mileage.IntDescription = Convert.ToInt32(convertInfo_Mileage.Description);

                var convertInfo_Size = addedItem.AdInfo.SingleOrDefault(x => x.Name.Equals("Size"));
                if (convertInfo_Size != null)
                    if (!String.IsNullOrEmpty(convertInfo_Size.Description))
                        convertInfo_Size.IntDescription = Convert.ToInt32(convertInfo_Size.Description);


                var convertInfo_Year = addedItem.AdInfo.SingleOrDefault(x => x.Name.Equals("Year"));
                if (newItem.SubCategoryName.Equals("Automotive Parts"))
                {
                    if (!String.IsNullOrEmpty(convertInfo_Year.Description))
                    {
                        if (convertInfo_Year.Description.Contains("-"))
                        {
                            var years = convertInfo_Year.Description.Split('-');
                            convertInfo_Year.Description = years[0];
                            convertInfo_Year.IntDescription = Convert.ToInt32(years[0]);
                            if (years.Length > 1)
                            {
                                for (int i = Convert.ToInt32(years[0]); i <= Convert.ToInt32(years[1]); i++)
                                {
                                    addedItem.AdInfo.Add(new Info() { Name = "Year", Description = i.ToString(), IntDescription = i, ClassifiedAd = addedItem });
                                }
                            }
                        }
                        else
                            convertInfo_Year.IntDescription = Convert.ToInt32(convertInfo_Year.Description);
                    }
                }
                else
                {
                    if (convertInfo_Year != null)
                        if (!String.IsNullOrEmpty(convertInfo_Year.Description))
                            convertInfo_Year.IntDescription = Convert.ToInt32(convertInfo_Year.Description);
                }

                // pet age
                var convertAge = addedItem.AdInfo.SingleOrDefault(x => x.Name.Equals("Age"));
                if (convertAge != null)
                {
                    if (!String.IsNullOrEmpty(convertAge.Description))
                    {
                        convertAge.IntDescription = Convert.ToInt32(convertAge.Description);
                        convertAge.Description += " " + newItem.AgeType;

                        if (!String.IsNullOrEmpty(newItem.AgeType))
                        {
                            if (newItem.AgeType == "Days" && convertAge.IntDescription > 7)
                            {
                                double _weeks = (double)convertAge.IntDescription / 7.0;
                                double _months = (double)convertAge.IntDescription / 31;
                                double _year = (double)convertAge.IntDescription / 365;
                                int weeks = (int)Math.Floor(_weeks);
                                int months = (int)Math.Floor(_months);
                                int years = (int)Math.Floor(_year);

                                if (weeks > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = weeks + " " + "Weeks", IntDescription = weeks, ClassifiedAd = addedItem });
                                if (months > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = months + " " + "Months", IntDescription = months, ClassifiedAd = addedItem });
                                if (years > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = years + " " + "Years", IntDescription = years, ClassifiedAd = addedItem });
                            }
                            else if (newItem.AgeType == "Weeks" && convertAge.IntDescription > 1)
                            {
                                double _days = ((double)convertAge.IntDescription * 7);
                                double _months = (double)convertAge.IntDescription / 4;
                                double _year = (double)convertAge.IntDescription / 48;
                                int days = (int)Math.Floor(_days);
                                int months = (int)Math.Floor(_months);
                                int years = (int)Math.Floor(_year);

                                if (days > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = days + " " + "Days", IntDescription = days, ClassifiedAd = addedItem });
                                if (months > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = months + " " + "Months", IntDescription = months, ClassifiedAd = addedItem });
                                if (years > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = years + " " + "Years", IntDescription = years, ClassifiedAd = addedItem });
                            }
                            else if (newItem.AgeType == "Months" && convertAge.IntDescription > 1)
                            {
                                double _days = (double)convertAge.IntDescription * 31;
                                double _weeks = (double)convertAge.IntDescription * 4;
                                double _year = (double)convertAge.IntDescription / 12;
                                int days = (int)Math.Floor(_days);
                                int weeks = (int)Math.Floor(_weeks);
                                int years = (int)Math.Floor(_year);

                                if (days > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = days + " " + "Days", IntDescription = days, ClassifiedAd = addedItem });
                                if (weeks > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = weeks + " " + "Weeks", IntDescription = weeks, ClassifiedAd = addedItem });
                                if (years > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = years + " " + "Years", IntDescription = years, ClassifiedAd = addedItem });
                            }
                            else if (newItem.AgeType == "Years" && convertAge.IntDescription > 1)
                            {
                                double _days = (double)convertAge.IntDescription * 365;
                                double _weeks = (double)convertAge.IntDescription * 12 * 4;
                                double _months = (double)convertAge.IntDescription * 12;
                                int days = (int)Math.Floor(_days);
                                int weeks = (int)Math.Floor(_weeks);
                                int months = (int)Math.Floor(_months);

                                if (days > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = days + " " + "Days", IntDescription = days, ClassifiedAd = addedItem });
                                if (weeks > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = weeks + " " + "Weeks", IntDescription = weeks, ClassifiedAd = addedItem });
                                if (months > 0)
                                    addedItem.AdInfo.Add(new Info() { Name = "Age", Description = months + " " + "Months", IntDescription = months, ClassifiedAd = addedItem });
                            }
                        }
                    }
                }

                // format adinfo
                var format_EngineSize = addedItem.AdInfo.SingleOrDefault(x => x.Name.Equals("Engine Size"));
                if (format_EngineSize != null)
                    if (!String.IsNullOrEmpty(format_EngineSize.Description))
                        format_EngineSize.Description.Replace(" ", "");

                var sc = newthreadcontext.SubCategoryDB.Include("Category").SingleOrDefault(i => i.stringId.Equals(newItem.SubCategorystringId));

                addedItem.Category = sc.Category;
                addedItem.SubCategory = sc;

                ICollection<HttpPostedFileBase> PhotoUploads = new List<HttpPostedFileBase>();
                // 3
                if (newItem.PhotoUpload1 != null)
                    PhotoUploads.Add(newItem.PhotoUpload1);
                if (newItem.PhotoUpload2 != null)
                    PhotoUploads.Add(newItem.PhotoUpload2);
                if (newItem.PhotoUpload3 != null)
                    PhotoUploads.Add(newItem.PhotoUpload3);
                if (newItem.PhotoUpload4 != null)
                    PhotoUploads.Add(newItem.PhotoUpload4);
                if (newItem.PhotoUpload5 != null)
                    PhotoUploads.Add(newItem.PhotoUpload5);
                if (newItem.PhotoUpload6 != null)
                    PhotoUploads.Add(newItem.PhotoUpload6);
                if (newItem.PhotoUpload7 != null)
                    PhotoUploads.Add(newItem.PhotoUpload7);
                if (newItem.PhotoUpload8 != null)
                    PhotoUploads.Add(newItem.PhotoUpload8);
                if (newItem.PhotoUpload9 != null)
                    PhotoUploads.Add(newItem.PhotoUpload9);
                if (newItem.PhotoUpload10 != null)
                    PhotoUploads.Add(newItem.PhotoUpload10);
                if (newItem.PhotoUpload11 != null)
                    PhotoUploads.Add(newItem.PhotoUpload11);
                if (newItem.PhotoUpload12 != null)
                    PhotoUploads.Add(newItem.PhotoUpload12);

                if (PhotoUploads.Count > 0)
                {
                    var dir = Server.MapPath("~/Photos/" + addedItem.StringId.Substring(2, 4) + "/" + addedItem.StringId.Substring(0, 4));
                    Directory.CreateDirectory(dir);
                    var thumb = false;
                    int counter = 10;
                    int counter2 = 11;
                    foreach (var PhotoUpload in PhotoUploads)
                    {
                        if (!thumb)
                        {
                            counter = CompressPhoto(dir, counter, PhotoUpload, new Photo() { SetThumbnail = true, ClassifiedAd = addedItem });
                            thumb = true;
                        }

                        //
                        // Compress image
                        counter2 = CompressPhoto(dir, counter2, PhotoUpload, new Photo() { AdListThumbnail = true, ClassifiedAd = addedItem });

                        counter = DefaultCompressionPng(dir, counter, PhotoUpload, new Photo() { ClassifiedAd = addedItem });
                    }
                }
                // 5
                var country = newthreadcontext.CountryDB.Include("Regions").Include("Regions.ClassifiedAds").SingleOrDefault(a => a.Id == newItem.CountryId);

                addedItem.Country = country;
                country.Regions.SingleOrDefault(a => a.Id == newItem.RegionId).ClassifiedAds.Add(addedItem);

                // Initilize the user associated with the Ad

                addedItem.UserCreator = newthreadcontext.Users.SingleOrDefault(x => x.Email == CurrentUser.Email);
                //format the phone number
                addedItem.UserContactPhone = (addedItem.UserContactPhone.Contains("-") ? addedItem.UserContactPhone : addedItem.UserContactPhone.Insert(3, "-").ToString());
                if (!String.IsNullOrEmpty(addedItem.UserContactPhone2))
                    addedItem.UserContactPhone2 = (addedItem.UserContactPhone2.Contains("-") ? addedItem.UserContactPhone2 : addedItem.UserContactPhone2.Insert(3, "-").ToString());
                if (!String.IsNullOrEmpty(addedItem.UserContactPhone3))
                    addedItem.UserContactPhone3 = (addedItem.UserContactPhone3.Contains("-") ? addedItem.UserContactPhone3 : addedItem.UserContactPhone3.Insert(3, "-").ToString());

                // increment count
                addedItem.Category.TotalClassifiedAdsCount++;
                addedItem.SubCategory.ClassifiedAdsCount++;
                newthreadcontext.SaveChanges();
                // Add to Lucene
                LuceneSearch.AddUpdateLuceneIndex(addedItem);
            }
            return (addedItem == null) ? null : mapper.Map<ClassifiedAdBase>(addedItem);
        }
        
        /// <summary>
        /// EDIT FORM
        /// </summary>
        /// <returns></returns>
        internal ClassifiedAdBase ClassifiedAdEdit(ClassifiedAdEdit editItem, HttpServerUtilityBase Server, string User)
        {
            // 1. Ensure user credibility
            // 2. Attempt to fetch obj
            // 3. Set last edited time
            // 4. Pull existing item
            // 5. Compare photos for changes and add/remove
            // 6. Save changes

            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.ClassifiedDB
                        .Include("AdPhotos")
                        .Include("Country")
                        .Include("Region")
                        .Include("UserCreator")
                        .Include("AdInfo")
                        .Include("Category")
                        .Include("SubCategory")
                        .SingleOrDefault(x => x.StringId == editItem.StringId && x.UserCreator.Id == CurrentUser.Id);

                if (o == null)
                {
                    return null;
                }
                else
                {
                    // convert string descriptions to int before we pull from db
                    var convertInfo_Mileage = o.AdInfo.SingleOrDefault(x => x.Name.Equals("Mileage"));
                    if (convertInfo_Mileage != null)
                        if (!String.IsNullOrEmpty(convertInfo_Mileage.Description))
                            convertInfo_Mileage.IntDescription = Convert.ToInt32(convertInfo_Mileage.Description.Replace(",", ""));

                    var convertInfo_Size = o.AdInfo.SingleOrDefault(x => x.Name.Equals("Size"));
                    if (convertInfo_Size != null)
                        if (!String.IsNullOrEmpty(convertInfo_Size.Description))
                            convertInfo_Size.IntDescription = Convert.ToInt32(convertInfo_Size.Description.Replace(",", ""));

                    // year
                    // delete old years
                    var deleteyears = o.AdInfo.Where(x => x.Name.Equals("Year")).ToList();
                    if (deleteyears != null && deleteyears.Count > 0)
                    {
                        foreach (var dy in deleteyears)
                        {
                            if (deleteyears.FirstOrDefault() != dy)
                            {
                                dy.ClassifiedAd = null;
                                o.AdInfo.Remove(dy);
                                newthreadcontext.InfosDB.Remove(dy);
                            }
                            else
                            {
                                dy.Description = null;
                                dy.IntDescription = 0;
                            }
                        }

                        var convertInfo_Year = new Info() { Description = editItem.AdInfo.SingleOrDefault(x => x.Name.Equals("Year")).Description, Name = "Year" };
                        if (editItem.SubCategoryName.Equals("Automotive Parts"))
                        {
                            if (!String.IsNullOrEmpty(convertInfo_Year.Description))
                            {
                                if (convertInfo_Year.Description.Contains("-"))
                                {
                                    var years = convertInfo_Year.Description.Split('-');
                                    convertInfo_Year.Description = years[0];
                                    convertInfo_Year.IntDescription = Convert.ToInt32(years[0]);
                                    var updatey = o.AdInfo.FirstOrDefault(x => x.Name.Equals("Year"));
                                    updatey.Description = convertInfo_Year.Description;
                                    updatey.IntDescription = convertInfo_Year.IntDescription;
                                    if (years.Length > 1)
                                    {
                                        for (int i = Convert.ToInt32(years[0]); i <= Convert.ToInt32(years[1]); i++)
                                        {
                                            o.AdInfo.Add(new Info() { Name = "Year", Description = i.ToString(), IntDescription = i, ClassifiedAd = o });
                                        }
                                    }
                                }
                                else
                                {
                                    convertInfo_Year.IntDescription = Convert.ToInt32(convertInfo_Year.Description);
                                    var updatey = o.AdInfo.FirstOrDefault(x => x.Name.Equals("Year"));
                                    updatey.Description = convertInfo_Year.Description;
                                    updatey.IntDescription = convertInfo_Year.IntDescription;
                                }
                            }
                        }
                        else
                        {
                            if (convertInfo_Year != null)
                                if (!String.IsNullOrEmpty(convertInfo_Year.Description))
                                    convertInfo_Year.IntDescription = Convert.ToInt32(convertInfo_Year.Description);
                            var updatey = o.AdInfo.FirstOrDefault(x => x.Name.Equals("Year"));
                            updatey.Description = convertInfo_Year.Description;
                            updatey.IntDescription = convertInfo_Year.IntDescription;
                        }
                    }

                    // pet age
                    var deleteages = o.AdInfo.Where(x => x.Name.Equals("Age")).ToList();
                    if (deleteages != null && deleteages.Count > 0)
                    {
                        foreach (var da in deleteages)
                        {
                            da.ClassifiedAd = null;
                            o.AdInfo.Remove(da);
                            newthreadcontext.InfosDB.Remove(da);
                        }
                        var convertAge = new Info() { Description = editItem.AdInfo.SingleOrDefault(x => x.Name.Equals("Age")).Description, Name = "Age" };
                        if (convertAge != null)
                        {
                            if (!String.IsNullOrEmpty(convertAge.Description))
                            {
                                convertAge.IntDescription = Convert.ToInt32(convertAge.Description);
                                convertAge.Description += " " + editItem.AgeType;
                                o.AdInfo.Add(convertAge);
                                if (!String.IsNullOrEmpty(editItem.AgeType))
                                {
                                    if (editItem.AgeType == "Days" && convertAge.IntDescription > 7)
                                    {
                                        double _weeks = (double)convertAge.IntDescription / 7.0;
                                        double _months = (double)convertAge.IntDescription / 31;
                                        double _year = (double)convertAge.IntDescription / 365;
                                        int weeks = (int)Math.Floor(_weeks);
                                        int months = (int)Math.Floor(_months);
                                        int years = (int)Math.Floor(_year);

                                        if (weeks > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = weeks + " " + "Weeks", IntDescription = weeks, ClassifiedAd = o });
                                        if (months > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = months + " " + "Months", IntDescription = months, ClassifiedAd = o });
                                        if (years > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = years + " " + "Years", IntDescription = years, ClassifiedAd = o });
                                    }
                                    else if (editItem.AgeType == "Weeks" && convertAge.IntDescription > 1)
                                    {
                                        double _days = ((double)convertAge.IntDescription * 7);
                                        double _months = (double)convertAge.IntDescription / 4;
                                        double _year = (double)convertAge.IntDescription / 48;
                                        int days = (int)Math.Floor(_days);
                                        int months = (int)Math.Floor(_months);
                                        int years = (int)Math.Floor(_year);

                                        if (days > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = days + " " + "Days", IntDescription = days, ClassifiedAd = o });
                                        if (months > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = months + " " + "Months", IntDescription = months, ClassifiedAd = o });
                                        if (years > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = years + " " + "Years", IntDescription = years, ClassifiedAd = o });
                                    }
                                    else if (editItem.AgeType == "Months" && convertAge.IntDescription > 1)
                                    {
                                        double _days = (double)convertAge.IntDescription * 31;
                                        double _weeks = (double)convertAge.IntDescription * 4;
                                        double _year = (double)convertAge.IntDescription / 12;
                                        int days = (int)Math.Floor(_days);
                                        int weeks = (int)Math.Floor(_weeks);
                                        int years = (int)Math.Floor(_year);

                                        if (days > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = days + " " + "Days", IntDescription = days, ClassifiedAd = o });
                                        if (weeks > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = weeks + " " + "Weeks", IntDescription = weeks, ClassifiedAd = o });
                                        if (years > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = years + " " + "Years", IntDescription = years, ClassifiedAd = o });
                                    }
                                    else if (editItem.AgeType == "Years" && convertAge.IntDescription > 1)
                                    {
                                        double _days = (double)convertAge.IntDescription * 365;
                                        double _weeks = (double)convertAge.IntDescription * 12 * 4;
                                        double _months = (double)convertAge.IntDescription * 12;
                                        int days = (int)Math.Floor(_days);
                                        int weeks = (int)Math.Floor(_weeks);
                                        int months = (int)Math.Floor(_months);

                                        if (days > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = days + " " + "Days", IntDescription = days, ClassifiedAd = o });
                                        if (weeks > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = weeks + " " + "Weeks", IntDescription = weeks, ClassifiedAd = o });
                                        if (months > 0)
                                            o.AdInfo.Add(new Info() { Name = "Age", Description = months + " " + "Months", IntDescription = months, ClassifiedAd = o });
                                    }
                                }
                            }
                            else
                            {
                                o.AdInfo.Add(convertAge);
                            }
                        }
                    }

                    // format adinfo
                    var format_EngineSize = o.AdInfo.SingleOrDefault(x => x.Name.Equals("Engine Size"));
                    if (format_EngineSize != null)
                        if (!String.IsNullOrEmpty(format_EngineSize.Description))
                            format_EngineSize.Description.Replace(" ", "");

                    o.EditTimeStamp = DateTime.Now;

                    if (o.ContactPrivacy != editItem.ContactPrivacy)
                    {
                        o.ContactPrivacy = editItem.ContactPrivacy;
                    }

                    /// Upadate Country/Region
                    /// 
                    if (o.Country.Id != editItem.CountryId)
                    {
                        var country = newthreadcontext.CountryDB.Include("Regions").Include("Regions.ClassifiedAds").SingleOrDefault(a => a.Id == editItem.CountryId);
                        o.Country = country;
                    }
                    if (o.Region.Id != editItem.RegionId)
                    {
                        var country = newthreadcontext.CountryDB.Include("Regions").Include("Regions.ClassifiedAds").SingleOrDefault(a => a.Id == editItem.CountryId);
                        country.Regions.SingleOrDefault(a => a.Id == editItem.RegionId).ClassifiedAds.Add(o);
                    }
                    if (editItem.Price == "Please Contact" && editItem.PriceInfo != "Please Contact")
                        editItem.PriceInfo = "Please Contact";
                    // update price
                    var pri = Convert.ToInt32(editItem.Price.Equals("Please Contact") ? "0" : editItem.Price.Replace(",", ""));
                    if (o.Price != pri)
                        o.Price = pri;

                    // update desc
                    if (o.Description != editItem.Description)
                    {
                        o.Description = editItem.Description;
                        o.HtmlFreeDescription = RemoveUnwantedTags(o.Description);
                    }

                    // update title
                    if (o.Title != editItem.Title)
                        o.Title = editItem.Title;

                    // update price info
                    if (o.PriceInfo != editItem.PriceInfo)
                    {
                        o.PriceInfo = editItem.PriceInfo;
                    }

                    // update AdInfo
                    if (editItem.AdInfo != null && o.AdInfo != null)
                    {
                        if (editItem.AdInfo.Count > 0 && o.AdInfo.Count > 0)
                        {
                            foreach (var ai in editItem.AdInfo)
                            {
                                if (!ai.Name.Equals("Age") && !ai.Name.Equals("Year"))
                                {
                                    var currentai = o.AdInfo.SingleOrDefault(x => x.Name.Equals(ai.Name));
                                    if (currentai.Description != null)
                                    {
                                        if (!currentai.Description.Equals(ai.Description))
                                        {
                                            currentai.Description = ai.Description;
                                        }
                                    }
                                    else
                                    {
                                        currentai.Description = ai.Description;
                                    }
                                }
                            }
                        }
                    }

                    if (!editItem.UserContactName.Equals(o.UserContactName))
                        o.UserContactName = editItem.UserContactName;

                    if (!editItem.UserContactPhone.Equals(o.UserContactPhone))
                    {
                        editItem.UserContactPhone = (editItem.UserContactPhone.Contains("-") ? editItem.UserContactPhone : editItem.UserContactPhone.Insert(3, "-").ToString());
                        o.UserContactPhone = editItem.UserContactPhone;
                    }
                    if (!String.IsNullOrEmpty(editItem.UserContactPhone2))
                    {
                        if (!editItem.UserContactPhone2.Equals(o.UserContactPhone2))
                        {
                            editItem.UserContactPhone2 = (editItem.UserContactPhone2.Contains("-") ? editItem.UserContactPhone2 : editItem.UserContactPhone2.Insert(3, "-").ToString());
                            o.UserContactPhone2 = editItem.UserContactPhone2;
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(o.UserContactPhone2))
                            o.UserContactPhone2 = null;
                    }
                    if (!String.IsNullOrEmpty(editItem.UserContactPhone3))
                    {
                        if (!editItem.UserContactPhone3.Equals(o.UserContactPhone3))
                        {
                            editItem.UserContactPhone3 = (editItem.UserContactPhone3.Contains("-") ? editItem.UserContactPhone3 : editItem.UserContactPhone3.Insert(3, "-").ToString());
                            o.UserContactPhone3 = editItem.UserContactPhone3;
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(o.UserContactPhone3))
                            o.UserContactPhone3 = null;
                    }

                    if (!editItem.UserContactEmail.Equals(o.UserContactEmail))
                        o.UserContactEmail = editItem.UserContactEmail;

                    // Update photos
                    var dir = Server.MapPath("~/Photos/" + editItem.StringId.Substring(2, 4) + "/" + editItem.StringId.Substring(0, 4));
                    string path;

                    // replace existing photo1
                    if (editItem.PhotoUpload1 != null && !editItem.PhotoUpload1bool)
                    {
                        // Delete thumbnail
                        var thumb = o.AdPhotos.SingleOrDefault(a => a.SetThumbnail == true);
                        try
                        {
                            path = Path.Combine(dir, thumb.FileName);
                            // delete physical
                            File.Delete(path);
                        }
                        catch (Exception) { Directory.CreateDirectory(dir); } // create directory if failed
                        //convert first img into thumbnail
                        if (thumb != null)
                            CompressPhotoEdit(dir, 10, editItem.PhotoUpload1, thumb);
                        else
                            CompressPhoto(dir, 10, editItem.PhotoUpload1, new Photo() { SetThumbnail = true, ClassifiedAd = o });

                        // Delete Ad List Tumbnail
                        var first_t = o.AdPhotos.SingleOrDefault(a => a.CountNum == 11 && a.AdListThumbnail == true && a.SetThumbnail == false);

                        if (first_t != null)
                        {
                            path = Path.Combine(dir, first_t.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 11, editItem.PhotoUpload1, first_t);
                        }
                        else
                        {
                            CompressPhoto(dir, 11, editItem.PhotoUpload1, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }

                        // Delete Photo
                        var first = o.AdPhotos.SingleOrDefault(a => a.CountNum == 11 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (first != null)
                        {
                            path = Path.Combine(dir, first.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 11, editItem.PhotoUpload1, first);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 11, editItem.PhotoUpload1, new Photo() { ClassifiedAd = o });
                        }

                    }
                    else if (editItem.PhotoUpload1bool)
                    {
                        // REMOVE IMG
                        // delete thumbnail
                        Photo _dThumb = o.AdPhotos.SingleOrDefault(x => x.CountNum == 10 && x.AdListThumbnail == false && x.SetThumbnail == true);
                        if (_dThumb != null)
                        {
                            path = Path.Combine(dir, _dThumb.FileName);
                            File.Delete(path);
                            _dThumb.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dThumb);
                            o.AdPhotos.Remove(_dThumb);
                        }
                        // delete first
                        Photo _dFirst = o.AdPhotos.SingleOrDefault(x => x.CountNum == 11 && x.SetThumbnail == false && x.AdListThumbnail == false);
                        if (_dFirst != null)
                        {
                            path = Path.Combine(dir, _dFirst.FileName);
                            File.Delete(path);
                            _dFirst.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFirst);
                            o.AdPhotos.Remove(_dFirst);
                        }
                        // delete first ad list tumb
                        Photo _dFirstALT = o.AdPhotos.SingleOrDefault(x => x.CountNum == 11 && x.SetThumbnail == false && x.AdListThumbnail == true);
                        if (_dFirstALT != null)
                        {
                            path = Path.Combine(dir, _dFirstALT.FileName);
                            File.Delete(path);
                            _dFirstALT.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFirstALT);
                            o.AdPhotos.Remove(_dFirstALT);
                        }
                    }

                    // replace photo 2
                    if (editItem.PhotoUpload2 != null && !editItem.PhotoUpload2bool)
                    {
                        // replace regular photo 2
                        var second = o.AdPhotos.SingleOrDefault(a => a.CountNum == 12 && a.SetThumbnail == false && a.AdListThumbnail == false);
                        if (second != null)
                        {
                            path = Path.Combine(dir, second.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 12, editItem.PhotoUpload2, second);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 12, editItem.PhotoUpload2, new Photo() { ClassifiedAd = o });
                        }

                        // remove adlist tumb photo 2
                        var second_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 12 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (second_ != null)
                        {
                            path = Path.Combine(dir, second_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 12, editItem.PhotoUpload2, second_);
                        }
                        else
                        {
                            CompressPhoto(dir, 12, editItem.PhotoUpload2, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload2bool)
                    {
                        // delete second
                        Photo _dSecond = o.AdPhotos.SingleOrDefault(x => x.CountNum == 12 && x.SetThumbnail == false && x.AdListThumbnail == false);
                        if (_dSecond != null)
                        {
                            path = Path.Combine(dir, _dSecond.FileName);
                            File.Delete(path);
                            _dSecond.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSecond);
                            o.AdPhotos.Remove(_dSecond);
                        }
                        // delete second alt
                        Photo _dSecond_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 12 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dSecond_ != null)
                        {
                            path = Path.Combine(dir, _dSecond_.FileName);
                            File.Delete(path);
                            _dSecond_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSecond_);
                            o.AdPhotos.Remove(_dSecond_);
                        }
                    }

                    // replace third photo
                    if (editItem.PhotoUpload3 != null && !editItem.PhotoUpload3bool)
                    {
                        // replace regular photo
                        var third = o.AdPhotos.SingleOrDefault(a => a.CountNum == 13 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (third != null)
                        {
                            path = Path.Combine(dir, third.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 13, editItem.PhotoUpload3, third);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 13, editItem.PhotoUpload3, new Photo() { ClassifiedAd = o });
                        }

                        // replace adlist thumbnail photo
                        var third_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 13 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (third_ != null)
                        {
                            path = Path.Combine(dir, third_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 13, editItem.PhotoUpload3, third_);
                        }
                        else
                        {
                            CompressPhoto(dir, 13, editItem.PhotoUpload3, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }

                    }
                    else if (editItem.PhotoUpload3bool)
                    {
                        // delete third
                        Photo _dThird = o.AdPhotos.SingleOrDefault(x => x.CountNum == 13 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dThird != null)
                        {
                            path = Path.Combine(dir, _dThird.FileName);
                            File.Delete(path);
                            _dThird.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dThird);
                            o.AdPhotos.Remove(_dThird);
                        }
                        // delete third alt
                        Photo _dThird_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 13 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dThird_ != null)
                        {
                            path = Path.Combine(dir, _dThird_.FileName);
                            File.Delete(path);
                            _dThird_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dThird_);
                            o.AdPhotos.Remove(_dThird_);
                        }
                    }

                    // replace fourth photo
                    if (editItem.PhotoUpload4 != null && !editItem.PhotoUpload4bool)
                    {
                        // replace fourth regular
                        var fourth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 14 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (fourth != null)
                        {
                            path = Path.Combine(dir, fourth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 14, editItem.PhotoUpload4, fourth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 14, editItem.PhotoUpload4, new Photo() { ClassifiedAd = o });
                        }

                        // replace forth adlist photo
                        var forth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 14 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (forth_ != null)
                        {
                            path = Path.Combine(dir, forth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 14, editItem.PhotoUpload4, forth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 14, editItem.PhotoUpload4, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload4bool)
                    {
                        // delete fourth
                        Photo _dFourth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 14 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dFourth != null)
                        {
                            path = Path.Combine(dir, _dFourth.FileName);
                            File.Delete(path);
                            _dFourth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFourth);
                            o.AdPhotos.Remove(_dFourth);
                        }
                        // delete third alt
                        Photo _dFourth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 14 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dFourth_ != null)
                        {
                            path = Path.Combine(dir, _dFourth_.FileName);
                            File.Delete(path);
                            _dFourth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFourth_);
                            o.AdPhotos.Remove(_dFourth_);
                        }
                    }

                    if (editItem.PhotoUpload5 != null && !editItem.PhotoUpload5bool)
                    {
                        //replace fifth photo regular
                        var fifth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 15 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (fifth != null)
                        {
                            path = Path.Combine(dir, fifth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 15, editItem.PhotoUpload5, fifth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 15, editItem.PhotoUpload5, new Photo() { ClassifiedAd = o });
                        }

                        // replace fifth photo adlist thumb
                        var fifth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 15 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (fifth_ != null)
                        {
                            path = Path.Combine(dir, fifth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 15, editItem.PhotoUpload5, fifth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 15, editItem.PhotoUpload5, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload5bool)
                    {
                        // delete fifth
                        Photo _dFifth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 15 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dFifth != null)
                        {
                            path = Path.Combine(dir, _dFifth.FileName);
                            File.Delete(path);
                            _dFifth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFifth);
                            o.AdPhotos.Remove(_dFifth);
                        }
                        // delete fifth alt
                        Photo _dFifth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 15 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dFifth_ != null)
                        {
                            path = Path.Combine(dir, _dFifth_.FileName);
                            File.Delete(path);
                            _dFifth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dFifth_);
                            o.AdPhotos.Remove(_dFifth_);
                        }
                    }

                    // replace sith photo
                    if (editItem.PhotoUpload6 != null && !editItem.PhotoUpload6bool)
                    {
                        // replace sith photo regular
                        var sixth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 16 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (sixth != null)
                        {
                            path = Path.Combine(dir, sixth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 16, editItem.PhotoUpload6, sixth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 16, editItem.PhotoUpload6, new Photo() { ClassifiedAd = o });
                        }
                        var sixth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 16 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (sixth_ != null)
                        {
                            path = Path.Combine(dir, sixth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 16, editItem.PhotoUpload6, sixth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 16, editItem.PhotoUpload6, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload6bool)
                    {
                        // delete sixth
                        Photo _dSixth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 16 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dSixth != null)
                        {
                            path = Path.Combine(dir, _dSixth.FileName);
                            File.Delete(path);
                            _dSixth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSixth);
                            o.AdPhotos.Remove(_dSixth);
                        }
                        // delete fifth alt
                        Photo _dSixth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 16 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dSixth_ != null)
                        {
                            path = Path.Combine(dir, _dSixth_.FileName);
                            File.Delete(path);
                            _dSixth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSixth_);
                            o.AdPhotos.Remove(_dSixth_);
                        }
                    }

                    // replace seventh photo
                    if (editItem.PhotoUpload7 != null && !editItem.PhotoUpload7bool)
                    {
                        // replace seventh photo regular
                        var seventh = o.AdPhotos.SingleOrDefault(a => a.CountNum == 17 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (seventh != null)
                        {
                            path = Path.Combine(dir, seventh.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 17, editItem.PhotoUpload7, seventh);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 17, editItem.PhotoUpload7, new Photo() { ClassifiedAd = o });
                        }
                        var seventh_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 17 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (seventh_ != null)
                        {
                            path = Path.Combine(dir, seventh_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 17, editItem.PhotoUpload7, seventh_);
                        }
                        else
                        {
                            CompressPhoto(dir, 17, editItem.PhotoUpload7, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload7bool)
                    {
                        // delete seventh
                        Photo _dSeventh = o.AdPhotos.SingleOrDefault(x => x.CountNum == 17 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dSeventh != null)
                        {
                            path = Path.Combine(dir, _dSeventh.FileName);
                            File.Delete(path);
                            _dSeventh.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSeventh);
                            o.AdPhotos.Remove(_dSeventh);
                        }
                        // delete seventh alt
                        Photo _dSeventh_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 17 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dSeventh_ != null)
                        {
                            path = Path.Combine(dir, _dSeventh_.FileName);
                            File.Delete(path);
                            _dSeventh_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dSeventh_);
                            o.AdPhotos.Remove(_dSeventh_);
                        }
                    }

                    // replace eighth photo
                    if (editItem.PhotoUpload8 != null && !editItem.PhotoUpload8bool)
                    {
                        // replace eighth photo regular
                        var eighth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 18 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (eighth != null)
                        {
                            path = Path.Combine(dir, eighth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 18, editItem.PhotoUpload8, eighth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 18, editItem.PhotoUpload8, new Photo() { ClassifiedAd = o });
                        }
                        var eighth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 18 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (eighth_ != null)
                        {
                            path = Path.Combine(dir, eighth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 18, editItem.PhotoUpload8, eighth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 18, editItem.PhotoUpload8, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload8bool)
                    {
                        // delete eighth
                        Photo _dEighth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 18 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dEighth != null)
                        {
                            path = Path.Combine(dir, _dEighth.FileName);
                            File.Delete(path);
                            _dEighth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dEighth);
                            o.AdPhotos.Remove(_dEighth);
                        }
                        // delete eighth alt
                        Photo _dEighth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 18 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dEighth_ != null)
                        {
                            path = Path.Combine(dir, _dEighth_.FileName);
                            File.Delete(path);
                            _dEighth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dEighth_);
                            o.AdPhotos.Remove(_dEighth_);
                        }
                    }

                    // replace ninth photo
                    if (editItem.PhotoUpload9 != null && !editItem.PhotoUpload9bool)
                    {
                        // replace ninth photo regular
                        var ninth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 19 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (ninth != null)
                        {
                            path = Path.Combine(dir, ninth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 19, editItem.PhotoUpload9, ninth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 19, editItem.PhotoUpload9, new Photo() { ClassifiedAd = o });
                        }
                        var ninth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 19 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (ninth_ != null)
                        {
                            path = Path.Combine(dir, ninth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 19, editItem.PhotoUpload9, ninth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 19, editItem.PhotoUpload9, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload9bool)
                    {
                        // delete ninth
                        Photo _dNinth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 19 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dNinth != null)
                        {
                            path = Path.Combine(dir, _dNinth.FileName);
                            File.Delete(path);
                            _dNinth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dNinth);
                            o.AdPhotos.Remove(_dNinth);
                        }
                        // delete ninth alt
                        Photo _dNinth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 19 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dNinth_ != null)
                        {
                            path = Path.Combine(dir, _dNinth_.FileName);
                            File.Delete(path);
                            _dNinth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dNinth_);
                            o.AdPhotos.Remove(_dNinth_);
                        }
                    }

                    // replace tenth photo
                    if (editItem.PhotoUpload10 != null && !editItem.PhotoUpload10bool)
                    {
                        // replace tenth photo regular
                        var tenth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 20 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (tenth != null)
                        {
                            path = Path.Combine(dir, tenth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 20, editItem.PhotoUpload10, tenth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 20, editItem.PhotoUpload10, new Photo() { ClassifiedAd = o });
                        }
                        var tenth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 20 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (tenth_ != null)
                        {
                            path = Path.Combine(dir, tenth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 20, editItem.PhotoUpload10, tenth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 20, editItem.PhotoUpload10, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload10bool)
                    {
                        // delete tenth
                        Photo _dTenth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 20 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dTenth != null)
                        {
                            path = Path.Combine(dir, _dTenth.FileName);
                            File.Delete(path);
                            _dTenth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dTenth);
                            o.AdPhotos.Remove(_dTenth);
                        }
                        // delete tenth alt
                        Photo _dTenth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 20 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dTenth_ != null)
                        {
                            path = Path.Combine(dir, _dTenth_.FileName);
                            File.Delete(path);
                            _dTenth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dTenth_);
                            o.AdPhotos.Remove(_dTenth_);
                        }
                    }

                    // replace eleventh photo
                    if (editItem.PhotoUpload11 != null && !editItem.PhotoUpload11bool)
                    {
                        // replace eleventh photo regular
                        var eleventh = o.AdPhotos.SingleOrDefault(a => a.CountNum == 21 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (eleventh != null)
                        {
                            path = Path.Combine(dir, eleventh.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 21, editItem.PhotoUpload11, eleventh);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 21, editItem.PhotoUpload11, new Photo() { ClassifiedAd = o });
                        }
                        var eleventh_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 21 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (eleventh_ != null)
                        {
                            path = Path.Combine(dir, eleventh_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 21, editItem.PhotoUpload11, eleventh_);
                        }
                        else
                        {
                            CompressPhoto(dir, 21, editItem.PhotoUpload11, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload11bool)
                    {
                        // delete eleventh
                        Photo _dEleventh = o.AdPhotos.SingleOrDefault(x => x.CountNum == 21 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dEleventh != null)
                        {
                            path = Path.Combine(dir, _dEleventh.FileName);
                            File.Delete(path);
                            _dEleventh.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dEleventh);
                            o.AdPhotos.Remove(_dEleventh);
                        }
                        // delete eleventh alt
                        Photo _dEleventh_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 21 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dEleventh_ != null)
                        {
                            path = Path.Combine(dir, _dEleventh_.FileName);
                            File.Delete(path);
                            _dEleventh_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dEleventh_);
                            o.AdPhotos.Remove(_dEleventh_);
                        }
                    }

                    // replace twelfth photo
                    if (editItem.PhotoUpload12 != null && !editItem.PhotoUpload12bool)
                    {
                        // replace eleventh photo regular
                        var twelfth = o.AdPhotos.SingleOrDefault(a => a.CountNum == 22 && a.AdListThumbnail == false && a.SetThumbnail == false);
                        if (twelfth != null)
                        {
                            path = Path.Combine(dir, twelfth.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            DefaultCompressionPngEdit(dir, 22, editItem.PhotoUpload12, twelfth);
                        }
                        else
                        {
                            DefaultCompressionPng(dir, 22, editItem.PhotoUpload12, new Photo() { ClassifiedAd = o });
                        }
                        var twelfth_ = o.AdPhotos.SingleOrDefault(a => a.CountNum == 22 && a.SetThumbnail == false && a.AdListThumbnail == true);
                        if (twelfth_ != null)
                        {
                            path = Path.Combine(dir, twelfth_.FileName);
                            // delete physical
                            File.Delete(path);
                            // update
                            CompressPhotoEdit(dir, 22, editItem.PhotoUpload12, twelfth_);
                        }
                        else
                        {
                            CompressPhoto(dir, 22, editItem.PhotoUpload12, new Photo() { AdListThumbnail = true, ClassifiedAd = o });
                        }
                    }
                    else if (editItem.PhotoUpload12bool)
                    {
                        // delete eleventh
                        Photo _dTwenfth = o.AdPhotos.SingleOrDefault(x => x.CountNum == 22 && x.AdListThumbnail == false && x.SetThumbnail == false);
                        if (_dTwenfth != null)
                        {
                            path = Path.Combine(dir, _dTwenfth.FileName);
                            File.Delete(path);
                            _dTwenfth.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dTwenfth);
                            o.AdPhotos.Remove(_dTwenfth);
                        }
                        // delete eleventh alt
                        Photo _dTwelfth_ = o.AdPhotos.SingleOrDefault(x => x.CountNum == 22 && x.AdListThumbnail == true && x.SetThumbnail == false);
                        if (_dTwelfth_ != null)
                        {
                            path = Path.Combine(dir, _dTwelfth_.FileName);
                            File.Delete(path);
                            _dTwelfth_.ClassifiedAd = null;
                            newthreadcontext.PhotosDB.Remove(_dTwelfth_);
                            o.AdPhotos.Remove(_dTwelfth_);
                        }
                    }

                    newthreadcontext.SaveChanges();
                    // Remove old Lucene
                    LuceneSearch.ClearLuceneIndexRecord(o.StringId);
                    // Add to Lucene
                    LuceneSearch.AddUpdateLuceneIndex(o);
                }

                var config = new MapperConfiguration(r => r.CreateMap<ClassifiedAd, ClassifiedAdBase>());
                IMapper mapper = config.CreateMapper();
                return (o == null) ? null : mapper.Map<ClassifiedAdBase>(o);
            }
        }

        internal void ClassifiedAdRemove(ClassifiedAd ad, HttpServerUtilityBase Server)
        {
            // remove user
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                CurrentUser.ClassifiedAds.Remove(ad);
                newthreadcontext.SaveChanges();
            }
            // remove country
            ad.Country = null;
            // remove region
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.RegionDB.First(x => x.Id == ad.Region.Id);
                if (o != null)
                {
                    o.ClassifiedAds.Remove(ad);
                    ad.Region = null;
                }
                newthreadcontext.SaveChanges();
            }
            // remove category
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.CategoryDB.First(x => x.Id == ad.Category.Id);
                if (ad.Status == 0)
                    o.TotalClassifiedAdsCount--;
                ad.Category = null;
                newthreadcontext.SaveChanges();
            }
            // remove subcategory
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.SubCategoryDB.Include("ClassifiedAds").First(x => x.stringId == ad.SubCategory.stringId);
                if (o != null)
                {
                    o.ClassifiedAds.Remove(ad);
                    if (ad.Status == 0)
                        o.ClassifiedAdsCount--;
                    ad.SubCategory = null;
                }
                newthreadcontext.SaveChanges();
            }
            // remove ad promotions
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                newthreadcontext.AdPromotionDB.RemoveRange(ad.AdPromotions);
                ad.AdPromotions = null;
                newthreadcontext.SaveChanges();
            }
            // remove photos
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var phos = newthreadcontext.PhotosDB.Include("ClassifiedAd").Where(x => x.ClassifiedAd.StringId == ad.StringId).ToList();
                foreach (var pho in phos)
                {
                    pho.ClassifiedAd = null;
                    newthreadcontext.PhotosDB.Remove(pho);
                }
                // delete photos
                var dir = Server.MapPath("~/Photos/" + ad.StringId.Substring(2, 4) + "/" + ad.StringId.Substring(0, 4));
                foreach (var pho in ad.AdPhotos)
                {
                    string path;
                    try
                    {
                        path = Path.Combine(dir, pho.FileName);
                        // delete physical
                        File.Delete(path);
                    }
                    catch (Exception) { }
                }
                ad.AdPhotos = null;
                newthreadcontext.SaveChanges();
            }
            // remove info
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.InfosDB.Include("ClassifiedAd").Where(x => x.ClassifiedAd.StringId == ad.StringId);
                if (o != null)
                {
                    foreach (var ai in o)
                    {
                        ai.ClassifiedAd = null;
                        newthreadcontext.InfosDB.Remove(ai);
                    }
                }
                ad.AdInfo = null;
                newthreadcontext.SaveChanges();
            }
            // remove reports
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.ReportDB.Include("ClassifiedAd").Where(x => x.ClassifiedAd.StringId == ad.StringId);
                if (o != null)
                {
                    foreach (var r in o)
                    {
                        r.ClassifiedAd = null;
                        newthreadcontext.ReportDB.Remove(r);
                    }
                }
                ad.Reports = null;
                newthreadcontext.SaveChanges();
            }
            // remove ad
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var o = newthreadcontext.ClassifiedDB.SingleOrDefault(x => x.StringId == ad.StringId);
                newthreadcontext.ClassifiedDB.Remove(o);
                newthreadcontext.SaveChanges();
            }
            // remove from lucene index
            LuceneSearch.ClearLuceneIndexRecord(ad.StringId);
        }

        // get user posted ads
        internal IPagedList<ClassifiedAdMyList> UserGetAdList(string name, string searchString = null, string searchCategory = "All Categories", string searchType = "All", int pageNumber = 1)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                IQueryable<ClassifiedAd> val;

                val = newthreadcontext.ClassifiedDB.Include("UserCreator").Include("Category").Include("AdPhotos").Where(a => a.UserCreator.UserName == name);

                if (!String.IsNullOrEmpty(searchString))
                    val = val.Where(x => (String.Compare(x.Title, searchString, StringComparison.OrdinalIgnoreCase) >= 0));

                if (!String.IsNullOrEmpty(searchCategory) && searchCategory != "All Categories")
                    val = val.Where(x => x.Category.Name.Equals(x.Category.Equals(searchCategory)));

                // get status
                val = searchType == "All" ? val : (searchType == "Open" ? val.Where(x => x.Status == 0) : (searchType == "Suspended" ? val.Where(x => x.Status == -1) : (searchType == "Closed" ? val.Where(x => x.Status == 1) : val)));

                // get order
                val = val.OrderByDescending(x => x.TimeStamp);

                var config = new MapperConfiguration(r => r.CreateMap<ClassifiedAd, ClassifiedAdMyList>()
                                            .ForMember(dest => dest.AdPhoto, opt => opt.MapFrom(x => (x.AdPhotos != null) ? x.AdPhotos.SingleOrDefault(y => y.AdListThumbnail == true && y.CountNum == 11).FileName : "")));

                IMapper mapper = config.CreateMapper();

                return val.Any() ? new PagedList<ClassifiedAdMyList>(mapper.Map<IEnumerable<ClassifiedAdMyList>>(val), pageNumber, RecordsPerPage.recordsPerPage) : new List<ClassifiedAdMyList>().ToPagedList(pageNumber, RecordsPerPage.recordsPerPage);
            }
        }

        // Get edit ad
        internal ClassifiedAdEditForm GetClassifiedAdWithAll(string stringId)
        {
            ClassifiedAd item;
            IEnumerable<Photo> pho = new List<Photo>();
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                item = newthreadcontext.ClassifiedDB
               .Include("AdPhotos")
               .Include("AdInfo")
               .Include("Country")
               .Include("Region")
               .Include("UserCreator")
               .Include("Category")
               .Include("SubCategory")
               .SingleOrDefault(a => a.StringId == stringId && a.UserCreator.Id == CurrentUser.Id && a.Status == 0
                   || a.StringId == stringId && a.UserCreator.Id == CurrentUser.Id && a.Status == -1);
            }

            if (item == null)
                return null;

            var config = new MapperConfiguration(r =>
            {
                r.CreateMap<ClassifiedAd, ClassifiedAdEditForm>()
                    .ForMember(dest => dest.PhotoUpload1, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 11).FileName))
                    .ForMember(dest => dest.PhotoUpload2, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 12).FileName))
                    .ForMember(dest => dest.PhotoUpload3, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 13).FileName))
                    .ForMember(dest => dest.PhotoUpload4, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 14).FileName))
                    .ForMember(dest => dest.PhotoUpload5, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 15).FileName))
                    .ForMember(dest => dest.PhotoUpload6, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 16).FileName))
                    .ForMember(dest => dest.PhotoUpload7, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 17).FileName))
                    .ForMember(dest => dest.PhotoUpload8, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 18).FileName))
                    .ForMember(dest => dest.PhotoUpload9, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 19).FileName))
                    .ForMember(dest => dest.PhotoUpload10, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 20).FileName))
                    .ForMember(dest => dest.PhotoUpload11, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 21).FileName))
                    .ForMember(dest => dest.PhotoUpload12, opt => opt.MapFrom(x => x.AdPhotos.SingleOrDefault(a => a.SetThumbnail == false && a.AdListThumbnail == false && a.CountNum == 22).FileName));
                r.CreateMap<Country, CountryBase>();
                r.CreateMap<Models.Region, RegionBase>();
                r.CreateMap<ApplicationUser, UserProfileContact>();
                r.CreateMap<Info, InfoForm>();
            });

            IMapper mapper = config.CreateMapper();

            var newit = mapper.Map<ClassifiedAdEditForm>(item);

            return (item == null) ? null : newit;
        }

        // Get one ad with details
        internal ClassifiedAdWithDetail GetClassifiedAdWithDetails(string stringId, bool increment = false)
        {
            // Check for session id
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var item = newthreadcontext.ClassifiedDB.Include("AdPhotos").Include("AdInfo")
                    .Include("Country").Include("Region").Include("Category").Include("SubCategory")
                    .Include("UserCreator.PremiumUserData").Include("UserCreator.PremiumUserData.PremiumUserPhotos")
                    .Include("UserCreator.PremiumUserData.PremiumUserInfos")
                    .SingleOrDefault(a => a.StringId == stringId);

                if (item != null)
                {
                    MapperConfiguration config = null;
                    if (item.UserCreator.PremiumUserData != null)
                    {
                        config = new MapperConfiguration(r =>
                        {
                            r.CreateMap<ClassifiedAd, ClassifiedAdWithDetail>()
                                .ForMember(dest => dest.AdPhotos, opt => opt.MapFrom(x => x.AdPhotos.Where(a => a.SetThumbnail == false)))
                                .ForMember(dest => dest.UserCreatorPremiumUserDataPremiumUserPhoto, opt => opt.MapFrom(x => x.UserCreator.PremiumUserData.PremiumUserPhotos.SingleOrDefault(y => y.Position == 1)))
                                .ForMember(dest => dest.AdInfo, opt => opt.MapFrom(x => x.AdInfo.Where(a => a.Description != null)))
                                .AfterMap((src, dest) => dest.Title = (src.AdType == "WANT" ? "Looking For: " : src.AdType == "TRADE" ? "Trading: " : null) + src.Title);
                            r.CreateMap<Photo, PhotoBase>();
                            r.CreateMap<Country, CountryBase>();
                            r.CreateMap<Models.Region, RegionBase>();
                            r.CreateMap<Info, InfoForm>();
                            r.CreateMap<PremiumUserPhoto, PremiumUserPhotoBase>();
                            r.CreateMap<PremiumUserInfo, PremiumUserInfoBase>();
                        });
                    }
                    else
                    {
                        config = new MapperConfiguration(r =>
                        {
                            r.CreateMap<ClassifiedAd, ClassifiedAdWithDetail>()
                                .ForMember(dest => dest.UserCreatorisSeller, opt => opt.Ignore())
                                .ForMember(dest => dest.UserCreatorPremiumUserDataUrlName, opt => opt.Ignore())
                                .ForMember(dest => dest.UserCreatorPremiumUserDataPremiumUserPhoto, opt => opt.Ignore())
                                .ForMember(dest => dest.UserCreatorPremiumUserDataAverageRating, opt => opt.Ignore())
                                .ForMember(dest => dest.UserCreatorPremiumUserDataPremiumUserName, opt => opt.Ignore())
                                .ForMember(dest => dest.UserCreatorPremiumUserDataClassifiedAdsViewCount, opt => opt.Ignore())
                                .ForMember(dest => dest.UserCreatorPremiumUserDataUserProfileStringId, opt => opt.Ignore())
                                .ForMember(dest => dest.UserCreatorPremiumUserDataUserReviewsCount, opt => opt.Ignore())
                                .ForMember(dest => dest.AdPhotos, opt => opt.MapFrom(x => x.AdPhotos.Where(a => a.SetThumbnail == false)))
                                .ForMember(dest => dest.AdInfo, opt => opt.MapFrom(x => x.AdInfo.Where(a => a.Description != null)))
                                .AfterMap((src, dest) => dest.Title = (src.AdType == "WANT" ? "Looking For: " : src.AdType == "TRADE" ? "Trading: " : null) + src.Title);
                            r.CreateMap<Photo, PhotoBase>();
                            r.CreateMap<Country, CountryBase>();
                            r.CreateMap<Models.Region, RegionBase>();
                            r.CreateMap<Info, InfoForm>();
                        });
                    }

                    IMapper mapper = config.CreateMapper();
                    var toret = mapper.Map<ClassifiedAdWithDetail>(item);
                    // Check if current user is ad owner
                    if (CurrentUser != null)
                    {
                        if (CurrentUser.Id == item.UserCreator.Id)
                        {
                            toret.IsOwner = true;
                        }
                    }
                    return toret;
                }
                return null;
            }
        }

        internal IEnumerable<ClassifiedAdList> ClassifiedAdGetRecentIndex()
        {/*
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                IQueryable<ClassifiedAd> items = newthreadcontext.ClassifiedDB
                    .Include("AdPhotos")
                    .Include("AdInfo")
                    .Include("Category")
                    .Include("SubCategory")
                    .Where(x => x.Status == 0)
                    .OrderByDescending(z => z.TimeStamp)
                    .Take(5);

                // Take
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                var config = new MapperConfiguration(r =>
                    {
                        r.CreateMap<ClassifiedAd, ClassifiedAdList>()
                            .ForMember(dest => dest.AdPhoto, opt => opt.MapFrom(src => src.AdPhotos.SingleOrDefault(x => x.SetThumbnail == true)))
                            .AfterMap((src, dest) => dest.AdInfo = dest.AdInfo.GroupBy(x => x.Name).Select(g => g.First()).ToList())
                            .AfterMap((src, dest) => dest.AdInfo = dest.AdInfo.Take(2).ToList())
                            .AfterMap((src, dest) => dest.Title = (src.AdType == "WANT" ? "Looking For: " : src.AdType == "TRADE" ? "Trading: " : null) + src.Title)
                            .AfterMap((src, dest) => dest.HtmlFreeDescription = HttpUtility.HtmlDecode(dest.HtmlFreeDescription))
                            .AfterMap((src, dest) => dest.HtmlFreeDescription = dest.HtmlFreeDescription != null ? dest.HtmlFreeDescription.Length > 100 ? dest.HtmlFreeDescription.Substring(0, 100) + "..." : dest.HtmlFreeDescription.TrimEnd(',', ' ') : dest.HtmlFreeDescription);
                        r.CreateMap<Country, CountryBase>();
                        r.CreateMap<Info, InfoForm>();
                        r.CreateMap<Photo, PhotoBase>();

                    });
                IMapper mapper = config.CreateMapper();
                return (items.Any() ? mapper.Map<IEnumerable<ClassifiedAdList>>(items.ToList()) : null);
            }*/
            // LUCENE
            // validation            
            // Setup query
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("EditTimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

            // Get only Open ads
            bq.Add(new TermQuery(new Term("Status", "0")), Occur.MUST);
            // Must have Pic
            //bq.Add(new TermQuery(new Term("AdPhoto")), Occur.MUST);

            // set up lucene searcher
            using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
            {
                var hits_limit = 5;
                var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                var results = LuceneSearch._mapLuceneToDataList(hits, searcher);
                analyzer.Close();
                searcher.Dispose();

                return results;
            }
        }

        internal IEnumerable<ClassifiedAdList> ClassifiedAdGetPopularIndex()
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                IQueryable<ClassifiedAd> items = newthreadcontext.ClassifiedDB
                    .Include("AdPhotos")
                    .Include("AdInfo")
                    .Include("Category")
                    .Include("SubCategory")
                    .Where(x => x.Status == 0)
                    .OrderByDescending(z => z.Views)
                    .Take(5);

                var config = new MapperConfiguration(r =>
                {
                    r.CreateMap<ClassifiedAd, ClassifiedAdList>()
                        .ForMember(dest => dest.AdPhoto, opt => opt.MapFrom(src => src.AdPhotos.SingleOrDefault(x => x.SetThumbnail == true)))
                        .AfterMap((src, dest) => dest.AdInfo = dest.AdInfo.GroupBy(x => x.Name).Select(g => g.First()).ToList())
                        .AfterMap((src, dest) => dest.AdInfo = dest.AdInfo.Take(2).ToList())
                        .AfterMap((src, dest) => dest.Title = (src.AdType == "WANT" ? "Looking For: " : src.AdType == "TRADE" ? "Trading: " : null) + src.Title)
                        .AfterMap((src, dest) => dest.HtmlFreeDescription = HttpUtility.HtmlDecode(dest.HtmlFreeDescription))
                        .AfterMap((src, dest) => dest.HtmlFreeDescription = dest.HtmlFreeDescription != null ? dest.HtmlFreeDescription.Length > 100 ? dest.HtmlFreeDescription.Substring(0, 100) + "..." : dest.HtmlFreeDescription.TrimEnd(',', ' ') : dest.HtmlFreeDescription);
                    r.CreateMap<Country, CountryBase>();
                    r.CreateMap<Info, InfoForm>();
                    r.CreateMap<Photo, PhotoBase>();

                });
                IMapper mapper = config.CreateMapper();
                return (items.Any() ? mapper.Map<IEnumerable<ClassifiedAdList>>(items.ToList()) : new List<ClassifiedAdList>());
            }
            /*
            // LUCENE
            // validation            
            // Setup query
            BooleanQuery bq = new BooleanQuery();
            var sortBy = new Sort();
            sortBy.SetSort(new SortField("EditTimeStampTicks", SortField.LONG, true), SortField.FIELD_SCORE);
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

            // Get only Open ads
            bq.Add(new TermQuery(new Term("Status", "0")), Occur.MUST);
            // Must have Pic
            bq.Add(new TermQuery(new Term("AdPhoto")), Occur.MUST);

            // set up lucene searcher
            using (var searcher = new IndexSearcher(LuceneSearch._getDir, false))
            {
                var hits_limit = 5;
                var hits = searcher.Search(bq, null, hits_limit, sortBy).ScoreDocs;
                var results = LuceneSearch._mapLuceneToDataList(hits, searcher);
                analyzer.Close();
                searcher.Dispose();

                return results;
            }*/
        }

        internal IEnumerable<ClassifiedAdList> ClassifiedAdGetRelated(string stringid, string price, string subcatid)
        {
            using (ApplicationDbContext newthreadcontext = new ApplicationDbContext())
            {
                var newprice = Int32.Parse(price);
                IQueryable<ClassifiedAd> items;

                if (newprice > 0)
                {
                    items = newthreadcontext.ClassifiedDB
                        .Include("AdPhotos")
                        .Include("AdInfo")
                        .Include("Category")
                        .Include("SubCategory")
                        .Where(x => x.Status == 0 && x.SubCategory.stringId == subcatid && x.Price <= newprice && x.StringId != stringid);
                }
                else
                {
                    items = newthreadcontext.ClassifiedDB
                        .Include("AdPhotos")
                        .Include("AdInfo")
                        .Include("Category")
                        .Include("SubCategory")
                        .Where(x => x.Status == 0 && x.SubCategory.stringId == subcatid && x.StringId != stringid);
                }

                // Sort
                items = items.OrderByDescending(z => z.Views);

                // Take
                items = items.Take(5);

                var config = new MapperConfiguration(r =>
                {
                    r.CreateMap<ClassifiedAd, ClassifiedAdList>()
                        .ForMember(dest => dest.AdPhoto, opt => opt.MapFrom(src => src.AdPhotos.SingleOrDefault(x => x.SetThumbnail == true)))
                        .AfterMap((src, dest) => dest.AdInfo = dest.AdInfo.GroupBy(x => x.Name).Select(g => g.First()).ToList())
                        .AfterMap((src, dest) => dest.AdInfo = dest.AdInfo.Take(2).ToList())
                        .AfterMap((src, dest) => dest.Title = (src.AdType == "WANT" ? "Looking For: " : src.AdType == "TRADE" ? "Trading: " : null) + src.Title)
                        .AfterMap((src, dest) => dest.HtmlFreeDescription = HttpUtility.HtmlDecode(dest.HtmlFreeDescription))
                        .AfterMap((src, dest) => dest.HtmlFreeDescription = dest.HtmlFreeDescription != null ? dest.HtmlFreeDescription.Length > 100 ? dest.HtmlFreeDescription.Substring(0, 100) + "..." : dest.HtmlFreeDescription.TrimEnd(',', ' ') : dest.HtmlFreeDescription);
                    r.CreateMap<Country, CountryBase>();
                    r.CreateMap<Info, InfoForm>();
                    r.CreateMap<Photo, PhotoBase>();

                });
                IMapper mapper = config.CreateMapper();
                return (items.Any() ? mapper.Map<IEnumerable<ClassifiedAdList>>(items.ToList()) : null);
            }
        }
    }
}