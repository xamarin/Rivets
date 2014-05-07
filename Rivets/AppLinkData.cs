using System;

namespace Rivets
{
	public class AppLinkData
	{
		public AppLinkData ()
		{
			Version = AppLinks.Version;
			UserAgent = AppLinks.UserAgent;
		}

		//target_url
		public string TargetUrl { get;set; }

		//version
		public string Version { get;set; }

		//user_agent
		public string UserAgent { get;set; }

		//extras
		public System.Json.JsonValue Extras { get;set; }
	}
}

