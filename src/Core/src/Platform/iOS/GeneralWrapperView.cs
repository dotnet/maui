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

    public static void Disconnect(MauiView view, WeakReference<IView>? childViewRef)
    {
        view.RemoveFromSuperview();

        if (childViewRef is not null && childViewRef.TryGetTarget(out var childView))
        {
            childView.DisconnectHandlers();
        }

        for (int i = view.Subviews.Length - 1; i >= 0; i--)
        {
            view.Subviews[i].RemoveFromSuperview();
        }
    }

    public void Disconnect()
    {
        Disconnect(this, ChildView);
    }

    public static void UpdatePlatformView(MauiView view, IView? newChildView, IMauiContext mauiContext, out WeakReference<IView>? childViewRef)
    {
        if (view.Subviews.Length > 0)
        {
            view.Subviews[0].RemoveFromSuperview();
        }

        if (newChildView is null)
        {
            view.View = null;
            childViewRef = null;
            return;
        }

        childViewRef = new(newChildView);

		var nativeView = newChildView.ToPlatform(mauiContext);

        if (nativeView is WrapperView)
        {
            // Disable clipping for WrapperView to allow the shadow to be displayed
            view.ClipsToBounds = false;
        }
        else
        {
            view.ClipsToBounds = true;
        }

        view.AddSubview(nativeView);
    }

    public void UpdatePlatformView(IView? newChildView, IMauiContext mauiContext)
    {
        UpdatePlatformView(this, newChildView, mauiContext, out var weakReference);
        ChildView = weakReference;
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
