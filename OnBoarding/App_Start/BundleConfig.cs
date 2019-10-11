using System.Web.Optimization;

namespace OnBoarding
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
            
            //1. Bundled Old CSS
            bundles.Add(new StyleBundle("~/bundles/css").Include(
                    "~/Content/css/bootstrap/bootstrap.min.css",
                    "~/Content/css/site.css",
                    "~/Content/css/sb-admin-2.css",
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
                        "~/Assets/jquery/jquery-{version}.js"));

            //3. JQuery JS
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Assets/jquery/jquery.validate*"));
            
            //4. Bundled Bootstrap JS
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js"));

            //5. Bundled Custom JS
            bundles.Add(new ScriptBundle("~/bundles/customjs").Include(
                        "~/Assets/bootstrap/js/alert.js",
                        "~/Scripts/bootstrap.min.js",
                         "~/Scripts/respond.js",
                         "~/Assets/metismenu/dist/metisMenu.min.js",
                         "~/Assets/datepicker/bootstrap-datepicker.js",
                         "~/Assets/raphael/raphael-min.js",
                         "~/Assets/datatables/DataTables-1.10.16/js/jquery.dataTables.min.js",
                         "~/Assets/datatables/DataTables-1.10.16/js/dataTables.bootstrap.min.js",
                         "~/Assets/fwizard/form-wizard.js",
                         "~/Assets/select2/select2.min.js",
                         "~/Scripts/sb-admin-2.js",
                         "~/Scripts/jasny-bootstrap.min.js",
                         "~/Assets/toastr/toastr.js"));
        }

        public static void RegisterAdminBundles(BundleCollection bundles)
        {
            //1. Bundled bootstrap admin CSS
            bundles.Add(new StyleBundle("~/bundles/bootstrapadmincss").Include(
                    "~/Assets/jtable/jquery-ui-1.12.1/jquery-ui.css",
                    "~/Assets/jtable/jquery-ui-1.12.1/jquery-ui.theme.css",
                    "~/Assets/jtable/themes/jqueryui/jtable_jqueryui.css",
                    "~/Assets/jtable/themes/metro/blue/jtable.css",
                    "~/Assets/toastr/build/toastr.min.css",
                    "~/Content/css/custom.css",
                    "~/Assets/bootstrap/css/bootstrap.min.css",
                    "~/Assets/ionicons/css/ionicons.min.css",
                    "~/Assets/fontawesome/web-fonts-with-css/css/fontawesome-all.min.css",
                    "~/Assets/summernote/summernote-lite.css",
                    "~/Assets/flag-icon-css/css/flag-icon.min.css",
                    "~/Content/css/demo.css",
                    "~/Assets/fwizard/form-wizard.css",
                    "~/Content/css/style.css",
                    "~/Assets/select2/select2.css",
                    "~/Assets/datatables/DataTables-1.10.16/css/dataTables.bootstrap.min.css",
                    "~/Assets/datepicker/datepicker.css"));

            //2. Bundled bootstrap admin CSS
            bundles.Add(new StyleBundle("~/bundles/bootstrapfrontendcss").Include( 
                    "~/Assets/toastr/build/toastr.min.css",
                    "~/Content/css/custom.css",
                    "~/Assets/bootstrap/css/bootstrap.min.css",
                    "~/Assets/ionicons/css/ionicons.min.css",
                    "~/Assets/fontawesome/web-fonts-with-css/css/fontawesome-all.min.css",
                    "~/Assets/summernote/summernote-lite.css",
                    "~/Assets/flag-icon-css/css/flag-icon.min.css",
                    "~/Content/css/demo.css",
                    "~/Content/css/style2.css",
                    "~/Assets/select2/select2.css",
                     "~/Assets/datatables/DataTables-1.10.16/css/dataTables.bootstrap4.min.css",
                    "~/Assets/datepicker/datepicker.css"));

            //3. Bundled bootstrap admin js
            bundles.Add(new ScriptBundle("~/bundles/bootstrapadminjs").Include(
                        "~/Assets/jquery/jquery-3.4.1.min.js",
                        "~/Assets/jquery/jquery-ui-1.12.1.min.js",
                        "~/Assets/jtable/jquery.jtable.min.js",
                        "~/Assets/popper.js",
                        "~/Assets/fileSaver.js",
                         "~/Assets/tooltip.js",
                         "~/Assets/bootstrap/js/bootstrap.min.js",
                         "~/Assets/nicescroll/jquery.nicescroll.min.js",
                         "~/Assets/scroll-up-bar/dist/scroll-up-bar.min.js",
                         "~/Scripts/js/sa-functions.js",
                         "~/Assets/chart.min.js",
                         "~/Assets/summernote/summernote-lite.js",
                         "~/Assets/jquery/jquery.validate.js",
                         "~/Scripts/js/scripts.js",
                         "~/Scripts/js/custom.js",
                         "~/Scripts/js/demo.js",
                         "~/Assets/jquery.accordion-wizard.js",
                          "~/Assets/fwizard/form-wizard.js",
                         "~/Assets/toastr/build/toastr.min.js",
                         "~/Assets/select2/select2.js",
                         "~/Scripts/js/additional-methods.min.js",
                         "~/Assets/datatables/datatables.min.js",
                         "~/Assets/datatables/DataTables-1.10.16/js/dataTables.bootstrap4.min.js",
                         "~/Assets/datepicker/bootstrap-datepicker.js"));

            foreach (var bundle in BundleTable.Bundles)
            {
                bundle.Transforms.Clear();
            }
        }
    }
}
