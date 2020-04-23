using System.Web.Mvc;

namespace Trinbago_MVC5.Extensions
{
    public static class AdSenseHtmlHelperExtension
    {
        private static MvcHtmlString Code(long slotNumber, string name, string format = "auto")
        {
            return new MvcHtmlString("<!--AdSense-->" +
            "<ins class=\"adsbygoogle "+ name + "\" style=\"display:block;\" data-ad-client=\"ca-pub-6449002362205302\" data-ad-slot=\"" + slotNumber + "\" data-ad-format=\"" + format + "\" data-full-width-responsive=\"false\"></ins>" +
            "<script>(adsbygoogle = window.adsbygoogle || []).push({ });</script>");
        }

        public static MvcHtmlString GoogleAdSense(this HtmlHelper html, string name = null)
        {
            if (name == "g-adsense-addetails_below_contact")
            {
                return Code(9788376561, "g-adsense-addetails_below_contact");
            }
            else if (name == "g-adsense-addetails_above_title")
            {
                return Code(7215443292, "g-adsense-addetails_above_title", format: "horizontal");
            }
            else if (name == "g-adsense-adlist_top")
            {
                return Code(8586975539, "g-adsense-adlist_top", format: "horizontal");
            }
            else if (name == "g-adsense-adlist_bottom")
            {
                return Code(5052314383, "g-adsense-adlist_bottom", format: "horizontal");
            }
            else if (name == "g-adsense-home_0")
            {
                return Code(2448473404, "g-adsense-home_0");
            }
            else if (name == "g-adsense-home_1")
            {
                return Code(9616516984, "g-adsense-home_1");
            }
            else if (name == "g-adsense-home_2")
            {
                return Code(8399159302, "g-adsense-home_2");
            }
            return null;
        }
    }
}