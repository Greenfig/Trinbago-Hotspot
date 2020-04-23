using System.Web.Mvc;
using System.Web.Routing;
using Trinbago_MVC5.Extensions;

namespace Trinbago_MVC5
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("500.aspx");
            routes.IgnoreRoute("Home/Index");
            routes.MapMvcAttributeRoutes(); // enable attribute routing
            // Images
            routes.Add("ImagesRoute", new Route("Images/{a}/{b}/{FileName}",
                new ImageRouteHandler()));
            //Login
            routes.Add(new Route("Account/Login", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(new { controller = "Account", action = "Login" }),
                DataTokens = new RouteValueDictionary(new { scheme = "https://trinbagohotspot.com/" })
            });            
        }
    }
}
