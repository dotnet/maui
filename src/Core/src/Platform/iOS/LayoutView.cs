using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class LayoutView : MauiView
	{
		bool _userInteractionEnabled;

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			// When children are allowed to overflow this layout's bounds, we raise the
			// zPosition of this view's layer so that overflowing children render on top
			// of any sibling views that would otherwise be drawn over them due to z-order.
			Layer.ZPosition = (!ClipsToBounds && HasSubviewsOutsideBounds()) ? 1 : 0;
		}

		bool HasSubviewsOutsideBounds()
		{
			if (CrossPlatformLayout is not ILayout layout)
				return false;

			var width = Bounds.Width;
			var height = Bounds.Height;

			// Use a small tolerance (1.0 DIU) to absorb sub-pixel rounding
			// differences from layout calculations, consistent with Android/Windows.
			const double tolerance = 1.0;

			for (int i = 0; i < layout.Count; i++)
			{
				var frame = layout[i].Frame;
				if (frame.Right > width + tolerance || frame.Bottom > height + tolerance
					|| frame.Left < -tolerance || frame.Top < -tolerance)
				{
					return true;
				}
			}

			return false;
		}

		public override void SubviewAdded(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.SubviewAdded(uiview);
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.WillRemoveSubview(uiview);
		}

		public override UIView? HitTest(CGPoint point, UIEvent? uievent)
		{
			var result = base.HitTest(point, uievent);

			if (result is null)
			{
				return null;
			}

			if (!_userInteractionEnabled && Equals(result))
			{
				// If user interaction is disabled (IOW, if the corresponding Layout is InputTransparent),
				// then we exclude the LayoutView itself from hit testing. But it's children are valid
				// hit testing targets.

				return null;
			}

			if (result is LayoutView layoutView && !layoutView.UserInteractionEnabledOverride)
			{
				// If the child is a layout then we need to check the UserInteractionEnabledOverride
				// since layouts always have user interaction enabled.

				return null;
			}

			return result;
		}

		internal bool UserInteractionEnabledOverride => _userInteractionEnabled;

		public override bool UserInteractionEnabled
		{
			get => base.UserInteractionEnabled;
			set
			{
				// We leave the base UIE value true no matter what, so that hit testing will find children
				// of the LayoutView. But we track the intended value so we can use it during hit testing
				// to ignore the LayoutView itself, if necessary.

				base.UserInteractionEnabled = true;
				_userInteractionEnabled = value;
			}
		}
	}
}