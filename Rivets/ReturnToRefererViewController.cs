using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Rivets
{
	public class ReturnToRefererViewController : UIViewController
	{
		const float VIEW_ANIMATION_DURATION = 0.25f;

		public ReturnToRefererViewController(IntPtr handle) : base(handle)
		{
			Initialize (null);
		}

		public ReturnToRefererViewController () : base()
		{
			Initialize (null);
		}

		public ReturnToRefererViewController(UINavigationController navController) : base()
		{
			Initialize (navController);
		}

		public delegate void WillNavigateToAppLinkDelegate(AppLink appLink);
		public event WillNavigateToAppLinkDelegate OnWillNavigateToAppLink;

		public delegate void DidNavigateToAppLinkDelegate(AppLink appLink, NavigationResult result);
		public event DidNavigateToAppLinkDelegate OnDidNavigateToAppLink;

		ReturnToRefererView view;
		ReturnToRefererView ReturnToView { 
			get {
				if (view == null) {
					view = new ReturnToRefererView ();
					if (attachedToNavController != null) {
						attachedToNavController.View.AddSubview (view);
					}
				}
				return view;
			}
			set {
				if (view != null)
					view.OnReturnToRefererView -= HandleOnReturnToRefererViewAction;

				view = value;

				view.OnReturnToRefererView += HandleOnReturnToRefererViewAction;

				if (attachedToNavController != null) {
					view.IncludeStatusBarInSize = IncludeStatusBarInSize.Always;
				}
			}
		}

		async void HandleOnReturnToRefererViewAction(AppLink appLink)
		{
			if (appLink != null) {
				await OpenRefererAppLink(appLink);
				CloseViewAnimated(false);
			} else {
				CloseViewAnimated(true);
			}
		}

		UINavigationController attachedToNavController;
		List<NSObject> observers = new List<NSObject>();

		void Initialize(UINavigationController navController)
		{
			attachedToNavController = navController;

			var nc = NSNotificationCenter.DefaultCenter;

			observers.Add (nc.AddObserver (UIApplication.WillChangeStatusBarFrameNotification, StatusBarFrameWillChange));
			observers.Add (nc.AddObserver (UIApplication.DidChangeStatusBarFrameNotification, StatusBarFrameDidChange));
			observers.Add (nc.AddObserver (UIDevice.OrientationDidChangeNotification, OrientationDidChange));
		}

		void Dispose()
		{
			NSNotificationCenter.DefaultCenter.RemoveObservers (observers);

			base.Dispose ();
		}

		void ShowViewForRefererAppLink (AppLink refererAppLink)
		{
			ReturnToView.RefererAppLink = refererAppLink;

			ReturnToView.SizeToFit ();

			if (attachedToNavController != null) {
				if (ReturnToView.Closed) {
					BeginInvokeOnMainThread (() => {
						MoveNavigationBar();
					});
				}
			}
		}

		void ShowViewForRefererUrl(NSUrl url)
		{
			var apurl = new AppLinkUrl(url.ToString());
			ShowViewForRefererAppLink (apurl.Referrer);
		}

		void RemoveFromNavController ()
		{
			if (attachedToNavController != null) {
				ReturnToView.RemoveFromSuperview ();
				attachedToNavController = null;
			}
		}

		void StatusBarFrameWillChange (NSNotification notification) 
		{
			RectangleF newFrame = ((NSValue)notification.UserInfo [UIApplication.StatusBarFrameUserInfoKey]).RectangleFValue;

			if (attachedToNavController != null && !ReturnToView.Closed) {
				if (newFrame.Size.Height >= 40) {
					UIViewAnimationOptions options = UIViewAnimationOptions.BeginFromCurrentState;
					UIView.Animate (VIEW_ANIMATION_DURATION, 0f, options, () => {
						ReturnToView.Frame = new RectangleF(0, 0, ReturnToView.Frame.Size.Width, 0);
					}, null);
				}
			}
		}

		void StatusBarFrameDidChange (NSNotification notification) {

			var newFrame = ((NSValue)notification.UserInfo [UIApplication.StatusBarFrameUserInfoKey]).RectangleFValue;

			if (attachedToNavController != null && !ReturnToView.Closed) {
				if (newFrame.Size.Height >= 40) {
					UIViewAnimationOptions options = UIViewAnimationOptions.BeginFromCurrentState;
					UIView.Animate (VIEW_ANIMATION_DURATION, 0f, options, () => {
						ReturnToView.SizeToFit();
						MoveNavigationBar();
					}, null);
				}
			}
		}

		void OrientationDidChange (NSNotification notification)
		{
			if (attachedToNavController != null && !ReturnToView.Closed && ReturnToView.Frame.Size.Height > 0) {
				BeginInvokeOnMainThread (MoveNavigationBar);
			}
		}

		void MoveNavigationBar()
		{
			if (ReturnToView.Closed || ReturnToView.RefererAppLink == null) {
				return;
			}

			var oldFrame = attachedToNavController.NavigationBar.Frame;

			attachedToNavController.NavigationBar.Frame = new RectangleF(0,
				ReturnToView.Frame.Size.Height,
				attachedToNavController.NavigationBar.Frame.Size.Width,
				attachedToNavController.NavigationBar.Frame.Size.Height);

			var dy = attachedToNavController.NavigationBar.Frame.Bottom - oldFrame.Bottom;
			var navigationView = attachedToNavController.VisibleViewController.View.Superview;

			navigationView.Frame = new RectangleF(navigationView.Frame.Location.X,
				navigationView.Frame.Location.Y + dy,
				navigationView.Frame.Size.Width,
				navigationView.Frame.Size.Height - dy);

		}

		void CloseViewAnimated(bool animated) 
		{
			NSAction closer = () => {
				if (attachedToNavController != null) {
					var oldFrame = attachedToNavController.NavigationBar.Frame;

					attachedToNavController.NavigationBar.Frame = new RectangleF(0,
						ReturnToView.StatusBarHeight (),
						attachedToNavController.NavigationBar.Frame.Size.Width,
						attachedToNavController.NavigationBar.Frame.Size.Height);

					var dy = attachedToNavController.NavigationBar.Frame.Bottom - oldFrame.Bottom;
					var navigationView = attachedToNavController.VisibleViewController.View.Superview;
					navigationView.Frame = new RectangleF(navigationView.Frame.Location.X,
						navigationView.Frame.Location.Y + dy,
						navigationView.Frame.Size.Width,
						navigationView.Frame.Size.Height - dy);
				}

				ReturnToView.Frame = new RectangleF(ReturnToView.Frame.Location.X,
					ReturnToView.Frame.Location.Y,
					ReturnToView.Frame.Size.Width,
					0);
			};

			if (animated) {
				UIViewAnimationOptions options = UIViewAnimationOptions.BeginFromCurrentState;
				UIView.Animate (VIEW_ANIMATION_DURATION, 0f, options, closer, () => ReturnToView.Closed = true);
			} else {
				closer ();
				ReturnToView.Closed = true;
			}
		}

		async Task OpenRefererAppLink (AppLink refererAppLink)
		{
			if (refererAppLink != null) {
				var evt = OnWillNavigateToAppLink;
				if (evt != null)
					evt (refererAppLink);

				var navigator = new AppLinkNavigator ();
				var result = await navigator.Navigate (refererAppLink, null);

				var evt2 = OnDidNavigateToAppLink;
				if (evt2 != null)
					evt2 (refererAppLink, result);
			}
		}
	}
}

