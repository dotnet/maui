using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using AndroidX.Core.View;
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

		internal static void ConfigureTranslucentSystemBars(this Window? window, Activity activity)
		{
			if (window is null)
			{
				return;
			}

			// Set appropriate system bar appearance for readability
			var windowInsetsController = WindowCompat.GetInsetsController(window, window.DecorView);
			if (windowInsetsController is not null)
			{
				// Automatically adjust icon/text colors based on app theme
				var configuration = activity.Resources?.Configuration;
				var isLightTheme = configuration != null &&
					(configuration.UiMode & UiMode.NightMask) != UiMode.NightYes;

				windowInsetsController.AppearanceLightStatusBars = isLightTheme;
				windowInsetsController.AppearanceLightNavigationBars = isLightTheme;
			}
		}
	}
}
