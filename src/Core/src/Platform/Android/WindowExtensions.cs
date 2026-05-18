using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
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

		//TODO : Make it public in NET 11.
		internal static void ConfigureTranslucentSystemBars(this Window? window, Activity activity)
		{
			window.UpdateSystemBarAppearance(activity, updateStatusBar: true, updateNavigationBar: true);
		}

		internal static void UpdateSystemBarAppearance(
			this Window? window,
			Activity? activity,
			bool updateStatusBar,
			bool updateNavigationBar,
			Color? statusBarBackgroundColor = null,
			Color? statusBarForegroundColor = null,
			Color? navigationBarBackgroundColor = null,
			Color? navigationBarForegroundColor = null)
		{
			if (window is null)
			{
				return;
			}

			// Set appropriate system bar appearance for readability using API 30+ methods
			var windowInsetsController = WindowCompat.GetInsetsController(window, window.DecorView);
			if (windowInsetsController is not null)
			{
				// Automatically adjust icon/text colors based on explicit chrome colors when available,
				// falling back to the app theme when MAUI doesn't know the system bar background.
				var configuration = activity?.Resources?.Configuration;
				var isLightTheme = configuration is null ||
					(configuration.UiMode & UiMode.NightMask) != UiMode.NightYes;

				if (updateStatusBar)
				{
					windowInsetsController.AppearanceLightStatusBars =
						GetLightSystemBarAppearance(statusBarBackgroundColor, statusBarForegroundColor) ?? isLightTheme;
				}

				if (updateNavigationBar)
				{
					windowInsetsController.AppearanceLightNavigationBars =
						GetLightSystemBarAppearance(navigationBarBackgroundColor, navigationBarForegroundColor) ?? isLightTheme;
				}
			}
		}

		static bool? GetLightSystemBarAppearance(Color? backgroundColor, Color? foregroundColor)
		{
			if (backgroundColor?.Alpha > 0)
			{
				return backgroundColor.GetLuminosity() > 0.5;
			}

			if (foregroundColor?.Alpha > 0)
			{
				return foregroundColor.GetLuminosity() <= 0.5;
			}

			return null;
		}
	}
}
