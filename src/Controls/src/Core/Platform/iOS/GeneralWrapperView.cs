using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform;

class GeneralWrapperView : MauiView, ICrossPlatformLayout
{
    //Make this a weakreference
    public WeakReference<IView> ChildView { get; private set; }

    public GeneralWrapperView(IView childView, IMauiContext context)
    {
        CrossPlatformLayout = this;
        ChildView = new(childView);
        UpdatePlatformView(childView, context);
    }

    internal void UpdatePlatformView(IView childView, IMauiContext context)
    {
        var nativeView = childView.ToPlatform(context);

        if (nativeView.Superview == this)
        {
            nativeView.RemoveFromSuperview();
        }

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
        if (ChildView is null || !ChildView.TryGetTarget(out var childView))
        {
            return Size.Zero;
        }

        return childView.Arrange(bounds);
    }

    public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
    {
        if (ChildView is null || !ChildView.TryGetTarget(out var childView))
        {
            return Size.Zero;
        }

        return childView.Measure(widthConstraint, heightConstraint);
    }
}