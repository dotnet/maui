using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui
{
	public static partial class WindowExtensions
	{
		internal static void UpdateTitle(this Activity platformWindow, IWindow window)
		{
			if (string.IsNullOrEmpty(window.Title))
				platformWindow.Title = ApplicationModel.AppInfo.Current.Name;
			else
				platformWindow.Title = window.Title;
		}

		internal static DisplayOrientation GetOrientation(this IWindow? window)
		{
			if (window == null)
				return DeviceDisplay.Current.MainDisplayInfo.Orientation;

			return window.Handler?.MauiContext?.GetPlatformWindow()?.Resources?.Configuration?.Orientation switch
			{
				Orientation.Landscape => DisplayOrientation.Landscape,
				Orientation.Portrait => DisplayOrientation.Portrait,
				Orientation.Square => DisplayOrientation.Portrait,
				_ => DisplayOrientation.Unknown
			};
		}

		internal static void UpdateWindowSoftInputModeAdjust(this IWindow platformView, SoftInput inputMode)
		{
			var activity = platformView?.Handler?.PlatformView as Activity ??
							platformView?.Handler?.MauiContext?.GetPlatformWindow();

			activity?
				.Window?
				.SetSoftInputMode(inputMode);
		}
	}
}
