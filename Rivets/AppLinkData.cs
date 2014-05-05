using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rivets
{
	public class AppLinkData
	{
		public AppLinkData ()
		{
			Version = "1.0";
			UserAgent = "Rivets.NET 1.0";
		}

		[JsonProperty("target_url")]
		public string TargetUrl { get;set; }

		[JsonProperty("version")]
		public string Version { get;set; }

		[JsonProperty("user_agent")]
		public string UserAgent { get;set; }

		[JsonProperty("extras")]
		public JObject Extras { get;set; }
	}
}

