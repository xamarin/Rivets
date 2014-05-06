using System;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;

namespace Rivets
{
	public enum IncludeStatusBarInSize {
		Never,
		IOS7AndLater,
		Always,
	}

	public class ReturnToRefererView : UIView
	{
		const float MARGIN_X = 8.5f;
		const float MARGIN_Y = 8.5f;
		const float CLOSE_BUTTON_WIDTH = 12.0f;
		const float CLOSE_BUTTON_HEIGHT = 12.0f;

		public ReturnToRefererView() : base()
		{
			InitializeViews ();
		}

		public ReturnToRefererView(MonoTouch.Foundation.NSCoder coder) : base(coder)
		{
			InitializeViews ();
		}

		public ReturnToRefererView(IntPtr handle) : base(handle)
		{
			InitializeViews ();
		}

		public ReturnToRefererView (RectangleF frame) : base(frame)
		{
			InitializeViews ();
		}

		public delegate void ReturnToRefererViewDelegate (AppLink appLink);
		public event ReturnToRefererViewDelegate OnReturnToRefererView;

		public UIColor TextColor { 
			get { return textColor; }
			set {
				textColor = value;
				UpdateColors ();
			}
		}

		public UIColor BgColor { 
			get { return backgroundColor; }
			set {
				backgroundColor = value;
				UpdateColors ();
			}
		}

		public AppLink RefererAppLink { 
			get { return refererAppLink; }
			set {
				refererAppLink = value;
				UpdateLabelText ();
			}
		}

		public IncludeStatusBarInSize IncludeStatusBarInSize { 
			get { return includeStatusBarInSize; }
			set {
				includeStatusBarInSize = value;
				SetNeedsLayout ();
			}
		}

		AppLink refererAppLink;
		IncludeStatusBarInSize includeStatusBarInSize;
		UIColor textColor = UIColor.White;
		UIColor backgroundColor = UIColor.Blue;
		public bool Closed = false;
		UILabel labelView;
		UIButton closeButton;
		UITapGestureRecognizer insideTapGestureRecognizer;
		//UIView viewToMoveWithNavController;

		void InitializeViews()
		{

			if (labelView == null && closeButton == null) {
				closeButton = new UIButton (UIButtonType.Custom);
				closeButton.BackgroundColor = UIColor.Clear;
				closeButton.UserInteractionEnabled = true;
				closeButton.ClipsToBounds = true;
				closeButton.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleTopMargin;
				closeButton.TouchUpInside += delegate {
					var evt = OnReturnToRefererView;
					if (evt != null)
						evt(null);	
				};
				AddSubview (closeButton);


				labelView = new UILabel (RectangleF.Empty);
				labelView.Font = UIFont.SystemFontOfSize (UIFont.SmallSystemFontSize);
				labelView.TextColor = UIColor.White;
				labelView.BackgroundColor = UIColor.Clear;
				labelView.TextAlignment = UITextAlignment.Center;
				labelView.ClipsToBounds = true;
				labelView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;

				UpdateLabelText();

				AddSubview(labelView);


				insideTapGestureRecognizer = new UITapGestureRecognizer (gr => {
					var evt = OnReturnToRefererView;
					if (evt != null)
						evt(RefererAppLink);
				});
				labelView.UserInteractionEnabled = true;
				labelView.AddGestureRecognizer (insideTapGestureRecognizer);

				UpdateColors ();
			}
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

			var bounds = this.Bounds;

			var labelSize = labelView.SizeThatFits (bounds.Size);
			labelView.PreferredMaxLayoutWidth = labelView.Bounds.Size.Width;
			labelView.Frame = new RectangleF(MARGIN_X,
				bounds.Bottom - labelSize.Height - MARGIN_Y,
				bounds.Right - CLOSE_BUTTON_WIDTH - 3 * MARGIN_X,
				labelSize.Height);

			closeButton.Frame = new RectangleF(bounds.Right - CLOSE_BUTTON_WIDTH - MARGIN_X,
				labelView.Center.Y - CLOSE_BUTTON_HEIGHT / 2.0f,
				CLOSE_BUTTON_WIDTH,
				CLOSE_BUTTON_HEIGHT);
		}

		public override SizeF SizeThatFits (SizeF size)
		{
			if (Closed || !HasRefererData) {
				return new SizeF(size.Width, 0.0f);
			}

			var labelSize = labelView.SizeThatFits(size);
			return new SizeF(size.Width, labelSize.Height + 2 * MARGIN_X + StatusBarHeight());

		}

		public float StatusBarHeight()
		{
			var app = UIApplication.SharedApplication;
			var is7orHigher = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);

			var include = (IncludeStatusBarInSize == IncludeStatusBarInSize.IOS7AndLater && is7orHigher) ||
				IncludeStatusBarInSize == IncludeStatusBarInSize.Always;
			if (include && !app.StatusBarHidden) {
				var landscape = app.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft
				                || app.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight;
				return landscape ? app.StatusBarFrame.Size.Width : app.StatusBarFrame.Size.Height;
			}

			return 0;
		}




	
		void UpdateLabelText()
		{
			var appName = string.Empty;

			if (RefererAppLink != null && RefererAppLink.Targets != null && RefererAppLink.Targets.Any ())
				appName = RefererAppLink.Targets [0].AppName;

			labelView.Text = LocalizedLabelForReferer (appName);
		}

		void UpdateColors ()
		{
			var closeButtonImage = DrawCloseButtonImageWithColor(textColor ?? UIColor.White);


			labelView.TextColor = textColor;
			BackgroundColor = backgroundColor;

			closeButton.SetBackgroundImage(closeButtonImage, UIControlState.Normal);
		}

		UIImage DrawCloseButtonImageWithColor(UIColor color)
		{
			UIGraphics.BeginImageContextWithOptions (new SizeF (CLOSE_BUTTON_WIDTH, CLOSE_BUTTON_HEIGHT), false, 0.0f);

			var context = UIGraphics.GetCurrentContext ();

			context.SetStrokeColor (color.CGColor);
			context.SetFillColor (color.CGColor);
			context.SetLineWidth (1.25f);

			var inset = 0.5f;

			context.MoveTo (inset, inset);
			context.AddLineToPoint (CLOSE_BUTTON_WIDTH - inset, CLOSE_BUTTON_HEIGHT - inset);
			context.StrokePath ();

			context.MoveTo (CLOSE_BUTTON_WIDTH - inset, inset);
			context.AddLineToPoint (inset, CLOSE_BUTTON_HEIGHT - inset);
			context.StrokePath ();

			var result = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return result;
		}

		string LocalizedLabelForReferer(string refererName)
		{
			return string.Format ("Touch to return to {0}", refererName ?? "previous app");
		}

		public bool HasRefererData {
			get { return refererAppLink != null && refererAppLink.Targets != null && refererAppLink.Targets.Any (); }
		}

//		void closeButtonTapped:(id)sender {
//			[_delegate returnToRefererViewDidTapInsideCloseButton:self];
//		}
//
//		- (void)onTapInside:(UIGestureRecognizer*)sender {
//			[_delegate returnToRefererViewDidTapInsideLink:self link:_refererAppLink];
//		}
	}
}

