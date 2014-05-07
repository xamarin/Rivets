
using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Drawing;

namespace RivetsSampleiPhone
{
	public partial class ProductViewController : DialogViewController
	{
		public ProductViewController (string productId) : base (UITableViewStyle.Grouped, null, true)
		{
			ProductId = productId;
			Initialize ();
		}

		public ProductViewController (string productId, Rivets.AppLink refererAppLink) : base (UITableViewStyle.Grouped, null, refererAppLink == null)
		{
			ProductId = productId;
			RefererAppLink = refererAppLink;
			Initialize ();
		}

		void Initialize()
		{
			Root = new RootElement ("View Product") {
				new Section () {
					new StyledStringElement("Product ID", ProductId)
				},
			};
		}

		public string ProductId { get; private set; }
		public Rivets.AppLink RefererAppLink { get; private set; }

		Rivets.RefererViewBar refererViewBar;

		UIStatusBarStyle originalStatusBarStyle = UIApplication.SharedApplication.StatusBarStyle;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (RefererAppLink != null) {
				refererViewBar = new Rivets.RefererViewBar (this);
				refererViewBar.OnClosedRefererOverlay += () => InvokeOnMainThread (() => {

					// Remove the Referer Overlay
					refererViewBar.Remove ();

					// Go back to our main root controller since the action was closed
					NavigationController.PopViewControllerAnimated (true);
				});
			}
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (RefererAppLink != null) {

				// Make things prettier
				UIApplication.SharedApplication.SetStatusBarStyle (UIStatusBarStyle.LightContent, true);

				// Display the Referer Overlay
				refererViewBar.ShowRefererOverlay (RefererAppLink);
			}
		}

		public override void ViewWillDisappear (bool animated)
		{
			// Return statusbar to oringal style
			UIApplication.SharedApplication.SetStatusBarStyle (originalStatusBarStyle, true);

			base.ViewWillDisappear (animated);
		}
	}
}
