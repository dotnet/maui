using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

#if WINDOWS
using Microsoft.UI.Xaml.Media;
#endif

#if __IOS__
using CoreGraphics;
using ObjCRuntime;
#endif

namespace Microsoft.Maui
{
	public static class ViewExtensions
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

			var uiWindow = nativeView.GetUIWindow();
			if (uiWindow == null)
				return new Rectangle();

			nfloat X;
			nfloat Y;
			nfloat Width;
			nfloat Height;
			
			var convertPoint = nativeView.ConvertRectToView(nativeView.Bounds, uiWindow);

			X = convertPoint.X;
			Y = convertPoint.Y;
			Width = convertPoint.Width;
			Height = convertPoint.Height;

			return new Rectangle(X, Y, Width, Height);
		}

		internal static UIKit.UIWindow? GetUIWindow(this UIKit.UIView view)
		{
			if (view is UIKit.UIWindow window)
				return window;

			if (view.Superview != null)
				return GetUIWindow(view.Superview);

			return null;
		}
#elif WINDOWS
		internal static Rectangle GetNativeViewBounds(this IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView != null)
			{
				var root = nativeView.XamlRoot;
				var offset = nativeView.TransformToVisual(root.Content) as MatrixTransform;
				if (offset != null)
					return new Rectangle(offset.Matrix.OffsetX, offset.Matrix.OffsetY, nativeView.ActualWidth, nativeView.ActualHeight);
			}

			return new Rectangle();
		}
#else
		internal static Rectangle GetNativeViewBounds(this IView view) => view.Frame;
#endif
	}
}
