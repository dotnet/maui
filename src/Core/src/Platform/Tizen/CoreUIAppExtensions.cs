using System;
using Tizen.Applications;
using EWindow = ElmSharp.Window;

namespace Microsoft.Maui
{
	internal static class CoreUIAppExtensions
	{
		public static IWindow GetWindow(this CoreUIApplication application)
		{
			if (MauiApplication.Current.VirtualWindow != null)
				return MauiApplication.Current.VirtualWindow;

			var nativeWindow = MauiApplication.Current.MainWindow;

			foreach (var window in MauiApplication.Current.Application.Windows)
			{
				if (window?.Handler?.NativeView is EWindow win && win == nativeWindow)
					return window;
			}

			throw new InvalidOperationException("Window Not Found");
		}
	}
}
