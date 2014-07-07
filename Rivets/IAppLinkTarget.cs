using System;
using System.Collections.Generic;

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

	public class WindowsAppLinkTarget : IAppLinkTarget
	{
		public Uri Url { get;set; }
		public string AppName { get;set; }
		public string AppId { get;set; }
	}

	public class WindowsPhoneAppLinkTarget : WindowsAppLinkTarget
	{
	}

	public class WindowsUniversalAppLinkTarget : WindowsAppLinkTarget
	{
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
		//target_url
		public Uri TargetUrl { get;set; }

		//url
		public Uri Url { get;set; }

		//app_name
		public string AppName { get;set; }

		//app_store_id
		public string AppStoreId { get;set; }
	}
}

