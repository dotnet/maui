using System;
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
			if (CrossPlatformMeasure == null)
			{
				return base.SizeThatFits(size);
			}

			var width = size.Width;
			var height = size.Height;

			var crossPlatformSize = CrossPlatformMeasure(width, height);

			return crossPlatformSize.ToCGSize();
		}

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.LayoutVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.LayoutVirtualView
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var bounds = AdjustForSafeArea(Bounds).ToRectangle();
			CrossPlatformMeasure?.Invoke(bounds.Width, bounds.Height);
			CrossPlatformArrange?.Invoke(bounds);
		}

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();
			Superview?.SetNeedsLayout();
		}

		public override void SubviewAdded(UIView uiview)
		{
			base.SubviewAdded(uiview);
			Superview?.SetNeedsLayout();
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			base.WillRemoveSubview(uiview);
			Superview?.SetNeedsLayout();
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rect, Size>? CrossPlatformArrange { get; set; }

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