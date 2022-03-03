using Android.Views;
using Microsoft.Maui.Graphics;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui
{
	public static partial class ViewHandlerExtensions
	{
		internal static Size GetDesiredSizeFromHandler(this IViewHandler viewHandler, double widthConstraint, double heightConstraint)
		{
			var Context = viewHandler.MauiContext?.Context;
			var platformView = viewHandler.ToPlatform();
			var VirtualView = viewHandler.VirtualView;

			if (platformView == null || VirtualView == null || Context == null)
			{
				return Size.Zero;
			}

			// Create a spec to handle the native measure
			var widthSpec = Context.CreateMeasureSpec(widthConstraint, VirtualView.Width, VirtualView.MaximumWidth);
			var heightSpec = Context.CreateMeasureSpec(heightConstraint, VirtualView.Height, VirtualView.MaximumHeight);

			platformView.Measure(widthSpec, heightSpec);

			// Convert back to xplat sizes for the return value
			return Context.FromPixels(platformView.MeasuredWidth, platformView.MeasuredHeight);

		}

		internal static void PlatformArrangeHandler(this IViewHandler viewHandler, Rect frame)
		{
			var platformView = viewHandler.ToPlatform();

			var Context = viewHandler.MauiContext?.Context;
			var MauiContext = viewHandler.MauiContext;

			if (platformView == null || MauiContext == null || Context == null)
			{
				return;
			}

			if (frame.Width < 0 || frame.Height < 0)
			{
				// This is a legacy layout value from Controls, nothing is actually laying out yet so we just ignore it
				return;
			}

			var left = Context.ToPixels(frame.Left);
			var top = Context.ToPixels(frame.Top);
			var bottom = Context.ToPixels(frame.Bottom);
			var right = Context.ToPixels(frame.Right);

			platformView.Layout((int)left, (int)top, (int)right, (int)bottom);

			viewHandler.Invoke(nameof(IView.Frame), frame);
		}
	}
}
