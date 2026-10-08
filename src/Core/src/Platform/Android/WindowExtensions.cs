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

				windowInsetsController.AppearanceLightStatusBars =
					GetStatusBarAppearance(activity, isLightTheme, RuntimeFeature.IsMaterial3Enabled);

				windowInsetsController.AppearanceLightNavigationBars = isLightTheme;
			}
		}

		internal static bool GetStatusBarAppearance(Context context, bool isLightTheme, bool isMaterial3)
		{
			// Material 3 draws its surface behind the transparent edge-to-edge status bar.
			// The Material 2 app bar uses colorPrimary in the same area.
			var backgroundAttribute = isMaterial3
				? Resource.Attribute.colorSurface
				: global::Android.Resource.Attribute.ColorPrimary;

			return TryGetThemeColor(context, backgroundAttribute, out var backgroundColor)
				? IsLightColor(backgroundColor)
				: isLightTheme;
		}

		static bool TryGetThemeColor(Context context, int attribute, out AColor color)
		{
			color = default;

			if (context.Theme is null)
				return false;

			using var ta = context.Theme.ObtainStyledAttributes([attribute]);

			if (!ta.HasValue(0))
				return false;

			color = new AColor(ta.GetColor(0, 0));
			return true;
		}

		static bool IsLightColor(AColor color) =>
			ColorUtils.CalculateLuminance(color.ToArgb()) > 0.5;
	}
}