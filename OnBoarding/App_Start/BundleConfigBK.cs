using System.Web.Optimization;

namespace OnBoarding.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //1. Bundled CSS
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/bundles/css").Include(
                     "~/Assets/bootstrap/dist/css/bootstrap.min.css",
                     "~/Content/site.css",
                     "~/Content/sb-admin-2.css",
                     "~/Assets/jasny-bootstrap/jasny-bootstrap.min.css",
                     "~/Assets/metisMenu/dist/metisMenu.min.css",
                     "~/Assets/font-awesome/css/font-awesome.min.css",
                     "~/Assets/morrisjs/morris.css",
                     "~/Assets/fwizard/form-wizard.css",
                     "~/Assets/datepicker/datepicker.css",
                     "~/Assets/select2/select2.min.css",
                     "~/Assets/toastr/toastr.css",
                      "~/Assets/datatables/media/css/dataTables.bootstrap.css",
                     "~/Assets/datatables/media/css/jquery.dataTables.min.css",
                     "~/Assets/datatables/media/css/responsive.dataTables.min.css"));

            //2. Bundled JS
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.3.1.js",
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Assets/fwizard/jquery.validate.js",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/customjs").Include(
                        "~/Assets/bootstrap/js/alert.js",
                        "~/Assets/bootstrap/dist/js/bootstrap.min.js",
                         "~/Scripts/respond.js",
                         "~/Assets/metisMenu/dist/metisMenu.min.js",
                         "~/Assets/datepicker/bootstrap-datepicker.js",
                         "~/Assets/raphael/raphael-min.js",
                         "~/Assets/datatables/media/js/jquery.dataTables.min.js",
                         "~/Assets/datatables/media/js/dataTables.bootstrap.min.js",
                         "~/Assets/fwizard/form-wizard.js",
                         "~/Assets/select2/select2.min.js",
                         "~/Scripts/sb-admin-2.js",
                         "~/Scripts/jasny-bootstrap.min.js",
                         "~/Assets/toastr/toastr.js"));
        }
    }
}
