using System;
using System.Threading.Tasks;
using System.Linq;
using System.Json;
using System.Text;

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
		const string KEY_APP_LINK_DATA = "al_applink_data";
		const string KEY_REFERER_DATA = "referer_app_link";

		public AppLinkNavigator ()
		{
		}
			
		static AppLinkNavigator()
		{
			DefaultResolver = new HttpClientAppLinkResolver ();
		}

		public static IAppLinkResolver DefaultResolver { get; set; }

		public event WillNavigateToWebUrlDelegate WillNavigateToWebUrl;

		#region Overloads
		public async Task<NavigationResult> Navigate(AppLink appLink)
		{
			return await Navigate (appLink, null, null);
		}

		public async Task<NavigationResult> Navigate(Uri url)
		{
			return await Navigate (url, null, null);
		}

		public async Task<NavigationResult> Navigate(string url)
		{
			return await Navigate (url, null, null);
		}

		public async Task<NavigationResult> Navigate(AppLink appLink, RefererAppLink refererAppLink)
		{
			return await Navigate (appLink, null, refererAppLink);
		}

		public async Task<NavigationResult> Navigate(Uri url, RefererAppLink refererAppLink)
		{
			return await Navigate (url, null, refererAppLink);
		}

		public async Task<NavigationResult> Navigate(string url, RefererAppLink refererAppLink)
		{
			return await Navigate (url, null, refererAppLink);
		}

		public async Task<NavigationResult> Navigate (AppLink appLink, JsonObject extras)
		{
			return await Navigate (appLink, extras, null);
		}

		public async Task<NavigationResult> Navigate (Uri url, JsonObject extras)
		{
			return await Navigate (url, extras, null);
		}

		public async Task<NavigationResult> Navigate (string url, JsonObject extras)
		{
			return await Navigate (url, extras, null);
		}
		#endregion

		#region Overloads that do implicit resolving of App Links
		public async Task<NavigationResult> Navigate (Uri url, JsonObject extras, RefererAppLink refererAppLink)
		{
			var appLink = await DefaultResolver.ResolveAppLinks (url);

			if (appLink != null)
				return await Navigate (appLink, extras, refererAppLink);

			return NavigationResult.Failed;
		}

		public async Task<NavigationResult> Navigate (string url, JsonObject extras, RefererAppLink refererAppLink)
		{
			var uri = new Uri (url);
			return await Navigate(uri, extras, refererAppLink);
		}
		#endregion

		#if PORTABLE
		public async Task<NavigationResult> Navigate (AppLink appLink, JsonObject extras, RefererAppLink refererAppLink)
		{
			throw new NotSupportedException ("You can't run this from the Portable Library.  Reference a platform Specific Library Instead");
		}
		#elif __IOS__
		public async Task<NavigationResult> Navigate (AppLink appLink, JsonObject extras, RefererAppLink refererAppLink)
		{
			try {
				// Find the first eligible/launchable target in the BFAppLink.
				var eligibleTarget = appLink.Targets.FirstOrDefault(t => 
					UIApplication.SharedApplication.CanOpenUrl(t.Url));

				if (eligibleTarget != null) {
					var appLinkUrl = BuildUrl(appLink, eligibleTarget.Url, extras, refererAppLink);
					
					// Attempt to navigate
					if (UIApplication.SharedApplication.OpenUrl(appLinkUrl))
						return NavigationResult.App;
				}

				// Fall back to opening the url in the browser if available.
				if (appLink.WebUrl != null) {
					var navigateUrl = BuildUrl(appLink, appLink.WebUrl, extras, refererAppLink);

					var handled = RaiseWillNavigateToWebUrl(navigateUrl);

					if (handled)
						return NavigationResult.Web;

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
		#elif __ANDROID__
		public async Task<NavigationResult> Navigate (AppLink appLink, JsonObject extras, RefererAppLink refererAppLink)
		{
			var context = Android.App.Application.Context;
			var pm = context.PackageManager;

			var appLinkDataJson = JsonSerializeAppLinkData (appLink, extras);

			Intent eligibleTargetIntent = null;

			foreach (var t in appLink.Targets) {
				var target = t as AndroidAppLinkTarget;

				if (target == null)
					continue;

				var targetIntent = new Intent (Intent.ActionView);

				if (target.Url != null)
					targetIntent.SetData (Android.Net.Uri.Parse(target.Url.ToString()));
				else
					targetIntent.SetData (Android.Net.Uri.Parse(appLink.SourceUrl.ToString()));

				targetIntent.SetPackage (target.Package);

				if (target.Class != null)
					targetIntent.SetClassName (target.Package, target.Class);

				targetIntent.PutExtra (KEY_APP_LINK_DATA, appLinkDataJson);

				var resolved = pm.ResolveActivity (targetIntent, Android.Content.PM.PackageInfoFlags.MatchDefaultOnly);
				if (resolved != null) {
					eligibleTargetIntent = targetIntent;
					break;
				}
			}

			if (eligibleTargetIntent != null) {
			
				eligibleTargetIntent.AddFlags (ActivityFlags.NewTask);
				context.StartActivity (eligibleTargetIntent);

				return NavigationResult.App;
			}

			// Fall back to the web if it's available
			if (appLink.WebUrl != null) {

                var webUrl = BuildUrl(appLink, appLink.WebUrl, extras);

				var handled = RaiseWillNavigateToWebUrl (webUrl);

				if (!handled) {
					Intent launchBrowserIntent = new Intent (Intent.ActionView, Android.Net.Uri.Parse (webUrl.ToString ()));
					launchBrowserIntent.AddFlags (ActivityFlags.NewTask);
					context.StartActivity (launchBrowserIntent);
				}

				return NavigationResult.Web;
			}

			return NavigationResult.Failed;
		}
        #elif WINDOWS_PHONE
        public async Task<NavigationResult> Navigate(AppLink appLink, JsonObject extras, RefererAppLink refererAppLink)
        {
            try
            {
                // Find the first eligible/launchable target in the BFAppLink.
                foreach (var target in appLink.Targets)
                {
                    var wpTarget = target as WindowsPhoneAppLinkTarget;

                    if (wpTarget == null)
                        continue;

                    var url = BuildUrl(appLink, wpTarget.Url, extras);
                    var launched = await Windows.System.Launcher.LaunchUriAsync(url);

                    if (launched)
                        return NavigationResult.App;
                }

                // Fall back to opening the url in the browser if available.
                if (appLink.WebUrl != null)
                {
                    var navigateUrl = BuildUrl(appLink, appLink.WebUrl, extras);

					var handled = RaiseWillNavigateToWebUrl(navigateUrl);
					if (handled)
						return NavigationResult.Web

                    var launched = await Windows.System.Launcher.LaunchUriAsync(navigateUrl);

                    if (launched)
                        return NavigationResult.Web;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // Otherwise, navigation fails.
            return NavigationResult.Failed;
        }
        #else
		public async Task<NavigationResult> Navigate (AppLink appLink, JsonObject extras, RefererAppLink refererAppLink)
		{
			if (appLink.WebUrl != null) {
				
				var navigateUrl = BuildUrl(appLink, appLink.WebUrl, extras);

				var handled = RaiseWillNavigateToWebUrl(navigateUrl);
				if (handled)
					return NavigationResult.Web
				
				System.Diagnostics.Process.Start(navigateUrl.ToString());
				return NavigationResult.Web;
			}
			
			return NavigationResult.Failed;
		}
		#endif

		bool RaiseWillNavigateToWebUrl(Uri webUrl)
		{
			var evt = WillNavigateToWebUrl;
			if (evt != null) {
				var eventArgs = new WebNavigateEventArgs (webUrl);
				WillNavigateToWebUrl (this, eventArgs);

				return eventArgs.Handled;
			}

			return false;
		}

        Uri BuildUrl(AppLink appLink, Uri targetUrl, JsonObject extras, RefererAppLink refererAppLink = null)
        {
            var appLinkDataJson = JsonSerializeAppLinkData(appLink, extras);
            var builder = new UriBuilder(targetUrl);
			var query = Utility.ParseQueryString(builder.Query);
            query[KEY_APP_LINK_DATA] = appLinkDataJson;

            if (refererAppLink != null)
            {

                var refererAppLinkJson = JsonSerializeRefererAppLink(refererAppLink);

                if (!string.IsNullOrEmpty(refererAppLinkJson))
                    query[KEY_REFERER_DATA] = refererAppLinkJson;
            }

            var querystr = new StringBuilder();
            
            foreach (var key in query.Keys)
            {
                querystr.Append(Utility.UrlEncode(key));
                querystr.Append("=");
                querystr.Append(Utility.UrlEncode(query[key]));
                querystr.Append("&");
            }

            builder.Query = querystr.ToString().TrimEnd('&');

            return builder.Uri;
        }

		string JsonSerializeAppLinkData(AppLink appLink, JsonObject extras)
		{
			var j = new JsonObject ();
			j ["target_url"] = appLink.SourceUrl.ToString ();
			j ["version"] = AppLinks.Version;
			j ["user_agent"] = AppLinks.UserAgent;
			j ["extras"] = extras;

			return j.ToString ();
		}

		string JsonSerializeRefererAppLink(RefererAppLink refererAppLink)
		{
			var j = new JsonObject ();
			if (refererAppLink.TargetUrl != null)
				j ["target_url"] = refererAppLink.TargetUrl.ToString ();
			if (!string.IsNullOrEmpty (refererAppLink.AppName))
				j ["app_name"] = refererAppLink.AppName;
			if (!string.IsNullOrEmpty (refererAppLink.AppStoreId))
				j ["app_store_id"] = refererAppLink.AppStoreId;
			if (refererAppLink.Url != null)
				j ["url"] = refererAppLink.Url.ToString ();

			return j.ToString ();
		}
	}	
}

