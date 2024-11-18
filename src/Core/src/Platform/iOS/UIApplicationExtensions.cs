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
#pragma warning disable CA1422 // Validate platform compatibility
			else if (application.GetKeyWindow() is UIWindow keyWindow)
				safeAreaInsets = keyWindow.SafeAreaInsets;
#pragma warning disable CA1416 // TODO: 'UIApplication.Windows' is unsupported on: 'ios' 15.0 and later.
			else if (application.Windows.Length > 0)
				safeAreaInsets = application.Windows[0].SafeAreaInsets;
#pragma warning restore CA1416
			else
				safeAreaInsets = UIEdgeInsets.Zero;
#pragma warning restore CA1422 // Validate platform compatibility

			return safeAreaInsets;
		}

		public static UIWindow? GetKeyWindow(this UIApplication application)
		{
#pragma warning disable CA1416 // TODO: 'UIApplication.Windows' is unsupported on: 'ios' 15.0 and later.
#pragma warning disable CA1422 // Validate platform compatibility
			var windows = application.Windows;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416

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
			// If there's only one window to return then just return that window
			var windows = IPlatformApplication.Current?.Application?.Windows ?? Array.Empty<IWindow>();

			if (windows.Count == 1)
				return windows[0];

			if (OperatingSystem.IsIOSVersionAtLeast(13))
			{
				foreach (var windowScene in application.ConnectedScenes)
				{
					if (windowScene is UIWindowScene uiWindowScene)
					{
						if (uiWindowScene.Windows.Length == 1 && uiWindowScene.Windows[0].GetWindow() is IWindow window)
						{
							return window;
						}
					}
				}
			}
			else
			{
				if (application.Windows.Length == 1)
					return application.Windows[0].GetWindow();
			}

			return application.GetKeyWindow().GetWindow();
		}
	}
}