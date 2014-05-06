
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

		Rivets.ReturnToRefererViewController returnToRefererController;

		UIStatusBarStyle originalStatusBarStyle = UIApplication.SharedApplication.StatusBarStyle;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			returnToRefererController = new Rivets.ReturnToRefererViewController (NavigationController);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (RefererAppLink != null) {

				UIApplication.SharedApplication.SetStatusBarStyle (UIStatusBarStyle.LightContent, true);
				returnToRefererController.ShowViewForRefererAppLink (RefererAppLink);
			}
		}

		public override void ViewWillDisappear (bool animated)
		{
			UIApplication.SharedApplication.SetStatusBarStyle (originalStatusBarStyle, true);
			base.ViewWillDisappear (animated);
		}

		public override UIStatusBarStyle PreferredStatusBarStyle ()
		{
			return RefererAppLink != null ? UIStatusBarStyle.LightContent : UIStatusBarStyle.Default;
		}
	}
}
