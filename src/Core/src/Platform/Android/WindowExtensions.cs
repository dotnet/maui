using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using AColor = Android.Graphics.Color;
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

			var windowInsetsController = WindowCompat.GetInsetsController(window, window.DecorView);
			if (windowInsetsController is not null)
			{
				var configuration = activity.Resources?.Configuration;
				var isLightTheme = configuration is null ||
					(configuration.UiMode & UiMode.NightMask) != UiMode.NightYes;

				// Resolve the actual status bar background color from the current theme and
				// choose icon/text appearance based on its luminance. If the theme color cannot
				// be resolved, preserve the previous theme-based behavior.
				if (TryGetThemeColor(activity, global::Android.Resource.Attribute.ColorPrimary, out var statusBarColor))
					windowInsetsController.AppearanceLightStatusBars = IsLightColor(statusBarColor);
				else
					windowInsetsController.AppearanceLightStatusBars = isLightTheme;

				windowInsetsController.AppearanceLightNavigationBars = isLightTheme;
			}
		}

		static bool TryGetThemeColor(Activity activity, int attribute, out AColor color)
		{
			color = default;

			if (activity.Theme is null)
				return false;

			using var ta = activity.Theme.ObtainStyledAttributes([attribute]);

			if (!ta.HasValue(0))
				return false;

			color = new AColor(ta.GetColor(0, 0));
			return true;
		}

		static bool IsLightColor(AColor color) =>
			ColorUtils.CalculateLuminance(color.ToArgb()) > 0.5;
	}
}