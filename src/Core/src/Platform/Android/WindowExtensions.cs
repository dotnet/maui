using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using AndroidX.Core.View;
using AColor = Android.Graphics.Color;
using AndroidX.Core.Graphics;
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

		//TODO : Make it public in NET 11.
		internal static void ConfigureTranslucentSystemBars(this Window? window, Activity activity)
		{
			if (window is null)
			{
				return;
			}

			// Set appropriate system bar appearance for readability using API 30+ methods
			var windowInsetsController = WindowCompat.GetInsetsController(window, window.DecorView);
			if (windowInsetsController is not null)
			{
				// Automatically adjust icon/text colors based on app theme
				var configuration = activity.Resources?.Configuration;
				var isLightTheme = configuration is null ||
					(configuration.UiMode & UiMode.NightMask) != UiMode.NightYes;

				// Use white as the fallback so light-theme apps keep dark status bar icons
				// if the theme color cannot be resolved. The fallback is defensive; Android
				// themes should normally resolve colorPrimary.
				var statusBarColor = GetThemeColor(activity, global::Android.Resource.Attribute.ColorPrimary, AColor.White);
				
				windowInsetsController.AppearanceLightStatusBars = IsLightColor(statusBarColor);
				windowInsetsController.AppearanceLightNavigationBars = isLightTheme;
			}
		}

		static AColor GetThemeColor(Activity activity, int attribute, AColor fallback)
		{
			if (activity.Theme is null)
				return fallback;

			using var ta = activity.Theme.ObtainStyledAttributes([attribute]);

			return new AColor(ta.GetColor(0, fallback.ToArgb()));
		}

		static bool IsLightColor(AColor color)
	=> AndroidX.Core.Graphics.ColorUtils.CalculateLuminance(color.ToArgb()) > 0.5;
	}
}
