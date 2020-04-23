using System.Collections.Generic;
using System.Web;

namespace Trinbago_MVC5.Areas.ClassifiedAd.Models
{
    public class UploadPhoto
    {
        public string Src { get; set; }
        public int PhotoLayoutIndex { get; set; }
        public string ContentType { get; set; }
        public string Raw_FileName { get; set; }
        public string AdDetails_FileName { get; set; }
        public string AdList_FileName { get; set; }
        public string Original_FileName { get; set; }
    }

    public class FormDataUpload
    {
        public string StringId { get; set; }
        public ICollection<HttpPostedFileBase> Photos { get; set; }
        public int MaxPhotoCount { get; set; }
        public int CurrentPhotoCount { get; set; }
    }

    public class FormDataDelete
    {
        public int CurrentPhotoCount { get; set; }
        public string StringId { get; set; }
        public string Raw_FileName { get; set; }
        public string AdDetails_FileName { get; set; }
        public string AdList_FileName { get; set; }
    }
}