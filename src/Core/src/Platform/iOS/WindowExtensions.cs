﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

		public static async Task<RenderedView?> RenderAsImage(this IWindow window, RenderType type)
		{
			await Task.Delay(5);
			return null;
		}
	}
}
