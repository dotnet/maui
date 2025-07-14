using System;
using Android.Content;

namespace Microsoft.Maui.Platform;

class GeneralWrapperView : ContentViewGroup, ICrossPlatformLayout
{
	public WeakReference<IView>? ChildView { get; private set; }

	public GeneralWrapperView(Context context, IView childView, IMauiContext mauiContext) : base(context)
	{
		CrossPlatformLayout = this;
		UpdatePlatformView(childView, mauiContext);
	}

	public void Disconnect()
	{
		if (ChildView == null || !ChildView.TryGetTarget(out var childView))
		{
			return;
		}

		if (ChildCount > 0)
		{
			GetChildAt(0)?.RemoveFromParent();
		}

		childView.DisconnectHandlers();
	}

	public void UpdatePlatformView(IView? newChildView, IMauiContext mauiContext)
	{
		if (ChildCount > 0)
		{
			GetChildAt(0)?.RemoveFromParent();
		}

		if (newChildView is null)
		{
			ChildView = null;
			return;
		}

		ChildView = new(newChildView);
		var nativeView = newChildView.ToPlatform(mauiContext);
		AddView(nativeView);
	}

	Graphics.Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
	{
		if (ChildView == null || !ChildView.TryGetTarget(out var childView))
			return Graphics.Size.Zero;

		return childView.Measure(widthConstraint, heightConstraint);
	}

	public Graphics.Size CrossPlatformArrange(Graphics.Rect bounds)
	{
		if (ChildView == null || !ChildView.TryGetTarget(out var childView))
			return Graphics.Size.Zero;
		return childView.Arrange(new Graphics.Rect(0, 0, bounds.Width, bounds.Height));

	}
}