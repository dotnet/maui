using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Android.Extensions
{
	internal static class DragEventExtensions
	{
		internal static Point? CalculatePosition(this DragEvent? e, IElement? sourceElement, IElement? relativeElement)
		{
			var context = sourceElement?.Handler?.MauiContext?.Context;

			if (context is null | e is null)
				return null;

			double rawX = 0;
			double rawY = 0;

			// DragEvents don't natively have a way to get a raw X or Y position - unlike MotionEvents
			// Instead the GetX() and GetY() get the position relative to the view that's receiving the drag
			// With that we can calculate the rawX and Y manually
			if (sourceElement?.Handler?.PlatformView is AView sourceView)
			{
				var location = sourceView.GetLocationOnScreenPx();

				rawX = e!.GetX() + location.X;
				rawY = e!.GetY() + location.Y;
			}

			if (relativeElement is null)
			{
				return new Point(context.FromPixels(rawX), context.FromPixels(rawY));
			}

			if (relativeElement?.Handler?.PlatformView is AView aView)
			{
				var relativeElementLocation = aView.GetLocationOnScreenPx();

				var x = rawX - relativeElementLocation.X;
				var y = rawY - relativeElementLocation.Y;

				return new Point(context.FromPixels(x), context.FromPixels(y));
			}

			return null;
		}
	}
}
