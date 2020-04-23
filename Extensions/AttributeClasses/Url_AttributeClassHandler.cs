using StackExchange.Profiling;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Trinbago_MVC5.Managers;
/// <summary>
/// Responsible for modifying browser URLs
/// </summary>
namespace Trinbago_MVC5.Extensions.AttributeClasses
{
    /// <summary>
    /// Corrects incoming url requests for Ad List and Ad Details
    /// </summary>
    sealed public class UrlCorrectAttribute : ActionFilterAttribute, IActionFilter
    {
        private SeoManager _seoManager;
        private BaseApplicationManager _baseManager;

        public SeoManager SeoManager
        {
            get
            {
                return _seoManager ?? new SeoManager();
            }
            set
            {
                _seoManager = value;
            }
        }

        public BaseApplicationManager BaseManager
        {
            get
            {
                return _baseManager ?? new BaseApplicationManager();
            }
            set
            {
                _baseManager = value;
            }
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
#if DEBUG
            var profiler = MiniProfiler.Current;
            using (profiler.Step("url correct"))
            {
#endif
                // base
                OnActionExecuting(filterContext);

                string searchString = null;
                // Append searchString to url
                if (filterContext.ActionParameters.ContainsKey("searchString"))
                {
                    searchString = (string)filterContext.ActionParameters["searchString"] ?? null;
                    if (searchString != null)
                    {
                        filterContext.ActionParameters["searchString"] = (string)GetSearchString(searchString);
                    }
                }

                // redirect check .. Read and delete temp value
                if (filterContext.Controller.TempData["wasRedirected"] != null) { filterContext.Controller.TempData["wasRedirected"] = null; return; }
                // Get parameters
                var parameters = filterContext.ActionParameters;
                string url = null;
                string requestUrl = null;
                string searchQuery = null;
                searchQuery = filterContext.RequestContext.HttpContext.Request.QueryString.ToString();
                requestUrl = filterContext.RequestContext.HttpContext.Request.RawUrl;

                // Check controller
                // If AdLIst
                if (filterContext.ActionDescriptor.ActionName.Equals("AdList"))
                {
                    // Allow query strings for catId subCatId CountryId RegionId
                    var newSearchQuery = HttpUtility.ParseQueryString(searchQuery);
                    int catId = 0;
                    int subCatId = 0;
                    int CountryId = 0;
                    int RegionId = 0;
                    string modelBodyType = null;
                    string modelRentalType = null;

                    int.TryParse(newSearchQuery.Get("catId"), out catId);
                    newSearchQuery.Remove("catId");
                    int.TryParse(newSearchQuery.Get("subCatId"), out subCatId);
                    newSearchQuery.Remove("subCatId");
                    int.TryParse(newSearchQuery.Get("CountryId"), out CountryId);
                    newSearchQuery.Remove("CountryId");
                    int.TryParse(newSearchQuery.Get("RegionId"), out RegionId);
                    newSearchQuery.Remove("RegionId");
                    modelBodyType = newSearchQuery.Get("modelBodyType");
                    newSearchQuery.Remove("modelBodyType");
                    modelBodyType = newSearchQuery.Get("modelRentalType");
                    newSearchQuery.Remove("modelRentalType");

                    searchQuery = newSearchQuery.ToString();

                    if (catId > 0 || subCatId > 0 || CountryId > 0 || RegionId > 0)
                    {
                        if (catId == 0) catId = (int)parameters["catId"];
                        if (subCatId == 0) subCatId = (int)parameters["subCatId"];
                        if (CountryId == 0) CountryId = (int)parameters["CountryId"];
                        if (RegionId == 0) RegionId = (int)parameters["RegionId"];
                        url = SeoManager.GenerateAdListUrl(
                            ref catId,
                            ref subCatId,
                            ref CountryId,
                            ref RegionId);
                        if (catId > 0) parameters["catId"] = catId;
                        if (subCatId > 0) parameters["subCatId"] = subCatId;
                        if (CountryId > 0) parameters["CountryId"] = CountryId;
                        if (RegionId > 0) parameters["RegionId"] = RegionId;
                    }
                    else
                        url = SeoManager.GenerateAdListUrl(
                            (int)parameters["catId"],
                            (int)parameters["subCatId"],
                            (int)parameters["CountryId"],
                            (int)parameters["RegionId"]);

                    if (url == null)
                    {
                        filterContext.Result = new HttpStatusCodeResult(404);
                        return;
                    }
                    else
                    {
                        // Append searchString to url
                        if (filterContext.Controller.TempData["AdSearchIdNotFound"] == null)
                        {
                            url = string.Format("{0}{1}", url, searchString != null ? "/" + new SeoManager().GetSeoTitle(searchString) : null);
                        }
                    }

                    // check if modelbodytype value is a parameter
                    if (modelBodyType == null)                    
                        modelBodyType = (string)parameters["modelBodyType"];
                    // check if modelrentaltype value is a parameter
                    if (modelRentalType == null)
                        modelRentalType = (string)parameters["modelRentalType"];

                    // insert modelbodytype into url
                    if (modelBodyType != null && url.Contains("vehicles-cars-trucks"))
                        url = url.Replace("vehicles-cars-trucks/", "vehicles-cars-trucks/" + modelBodyType + "/");
                    // insert modelrentaltype into url
                    if (modelRentalType != null && url.Contains("real-estate-apartments-condos-rental"))
                        url = url.Replace("real-estate-apartments-condos-rental/", "real-estate-apartments-condos-rental/" + modelRentalType + "/");
                    if(modelRentalType != null && url.Contains("real-estate-land-rental-leasing"))
                        url = url.Replace("real-estate-land-rental-leasing/", "real-estate-land-rental-leasing/" + modelRentalType + "/");
                    if (modelRentalType != null && url.Contains("real-estate-house-rental"))
                        url = url.Replace("real-estate-house-rental/", "real-estate-house-rental/" + modelRentalType + "/");
                }

                // If ListDetails
                else if (filterContext.ActionDescriptor.ActionName.Equals("AdDetails"))
                {
                    url = SeoManager.GenerateAdDetailUrl((int)parameters["Id"]);
                    if (url == null)
                    {
                        // check if ad is suspended
                        if (SeoManager.IsAdSuspended((int)parameters["Id"]))
                        {
                            filterContext.Result = new RedirectResult(new UrlHelper(filterContext.RequestContext).Action("AdSuspended", "Errors", new { Area = "" }));
                            return;
                        }
                        filterContext.Result = new HttpStatusCodeResult(410);
                        return;
                    }
                }

                // Compare ulrs
                if (url != null)
                {
                    var responseUrl = string.Format("{0}{1}", url, searchQuery != "" ? "?" + searchQuery : null);
                    if (requestUrl != responseUrl)
                    {
                        filterContext.Result = new RedirectResult(responseUrl, true);
                        filterContext.Controller.TempData["wasRedirected"] = true;
                        return;
                    }
                }
#if DEBUG
            }
#endif
        }
        private object GetSearchString(object id)
        {
            if (id != null)
            {
                string idValue = id.ToString();
                return Regex.Replace(Regex.Replace(idValue, @"-", " "), @"\s+", " ");
            }
            return null;
        }
    }

    sealed public class OldListDetailRoute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Base
            OnActionExecuting(filterContext);

            // Get parameters
            var parameters = filterContext.ActionParameters;
            if (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.Equals("ClassifiedAd") && filterContext.ActionDescriptor.ActionName.Equals("ListDetails"))
            {
                var stringid = parameters["stringId"];
                if (stringid != null)
                {
                    parameters["stringId"] = (string)GetIdValue(stringid);
                }
            }

        }
        private object GetIdValue(object id)
        {
            if (id != null)
            {
                string idValue = id.ToString();

                var regex = new Regex(@"^(?<stringId>([a-z0-9])+).*$");
                var match = regex.Match(idValue);

                if (match.Success)
                {
                    return match.Groups["stringId"].Value;
                }
            }

            return id;
        }
    }
}