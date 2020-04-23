using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Trinbago_MVC5.Areas.Admin.Models;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Extensions.AttributeClasses
{
    sealed public class FileBaseValidateAttribute : ValidationAttribute
    {
        private string _methodName { get; set; }

        public FileBaseValidateAttribute(string methodname)
        {
            _methodName = methodname;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as HttpPostedFileBase;
            if (file != null)
            {
                ErrorMessage = string.Format("File: {0} is not the required format!\n", validationContext.DisplayName);
                if (!AllowedFileTypes.AllowedFileTypesValidation(file, _methodName))
                    return new ValidationResult(ErrorMessage, new List<string>() { validationContext.DisplayName });
            }
            return null;
        }
    }

    sealed public class ValidateFilter : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            // base
            OnActionExecuting(filterContext);

            // Get parameters
            var parameters = filterContext.ActionParameters;

            // Check controller
            // If AdLIst
            if (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.Equals("ClassifiedAd") && filterContext.ActionDescriptor.ActionName.Equals("AdList"))
            {
                var minPrice = (string)parameters["minPrice"];
                var maxPrice = (string)parameters["maxPrice"];
                var minMile = (string)parameters["minMile"];
                var maxMile = (string)parameters["maxMile"];
                var minYear = (string)parameters["minYear"];
                var maxYear = (string)parameters["maxYear"];
                var modEngineSize = (string)parameters["modEngineSize"];
                var minSize = (string)parameters["minSize"];
                var maxSize = (string)parameters["maxSize"];
                var minAge = (string)parameters["minAge"];
                var maxAge = (string)parameters["maxAge"];

                if (minPrice == "" && maxPrice == "" && minMile == "" && maxMile == "" &&
                    minYear == "" && maxYear == "" && modEngineSize == null && minSize == "" &&
                    maxSize == "" && minAge == "" && maxAge == "") return;
                //----------REGEX-------------
                // Price
                Regex regex_minmaxPrice = new Regex(@"^([1-9]{1}[0-9]{0,2}(\,[0-9]{3})*|([1-9]{1}[0-9]*))$");
                // Cars
                Regex regex_minmaxMile = new Regex(@"^([1-9]{1}[0-9]{0,2}(\,[0-9]{3})*|([1-9]{1}[0-9]*))$");
                Regex regex_minmaxYear = new Regex(@"^([1-2]{1}[0-9]{3})$");
                Regex regex_modEngineSize = new Regex(@"^([0-9]{1}\.[0-9]{1}\s?[L])$|^([1-9]{1}[0-9]{1}\s?[L])$|^([1-9]{1}[0-9]{1,4}\s?[c][c])$|^(([V]|[v])([6]|[8]|[1][2]))$");
                Regex regex_minmaxSize = new Regex(@"^([1-9]{1}[0-9]{0,2}(\,[0-9]{3})*|([1-9]{1}[0-9]*))$");
                // Pets
                Regex regex_minmaxAge = new Regex(@"^[1-9]?[0-9]?[0-9]$");
                // Real Estate
                // Jobs

                // Default
                // Price
                if (!string.IsNullOrEmpty(minPrice))
                {
                    if (!regex_minmaxSize.Match(minPrice).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("minPrice", "Invalid format. Example (12345)");
                    }
                }
                if (!string.IsNullOrEmpty(maxPrice))
                {
                    if (!regex_minmaxSize.Match(maxPrice).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("maxPrice", "Invalid format. Example (12345)");
                    }
                }

                // Cars
                // Mile
                if (!string.IsNullOrEmpty(minMile))
                {
                    if (!regex_minmaxMile.Match(minMile).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("minMile", "Invalid format. Example (12345)");
                    }
                }
                if (!string.IsNullOrEmpty(maxMile))
                {
                    if (!regex_minmaxMile.Match(maxMile).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("maxMile", "Invalid format. Example (12345)");
                    }
                }
                // Year
                if (!string.IsNullOrEmpty(minYear))
                {
                    if (!regex_minmaxYear.Match(minYear).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("minYear", "Invalid format. Example (1900)");
                    }
                }
                if (!string.IsNullOrEmpty(maxYear))
                {
                    if (!regex_minmaxYear.Match(maxYear).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("maxYear", "Invalid format. Example (1900)");
                    }
                }
                // Engine Size
                if (!string.IsNullOrEmpty(modEngineSize))
                {
                    if (!regex_modEngineSize.Match(modEngineSize).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("modEngineSize", "Invalid format. Example (1500cc or 1.5L or V6)");
                    }
                }
                // Size
                if (!string.IsNullOrEmpty(minSize))
                {
                    if (!regex_minmaxSize.Match(minSize).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("minSize", "Invalid format. Example (12345)");
                    }
                }
                if (!string.IsNullOrEmpty(maxSize))
                {
                    if (!regex_minmaxSize.Match(maxSize).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("maxSize", "Invalid format. Example (12345)");
                    }
                }

                // Pets
                if (!string.IsNullOrEmpty(minAge))
                {
                    if (!regex_minmaxAge.Match(minAge).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("minAge", "Invalid format. Example (12345)");
                    }
                }
                if (!string.IsNullOrEmpty(maxAge))
                {
                    if (!regex_minmaxAge.Match(maxAge).Success)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("maxAge", "Invalid format. Example (12345)");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Validate AdInfos from Create and Edit Ad forms
    /// </summary>
    sealed partial class ValidateAdInfoFilter : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Get parameters
            var parameters = filterContext.ActionParameters;
            IEnumerable<InfoForm> AdInfos = null;
            if (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.Equals("ClassifiedAd") && filterContext.ActionDescriptor.ActionName.Equals("CreateAd"))
            {
                AdInfos = ((ClassifiedAdAdd)parameters["newItem"]).AdInfo;
            }
            else if (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.Equals("ClassifiedAd") && filterContext.ActionDescriptor.ActionName.Equals("MyAdEdit"))
            {
                AdInfos = ((ClassifiedAdEdit)parameters["editItem"]).AdInfo;
            }
            else if (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.Equals("Admin") && filterContext.ActionDescriptor.ActionName.Equals("AdminEditUserAd"))
            {
                AdInfos = ((AdminClassifiedAdEdit)parameters["editItem"]).AdInfo;
            }

            if (AdInfos == null)
            {
                OnActionExecuting(filterContext);
                return;
            }

            var adinfocounter = 0;

            // public RegEx
            //------------------------------------------------------------------------------------
            int MaxLength = 40;
            // Cars
            var regex_Mile = new Regex(@"^([1-9]{1}[0-9]{0,2}(\,[0-9]{3})*|([1-9]{1}[0-9]*))$");
            var regex_Year = new Regex(@"^([1-2]{1}[0-9]{3})$");
            var regex_modEngineSize = new Regex(@"^([0-9]{1}\.[0-9]{1}\s?[L])$|^([1-9]{1}[0-9]{1}\s?[L])$|^([1-9]{1}[0-9]{1,4}\s?[c][c])$|^(([V]|[v])([6]|[8]|[1][2]))$");
            var regex_Size = new Regex(@"^([1-9]{1}[0-9]{0,2}(\,[0-9]{3})*|([1-9]{1}[0-9]*))$");
            // Pets
            var regex_Age = new Regex(@"^[1-9]?[0-9]?[0-9]$");

            foreach (var ai in AdInfos)
            {
                if (!string.IsNullOrEmpty(ai.Description))
                {
                    if (ai.Description.Length > MaxLength)
                    {
                        filterContext.Controller.ViewData.ModelState.AddModelError("AdInfo[" + adinfocounter + "].Description", ai.Name + " field exceeds the maximum length of 40!");
                    }
                    else
                    {
                        if (ai.Name.Equals("Make"))
                        {
                            if (string.IsNullOrEmpty(ai.Description))
                            {
                                filterContext.Controller.ViewData.ModelState.AddModelError("AdInfo[" + adinfocounter + "].Description", "Make is required.");
                            }
                        }
                        else if (ai.Name.Equals("Model"))
                        {
                            if (string.IsNullOrEmpty(ai.Description))
                            {
                                filterContext.Controller.ViewData.ModelState.AddModelError("AdInfo[" + adinfocounter + "].Description", "Model name is required.");
                            }
                        }
                        else if (ai.Name.Equals("Year"))
                        {
                            if (!string.IsNullOrEmpty(ai.Description))
                            {
                                if (!regex_Year.Match(ai.Description).Success)
                                {
                                    filterContext.Controller.ViewData.ModelState.AddModelError("AdInfo[" + adinfocounter + "].Description", "Invalid format. Example (1900)");
                                }
                            }
                        }
                        else if (ai.Name.Equals("Mileage"))
                        {
                            if (!string.IsNullOrEmpty(ai.Description))
                            {
                                if (!regex_Mile.Match(ai.Description).Success)
                                {
                                    filterContext.Controller.ViewData.ModelState.AddModelError("AdInfo[" + adinfocounter + "].Description", "Invalid format. Example (12345)");
                                }
                            }
                        }

                        else if (ai.Name.Equals("Engine Size"))
                        {
                            if (!string.IsNullOrEmpty(ai.Description))
                            {
                                if (!regex_modEngineSize.Match(ai.Description).Success)
                                {
                                    filterContext.Controller.ViewData.ModelState.AddModelError("AdInfo[" + adinfocounter + "].Description", "Invalid format. Example (1500cc or 1.5L or V6)");
                                }
                            }
                        }
                        else if (ai.Name.Equals("Size"))
                        {
                            if (!string.IsNullOrEmpty(ai.Description))
                            {
                                if (!regex_Size.Match(ai.Description).Success)
                                {
                                    filterContext.Controller.ViewData.ModelState.AddModelError("AdInfo[" + adinfocounter + "].Description", "Invalid format. Example (1900)");
                                }
                            }
                        }
                        else if (ai.Name.Equals("Age"))
                        {
                            if (!string.IsNullOrEmpty(ai.Description))
                            {
                                if (!regex_Age.Match(ai.Description).Success)
                                {
                                    filterContext.Controller.ViewData.ModelState.AddModelError("AdInfo[" + adinfocounter + "].Description", "Invalid length. Range (1-999)");
                                }
                            }
                        }
                        else if (ai.Name.Equals("Company Name"))
                        {
                            if (string.IsNullOrEmpty(ai.Description))
                            {
                                filterContext.Controller.ViewData.ModelState.AddModelError("AdInfo[" + adinfocounter + "].Description", "Company Name is required");
                            }
                        }
                    }
                }
                adinfocounter++;
            }
            OnActionExecuting(filterContext);
        }
    }
}