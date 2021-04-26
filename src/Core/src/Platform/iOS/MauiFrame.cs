using CoreGraphics;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiFrame : UIView
	{
		CGSize _previousSize;

		public override void LayoutSubviews()
		{
			if (_previousSize != Bounds.Size)
				SetNeedsDisplay();

			base.LayoutSubviews();
		}

		public override void Draw(CGRect rect)
		{
			var child = Subviews[0];

			if (child != null)
				child.Frame = Bounds;

			base.Draw(rect);

			_previousSize = Bounds.Size;
		}
	}

	public class MauiFrameContent : UIView
	{
		public override void RemoveFromSuperview()
		{
			for (var i = Subviews.Length - 1; i >= 0; i--)
			{
				var item = Subviews[i];
				item.RemoveFromSuperview();
			}
		}

		public override bool PointInside(CGPoint point, UIEvent? uievent)
		{
			foreach (var view in Subviews)
			{
				if (view.HitTest(ConvertPointToView(point, view), uievent) != null)
					return true;
			}

			return false;
		}
	}
}