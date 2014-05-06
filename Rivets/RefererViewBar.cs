using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using System.Collections.Generic;

namespace Rivets
{
	public class RefererViewBar
	{
		public RefererViewBar (UIViewController parentController)
		{
			initialize (parentController);
		}

		public delegate void NavigatedToAppLinkDelegate(AppLink appLink, NavigationResult result);
		public event NavigatedToAppLinkDelegate OnNavigatedToAppLink;

		public delegate void ClosedRefererOverlayDelegate();
		public event ClosedRefererOverlayDelegate OnClosedRefererOverlay;

		const float BAR_HEIGHT = 32f;
		const float BUTTON_WIDTH = 32f;

		List<NSObject> observers = new List<NSObject> ();

		void initialize(UIViewController controller)
		{
			var nav = controller as UINavigationController;

			if (nav == null && controller.NavigationController != null)
				nav = controller.NavigationController;

			if (nav != null) {
				navBar = nav.NavigationBar;
				navController = nav;
			}

			var nc = NSNotificationCenter.DefaultCenter;

			observers.Add (nc.AddObserver (UIApplication.WillChangeStatusBarFrameNotification, StatusBarFrameWillChange));
			observers.Add (nc.AddObserver (UIApplication.DidChangeStatusBarFrameNotification, StatusBarFrameDidChange));
			observers.Add (nc.AddObserver (UIDevice.OrientationDidChangeNotification, OrientationDidChange));

			rootView = new UIView (controller.View.Bounds);
			rootView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

			controllerView = controller.View;
			controllerView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			if (controllerView.Superview != null)
				controllerView.RemoveFromSuperview ();

			rootView.AddSubview (controllerView);

			var statusBarHeight = GetStatusBarHeight ();

			baseView = new UIView (new RectangleF (0, 0, controller.View.Bounds.Width, BAR_HEIGHT + statusBarHeight));


			labelText = new UILabel (new RectangleF (0, statusBarHeight, baseView.Frame.Width - BUTTON_WIDTH, BAR_HEIGHT));
			labelText.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			labelText.Text = "Tap to return to previous app";
			labelText.Font = UIFont.SystemFontOfSize (UIFont.SmallSystemFontSize);
			labelText.TextAlignment = UITextAlignment.Center;
			labelText.UserInteractionEnabled = true;

			buttonClose = new UIButton (UIButtonType.Custom);
			buttonClose.Frame = new RectangleF (labelText.Frame.Right, statusBarHeight, BUTTON_WIDTH, BUTTON_WIDTH);
			buttonClose.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin;

			baseView.AddSubviews (labelText, buttonClose);

			//baseView.Frame = new RectangleF (baseView.Frame.X, baseView.Frame.Y, baseView.Frame.Width, 0f);
			controller.View = rootView;

			controller.View.AddSubview (baseView);
			controller.View.BringSubviewToFront (baseView);

			if (navController != null && navBar != null) {

				navBar.ClipsToBounds = false;
				navController.View.ClipsToBounds = false;

				navController.View.AddSubview (baseView);
				navController.View.BringSubviewToFront (baseView);
			}
			else
				rootView.AddSubview (baseView);
	
			AttachedToController = controller;


			buttonClose.TouchUpInside += delegate {

				var evt = OnClosedRefererOverlay;
				if (evt != null)
					evt();
			};

			tapGestureLabel = new UITapGestureRecognizer (async g => {
				await OpenRefererAppLink(RefererLink);
			});
			labelText.AddGestureRecognizer (tapGestureLabel);

			UpdateColors ();
		}

		UIColor bgColor = UIColor.Gray;
		UIColor fgColor = UIColor.White;

		public UIColor BackgroundColor {
			get { return bgColor; }
			set {
				bgColor = value;
				UpdateColors ();
			}
		}

		public UIColor ForegroundColor {
			get { return fgColor; }
			set {
				fgColor = value;
				UpdateColors ();
			}
		}

		UITapGestureRecognizer tapGestureLabel;

		UINavigationController navController;
		UINavigationBar navBar;

