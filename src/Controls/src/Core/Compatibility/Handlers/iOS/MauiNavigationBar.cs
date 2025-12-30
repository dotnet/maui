using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Controls.Handlers.Compatibility;

/// <summary>
/// This class is used to adjust the navigation bar for the title bar on Mac Catalyst when a new titlebar is added or removed.
/// </summary>
internal class MauiNavigationBar : UINavigationBar
{
	internal bool TitleBarNeedsRefresh { get; set; }
	nfloat? _originalSafeAreaConstant = null;

	[Internals.Preserve(Conditional = true)]
	public MauiNavigationBar() : base()
	{
	}

	[Internals.Preserve(Conditional = true)]
	public MauiNavigationBar(Foundation.NSCoder coder) : base(coder)
	{
	}

	[Internals.Preserve(Conditional = true)]
	protected MauiNavigationBar(Foundation.NSObjectFlag t) : base(t)
	{
	}

	[Internals.Preserve(Conditional = true)]
	protected internal MauiNavigationBar(IntPtr handle) : base(handle)
	{
	}

	[Internals.Preserve(Conditional = true)]
	public MauiNavigationBar(RectangleF frame) : base(frame)
	{
	}

	protected internal MauiNavigationBar(NativeHandle handle) : base(handle)
	{
	}

	public override void SafeAreaInsetsDidChange()
	{
		if (_originalSafeAreaConstant is null)
		{
			_originalSafeAreaConstant = SafeAreaInsets.Top;
		}

		base.SafeAreaInsetsDidChange();
#if MACCATALYST
        AdjustForTitleBar();
#endif
	}

#if MACCATALYST
    void AdjustForTitleBar()
    {
        var controller = Window?.RootViewController as WindowViewController;

        if (controller?.HasCustomTitleBar == true && _originalSafeAreaConstant is nfloat originalSafeAreaConstant)
        {
            var currentSafeAreaTop = SafeAreaInsets.Top;
            var titleBarHeight = controller._contentWrapperTopConstraint?.Constant ?? 0;

#pragma warning disable CS0618 // Type or member is obsolete
            if (currentSafeAreaTop > 0 && Frame.Y < originalSafeAreaConstant && titleBarHeight == 0)
            {
                controller.IsFirstLayout = false;
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
                Frame = new CGRect(Frame.X, originalSafeAreaConstant, Frame.Width, Frame.Height);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
                TitleBarNeedsRefresh = true;
            }
            else if (controller.IsFirstLayout)
            {
                controller.IsFirstLayout = false;
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
                Frame = new CGRect(Frame.X, Math.Max(originalSafeAreaConstant - titleBarHeight, 0), Frame.Width, Frame.Height);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
                TitleBarNeedsRefresh = true;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
#endif

	internal void RefreshIfNeeded()
	{
		if (TitleBarNeedsRefresh)
		{
			Superview?.SetNeedsLayout();
			TitleBarNeedsRefresh = false;
		}
	}
}
