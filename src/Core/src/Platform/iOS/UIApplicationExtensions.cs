using ObjCRuntime;
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

		public static IWindow? GetWindow(this UIApplication application) =>
			application.GetKeyWindow().GetWindow();

		public static IWindow? GetWindow(this UIWindow? nativeWindow)
		{
			if (nativeWindow is null)
				return null;

			foreach (var window in MauiUIApplicationDelegate.Current.Application.Windows)
			{
				if (window?.Handler?.NativeView == nativeWindow)
					return window;
			}

			return null;
		}
	}
}