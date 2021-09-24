using System;
using UIKit;

namespace Microsoft.Maui
{
	internal static class UIApplicationExtensions
	{
		public static UIWindow? GetKeyWindow(this UIApplication application)
		{
			var windows = application.Windows;

			for (int i = 0; i < windows.Length; i++)
			{
				var window = windows[i];
				if (window.IsKeyWindow)
					return window;
			}

			return null;
		}

		public static IWindow? GetWindow(this UIApplication application)
		{
			if (MauiUIApplicationDelegate.Current.VirtualWindow != null)
				return MauiUIApplicationDelegate.Current.VirtualWindow;

			var nativeWindow = application.GetKeyWindow();
			foreach (var window in MauiUIApplicationDelegate.Current.Application.Windows)
			{
				if (window?.Handler?.NativeView is UIWindow win && win == nativeWindow)
					return window;
			}

			return null;
		}
	}
}
