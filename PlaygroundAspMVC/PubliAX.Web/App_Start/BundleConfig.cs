using System.Web;
using System.Web.Optimization;

namespace PubliAX.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.10.2.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/fastclick").Include(
                      "~/Scripts/fastclick.js"));

            bundles.Add(new ScriptBundle("~/bundles/nprogress").Include(
                      "~/Scripts/nprogress.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/Gentelella").Include(
                      "~/Libs/Gentelella/js/custom.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/KnockoutJs").Include(
                      "~/Scripts/knockout-3.5.0.debug.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/inputmask").Include(
            //~/Scripts/Inputmask/dependencyLibs/inputmask.dependencyLib.js",  //if not using jquery
            "~/Scripts/Inputmask/inputmask.js",
            "~/Scripts/Inputmask/jquery.inputmask.js",
            "~/Scripts/Inputmask/inputmask.extensions.js",
            "~/Scripts/Inputmask/inputmask.date.extensions.js",
            //and other extensions you want to include
            "~/Scripts/Inputmask/inputmask.numeric.extensions.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqDataTable").Include(
                     "~/Scripts/DataTables/jquery.dataTables.js",
                     "~/Scripts/DataTables/dataTables.bootstrap.min.js"));

        }
    }
}

