using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
		internal static IWindow? GetHostedWindow(this UIWindow? uiWindow)
		{
			if (uiWindow is null)
				return null;

			var windows = WindowExtensions.GetWindows();
			foreach (var window in windows)
			{

				if (window.Handler?.PlatformView is UIWindow win)
				{
					if (win == uiWindow)
						return window;
				}
			}

			return null;
		}

		public static float GetDisplayDensity(this UIWindow uiWindow) =>
			(float)(uiWindow.Screen?.Scale ?? new nfloat(1.0f));

#pragma warning disable CA1416 // UIApplication.StatusBarOrientation has [UnsupportedOSPlatform("ios9.0")]. (Deprecated but still works)
		internal static DisplayOrientation GetOrientation(this IWindow window) =>
			UIApplication.SharedApplication.StatusBarOrientation.IsLandscape()
				? DisplayOrientation.Landscape
				: DisplayOrientation.Portrait;
#pragma warning restore CA1416
	}
}
