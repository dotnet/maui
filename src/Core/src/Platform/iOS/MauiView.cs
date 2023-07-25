using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public abstract class MauiView : UIView, ICrossPlatformLayoutBacking
	{
		static bool? _respondsToSafeArea;

		double _lastMeasureHeight = double.NaN;
		double _lastMeasureWidth = double.NaN;

		WeakReference<IView>? _reference;
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		public IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		bool RespondsToSafeArea()
		{
			if (_respondsToSafeArea.HasValue)
				return _respondsToSafeArea.Value;
			return (bool)(_respondsToSafeArea = RespondsToSelector(new Selector("safeAreaInsets")));
		}

		protected CGRect AdjustForSafeArea(CGRect bounds)
		{
			if (View is not ISafeAreaView sav || sav.IgnoreSafeArea || !RespondsToSafeArea())
			{
				return bounds;
			}

#pragma warning disable CA1416 // TODO 'UIView.SafeAreaInsets' is only supported on: 'ios' 11.0 and later, 'maccatalyst' 11.0 and later, 'tvos' 11.0 and later.
			return SafeAreaInsets.InsetRect(bounds);
#pragma warning restore CA1416
		}

		protected bool IsMeasureValid(double widthConstraint, double heightConstraint)
		{
			// Check the last constraints this View was measured with; if they're the same,
			// then the current measure info is already correct and we don't need to repeat it
			return heightConstraint == _lastMeasureHeight && widthConstraint == _lastMeasureWidth;
		}

		protected void InvalidateConstraintsCache()
		{
			_lastMeasureWidth = double.NaN;
			_lastMeasureHeight = double.NaN;
		}

		protected void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
		{
			_lastMeasureWidth = widthConstraint;
			_lastMeasureHeight = heightConstraint;
		}

		public override void SafeAreaInsetsDidChange()
		{
			base.SafeAreaInsetsDidChange();

			if (View is ISafeAreaView2 isav2)
				isav2.SafeAreaInsets = this.SafeAreaInsets.ToThickness();
		}

		public ICrossPlatformLayout? CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;
		}

		Size CrossPlatformArrange(Rect bounds)
		{
			return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Size.Zero;
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (_crossPlatformLayoutReference == null)
			{
				return base.SizeThatFits(size);
			}

			var widthConstraint = size.Width;
			var heightConstraint = size.Height;

			var crossPlatformSize = CrossPlatformMeasure(widthConstraint, heightConstraint);

			CacheMeasureConstraints(widthConstraint, heightConstraint);

			return crossPlatformSize.ToCGSize();
		}

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.LayoutVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.LayoutVirtualView
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (_crossPlatformLayoutReference == null)
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
				CrossPlatformMeasure(widthConstraint, heightConstraint);
				CacheMeasureConstraints(widthConstraint, heightConstraint);
			}

			CrossPlatformArrange(bounds);
		}

		public override void SetNeedsLayout()
		{
			InvalidateConstraintsCache();
			base.SetNeedsLayout();
			Superview?.SetNeedsLayout();
		}
	}
}
