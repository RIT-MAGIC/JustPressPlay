using System.Web;
using System.Web.Optimization;

namespace JustPressPlay
{
	public class BundleConfig
	{
		// For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
		public static void RegisterBundles(BundleCollection bundles)
		{
            bundles.IgnoreList.Clear();

			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/knockoutjs").Include(
                        "~/Scripts/knockout-{version}.js"));

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));
            
            // Includes every necessary CSS file
			bundles.Add(new Bundle("~/Content/css").Include(
                        "~/Content/css/fonts.css",
                        "~/Content/css/select2.css"
                        /*"~/Content/site.css"*/));

            // Uncomment to enable javascript file optimizations
            //BundleTable.EnableOptimizations = true;
		}
	}
}