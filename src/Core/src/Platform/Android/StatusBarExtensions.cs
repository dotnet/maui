using System.Collections.Generic;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AColor = Android.Graphics.Color;
using Activity = Android.App.Activity;

namespace Microsoft.Maui.Platform
{
	internal static class StatusBarExtensions
	{
		static readonly Dictionary<Activity, View> StatusBarOverlays = new();

		class WindowInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
		{
			private readonly View? _overlay;

			public WindowInsetsListener(View? overlay = null)
			{
				_overlay = overlay;
			}

			public WindowInsetsCompat? OnApplyWindowInsets(View? view, WindowInsetsCompat? insets)
			{
				if (insets == null || view == null)
				{
					return insets;
				}

				// Get the system bars insets (status bar and navigation bar)
				var systemBarsInsets = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());

				if (_overlay != null)
				{
					// Position the overlay at the top of the screen covering the status bar area
					var statusBarInsets = insets.GetInsets(WindowInsetsCompat.Type.StatusBars());
					
					var layoutParams = new FrameLayout.LayoutParams(
						ViewGroup.LayoutParams.MatchParent,
						statusBarInsets?.Top ?? 0)
					{
						Gravity = GravityFlags.Top
					};
					_overlay.LayoutParameters = layoutParams;
				}
				
				// Apply padding to the root view to prevent content overlap with system bars
				view.SetPadding(
					systemBarsInsets?.Left ?? 0,
					systemBarsInsets?.Top ?? 0,
					systemBarsInsets?.Right ?? 0,
					systemBarsInsets?.Bottom ?? 0
				);
			
				return insets;
			}
		}

		/// <summary>
		/// Sets the status bar appearance for Android API 36+ where edge-to-edge is enforced.
		/// On Android 16 (API 36), you can no longer change the status bar color directly,
		/// and must use WindowInsetsControllerCompat to set the appearance.
		/// Also handles proper window insets to prevent content overlap with system bars.
		/// </summary>
		/// <param name="activity">The activity to set the status bar appearance for</param>
		/// <param name="isLightStatusBar">Whether to use light status bar (dark icons) or dark status bar (light icons)</param>
		public static void SetStatusBarAppearance(this Activity activity, bool isLightStatusBar)
		{
			if (activity?.Window == null)
			{
				return;
			}

			// For Android API 36+, use WindowInsetsControllerCompat and handle edge-to-edge properly
			if (Build.VERSION.SdkInt >= (BuildVersionCodes)36) // Android 16 API 36+
			{
				// Enable edge-to-edge display
				WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);

				var windowInsetsController =
					WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);

				if (windowInsetsController is not null)
				{
					windowInsetsController.AppearanceLightStatusBars = isLightStatusBar;
				}

				// Set up window insets handling to prevent content overlap
				SetupWindowInsets(activity);
			}

			// For older versions, the existing status bar color setting approach continues to work
			// This is handled by other parts of the framework that call SetStatusBarColor directly
		}

		/// <summary>
		/// Sets the status bar background color for Android API 36+ by creating a colored overlay.
		/// On Android 16 (API 36), the status bar must be transparent due to edge-to-edge enforcement,
		/// so we create a colored overlay view behind it to achieve the desired visual appearance.
		/// </summary>
		/// <param name="activity">The activity to set the status bar color for</param>
		/// <param name="color">The color to use for the status bar background</param>
		/// <param name="isLightStatusBar">Whether to use light status bar (dark icons) or dark status bar (light icons)</param>
		public static void SetStatusBarColor(this Activity activity, AColor color, bool isLightStatusBar)
		{
			if (activity?.Window == null)
			{
				return;
			}

			// For Android API 36+, create a colored overlay behind the transparent status bar
			if (Build.VERSION.SdkInt >= (BuildVersionCodes)36) // Android 16 API 36+
			{
				// Set the appearance first
				activity.SetStatusBarAppearance(isLightStatusBar);
				
				// Create or update the status bar overlay
				CreateStatusBarOverlay(activity, color);
			}
			else
			{
				// For older versions, use the standard approach
				activity.Window.SetStatusBarColor(color);
				activity.SetStatusBarAppearance(isLightStatusBar);
			}
		}

		/// <summary>
		/// Sets the status bar color based on the current theme colors.
		/// Uses the app's primary color for the status bar background.
		/// </summary>
		/// <param name="activity">The activity to set the status bar color for</param>
		/// <param name="color">The color to use for the status bar background</param>
		public static void SetStatusBarColor(this Activity activity, AColor color)
		{
			if (activity?.Window == null)
			{
				return;
			}

			// Determine if we should use light status bar based on current theme
			bool isLightTheme = IsLightTheme(activity);
			
			// Set the status bar color with appropriate appearance
			activity.SetStatusBarColor(color, isLightTheme);
		}

		/// <summary>
		/// Sets the status bar appearance based on the current theme.
		/// </summary>
		/// <param name="activity">The activity to set the status bar appearance for</param>
		public static void SetStatusBarAppearance(this Activity activity)
		{
			if (activity?.Window == null)
			{
				return;
			}

			// Determine if we should use light status bar based on current theme
			bool isLightTheme = IsLightTheme(activity);
			
			// Set the status bar appearance
			activity.SetStatusBarAppearance(isLightTheme);
		}

		static void CreateStatusBarOverlay(Activity activity, AColor color)
		{
			if (activity?.Window?.DecorView is not ViewGroup decorView)
			{
				return;
			}

			// Remove existing overlay if it exists
			if (StatusBarOverlays.TryGetValue(activity, out var existingOverlay))
			{
				if (existingOverlay.Parent is ViewGroup parent)
				{
					parent.RemoveView(existingOverlay);
				}

				StatusBarOverlays.Remove(activity);
			}

			// Create a new colored overlay view
			var overlay = new View(activity)
			{
				Id = View.GenerateViewId()
			};
			overlay.SetBackgroundColor(color);

			// Add the overlay to the decor view
			decorView.AddView(overlay);
			StatusBarOverlays[activity] = overlay;

			// Position the overlay over the status bar area
			PositionStatusBarOverlay(activity, overlay);
		}

		static void PositionStatusBarOverlay(Activity activity, View overlay)
		{
			if (activity?.Window?.DecorView == null)
			{
				return;
			}

			// Set up a layout listener to position the overlay correctly once we have window insets
			var listener = new WindowInsetsListener(overlay);
			ViewCompat.SetOnApplyWindowInsetsListener(activity.Window.DecorView, listener);
		}

		static bool IsLightTheme(Activity activity)
		{
			// Check if the current theme is light or dark
			var uiModeFlags = activity.Resources?.Configuration?.UiMode & UiMode.NightMask;
			return uiModeFlags != UiMode.NightYes;
		}

		static void SetupWindowInsets(Activity activity)
		{
			if (activity?.Window?.DecorView == null)
			{
				return;
			}

			// Only set up basic window insets if no overlay is being used
			// If an overlay is being used, PositionStatusBarOverlay handles the insets
			if (!StatusBarOverlays.ContainsKey(activity))
			{
				var listener = new WindowInsetsListener();
				ViewCompat.SetOnApplyWindowInsetsListener(activity.Window.DecorView, listener);
			}
		}
	}
}