		UIView controllerView;
		UIView rootView;
		UIView baseView;
		UILabel labelText;
		UIButton buttonClose;

		public UIViewController AttachedToController { get; private set; }

		public AppLink RefererLink { get; private set; }

		public void ShowRefererOverlay(AppLink refererLink)
		{
			RefererLink = refererLink;

			if (RefererLink != null)
				Layout ();
		}

		void UpdateColors()
		{
			labelText.TextColor = fgColor;
			buttonClose.SetBackgroundImage (DrawCloseButtonImageWithColor (fgColor), UIControlState.Normal);
			baseView.BackgroundColor = bgColor;
		}

		UIImage DrawCloseButtonImageWithColor(UIColor color)
		{
			UIGraphics.BeginImageContextWithOptions (new SizeF (BUTTON_WIDTH, BUTTON_WIDTH), false, 0.0f);

			var context = UIGraphics.GetCurrentContext ();

			context.SetStrokeColor (color.CGColor);
			context.SetFillColor (color.CGColor);
			context.SetLineWidth (1.25f);

			var inset = 20.5f;

			context.MoveTo (inset, inset);
			context.AddLineToPoint (BUTTON_WIDTH- inset, BUTTON_WIDTH - inset);
			context.StrokePath ();

			context.MoveTo (BUTTON_WIDTH - inset, inset);
			context.AddLineToPoint (inset, BUTTON_WIDTH - inset);
			context.StrokePath ();

			var result = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return result;
		}

		void Layout()
		{
			if (RefererLink == null)
				return;

			var statusBarHeight = GetStatusBarHeight(); 

			if (navBar != null) {
				navBar.Frame = new RectangleF (navBar.Frame.X, BAR_HEIGHT + statusBarHeight, navBar.Frame.Width, navBar.Frame.Height);
			}

			controllerView.Frame = new RectangleF (rootView.Bounds.X, BAR_HEIGHT, rootView.Bounds.Width, rootView.Bounds.Height - BAR_HEIGHT);

			baseView.Frame = new RectangleF (baseView.Frame.X, baseView.Frame.Y, controllerView.Bounds.Width, BAR_HEIGHT + statusBarHeight);
			baseView.Superview.BringSubviewToFront (baseView);
		}

		public void Hide()
		{
			var statusBarHeight = GetStatusBarHeight ();

			if (navBar != null) {
				navBar.Frame = new RectangleF (navBar.Frame.X, rootView.Frame.Y + statusBarHeight, navBar.Frame.Width, navBar.Frame.Height);
			}

			baseView.Frame = new RectangleF (baseView.Frame.X, baseView.Frame.Y, baseView.Frame.Width, 0f);
			controllerView.Frame = new RectangleF (controllerView.Frame.X, rootView.Frame.Y, controllerView.Frame.Width, rootView.Frame.Height);
		}

		public void Remove()
		{
			Hide ();

			baseView.RemoveFromSuperview ();
			controllerView.RemoveFromSuperview ();
			AttachedToController.View = controllerView;
		}

		float GetStatusBarHeight()
		{
			var statusBarHeight = 0f; 

			if (UIDevice.CurrentDevice.CheckSystemVersion (7, 0) && !UIApplication.SharedApplication.StatusBarHidden) {
				statusBarHeight = UIApplication.SharedApplication.StatusBarFrame.Height;

				if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft
				    || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight)
					statusBarHeight = UIApplication.SharedApplication.StatusBarFrame.Width;
			}

			return statusBarHeight;
		}

		void StatusBarFrameWillChange (NSNotification notification) 
		{
			//Layout ();
		}

		void StatusBarFrameDidChange (NSNotification notification) 
		{
			//Layout ();
		}

		void OrientationDidChange (NSNotification notification)
		{
			Layout ();
		}

		async Task OpenRefererAppLink (AppLink refererAppLink)
		{
			if (refererAppLink != null) {

				Remove ();

				var navigator = new AppLinkNavigator ();
				var result = await navigator.Navigate (refererAppLink, null);

				var evt2 = OnNavigatedToAppLink;
				if (evt2 != null)
					evt2 (refererAppLink, result);
			}
		}
	}
}

