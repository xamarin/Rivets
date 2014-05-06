using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rivets
{
	public class AppLinkData
	{
		public AppLinkData ()
		{
			Version = AppLinks.Version;
			UserAgent = AppLinks.UserAgent;
		}

		[JsonProperty("target_url")]
		public string TargetUrl { get;set; }

		[JsonProperty("version")]
		public string Version { get;set; }

		[JsonProperty("user_agent")]
		public string UserAgent { get;set; }

		[JsonProperty("extras")]
		public JValue Extras { get;set; }
	}
}

