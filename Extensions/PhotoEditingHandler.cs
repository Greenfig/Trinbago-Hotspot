using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using Trinbago_MVC5.Models;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using System.Collections.Generic;
using System.Web.Hosting;

namespace Trinbago_MVC5.Extensions
{
    /// <summary>
    /// Image Compression
    /// </summary>
    public static class PhotoEditing
    {
        private static class PhotoScale
        {
            public static double SetThumbnialWidth => 310;
            public static double SetThumbnailHeight => 310;
            public static double AdDetailsThumbnailWidth => 700;
            public static double AdDetailsThumbnailHeight => 700;
        }
        public static void CompressUploadPhoto(string dir, bool AdListTumb, bool AdDetailsThumb, HttpPostedFileBase photoUpload, ref UploadPhoto photo)
        {
            string filerename = MySecurity.GetGen();
            string path = Path.Combine(dir, filerename);

            using (Image source = Image.FromStream(photoUpload.InputStream))
            {
                double widthRatio = ((AdListTumb == true ? PhotoScale.SetThumbnialWidth : (AdDetailsThumb == true ? PhotoScale.AdDetailsThumbnailWidth : PhotoScale.AdDetailsThumbnailWidth))) / source.Width;
                double heightRatio = ((AdListTumb == true ? PhotoScale.SetThumbnailHeight : (AdDetailsThumb == true ? PhotoScale.AdDetailsThumbnailHeight : PhotoScale.AdDetailsThumbnailHeight))) / source.Height;
                double ratio = (widthRatio < heightRatio) ? widthRatio : heightRatio;
                using (Image thumbnail = source.GetThumbnailImage((int)(source.Width * ratio), (int)(source.Height * ratio), AbortCallback, IntPtr.Zero))
                {
                    using (var memory = new MemoryStream())
                    {
                        ImageCodecInfo jpgEncoder = ImageCodecInfo.GetImageEncoders().SingleOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                        if (jpgEncoder != null)
                        {
                            Encoder encoder = Encoder.Quality;
                            EncoderParameters encoderParameters = new EncoderParameters(1);

                            long quality = 55;

                            EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                            encoderParameters.Param[0] = encoderParameter;

                            using (FileStream ms = new FileStream(path, FileMode.Create, FileAccess.Write))
                            {
                                // Save orientation
                                if (source.PropertyIdList.Contains(0x0112))
                                {
                                    int rotationValue = source.GetPropertyItem(0x0112).Value[0];
                                    switch (rotationValue)
                                    {
                                        case 1:
                                            // Landscape do nothing
                                            break;
                                        case 8:
                                            // Rotate 90 right
                                            thumbnail.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                            break;
                                        case 3:
                                            // Bottom up
                                            thumbnail.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                            break;
                                        case 6:
                                            // Rotated 90 left
                                            thumbnail.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                            break;
                                    }
                                }
                                thumbnail.Save(ms, jpgEncoder, encoderParameters);
                            }
                        }
                    }
                }
            }
            if (AdListTumb)
                photo.AdList_FileName = filerename;
            else
                photo.AdDetails_FileName = filerename;
        }

        public static void DefaultCompressionJpegUpload(string dir, HttpPostedFileBase photoUpload, ref UploadPhoto photo)
        {
            string filerename = MySecurity.GetGen();
            string path = Path.Combine(dir, filerename);

            using (Image source = Image.FromStream(photoUpload.InputStream, true, true))
            {
                ImageCodecInfo jpgEncoder = ImageCodecInfo.GetImageEncoders().SingleOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                if (jpgEncoder != null)
                {
                    Encoder encoder = Encoder.Quality;
                    EncoderParameters encoderParameters = new EncoderParameters(1);

                    long quality = 75;

                    EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                    encoderParameters.Param[0] = encoderParameter;

                    using (FileStream ms = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        // Save orientation
                        if (source.PropertyIdList.Contains(0x0112))
                        {
                            int rotationValue = source.GetPropertyItem(0x0112).Value[0];
                            switch (rotationValue)
                            {
                                case 1:
                                    // Landscape do nothing
                                    break;
                                case 8:
                                    // Rotate 90 right
                                    source.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                    break;
                                case 3:
                                    // Bottom up
                                    source.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                    break;
                                case 6:
                                    // Rotated 90 left
                                    source.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                    break;
                            }
                        }
                        source.Save(ms, jpgEncoder, encoderParameters);
                    }
                }
            }
            photo.Raw_FileName = filerename;
            photo.ContentType = photoUpload.ContentType;
        }

