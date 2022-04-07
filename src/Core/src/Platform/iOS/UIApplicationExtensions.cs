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
#pragma warning disable CA1416 // 'UIApplication.Windows' is unsupported on: 'ios' 15.0 and later.
			else if (application.Windows.Length > 0)
				safeAreaInsets = application.Windows[0].SafeAreaInsets;
#pragma warning restore CA1416 // Not sure safe to suppress
			else
				safeAreaInsets = UIEdgeInsets.Zero;

			return safeAreaInsets;
		}

		public static UIWindow? GetKeyWindow(this UIApplication application)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(15))
			{
				var windows = application.Windows;

				for (int i = 0; i < windows.Length; i++)
				{
					var window = windows[i];
					if (window.IsKeyWindow)
						return window;
				}
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
			if (windowScene is null || !OperatingSystem.IsIOSVersionAtLeast(13))
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