using System;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics;


#if !PORTABLE
using Newtonsoft.Json.Linq;
#endif
using System.Collections.Generic;

namespace Rivets
{
	public class AppLinkUrl
	{
		public AppLinkUrl (Uri url)
		{
			Initialize (url);
		}

		public AppLinkUrl(string url)
		{
			Initialize (new Uri (url));
		}

		#if PORTABLE
		void Initialize(Uri url)
		{
			throw new NotImplementedException ("Not Implemented in Portable Class Library.  Use a platform specific assembly instead.");
		}
		#else
		void Initialize(Uri url)
		{
			InputUrl = url;
			TargetUrl = url;

			var targetQueryParameters = System.Web.HttpUtility.ParseQueryString (url.Query);
			TargetQueryParameters = new Dictionary<string, string> ();
			NameValueCollectionToDictionary (targetQueryParameters, TargetQueryParameters);

			InputQueryParameters = new Dictionary<string, string> ();
			var inputQueryParameters = System.Web.HttpUtility.ParseQueryString (url.Query);
			NameValueCollectionToDictionary (inputQueryParameters, InputQueryParameters);

			var appLinkData = InputQueryParameters ["ap_applink_data"];

			if (!string.IsNullOrEmpty (appLinkData)) {
				var json = System.Web.HttpUtility.UrlDecode (appLinkData);

				AppLinkData = DeserializeAppLinkData (json);

				if (AppLinkData != null) {

					// Try to get the target url from the applink data
					try { 
						TargetUrl = new Uri(AppLinkData.TargetUrl); 
						if (!string.IsNullOrEmpty(TargetUrl.Query)) {
							var newTargetQueryParameters = System.Web.HttpUtility.ParseQueryString(TargetUrl.Query);

							if (newTargetQueryParameters != null && newTargetQueryParameters.Count > 0) {
								TargetQueryParameters = new Dictionary<string, string>();
								NameValueCollectionToDictionary(newTargetQueryParameters, TargetQueryParameters);
							}
						}
					}
					catch { 
						TargetUrl = url; 
						TargetQueryParameters = InputQueryParameters;
					}

					var referrerData = InputQueryParameters ["referer_app_link"];
					if (!string.IsNullOrEmpty (referrerData)) {

						try {
							var jsonReferer = JObject.Parse (referrerData);
							var referrerUrl = new Uri(jsonReferer["url"].ToString());
							var referrerAppName = jsonReferer["app_name"].ToString();
							// According to specs, the app store id shouldn't get passed in the referrer
							//var referrerAppStoreId = jsonReferer["app_store_id"].ToString();

							// Create a new AppLink object with the target set to the referrer info
							Referrer = new AppLink {
								SourceUrl = referrerUrl,
								Targets = new List<IAppLinkTarget> {
									new IOSAppLinkTarget {
										AppName = referrerAppName,
										Url = referrerUrl
									}
								}
							};
						} catch (Exception ex) {
							Debug.WriteLine (ex);
						}	
					}
				}
			}

		}

		AppLinkData DeserializeAppLinkData(string appLinkDataJson)
		{
			AppLinkData result = null;

			try {
				result = (AppLinkData)Newtonsoft.Json.JsonConvert.DeserializeObject(appLinkDataJson);
			}catch (Exception ex) {
				Debug.WriteLine (ex);
			}

			return result;
		}

		void NameValueCollectionToDictionary(NameValueCollection query, Dictionary<string, string> dict)
		{
			foreach (var key in query.AllKeys) {
				if (dict.ContainsKey (key))
					dict [key] = query [key];
				else
					dict.Add (key, query [key]);
			}
		}
		#endif

		public Uri TargetUrl { get; private set; }
		public Dictionary<string, string> TargetQueryParameters { get; private set; }
		public AppLinkData AppLinkData { get; private set; }
		public AppLink Referrer { get; private set; }
		public Uri InputUrl { get; private set; }
		public Dictionary<string, string> InputQueryParameters { get; private set; }
	}
}

