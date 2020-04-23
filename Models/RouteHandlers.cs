using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;

namespace Trinbago_MVC5.Models
{
    public class ImageRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            string fn = requestContext.RouteData.Values["FileName"] as string;
            string a = requestContext.RouteData.Values["a"] as string;
            string b = requestContext.RouteData.Values["b"] as string;

            if (string.IsNullOrEmpty(fn))
            {
                // return a 404 HttpHandler here
                return null;
            }
            else
            {
                requestContext.HttpContext.Response.Clear();
                requestContext.HttpContext.Response.ContentType = GetContentType(requestContext.HttpContext.Request.Url.ToString());

                string filepath = requestContext.HttpContext.Server.MapPath("~/Photos/" + a + "/" + b + "/" + fn);
                try
                {
                    requestContext.HttpContext.Response.WriteFile(filepath);
                    requestContext.HttpContext.Response.End();
                }
                catch (Exception) { return null; }   
            }
            return null;
        }

        private static string GetContentType(String path)
        {
            switch (Path.GetExtension(path))
            {
                case ".bmp": return "Image/bmp";
                case ".gif": return "Image/gif";
                case ".jpg": return "Image/jpeg";
                case ".png": return "Image/png";
                default: break;
            }
            return "";
        }
    }

    public class SeoFriendlyRoute : Route
    {
        public SeoFriendlyRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler)
        {
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var routeData = base.GetRouteData(httpContext);

            if (routeData != null)
            {
                if (routeData.Values.ContainsKey("stringId"))
                    routeData.Values["stringId"] = GetIdValue(routeData.Values["stringId"]);
            }

            return routeData;
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