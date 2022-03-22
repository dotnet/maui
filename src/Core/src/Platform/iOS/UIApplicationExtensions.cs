using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class UIApplicationExtensions
	{
		internal static UIEdgeInsets GetSafeAreaInsetsForWindow(this UIApplication application)
		{
			UIEdgeInsets safeAreaInsets;

			if (!OperatingSystem.IsIOSVersionAtLeast(11))
				safeAreaInsets = new UIEdgeInsets(UIApplication.SharedApplication.StatusBarFrame.Size.Height, 0, 0, 0);
			else if (application.GetKeyWindow() is UIWindow keyWindow)
				safeAreaInsets = keyWindow.SafeAreaInsets;
			else if (application.Windows.Length > 0)
				safeAreaInsets = application.Windows[0].SafeAreaInsets;
			else
				safeAreaInsets = UIEdgeInsets.Zero;

			return safeAreaInsets;
		}

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

		public static IWindow? GetWindow(this UIWindow? platformWindow)
		{
			if (platformWindow is null)
				return null;

			foreach (var window in MauiUIApplicationDelegate.Current.Application.Windows)
			{
				if (window?.Handler?.PlatformView == platformWindow)
					return window;
			}

			return null;
		}

		public static IWindow? GetWindow(this UIWindowScene? windowScene)
		{
			if (windowScene is null)
				return null;

			foreach (var window in windowScene.Windows)
			{
				var managedWindow = window.GetWindow();

				if (managedWindow is not null)
					return managedWindow;
			}

			return null;
		}
	}
}