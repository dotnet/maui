using Android.Views;
using Microsoft.Maui.Graphics;
using NativeView = Android.Views.View;

namespace Microsoft.Maui
{
	public static partial class ViewHandlerExtensions
	{
		internal static Size GetDesiredSizeFromHandler(this IViewHandler viewHandler, double widthConstraint, double heightConstraint)
		{
			var Context = viewHandler.MauiContext?.Context;
			var nativeView = viewHandler.GetWrappedNativeView();
			var VirtualView = viewHandler.VirtualView;

			if (nativeView == null || VirtualView == null || Context == null)
			{
				return Size.Zero;
			}

			// Create a spec to handle the native measure
			var widthSpec = Context.CreateMeasureSpec(widthConstraint, VirtualView.Width, VirtualView.MaximumWidth);
			var heightSpec = Context.CreateMeasureSpec(heightConstraint, VirtualView.Height, VirtualView.MaximumHeight);

			nativeView.Measure(widthSpec, heightSpec);

			// Convert back to xplat sizes for the return value
			return Context.FromPixels(nativeView.MeasuredWidth, nativeView.MeasuredHeight);

		}

		internal static void NativeArrangeHandler(this IViewHandler viewHandler, Rectangle frame)
		{
			var nativeView = viewHandler.GetWrappedNativeView();

			var Context = viewHandler.MauiContext?.Context;
			var MauiContext = viewHandler.MauiContext;
			var VirtualView = viewHandler.VirtualView;

			if (nativeView == null || MauiContext == null || Context == null)
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

			nativeView.Layout((int)left, (int)top, (int)right, (int)bottom);

			viewHandler.Invoke(nameof(IView.Frame), frame);
		}
	}
}
