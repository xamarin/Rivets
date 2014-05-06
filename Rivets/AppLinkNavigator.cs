using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
#if __IOS__
using MonoTouch.UIKit;
#endif

#if __ANDROID__
using Android.Content;
#endif

namespace Rivets
{
	public class AppLinkNavigator : IAppLinkNavigation
	{
		public AppLinkNavigator ()
		{
		}
			
		static AppLinkNavigator()
		{
			DefaultResolver = new HttpClientAppLinkResolver ();
		}

		public static IAppLinkResolver DefaultResolver { get; set; }

		public async Task<NavigationResult> Navigate (AppLink appLink, AppLinkData appLinkData)
		{
			return await Navigate (appLink, appLinkData, null);
		}

		public async Task<NavigationResult> Navigate (Uri url, AppLinkData appLinkData)
		{
			return await Navigate (url, appLinkData, null);
		}

		public async Task<NavigationResult> Navigate (string url, AppLinkData appLinkData)
		{
			return await Navigate (url, appLinkData, null);
		}

		public async Task<NavigationResult> Navigate (Uri url, AppLinkData appLinkData, RefererAppLink refererAppLink)
		{
			var appLink = await DefaultResolver.ResolveAppLinks (url);

			if (appLink != null)
				return await Navigate (appLink, appLinkData);

			return NavigationResult.Failed;
		}

		public async Task<NavigationResult> Navigate (string url, AppLinkData appLinkData, RefererAppLink refererAppLink)
		{
			var uri = new Uri (url);
			return await Navigate(uri, appLinkData);
		}

		#if PORTABLE
		public async Task<NavigationResult> Navigate (AppLink appLink, AppLinkData appLinkData, RefererAppLink refererAppLink)
		{
			throw new NotSupportedException ("You can't run this from the Portable Library.  Reference a platform Specific Library Instead");
		}
		#elif __IOS__
		public async Task<NavigationResult> Navigate (AppLink appLink, AppLinkData appLinkData, RefererAppLink refererAppLink)
		{
			try {
				// Find the first eligible/launchable target in the BFAppLink.
				IAppLinkTarget eligibleTarget = default(IAppLinkTarget);

				foreach (var target in appLink.Targets) {
					if (UIApplication.SharedApplication.CanOpenUrl(target.Url)) {
						eligibleTarget = target;
						break;
					}
				}

				if (eligibleTarget != null) {
					var appLinkUrl = BuildUrl(appLink, eligibleTarget.Url, appLinkData, refererAppLink);
					
					// Attempt to navigate
					if (UIApplication.SharedApplication.OpenUrl(appLinkUrl))
						return NavigationResult.App;
				}

				// Fall back to opening the url in the browser if available.
				if (appLink.WebUrl != null) {
					var navigateUrl = BuildUrl(appLink, appLink.WebUrl, appLinkData, refererAppLink);
					
					// Attempt to navigate
					if (UIApplication.SharedApplication.OpenUrl(navigateUrl))
						return NavigationResult.Web;		
				}
			}
			catch (Exception ex) {
				Console.WriteLine(ex);
			}

			// Otherwise, navigation fails.
			return NavigationResult.Failed;
		}

		Uri BuildUrl(AppLink appLink, Uri targetUrl, AppLinkData appLinkData, RefererAppLink refererAppLink) 
		{
			if (appLinkData == null)
				appLinkData = new AppLinkData ();

			appLinkData.TargetUrl = appLink.SourceUrl.ToString();

			var json = Newtonsoft.Json.JsonConvert.SerializeObject (appLinkData);
			
			var builder = new UriBuilder (targetUrl);
			var query = System.Web.HttpUtility.ParseQueryString (builder.Query);
			query ["al_applink_data"] = System.Web.HttpUtility.UrlEncode(json);

			if (refererAppLink != null)
				query ["referer_app_link"] = System.Web.HttpUtility.UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject (refererAppLink));

			builder.Query = query.ToString ();
			
			return builder.Uri;
		}
		#elif __ANDROID__
		public async Task<NavigationResult> Navigate (AppLink appLink, AppLinkData appLinkData)
		{
			var context = Android.App.Application.Context;
			var pm = context.PackageManager;

			Intent eligibleTargetIntent = null;
			foreach (var t in appLink.Targets) {
				var target = t as AndroidAppLinkTarget;

				if (target == null)
					continue;

				Intent targetIntent = new Intent (Intent.ActionView);

				if (target.Url != null)
					targetIntent.SetData (Android.Net.Uri.Parse(target.Url.ToString()));
				else
					targetIntent.SetData (Android.Net.Uri.Parse(appLink.SourceUrl.ToString()));

				targetIntent.SetPackage (target.Package);

				if (target.Class != null)
					targetIntent.SetClassName (target.Package, target.Class);

				targetIntent.PutExtra ("al_applink_data", Newtonsoft.Json.JsonConvert.SerializeObject(appLinkData));

				var resolved = pm.ResolveActivity (targetIntent, Android.Content.PM.PackageInfoFlags.MatchDefaultOnly);
				if (resolved != null) {
					eligibleTargetIntent = targetIntent;
					break;
				}
			}

			if (eligibleTargetIntent != null) {
				context.StartActivity (eligibleTargetIntent);
				return NavigationResult.App;
			}

			// Fall back to the web if it's available
			if (appLink.WebUrl != null) {
				var appLinkDataJson = string.Empty;
				try {
					appLinkDataJson = Newtonsoft.Json.JsonConvert.SerializeObject (appLinkData);
				} catch (Exception e) {
					Console.WriteLine (e);
					return NavigationResult.Failed;
				}

				var builder = new UriBuilder (appLink.WebUrl);
				var query = System.Web.HttpUtility.ParseQueryString (builder.Query);
				query ["al_applink_data"] = appLinkDataJson;
				builder.Query = query.ToString ();
				var webUrl = builder.ToString ();
				Intent launchBrowserIntent = new Intent (Intent.ActionView, Android.Net.Uri.Parse(webUrl));
				context.StartActivity (launchBrowserIntent);
				return NavigationResult.Web;
			}

			return NavigationResult.Failed;
		}
		#else
		public async Task<NavigationResult> Navigate (AppLink appLink, AppLinkData appLinkData)
		{
			if (appLink.WebUrl != null) {
				System.Diagnostics.Process.Start(appLink.WebUrl.ToString());
				return NavigationResult.Web;
			}
			
			return NavigationResult.Failed;
		}
		#endif
	}	
}

