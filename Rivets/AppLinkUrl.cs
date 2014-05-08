using System;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Json;

namespace Rivets
{
	public class AppLinkUrl
	{
		public AppLinkUrl (Uri url, string apLinkDataJson)
		{
			Initialize (url, apLinkDataJson);
		}

		public AppLinkUrl(string url, string apLinkDataJson)
		{
			Initialize (new Uri (url), apLinkDataJson);
		}

		public AppLinkUrl (Uri url)
		{
			Initialize (url, null);
		}

		public AppLinkUrl(string url)
		{
			Initialize (new Uri (url), null);
		}

		#if PORTABLE
		void Initialize(Uri url, string appLinkDataJson)
		{
			throw new NotImplementedException ("Not Implemented in Portable Class Library.  Use a platform specific assembly instead.");
		}
		#else
		void Initialize(Uri url, string appLinkDataJson)
		{
			#if __ANDROID__
			if (string.IsNullOrEmpty(appLinkDataJson))
				throw new ArgumentNullException("apLinkDataJson", "On Android you should pass the 'Intent.GetStringExtra(\"ap_applink_data\")' contents into the ctor");
			#endif

			InputUrl = url;
			TargetUrl = url;

			TargetQueryParameters = Utility.ParseQueryString(url.Query);
			InputQueryParameters = Utility.ParseQueryString(url.Query);

			var appLinkData = string.Empty;

			if (!string.IsNullOrEmpty (appLinkDataJson)) {
				appLinkData = appLinkDataJson;
			} else {
				if (InputQueryParameters.ContainsKey ("al_applink_data"))
					appLinkData = InputQueryParameters ["al_applink_data"];
			}

			if (!string.IsNullOrEmpty (appLinkData)) {

				JsonValue json = null;

				try {
					json = JsonObject.Parse(appLinkData);
				} catch (Exception ex) {
					Debug.WriteLine (ex);
				}

				if (json != null) {

					if (json.ContainsKey("version"))
						AppLinksVersion = (string)json ["version"];

					if (json.ContainsKey ("extras"))
						Extras = json ["extras"] ?? new JsonObject();

					if (json.ContainsKey ("user_agent"))
						UserAgent = (string)json ["user_agent"];
						
					// Try to get the target url from the applink data
					try { 
						TargetUrl = new Uri((string)json["target_url"]);
						TargetQueryParameters = Utility.ParseQueryString(TargetUrl.Query);
					}
					catch { 
						TargetUrl = url; 
						TargetQueryParameters = InputQueryParameters;
					}

					var refererData = string.Empty;
					if (InputQueryParameters.ContainsKey("referer_app_link"))
						refererData = InputQueryParameters ["referer_app_link"];

					if (!string.IsNullOrEmpty (refererData)) {

						try {
							var jsonReferer = JsonObject.Parse(refererData);

							var referrerUrl = new Uri((string)jsonReferer["url"]);
							var referrerAppName = (string)jsonReferer["app_name"];
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
		#endif

		public Uri TargetUrl { get; private set; }
		public Dictionary<string, string> TargetQueryParameters { get; private set; }
		public AppLink Referrer { get; private set; }
		public Uri InputUrl { get; private set; }
		public Dictionary<string, string> InputQueryParameters { get; private set; }
		public string AppLinksVersion { get; private set; }
		public string UserAgent { get; private set; }
		public JsonValue Extras { get; private set; }
	}
}

