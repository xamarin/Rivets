
using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace RivetsSampleiPhone
{
	public partial class MainViewController : DialogViewController
	{
		ProductViewController productViewController;

		public MainViewController () : base (UITableViewStyle.Grouped, null)
		{
			Root = new RootElement ("Rivets Sample") {
				new Section () {
					new StyledStringElement("Show Product: widget", () => {
						productViewController = new ProductViewController("widget");
						NavigationController.PushViewController(productViewController, true);
					}),
					new StyledStringElement("App Link to widget", async () => {
						var url = "https://rawgit.com/Redth/Rivets/master/Rivets.Tests/Html/SimpleiOSMetaData.html";

						var result = await Rivets.AppLinks.Navigator.Navigate(url);

						Console.WriteLine(result);
					}),

					new StyledStringElement("App Link to widget with Referer", async () => {
						var url = "https://rawgit.com/Redth/Rivets/master/Rivets.Tests/Html/SimpleiOSMetaData.html";

						var referer = new Rivets.RefererAppLink {
							TargetUrl = new Uri(url),
							Url = new Uri("example://"),
							AppName = "Example Store"
						};

						var result = await Rivets.AppLinks.Navigator.Navigate(url, referer);

						Console.WriteLine(result);
					}),

					new StyledStringElement("Web Fallback Link", async () => {
						var url = "https://rawgit.com/Redth/Rivets/master/Rivets.Tests/Html/WebFallbackMetaData.html";

						var result = await Rivets.AppLinks.Navigator.Navigate(url);

						Console.WriteLine(result);
					})
				},
			};
		}
	}
}
