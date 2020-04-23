using System.Web.Mvc;
using Trinbago_MVC5.Extensions;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ExceptionHandler());
        }
    }
}
