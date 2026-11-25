using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class UIWindowExtensions
	{
		/// <summary>
		/// Gets the .NET MAUI window associated with the specified iOS or Mac Catalyst platform window.
		/// </summary>
		/// <param name="platformWindow">The <see cref="UIWindow"/> to resolve.</param>
		/// <returns>The associated <see cref="IWindow"/>, or <see langword="null"/> if <paramref name="platformWindow"/> is <see langword="null"/> or no matching window is found.</returns>
		public static IWindow? GetWindow(this UIWindow? platformWindow)
			=> platformWindow.GetWindow(IPlatformApplication.Current?.Application);

		internal static IWindow? GetWindow(this UIWindow? platformWindow, IApplication? application)
		{
			if (platformWindow is null)
				return null;

			foreach (var window in application?.Windows ?? Array.Empty<IWindow>())
			{
				if (window?.Handler?.PlatformView == platformWindow)
					return window;
			}

			return null;
		}

		/// <summary>
		/// Gets the .NET MAUI window associated with the specified iOS or Mac Catalyst window scene.
		/// </summary>
		/// <param name="windowScene">The <see cref="UIWindowScene"/> to resolve.</param>
		/// <returns>The associated <see cref="IWindow"/>, or <see langword="null"/> if <paramref name="windowScene"/> is <see langword="null"/> or no matching window is found.</returns>
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