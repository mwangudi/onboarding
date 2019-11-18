using System.Web.Optimization;

namespace OnBoarding
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            //1. JQuery JS
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Assets/jquery/jquery.validate*"));

            //2. Bundled bootstrap admin CSS
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
                    "~/Assets/datatables/DataTables-1.10.16/css/dataTables.bootstrap4.min.css",
                    "~/Assets/sweetalert/sweetalert2.css",
                    "~/Assets/datepicker/datepicker.css"));

            //3. Bundled bootstrap admin CSS
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

            //4. Bundled bootstrap admin js
            bundles.Add(new ScriptBundle("~/bundles/bootstrapadminjs").Include(
                        "~/Assets/jquery/jquery-3.4.1.min.js",
                        "~/Assets/jquery/jquery-ui-1.12.1.min.js",
                        "~/Assets/jtable/jquery.jtable.min.js",
                        "~/Assets/chart.min.js",
                        "~/Scripts/js/popper.js",
                         "~/Scripts/js/tooltip.js",
                         "~/Assets/bootstrap/js/bootstrap.min.js",
                         "~/Assets/nicescroll/jquery.nicescroll.min.js",
                         "~/Assets/scroll-up-bar/dist/scroll-up-bar.min.js",
                         "~/Scripts/js/sa-functions.js",
                         "~/Scripts/js/chart.min.js",
                         "~/Assets/summernote/summernote-lite.js",
                         "~/Assets/jquery/jquery.validate.js",
                         "~/Scripts/js/fileSaver.js",
                         "~/Scripts/js/scripts.js",
                         "~/Scripts/js/custom.js",
                         "~/Scripts/js/demo.js",
                         "~/Scripts/js/jquery.accordion-wizard.js",
                          "~/Assets/fwizard/form-wizard.js",
                         "~/Assets/toastr/build/toastr.min.js",
                         "~/Assets/select2/select2.js",
                         "~/Scripts/js/additional-methods.min.js",
                         "~/Assets/datatables/datatables.min.js",
                         "~/Assets/datatables/DataTables-1.10.16/js/dataTables.bootstrap4.min.js",
                         "~/Assets/sweetalert/sweetalert2.js",
                         "~/Assets/datepicker/bootstrap-datepicker.js"));

            foreach (var bundle in BundleTable.Bundles)
            {
                bundle.Transforms.Clear();
            }
        }
    }
}
