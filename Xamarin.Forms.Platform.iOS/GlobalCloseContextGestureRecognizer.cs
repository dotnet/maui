using System.Collections.Generic;
using System.Drawing;
#if __UNIFIED__
using Foundation;
using UIKit;
#else
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif
#if __UNIFIED__
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;
using NSAction = System.Action;

#else
using nfloat=System.Single;
using nint=System.Int32;
#endif

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