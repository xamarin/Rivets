using System;

namespace Rivets
{
	public class AppLinks
	{
		public AppLinks ()
		{
		}

		static AppLinks()
		{
			Navigator = new AppLinkNavigator ();
			DefaultResolver = new HttpClientAppLinkResolver ();
		}

		public static readonly string Version = AssemblyInfo.AssemblyVersion;
		public static readonly string UserAgent = "Xamarin.Rivets/" + Version;

		public static IAppLinkNavigation Navigator { get; set; }
		public static IAppLinkResolver DefaultResolver { get; set; }
	}
}

