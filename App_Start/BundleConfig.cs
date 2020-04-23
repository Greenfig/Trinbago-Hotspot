using System.Web.Optimization;

namespace Trinbago_MVC5
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            Configure1(bundles);
            //Configure2(bundles);
           
        }

        private static void Configure1(BundleCollection bundles)
        {
            //------------------------- _Layout--------------------------
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/outdatedbrowser/outdatedbrowser.css",
                "~/Content/bootstrap.css",
                "~/Content/Site.css",
                "~/Content/bootstrap-select.css",
                "~/Content/PagedList.css",
                "~/Content/venobox/veno.box.css",
                "~/Content/owlcarousel/owl.carousel.css",
                "~/Content/owlcarousel/owl.theme.default.css",
                "~/Content/font-awesome.css"));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/Scripts/outdatedbrowser/outdatedbrowser.js",
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.bundle.js",
                "~/Scripts/bootstrap-select.js",
                "~/Scripts/respond.js",
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*",
                "~/Scripts/venobox/veno.box.js",
                "~/Scripts/owlcarousel/owl.carousel.js",
                "~/Scripts/site_scripts.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/public").Include(                
                "~/Scripts/google-analytics.js"));

            bundles.Add(new ScriptBundle("~/bundles/social").Include(
                "~/Scripts/social.js"));

            bundles.Add(new ScriptBundle("~/bundles/tinymce").Include(
                "~/Scripts/tinymce/tinymce.js"));

            bundles.Add(new ScriptBundle("~/bundles/photoupldr").Include(
                "~/Scripts/jquery-sort/jquery-ui.js",
                "~/Scripts/jquery-sort/jquery.ui.touch-punch.js",
                "~/Scripts/photo-uploader.js"
                ));
        }

        private static void Configure2(BundleCollection bundles)
        {
            //------------------------- _Layout--------------------------
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-select.css",
                "~/Content/Site.css",
                "~/Content/themes/base/jquery-ui.css",
                "~/Content/font-awesome.css",
                "~/Content/venobox/veno.box.css",
                "~/Content/owlcarousel/owl.carousel.css",
                "~/Content/owlcarousel/owl.theme.default.css"));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/bootstrap-select.js",
                "~/Scripts/venobox/veno.box.js",
                "~/Scripts/respond.js",
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*",
                "~/Scripts/owlcarousel/owl.carousel.js",
                "~/Scripts/site_scripts.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery",
                @"https://code.jquery.com/jquery-3.1.1.min.js").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-ui",
                @"https://code.jquery.com/ui/1.12.1/jquery-ui.min.js").Include(
                "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/public").Include(
                "~/Scripts/social.js",
                "~/Scripts/google-analytics.js"));

            bundles.Add(new ScriptBundle("~/bundles/ckeditor").Include(
                "~/Scripts/ckeditor/ckeditor.js"));

            bundles.Add(new ScriptBundle("~/bundles/captcha").Include(
                "~/Scripts/api.js"));

            bundles.UseCdn = true;
        }

        private static void Configure3(BundleCollection bundles)
        {

            //------------------------- _Layout--------------------------
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/Site.css"));
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/Scripts/site_scripts.js",
                "~/Scripts/jquery.unobtrusive*"));
            bundles.Add(new ScriptBundle("~/bundles/public").Include(
                "~/Scripts/social.js",
                "~/Scripts/google-analytics.js"));
            //------------------------- Jquery--------------------------
            bundles.Add(new ScriptBundle("~/bundles/jquery",
                @"https://code.jquery.com/jquery-3.1.1.min.js").Include(
                "~/Scripts/jquery-{version}.js"));
            //----------------------- Jquery_UI-------------------------
            bundles.Add(new StyleBundle("~/Content/jquery-ui",
                @"https://cdnjs.cloudflare.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.css").Include(
                "~/Content/themes/base/jquery-ui.css"));
            bundles.Add(new ScriptBundle("~/bundles/jquery-ui",
                @"https://code.jquery.com/ui/1.12.1/jquery-ui.min.js").Include(
                "~/Scripts/jquery-ui-{version}.js"));
            //-------------------- Jquery_Validation--------------------
            bundles.Add(new ScriptBundle("~/bundles/jquery-validate",
                @"https://cdnjs.cloudflare.com/ajax/libs/jquery-validate/1.16.0/jquery.validate.min.js").Include(
                "~/Scripts/jquery.validate*"));
            bundles.Add(new ScriptBundle("~/bundles/jquery-validate-unobtrusive",
                @"https://cdnjs.cloudflare.com/ajax/libs/jquery-validation-unobtrusive/3.2.6/jquery.validate.unobtrusive.min.js").Include(
                "~/Scripts/jquery.validate*"));
            //---------------------- Bootstrap--------------------------
            bundles.Add(new StyleBundle("~/Content/bootstrap",
                @"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css").Include(
                "~/Content/bootstrap.css"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap",
                @"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js").Include(
                "~/Scripts/bootstrap.js"));
            //-------------------- Bootstrap_Select---------------------
            bundles.Add(new StyleBundle("~/Content/bootstrap-select",
                @"https://cdnjs.cloudflare.com/ajax/libs/bootstrap-select/1.12.3/css/bootstrap-select.min.css").Include(
                "~/Content/bootstrap-select.css"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap-select",
                @"https://cdnjs.cloudflare.com/ajax/libs/bootstrap-select/1.12.3/js/bootstrap-select.min.js").Include(
                "~/Scripts/bootstrap-select.js"));
            //---------------------- Font_Awesome-----------------------
            bundles.Add(new StyleBundle("~/Content/font-awesome",
                @"https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css").Include(
                "~/Content/font-awesome.css"));
            //----------------------- Venobox---------------------------
            bundles.Add(new StyleBundle("~/Content/venobox",
                @"https://cdnjs.cloudflare.com/ajax/libs/venobox/1.8.2/venobox.min.css").Include(
                "~/Content/venobox/veno.box.css"));
            bundles.Add(new ScriptBundle("~/bundles/venobox",
                @"https://cdnjs.cloudflare.com/ajax/libs/venobox/1.8.2/venobox.min.js").Include(
                "~/Scripts/venobox/veno.box.js"));
            //----------------------- Owl------------------------------
            bundles.Add(new StyleBundle("~/Content/owl",
                @"https://cdnjs.cloudflare.com/ajax/libs/owl-carousel/1.3.3/owl.carousel.min.css").Include(
                "~/Content/owlcarousel/owl.carousel.css"));
            bundles.Add(new ScriptBundle("~/bundles/owl",
                @"https://cdnjs.cloudflare.com/ajax/libs/owl-carousel/1.3.3/owl.carousel.min.js").Include(
                "~/Scripts/owlcarousel/owl.carousel.js"));
            //----------------------- Respond--------------------------
            bundles.Add(new ScriptBundle("~/bundles/respond",
                @"https://cdnjs.cloudflare.com/ajax/libs/respond.js/1.4.2/respond.js").Include(
                "~/Scripts/respond.js"));
            //----------------------- CKEditor-------------------------
            bundles.Add(new ScriptBundle("~/bundles/ckeditor").Include(
                "~/Scripts/ckeditor/ckeditor.js"));
            //----------------------- Captcha--------------------------
            bundles.Add(new ScriptBundle("~/bundles/captcha").Include(
                "~/Scripts/api.js"));

            bundles.UseCdn = true;
        }
    }
}
