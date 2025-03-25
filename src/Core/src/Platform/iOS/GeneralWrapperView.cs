using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform;

class GeneralWrapperView : MauiView, ICrossPlatformLayout
{
	public WeakReference<IView>? ChildView { get; private set; }

	public GeneralWrapperView(IView childView, IMauiContext mauiContext)
	{
		CrossPlatformLayout = this;
		UpdatePlatformView(childView, mauiContext);
	}

	public void Disconnect()
	{
		if (ChildView is null || !ChildView.TryGetTarget(out var childView))
		{
			return;
		}

		childView.DisconnectHandlers();
	}

	public void UpdatePlatformView(IView? newChildView, IMauiContext mauiContext)
	{
		if (Subviews.Length > 0)
		{
			Subviews[0].RemoveFromSuperview();
		}

		if (newChildView is null)
		{
			ChildView = null;
			return;
		}

		ChildView = new(newChildView);

		var nativeView = newChildView.ToPlatform(mauiContext);

		if (nativeView is WrapperView)
		{
			// Disable clipping for WrapperView to allow the shadow to be displayed
			ClipsToBounds = false;
		}
		else
		{
			ClipsToBounds = true;
		}

		AddSubview(nativeView);
	}

	public Size CrossPlatformArrange(Rect bounds)
	{
		if (ChildView == null || !ChildView.TryGetTarget(out var childView))
			return Size.Zero;

		return childView.Arrange(bounds);
	}

	public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
	{
		if (ChildView == null || !ChildView.TryGetTarget(out var childView))
			return Size.Zero;

		return childView.Measure(widthConstraint, heightConstraint);
	}
}
