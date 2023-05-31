using System;
using System.Drawing;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class LayoutView : MauiView
	{
		bool _userInteractionEnabled;


		// TODO: Possibly reconcile this code with ViewHandlerExtensions.MeasureVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.MeasureVirtualView
		public override CGSize SizeThatFits(CGSize size)
		{
			if (View is not ILayout layout)
			{
				return base.SizeThatFits(size);
			}

			var width = size.Width;
			var height = size.Height;

			var crossPlatformSize = layout.CrossPlatformMeasure(width, height);

			CacheMeasureConstraints(width, height);

			return crossPlatformSize.ToCGSize();
		}

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.LayoutVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.LayoutVirtualView
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (View is not ILayout layout)
			{
				return;
			}

			var bounds = AdjustForSafeArea(Bounds).ToRectangle();

			var widthConstraint = bounds.Width;
			var heightConstraint = bounds.Height;

			// If the SuperView is a MauiView (backing a cross-platform ContentView or Layout), then measurement
			// has already happened via SizeThatFits and doesn't need to be repeated in LayoutSubviews. But we
			// _do_ need LayoutSubviews to make a measurement pass if the parent is something else (for example,
			// the window); there's no guarantee that SizeThatFits has been called in that case.

			if (!IsMeasureValid(widthConstraint, heightConstraint) && Superview is not MauiView)
			{
				layout.CrossPlatformMeasure(widthConstraint, heightConstraint);
				CacheMeasureConstraints(widthConstraint, heightConstraint);
			}

			layout.CrossPlatformArrange(bounds);
		}

		public override void SetNeedsLayout()
		{
			InvalidateConstraintsCache();
			base.SetNeedsLayout();
			Superview?.SetNeedsLayout();
		}

		public override void SubviewAdded(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.SubviewAdded(uiview);
			Superview?.SetNeedsLayout();
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.WillRemoveSubview(uiview);
			Superview?.SetNeedsLayout();
		}

		public override UIView HitTest(CGPoint point, UIEvent? uievent)
		{
			var result = base.HitTest(point, uievent);

			if (!_userInteractionEnabled && this.Equals(result))
			{
				// If user interaction is disabled (IOW, if the corresponding Layout is InputTransparent),
				// then we exclude the LayoutView itself from hit testing. But it's children are valid
				// hit testing targets.

				return null!;
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