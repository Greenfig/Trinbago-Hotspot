using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Trinbago_MVC5.IdentityExtensions;

namespace Trinbago_MVC5.Extensions
{
    sealed public class AuthorizeEmailConfirmAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            if (httpContext.User.Identity.IsEmailConfirmed())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated || filterContext.HttpContext.User.IsInRole("Banned"))
            {
                base.HandleUnauthorizedRequest(filterContext);
                return;
            }
            filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                Area = "Account",
                                controller = "Account",
                                action = "EmailConfirmNeeded"
                            })
                        );
        }
    }

    public static class EmailSpamFilter
    {
        private static string dir = HostingEnvironment.MapPath("~/FilterList.txt");
        private static ICollection<string> EndsWith { get { return new List<string>(File.ReadAllLines(dir)); } }
        public static bool CheckEmailSpam(string email)
        {
            return EndsWith.Contains(email.Split('@')[1].ToLower());
        }
    }

    public class ExceptionHandler : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception.GetType() == typeof(HttpAntiForgeryException))
            {
                filterContext.Result = new RedirectResult("/"); // whatever the url that you want to redirect to
                filterContext.ExceptionHandled = true;
            }
            // Validation Exception
            else if (filterContext.Exception.GetType() == typeof(HttpRequestValidationException))
            {
                filterContext.Result = new RedirectResult("/");                
                filterContext.ExceptionHandled = true;
            }
            else if (filterContext.Exception.GetType() == typeof(SqlException))
            {
                filterContext.Result = new RedirectResult("/Errors/ErrorPage");
                
                filterContext.ExceptionHandled = true;
            }
            else if (filterContext.Exception.GetType() == typeof(TimeoutException))
            {
                filterContext.Result = new RedirectResult("/Errors/ErrorPage");
                filterContext.ExceptionHandled = true;
            }
            else if (filterContext.Exception.GetType() == typeof(InvalidOperationException))
            {
                filterContext.Result = new RedirectResult("/Errors/ErrorPage");
                filterContext.ExceptionHandled = true;
            }
            else if(filterContext.Exception.GetType() == typeof(HttpException))
            {
                filterContext.Result = new RedirectResult("/Errors/ErrorPage");
                filterContext.ExceptionHandled = true;
            }
            else if(filterContext.Exception.GetType() == typeof(HttpNotFoundResult))
            {
                filterContext.Result = new RedirectResult("/Errors/ResourceNotFound");
                filterContext.ExceptionHandled = true;
            }
            else if(filterContext.Exception.GetType() == typeof(HttpRequestValidationException))
            {
                filterContext.Result = new RedirectResult("/Errors/ErrorPage");
                filterContext.ExceptionHandled = true;
            }
            else
            {
                filterContext.Result = new RedirectResult("/Errors/ErrorPage");
                filterContext.ExceptionHandled = true;
            }
        }
    }

    public static class HtmlFilter
    {
        //http://stackoverflow.com/questions/12787449/html-agility-pack-removing-unwanted-tags-without-removing-content
        public static string RemoveUnwantedTags(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            var document = new HtmlDocument();
            document.LoadHtml(data);

            var acceptableTags = new string[] { };

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
    }

    /// <summary>
    /// Contains all security methods
    /// </summary>
    public static class MySecurity
    {
        /// <summary>
        /// Generate a random string Guid
        /// </summary>
        /// <returns></returns>
        public static string GetGen()
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
    }

    /// <summary>
    /// https://github.com/alisherdavronov/Recaptcha2
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ValidateRecaptcha2Attribute : ActionFilterAttribute
    {
        public string ErrorMessage { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var gRecaptchaResponseVariableName = @"g-recaptcha-response";
            var gRecaptchaResponse = filterContext.HttpContext.Request[gRecaptchaResponseVariableName];
            var gRecaptchaSecret = ConfigurationManager.AppSettings["Recaptcha2Secret"];
            var gRecaptchaRemoteIp = filterContext.HttpContext.Request.UserHostAddress;

            var verifier = new Recaptcha2Verifier(gRecaptchaResponse, gRecaptchaSecret, gRecaptchaRemoteIp);
            var verifyResponse = Task.Run(async () => await verifier.VerifyAsync()).Result;

            if (verifyResponse.Success) return;

            if (ErrorMessage == null) ErrorMessage = verifyResponse.ErrorCodes[0];
            filterContext.Controller.ViewData.ModelState.AddModelError(gRecaptchaResponseVariableName, ErrorMessage);
        }
    }

    class Recaptcha2Verifier
    {
        private readonly string gRecaptchaUrl = @"https://www.google.com/recaptcha/api/siteverify";
        private readonly string gRecaptchaResponse;
        private readonly string gRecaptchaSecret;
        private readonly string gRecaptchaRemoteIp;

        public Recaptcha2Verifier(string gRecaptchaResponse, string gRecaptchaSecret, string gRecaptchaRemoteIp)
        {
            this.gRecaptchaResponse = gRecaptchaResponse;
            this.gRecaptchaSecret = gRecaptchaSecret;
            this.gRecaptchaRemoteIp = gRecaptchaRemoteIp;
        }

        public async Task<Recaptcha2VerifyResponse> VerifyAsync()
        {
            var result = new Recaptcha2VerifyResponse();
            string responseString;

            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    {"secret", gRecaptchaSecret},
                    {"response", gRecaptchaResponse},
                    {"remoteip", gRecaptchaRemoteIp},
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(gRecaptchaUrl, content);
                responseString = await response.Content.ReadAsStringAsync();
            }

            if (string.IsNullOrWhiteSpace(responseString)) return result;

            result = JsonConvert.DeserializeObject<Recaptcha2VerifyResponse>(responseString);

            return result;
        }
    }

    class Recaptcha2VerifyResponse
    {
        [JsonProperty("success")]
        private bool? _success = null;

        public bool Success
        {
            get { return _success == true; }
        }

        [JsonProperty("error-codes")]
        private string[] _errorCodes = null;

        public string[] ErrorCodes
        {
            get { return _errorCodes ?? new string[0]; }
        }
    }

    public static class Recaptcha2HtmlHelperExtension
    {
        public static MvcHtmlString Recaptcha2(this HtmlHelper html, string name = "g-recaptcha-response")
        {
            var gRecaptchaScript = "<script src=\"https://www.google.com/recaptcha/api.js\" async defer></script>";
            var gRecaptchaSiteKey = ConfigurationManager.AppSettings["Recaptcha2SiteKey"];
            var gRecaptchaWidget = "<div class=\"g-recaptcha\" data-sitekey=\"" + gRecaptchaSiteKey + "\" style=\"transform: scale(0.77); transform - origin:0 0\"></div>";
            return new MvcHtmlString(gRecaptchaScript + gRecaptchaWidget);
        }
    }

    /// <summary>
    /// Use to verify file types
    /// </summary>
    public static class AllowedFileTypes
    {
        // Create Ad
        private static readonly byte[] PNG = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 }; // 16
        private static readonly byte[] JPG = { 255, 216, 255 };                                                // 3
        private static readonly byte[] BMP = { 66, 77 };                                                       // 2
        private static readonly byte[] GIF = { 71, 73, 70 };                                                   // 3
                                                                                                               // Apply To
        private static readonly byte[] DOC = { 208, 207, 17, 224, 161, 177, 26, 225 };                         // 8
        private static readonly byte[] PDF = { 37, 80, 68, 70, 45, 49, 46 };                                   // 7

        // Max Size in bytes
        private static readonly int MAX_PHOTO_SIZE = 6291456;
        private static readonly int MAX_FILE_SIZE = 3145728;

        public static bool AllowedFileTypesValidation(HttpPostedFileBase file, string methodname)
        {

            if (methodname.Equals("CreateAd"))
            {
                byte[] stream = new byte[16];
                var ext = file.InputStream.Read(stream, 0, 16);
                bool isRequired = false;

                if (stream.SequenceEqual(PNG))
                {
                    if (file.ContentLength <= MAX_PHOTO_SIZE)
                        isRequired = true;
                    return isRequired;
                }
                if (stream.Take(3).SequenceEqual(JPG))
                {
                    if (file.ContentLength <= MAX_PHOTO_SIZE)
                        isRequired = true;
                    return isRequired;
                }
                if (stream.Take(2).SequenceEqual(BMP))
                {
                    if (file.ContentLength <= MAX_PHOTO_SIZE)
                        isRequired = true;
                    return isRequired;
                }
                if (stream.Take(3).SequenceEqual(GIF))
                {
                    if (file.ContentLength <= MAX_PHOTO_SIZE)
                        isRequired = true;
                    return isRequired;
                }

                return isRequired;
            }

            if (methodname.Equals("ApplyToJob"))
            {
                byte[] stream = new byte[8];
                var ext = file.InputStream.Read(stream, 0, 8);
                bool isRequired = false;

                if (stream.SequenceEqual(DOC))
                {
                    if (file.ContentLength <= MAX_FILE_SIZE)
                        isRequired = true;
                    return isRequired;
                }

                if (stream.Take(7).SequenceEqual(PDF))
                {
                    if (file.ContentLength <= MAX_FILE_SIZE)
                        isRequired = true;
                    return isRequired;
                }

                return isRequired;
            }

            return false;
        }
    }
}