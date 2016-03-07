using System;
using System.Linq;
#if !PORTABLE
using System.Net.Http;
#endif
using System.Text;
using System.Json;
using System.Collections.Generic;

namespace Rivets
{
	public class FacebookIndexAppLinkResolver : IAppLinkResolver
	{
		const string GRAPH_API_URL = "https://graph.facebook.com";

		#if !PORTABLE
		HttpClient http = new HttpClient();
		#endif

		public FacebookIndexAppLinkResolver (string appId, string clientToken)
		{
			AppId = appId;
			ClientToken = clientToken;
		}

		public string AppId { get; private set; }
		public string ClientToken { get; private set; }

		public System.Threading.Tasks.Task<AppLink> ResolveAppLinks (string url)
		{
			var uri = new Uri (url);
			return ResolveAppLinks(uri);
		}

		public async System.Threading.Tasks.Task<AppLink> ResolveAppLinks (Uri url)
		{
			var appLinks = await ResolveMultipleAppLinks (url);

			return appLinks.FirstOrDefault ().Value;
		}

		public async System.Threading.Tasks.Task<Dictionary<Uri, AppLink>> ResolveMultipleAppLinks (params Uri[] urls)
		{
			#if PORTABLE
			throw new NotImplementedException("You can't call this method from the PCL assembly, please reference a platform specific assembly instead");
			#endif

			var results = new Dictionary<Uri, AppLink> ();

			var builder = new StringBuilder ();
			builder.Append (GRAPH_API_URL);
			builder.Append ("/?ids=");
		
			var ids = string.Join(",", from u in urls select u.ToString());

			builder.Append (Utility.UrlEncode (ids));
			builder.Append ("&type=al");
			builder.Append ("&access_token=");
			builder.Append (Utility.UrlEncode (AppId + "|" + ClientToken));

			#if PORTABLE
			var data = string.Empty;
			#else
			System.Diagnostics.Debug.WriteLine (builder.ToString ());
			var data = await http.GetStringAsync (builder.ToString ());
			#endif

			var json = JsonObject.Parse (data);


			foreach (var uri in urls) {

				if (!json.ContainsKey (uri.ToString ()))
					continue;

				var aljson = json [uri.ToString ()];

				var al = new AppLink {
					Targets = new List<IAppLinkTarget>(),
					SourceUrl = uri
				};

				if (aljson.ContainsKey ("web") && aljson["web"].ContainsKey("url"))
					al.WebUrl = new Uri ((string)aljson ["web"] ["url"]);

				if (aljson.ContainsKey ("ios")) {
					var items = (JsonArray)aljson ["ios"];
					al.Targets.AddRange(parseiOSTargets (items, () => new IOSAppLinkTarget()));
				}

				if (aljson.ContainsKey ("iphone")) {
					var items = (JsonArray)aljson ["iphone"];
					al.Targets.AddRange(parseiOSTargets (items, () => new IPhoneAppLinkTarget()));
				}

				if (aljson.ContainsKey ("ipad")) {
					var items = (JsonArray)aljson ["ipad"];
					al.Targets.AddRange(parseiOSTargets (items, () => new IPadAppLinkTarget()));
				}

				if (aljson.ContainsKey ("android")) {
					var items = (JsonArray)aljson ["android"];
					al.Targets.AddRange (parseAndroidTargets (items));
				}

				if (aljson.ContainsKey ("windows")) {
					var items = (JsonArray)aljson ["windows"];
					al.Targets.AddRange (parseWindowsTargets (items, () => new WindowsAppLinkTarget ()));
				}

				if (aljson.ContainsKey ("windows_phone")) {
					var items = (JsonArray)aljson ["windows_phone"];
					al.Targets.AddRange (parseWindowsTargets (items, () => new WindowsPhoneAppLinkTarget ()));
				}

				if (aljson.ContainsKey ("windows_universal")) {
					var items = (JsonArray)aljson ["windows_universal"];
					al.Targets.AddRange (parseWindowsTargets (items, () => new WindowsUniversalAppLinkTarget ()));
				}

				results.Add (uri, al);
			}

			return results;
		}

		List<IAppLinkTarget> parseiOSTargets(JsonArray json, Func<IOSAppLinkTarget> targetFactory)
		{
			var targets = new List<IAppLinkTarget> ();

			foreach (var item in json) { 
				var target = targetFactory ();

				if (item.ContainsKey ("url"))
					target.Url = new Uri ((string)item ["url"]);

				if (item.ContainsKey ("app_name"))
					target.AppName = (string)item ["app_name"];

				if (item.ContainsKey ("app_store_id"))
					target.AppName = (string)item ["app_name"];

				if (target.Url != null)
					targets.Add (target);
			}

			return targets;
		}

		List<IAppLinkTarget> parseAndroidTargets(JsonArray json)
		{
			var targets = new List<IAppLinkTarget> ();

			foreach (var item in json) { 
				var target = new AndroidAppLinkTarget ();

				if (item.ContainsKey ("url"))
					target.Url = new Uri ((string)item ["url"]);

				if (item.ContainsKey ("app_name"))
					target.AppName = (string)item ["app_name"];

				if (item.ContainsKey ("class"))
					target.AppName = (string)item ["class"];

				if (item.ContainsKey ("package"))
					target.AppName = (string)item ["package"];
					
				if (target.Url != null)
					targets.Add (target);
			}

			return targets;
		}

		List<IAppLinkTarget> parseWindowsTargets(JsonArray json, Func<WindowsAppLinkTarget> targetFactory)
		{
			var targets = new List<IAppLinkTarget> ();

			foreach (var item in json) {
				var target = targetFactory ();

				if (item.ContainsKey ("url"))
					target.Url = new Uri ((string)item ["url"]);

				if (item.ContainsKey ("app_id"))
					target.AppId = (string)item ["app_id"];

				if (item.ContainsKey ("app_name"))
					target.AppName = (string)item ["app_name"];

				if (target.Url != null)
					targets.Add (target);
			}

			return targets;
		}
	}
}