        public static byte[] FileToByteArray(string path)
        {
            return File.ReadAllBytes(path);
        }

        // Literally does nothing -- needed for task
        public static bool AbortCallback()
        {
            throw new NotImplementedException();
        }
    }

    public static class PhotoFileManager
    {
        public static void CreateTempPhotos(int adId, string stringId, UploadPhoto[] photos)
        {
            // copy files to temp folder to be deleted on successful edit submit
            var dir = HostingEnvironment.MapPath("~/Photos/" + stringId.Substring(2, 4) + "/" + stringId.Substring(0, 4));
            var temppath = Path.Combine(dir, "temp-" + adId);
            if (!Directory.Exists(temppath))
                Directory.CreateDirectory(temppath);
            foreach (var pho in photos.Where(x => x != null))
            {
                var orf = Path.Combine(dir, pho.Raw_FileName);
                var nrf = Path.Combine(temppath, pho.Raw_FileName);
                var oadf = Path.Combine(dir, pho.AdDetails_FileName);
                var nadf = Path.Combine(temppath, pho.AdDetails_FileName);
                if (pho.AdList_FileName != null)
                {
                    var oalf = Path.Combine(dir, pho.AdList_FileName);
                    var nalf = Path.Combine(temppath, pho.AdList_FileName);
                    if (File.Exists(nalf) && !File.Exists(oalf))
                        File.Copy(nalf, oalf);
                    if (!File.Exists(nalf))
                        File.Copy(oalf, nalf);
                }
                // Copy from temp
                if (File.Exists(nrf) && !File.Exists(orf))
                    File.Copy(nrf, orf);
                if (File.Exists(nadf) && !File.Exists(oadf))
                    File.Copy(nadf, oadf);

                // Copy to temp
                if (!File.Exists(nrf))
                    File.Copy(orf, nrf);
                if (!File.Exists(nadf))
                    File.Copy(oadf, nadf);
            }
        }

        public static void DeleteTempPhotos(int adId, string stringId)
        {
            // Delete temp folder
            var dir = HostingEnvironment.MapPath("~/Photos/" + stringId.Substring(2, 4) + "/" + stringId.Substring(0, 4));
            var temppath = Path.Combine(dir, "temp-" + adId);
            if (Directory.Exists(temppath))
                Directory.Delete(temppath, true);
        }
    }

    // Custom comparer for the photo class
    public class PhotoComparer : IEqualityComparer<ClassifiedAdPhoto>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(ClassifiedAdPhoto x, ClassifiedAdPhoto y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Original_FileName == y.Original_FileName &&
                    x.Raw_FileName == y.Raw_FileName &&
                    x.AdDetails_FileName == y.AdDetails_FileName &&
                    x.AdList_FileName == y.AdList_FileName;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(ClassifiedAdPhoto photo)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(photo, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashOriginalName = photo.Original_FileName == null ? 0 : photo.Original_FileName.GetHashCode();
            int hashRawName = photo.Raw_FileName == null ? 0 : photo.Raw_FileName.GetHashCode();
            int hashAdDetailsName = photo.AdDetails_FileName == null ? 0 : photo.AdDetails_FileName.GetHashCode();
            int hashAdListName = photo.AdList_FileName == null ? 0 : photo.AdList_FileName.GetHashCode();

            //Calculate the hash code for the product.
            return hashOriginalName ^ hashRawName ^ hashAdDetailsName ^ hashAdListName;
        }

    }
}