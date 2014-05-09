using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace RivetsSampleiPhone
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		UINavigationController navController;
		MainViewController mainController;
		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			mainController = new MainViewController ();
			navController = new UINavigationController (mainController);
			// If you have defined a root view controller, set it here:
			window.RootViewController = navController;
			
			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}

		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			var rurl = new Rivets.AppLinkUrl (url.ToString ());

			var id = string.Empty;

			if (rurl.InputQueryParameters.ContainsKey("id"))
				id = rurl.InputQueryParameters ["id"];

			if (rurl.InputUrl.Host.Equals ("products") && !string.IsNullOrEmpty (id)) {
				var c = new ProductViewController (id, rurl.Referrer);
				navController.PushViewController (c, true);
				return true;
			} else {
				navController.PopToRootViewController (true);
				return true;
			}
		}
	}
}

