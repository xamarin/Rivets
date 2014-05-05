using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
#if !PORTABLE
using System.Net.Http;
#endif
using System.Text.RegularExpressions;

namespace Rivets
{
	public class HttpClientAppLinkResolver : IAppLinkResolver
	{
		const string META_TAG_REGEX = @"<\s{0,}meta\s{0,}((property\s{0,}=\s{0,}('|"")(?<property>[^\""\']{1,})('|"")\s{1,1})|(content\s{0,}=('|"")(?<content>[^\""\']{0,})('|"")\s{1,})){1,2}\s{0,}/{0,1}>";

		const string META_TAG_PREFIX = "al";
		const string KEY_AL_VALUE = "value";
		const string KEY_APP_NAME = "app_name";
		const string KEY_APP_ID = "app_id";
		const string KEY_URL = "url";
		const string KEY_APP_STORE_ID = "app_store_id";
		const string KEY_SHOULD_FALLBACK = "should_fallback";
		const string KEY_WEB_URL = "url";
		const string KEY_WEB = "web";
		const string KEY_IOS = "ios";
		const string KEY_IPHONE = "iphone";
		const string KEY_IPAD = "ipad";
		const string KEY_WINDOWSPHONE = "windows_phone";

		const string KEY_ANDROID_PACKAGE = "al:android:package";
		const string KEY_ANDROID_CLASS = "al:android:class";
		const string KEY_ANDROID_URL = "al:android:url";
		const string KEY_ANDROID_APPNAME = "al:android:app_name";

		const string KEY_WEB_WEB_URL = "al:web:url";
		const string KEY_WEB_SHOULD_FALLBACK = "al:web:should_fallback";

		public HttpClientAppLinkResolver ()
		{
		}

		#if PORTABLE
		public async Task<AppLink> ResolveAppLinks (string url)
		{
			var uri = new Uri (url);
			return await ResolveAppLinks (uri);
		}

		public async Task<AppLink> ResolveAppLinks (Uri url)
		{
			throw new NotImplementedException ("You can't call ResolveAppLink from the Portable Library");
		}
		#endif

		#if !PORTABLE
		public async Task<AppLink> ResolveAppLinks (string url)
		{
			var uri = new Uri (url);
			return await ResolveAppLinks (uri);
		}

		public async Task<AppLink> ResolveAppLinks (Uri url)
		{
			var metadata = await GetAlMetaData (url.ToString ());
			var targets = new List<IAppLinkTarget> ();

			// ANDROID
			AddAndroidTargets (metadata, targets);

			// iPhone
			AddiOSTargets (metadata, KEY_IPHONE, targets);

			// iPad
			AddiOSTargets (metadata, KEY_IPAD, targets);

			// iOS
			AddiOSTargets (metadata, KEY_IOS, targets);

			// Windows Phone
			AddWindowsTargets (metadata, KEY_WINDOWSPHONE, targets);


			var shouldFallback = true; //Default is true in AppLinks spec
			Uri webUrl = null;

			var metaWebFallback = metadata.FirstOrDefault (m => m.Property.Equals (KEY_WEB_WEB_URL));
			if (metaWebFallback != null) {
				try { webUrl = new Uri(metaWebFallback.Content); }
				catch { }
			}

			var metaShouldFallback = metadata.FirstOrDefault (m => m.Property.Equals (KEY_WEB_SHOULD_FALLBACK));
			if (metaShouldFallback != null) {
				bool should = true;
				if (bool.TryParse (metaShouldFallback.Content, out should))
					shouldFallback = should;
			}

			return new AppLink {
				Targets = targets,
				WebUrl = shouldFallback ? webUrl : null,
				SourceUrl = url
			};
		}

		void AddiOSTargets(List<MetaData> metadata, string platform, List<IAppLinkTarget> targets)
		{
			var urls = metadata.Where (m => m.Property.Equals (META_TAG_PREFIX + ":" + platform + ":" + KEY_URL, StringComparison.InvariantCultureIgnoreCase));
			var appStoreIds = metadata.Where (m => m.Property.Equals (META_TAG_PREFIX + ":" + platform + ":" + KEY_APP_STORE_ID, StringComparison.InvariantCultureIgnoreCase));
			var appNames =  metadata.Where (m => m.Property.Equals (META_TAG_PREFIX + ":" + platform + ":" + KEY_APP_NAME, StringComparison.InvariantCultureIgnoreCase));

			if (urls != null) {
				for (int i = 0; i < urls.Count(); i++) {

					var target = new IOSAppLinkTarget ();

					if (platform == KEY_IPHONE)
						target = new IPhoneAppLinkTarget ();
					else if (platform == KEY_IPAD)
						target = new IPadAppLinkTarget ();

					try {
						target.Url = new Uri(urls.ElementAt (i).Content);
					} catch {
						continue;
					}

					if (appStoreIds != null) {
						var m = appStoreIds.ElementAtOrDefault (i);
						target.AppStoreId = m != null ? m.Content : null;
					}
					if (appNames != null) {
						var m = appNames.ElementAtOrDefault(i);
						target.AppName = m != null ? m.Content : null;
					}

					targets.Add (target);
				}
			}
		}

		void AddAndroidTargets(List<MetaData> metadata, List<IAppLinkTarget> targets)
		{
			var packages = metadata.Where (m => m.Property.Equals (KEY_ANDROID_PACKAGE, StringComparison.InvariantCultureIgnoreCase));
			var classes = metadata.Where (m => m.Property.Equals (KEY_ANDROID_CLASS, StringComparison.InvariantCultureIgnoreCase));
			var urls =  metadata.Where (m => m.Property.Equals (KEY_ANDROID_URL, StringComparison.InvariantCultureIgnoreCase)); 
			var appNames = metadata.Where (m => m.Property.Equals (KEY_ANDROID_APPNAME, StringComparison.InvariantCultureIgnoreCase));

			// Package is the only required property, so we'll use it to determine the count of items to go through
			if (packages != null) {
				for (int i = 0; i < packages.Count(); i++) {
					var target = new AndroidAppLinkTarget ();

					var p = packages.ElementAtOrDefault (i);
					if (p == null || string.IsNullOrEmpty(p.Content))
						continue;
					target.Package = p.Content;

					if (classes != null) {
						var m = classes.ElementAtOrDefault (i);
						target.Class = m != null ? m.Content : null;
					}
					if (urls != null) {
						var m = urls.ElementAtOrDefault (i);
						try {
							target.Url = m != null ? new Uri(m.Content) : null;
						} catch {
						}
					}
					if (appNames != null) {
						var m = appNames.ElementAtOrDefault (i);
						target.AppName = m != null ? m.Content : null;
					}

					targets.Add (target);
				}
			}
		}

		void AddWindowsTargets(List<MetaData> metadata, string platform, List<IAppLinkTarget> targets)
		{
			var urls = metadata.Where (m => m.Property.Equals (META_TAG_PREFIX + ":" + platform + ":" + KEY_URL, StringComparison.InvariantCultureIgnoreCase));
			var appIds = metadata.Where (m => m.Property.Equals (META_TAG_PREFIX + ":" + platform + ":" + KEY_APP_ID, StringComparison.InvariantCultureIgnoreCase));
			var appNames =  metadata.Where (m => m.Property.Equals (META_TAG_PREFIX + ":" + platform + ":" + KEY_APP_NAME, StringComparison.InvariantCultureIgnoreCase));

			if (urls != null) {
				for (int i = 0; i < urls.Count(); i++) {

					var target = new WindowsPhoneAppLinkTarget ();

					try {
						target.Url = new Uri(urls.ElementAt(i).Content);
					} catch {
						continue;
					}

					if (appIds != null) {
						var m = appIds.ElementAtOrDefault (i);
						target.AppId = m != null ? m.Content : null;
					}
					if (appNames != null) {
						var m = appIds.ElementAtOrDefault (i);
						target.AppName = m != null ? m.Content : null;
					}

					targets.Add (target);
				}
			}
		}
		#endif


		#if !PORTABLE

		public async Task<List<MetaData>> GetAlMetaData(string url)
		{
			var results = new List<MetaData> ();

			var http = new HttpClient ();

			// AppLinks defines a header "Prefer-Html-Meta-Tags"
			// which if the server obeys, it will only send down 
			// the meta tags from the html document (and only the applinks ones)
			http.DefaultRequestHeaders.Add ("Prefer-Html-Meta-Tags", "al");

			var resp = await http.GetAsync (url);

			resp.EnsureSuccessStatusCode ();

			var html = await resp.Content.ReadAsStringAsync ();

			var matches = Regex.Matches (html, META_TAG_REGEX);


			foreach (Match m in matches) {
				if (m.Groups ["property"] == null || !m.Groups["property"].Success)
					continue;

				var property = m.Groups ["property"].Value;
				var content = string.Empty;

				if (m.Groups["content"] != null && m.Groups["content"].Success)
					content = m.Groups ["content"].Value;

				if (string.IsNullOrEmpty (property) || !property.Contains (":"))
					continue;

				if (property.StartsWith (META_TAG_PREFIX, StringComparison.InvariantCultureIgnoreCase)) {
					var meta = new MetaData();
					meta.Property = property;
					if (!string.IsNullOrEmpty(content))
						meta.Content = content;

					results.Add (meta);
				}
			}

			return results;
		}

		#endif


		public class MetaData 
		{ 
			public string Property { get;set; }
			public string Content { get;set; }
		}
	}
}

 