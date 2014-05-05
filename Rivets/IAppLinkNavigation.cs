using System;
using System.Threading.Tasks;

namespace Rivets
{
	public interface IAppLinkNavigation
	{
		Task<NavigationResult> Navigate(AppLink appLink, AppLinkData appLinkData);

		Task<NavigationResult> Navigate(Uri url, AppLinkData appLinkData);

		Task<NavigationResult> Navigate(string url, AppLinkData appLinkData);
	}
}

