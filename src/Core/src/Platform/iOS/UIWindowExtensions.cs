using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class UIWindowExtensions
	{
		// TODO Add public docs
		public static IWindow? GetWindow(this UIWindow? platformWindow)
		{
			if (platformWindow is null)
				return null;

			foreach (var window in IPlatformApplication.Current?.Application?.Windows ?? Array.Empty<IWindow>())
			{
				if (window?.Handler?.PlatformView == platformWindow)
					return window;
			}

			return null;
		}

		// TODO Add public docs
		public static IWindow? GetWindow(this UIWindowScene? windowScene)
		{
			if (windowScene is null)
				return null;

#pragma warning disable CA1416 // TODO: 'UIApplication.Windows' is unsupported on: 'ios' 15.0 and later
			foreach (var window in windowScene.Windows)
			{
				var managedWindow = window.GetWindow();

				if (managedWindow is not null)
					return managedWindow;
			}
#pragma warning restore CA1416

			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return null;
			else if (windowScene.Delegate is IUIWindowSceneDelegate sd)
				return sd.GetWindow().GetWindow();

			return null;
		}
	}
}