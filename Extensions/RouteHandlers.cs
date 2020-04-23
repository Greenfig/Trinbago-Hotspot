using System;
using System.IO;
using System.Web;
using System.Web.Routing;

namespace Trinbago_MVC5.Extensions
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
                    requestContext.HttpContext.Response.TransmitFile(filepath);
                    requestContext.HttpContext.Response.End();
                }
                catch (Exception) {
                    requestContext.HttpContext.Response.TransmitFile("~/Images/TH/noimage.png");
                    requestContext.HttpContext.Response.End();
                }
                
            }
            return null;
        }

        private static string GetContentType(string path)
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
}