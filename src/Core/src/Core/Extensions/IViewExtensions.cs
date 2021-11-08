using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
#if __IOS__
using CoreGraphics;
using ObjCRuntime;
#endif

namespace Microsoft.Maui
{
	public static class IViewExtensions
	{
#if __ANDROID__
		internal static Rectangle GetNativeViewBounds(this IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null || nativeView.Context == null)
			{
				return new Rectangle();
			}

			var location = new int[2];
			nativeView.GetLocationOnScreen(location);
			return new Rectangle(
				location[0],
				location[1],
				nativeView.Context.ToPixels(view.Frame.Width),
				nativeView.Context.ToPixels(view.Frame.Height));
		}
#elif __IOS__
		internal static Rectangle GetNativeViewBounds(this IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null)
			{
				return new Rectangle();
			}

			nfloat X;
			nfloat Y;
			nfloat Width;
			nfloat Height;

			if (!nativeView.Transform.IsIdentity)
			{
				X = nativeView.Bounds.X;
				Y = nativeView.Bounds.Y;
				Width = nativeView.Bounds.Width;
				Height = nativeView.Bounds.Height;
			}
			else
			{
				X = nativeView.Frame.X;
				Y = nativeView.Frame.Y;
				Width = nativeView.Frame.Width;
				Height = nativeView.Frame.Height;
			}

			return new Rectangle(X, Y, Width, Height);
		}
#else
		internal static Rectangle GetNativeViewBounds(this IView view) => view.Frame;
#endif
	}
}
