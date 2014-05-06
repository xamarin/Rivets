using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rivets
{
	public interface IAppLinkTarget
	{
		Uri Url { get; set; }
		string AppName { get;set; }
	}

	public class AndroidAppLinkTarget : IAppLinkTarget
	{
		public Uri Url { get;set; }
		public string AppName { get;set; }
		public string Class { get;set; }
		public string Package { get;set; }
	}

	public class WindowsPhoneAppLinkTarget : IAppLinkTarget
	{
		public Uri Url { get;set; }
		public string AppName { get;set; }
		public string AppId { get;set; }
	}

	public class IOSAppLinkTarget : IAppLinkTarget
	{
		public Uri Url { get;set; }
		public string AppName { get;set; }
		public string AppStoreId { get;set; }
	}

	public class IPhoneAppLinkTarget : IOSAppLinkTarget
	{
	}

	public class IPadAppLinkTarget : IOSAppLinkTarget
	{
	}

	public class RefererAppLink
	{
		[JsonProperty("target_url")]
		public Uri TargetUrl { get;set; }

		[JsonProperty("url")]
		public Uri Url { get;set; }

		[JsonProperty("app_name")]
		public string AppName { get;set; }

		[JsonProperty("app_store_id")]
		public string AppStoreId { get;set; }
	}
}

