using UIKit;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class TitleBar
	{
		internal override void OnIsVisibleChanged(bool oldValue, bool newValue)
		{
			base.OnIsVisibleChanged(oldValue, newValue);
		// internal static void UpdateTitleBar(this UIWindow platformWindow, IWindow window, IMauiContext? mauiContext)

#if MACCATALYST
            var context = Handler?.MauiContext;

            var platformWindow = Window.Handler.PlatformView;

            if (platformWindow is UIWindow platWindow)
            {
                platWindow.SetTitleBarVisibility(newValue);
                // WindowExtensions.SetTitleBarVisibility(newValue);
                platWindow.UpdateTitleBar(Window, context);
            }
#endif
            // var window = Handler?.MauiContext?.GetWindow();

            // Handler.PlatformView.UpdateTitleBar(window, handler.MauiContext);

			// var navRootManager = Handler?.MauiContext?.GetNavigationRootManager();
			// navRootManager?.SetTitleBarVisibility(newValue);
		}

#pragma warning disable RS0016 // Add public types and members to the declared API
		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			var t =  base.ArrangeOverride(bounds);
            return t;
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			var t = base.MeasureOverride(widthConstraint, heightConstraint);
            return t;
		}

		// public override SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
		// {
		// 	return base.Measure(widthConstraint, heightConstraint, flags);
		// }

#pragma warning restore RS0016 // Add public types and members to the declared API

	}
}
