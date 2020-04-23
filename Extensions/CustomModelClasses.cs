using StackExchange.Profiling;
using System;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace Trinbago_MVC5.Extensions
{
    public static class ExpiryDate
    {
        public static DateTime CalculateDate(DateTime startTime, string adType, string categoryName, string subcategoryName)
        {
            if (adType == "SELL" && (categoryName == "Business Services" || subcategoryName == "Vehicle Services" || subcategoryName == "Pet & Animal Services" || subcategoryName == "Real Estate Services"))
            {
                return startTime.AddMonths(24);
            }
            else
            {
                return startTime.AddMonths(12);
            }
        }
    }

    /// <summary>
    /// Cache data to server memory
    /// https://stackoverflow.com/questions/18181273/how-can-i-create-a-class-that-caches-objects
    /// </summary>
    public static class CacheHelper
    {
        public static void SaveTocache(string cacheKey, object savedItem, DateTime duration)
        {
            if (IsIncache(cacheKey))
            {
                HttpContext.Current.Cache.Remove(cacheKey);
            }

            HttpContext.Current.Cache.Add(cacheKey, savedItem, null, Cache.NoAbsoluteExpiration, duration.Subtract(DateTime.Now), CacheItemPriority.Default, null);
        }

        /// <summary>
        /// Absolute Expiration Date
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="savedItem"></param>
        /// <param name="absoluteExpiration"></param>
        /// <param name="timeSpan"></param>
        public static void SaveTocache(string cacheKey, object savedItem, DateTime absoluteExpiration, TimeSpan timeSpan)
        {
            if (IsIncache(cacheKey))
            {
                HttpContext.Current.Cache.Remove(cacheKey);
            }

            HttpContext.Current.Cache.Add(cacheKey, savedItem, null, absoluteExpiration, timeSpan, CacheItemPriority.Default, null);
        }

        public static T GetFromCache<T>(string cacheKey) where T : class
        {
            return HttpContext.Current.Cache[cacheKey] as T;
        }

        public static void RemoveFromCache(string cacheKey)
        {
            HttpContext.Current.Cache.Remove(cacheKey);
        }

        public static bool IsIncache(string cacheKey)
        {
            return HttpContext.Current.Cache[cacheKey] != null;
        }
    }

    public class CustomRazorViewEngine : RazorViewEngine
    {
        public CustomRazorViewEngine()
        {
            FileExtensions = new string[] { "cshtml" };
            AreaViewLocationFormats = new[]
             {
             "~/Areas/{2}/Views/{1}/{0}.cshtml",
             "~/Areas/{2}/Views/Shared/{0}.cshtml"
             };
            AreaMasterLocationFormats = new[]
             {
             "~/Areas/{2}/Views/{1}/{0}.cshtml"
             };
            AreaPartialViewLocationFormats = new[]
             {
             "~/Areas/{2}/Views/Shared/{0}.cshtml"
             };
            ViewLocationFormats = new[]
             {
             "~/Views/{1}/{0}.cshtml",
             "~/Views/Shared/{0}.cshtml"
             };
            MasterLocationFormats = new[]
             {
             "~/Views/{1}/{0}.cshtml"
             };
            PartialViewLocationFormats = new[]
             {
             "~/Views/Shared/{0}.cshtml"
             };
        }
    }

    public class ProfiledRazorViewEngine : CustomRazorViewEngine
    {

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            using (MiniProfiler.Current.Step("CreatePartialView " + partialPath))
            {
                IView view = base.CreatePartialView(controllerContext, partialPath);
                return new ProfiledView(view, partialPath);
            }
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            using (MiniProfiler.Current.Step("CreateView " + viewPath))
            {
                IView view = base.CreateView(controllerContext, viewPath, masterPath);
                return new ProfiledView(view, viewPath);
            }
        }

        private class ProfiledView : IView
        {

            private readonly IView view;
            private readonly string debugName;

            public ProfiledView(IView view, string debugName)
            {
                this.view = view;
                this.debugName = debugName;
            }

            public void Render(ViewContext viewContext, System.IO.TextWriter writer)
            {
                using (MiniProfiler.Current.Step("Rendering " + debugName))
                {
                    view.Render(viewContext, writer);
                }
            }

        }

    }
}
