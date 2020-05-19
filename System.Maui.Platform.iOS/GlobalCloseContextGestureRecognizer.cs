using Foundation;
using UIKit;
using NSAction = System.Action;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.iOS
{
	internal class GlobalCloseContextGestureRecognizer : UIGestureRecognizer
	{
		UIScrollView _scrollView;

		public GlobalCloseContextGestureRecognizer(UIScrollView scrollView, NSAction activated) : base(activated)
		{
			_scrollView = scrollView;

			ShouldReceiveTouch = OnShouldReceiveTouch;
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			State = UIGestureRecognizerState.Began;
			base.TouchesBegan(touches, evt);
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			State = UIGestureRecognizerState.Recognized;
			base.TouchesEnded(touches, evt);
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			State = UIGestureRecognizerState.Recognized;
			base.TouchesMoved(touches, evt);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				_scrollView = null;
			}
		}

		bool OnShouldReceiveTouch(UIGestureRecognizer r, UITouch t)
		{
			var scrollPos = t.LocationInView(_scrollView);
			var rect = new RectangleF(0, 0, _scrollView.ContentSize.Width, _scrollView.ContentSize.Height);
			return !rect.Contains(scrollPos);
		}
	}
}