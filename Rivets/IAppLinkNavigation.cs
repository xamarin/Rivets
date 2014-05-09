using System;
using System.Threading.Tasks;
using System.Json;

namespace Rivets
{
	public interface IAppLinkNavigation
	{

		/// <summary>
		/// Navigates to the specified AppLink without first resolving AppLink MetaData from a page
		/// </summary>
		/// <param name="appLink">App Link.</param>
		Task<NavigationResult> Navigate(AppLink appLink);

		/// <summary>
		/// Navigate the specified url.
		/// </summary>
		/// <param name="url">URL.</param>
		Task<NavigationResult> Navigate(Uri url);

		Task<NavigationResult> Navigate(string url);

		Task<NavigationResult> Navigate(AppLink appLink, RefererAppLink refererAppLink);

		Task<NavigationResult> Navigate(Uri url, RefererAppLink refererAppLink);

		Task<NavigationResult> Navigate(string url, RefererAppLink refererAppLink);

		Task<NavigationResult> Navigate(AppLink appLink, JsonObject extras);

		Task<NavigationResult> Navigate(Uri url, JsonObject extras);

		Task<NavigationResult> Navigate(string url, JsonObject extras);

		Task<NavigationResult> Navigate(AppLink appLink, JsonObject extras, RefererAppLink refererAppLink);

		Task<NavigationResult> Navigate(Uri url, JsonObject extras, RefererAppLink refererAppLink);

		Task<NavigationResult> Navigate(string url, JsonObject extras, RefererAppLink refererAppLink);

		event WillNavigateToWebUrlDelegate WillNavigateToWebUrl;
	}

	public delegate void WillNavigateToWebUrlDelegate(object sender, WebNavigateEventArgs e);

	public class WebNavigateEventArgs
	{
		public WebNavigateEventArgs(Uri webUrl)
		{
			Handled = false;
			WebUrl = webUrl;
		}

		public Uri WebUrl { get; private set; }
		public bool Handled { get; set; }
	}
}

