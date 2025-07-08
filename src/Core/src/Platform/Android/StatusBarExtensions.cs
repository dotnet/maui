using Android.Content.Res;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using Activity = Android.App.Activity;

namespace Microsoft.Maui.Platform;

internal static class StatusBarExtensions
{
	/// <summary>
	/// Sets the status bar appearance for Android API 36+ where edge-to-edge is enforced.
	/// On Android 16 (API 36), you can no longer change the status bar color directly,
	/// and must use WindowInsetsControllerCompat to set the appearance.
	/// </summary>
	/// <param name="activity">The activity to set the status bar appearance for</param>
	/// <param name="isLightStatusBar">Whether to use light status bar (dark icons) or dark status bar (light icons)</param>
	public static void SetStatusBarAppearance(this Activity activity, bool isLightStatusBar)
	{
		if (activity?.Window == null)
		{
			return;
		}

		// For Android API 36+, use WindowInsetsControllerCompat
		if (Build.VERSION.SdkInt >= (BuildVersionCodes)36)
		{
			var windowInsetsController =
				WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);

			if (windowInsetsController is not null)
			{
				windowInsetsController.AppearanceLightStatusBars = isLightStatusBar;
			}

			// Set up window insets handling to prevent content overlap
			SetupWindowInsets(activity);
		}

		// NOTE: For older versions, the existing status bar color setting approach continues to work
		// This is handled by other parts of the framework that call SetStatusBarColor directly.
	}

	/// <summary>
	/// Sets the status bar appearance based on the current theme.
	/// </summary>
	/// <param name="activity">The activity to set the status bar appearance for</param>
	public static void SetStatusBarAppearanceForCurrentTheme(this Activity activity)
	{
		if (activity?.Window == null)
		{
			return;
		}

		// Determine if we should use light status bar based on current theme
		bool isLightTheme = IsLightTheme(activity);
		activity.SetStatusBarAppearance(isLightTheme);
	}

	static bool IsLightTheme(Activity activity)
	{
		var uiModeFlags = activity.Resources?.Configuration?.UiMode & UiMode.NightMask;
		return uiModeFlags != UiMode.NightYes;
	}

	static void SetupWindowInsets(Activity activity)
	{
		if (activity?.Window?.DecorView == null)
		{
			return;
		}

		ViewCompat.SetOnApplyWindowInsetsListener(activity.Window.DecorView, new WindowInsetsListener());

		// Trigger the initial insets application
		ViewCompat.RequestApplyInsets(activity.Window.DecorView);
	}
}

class WindowInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
{
	public WindowInsetsCompat? OnApplyWindowInsets(View? view, WindowInsetsCompat? insets)
	{
		// Get the system bars insets (status bar and navigation bar)
		var systemBarsInsets = insets?.GetInsets(WindowInsetsCompat.Type.SystemBars());

		// Apply padding to the root view to prevent content overlap with system bars
		view?.SetPadding(
			systemBarsInsets?.Left ?? 0,
			systemBarsInsets?.Top ?? 0,
			systemBarsInsets?.Right ?? 0,
			systemBarsInsets?.Bottom ?? 0
		);
		
		return insets;
	}
}