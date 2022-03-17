using System;
using System.Collections.Generic;
using System.Text;
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
	}
}
