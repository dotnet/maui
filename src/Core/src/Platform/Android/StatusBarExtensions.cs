using Android.Content.Res;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using AColor = Android.Graphics.Color;
using Activity = Android.App.Activity;

namespace Microsoft.Maui.Platform
{
	internal static class StatusBarExtensions
	{
		const int StatusBarOverlayId = 0x7F000001;
		
		/// <summary>
		/// Sets up edge-to-edge handling for Android API 36+ where edge-to-edge is enforced.
		/// </summary>
		public static void SetEdgeToEdge(this Activity activity)
		{
			if (activity?.Window is null)
				return;

			if (Build.VERSION.SdkInt >= (BuildVersionCodes)36)
			{
				SetEdgeToEdge(activity.Window);
			}
		}

		/// <summary>
		/// Configures window for edge-to-edge display on Android API 36+.
		/// Sets transparent status bar, disables system window fitting, and applies proper window insets.
		/// </summary>
		public static void SetEdgeToEdge(this Window window)
		{
			if (window is null)
				return;

			if (Build.VERSION.SdkInt >= (BuildVersionCodes)36)
			{
				window.SetStatusBarColor(AColor.Transparent);
				WindowCompat.SetDecorFitsSystemWindows(window, false);
				SetWindowInsets(window);
			}
		}
		
		/// <summary>
		/// Sets the status bar color for the activity's window.
		/// </summary>
		public static void SetStatusBarColorWithEdgeToEdge(this Activity activity, AColor color)
		{
			activity?.Window?.SetStatusBarColorWithEdgeToEdge(color);
		}
		
		/// <summary>
		/// Sets the status bar color with proper handling for different Android API levels.
		/// On API 36+, uses edge-to-edge with colored overlay. On older versions, sets color directly.
		/// </summary>
		public static void SetStatusBarColorWithEdgeToEdge(this Window window, AColor color)
		{
			if (window is null)
				return;

			var context = window.Context;
			var uiModeFlags = context?.Resources?.Configuration?.UiMode & UiMode.NightMask;
			bool isLightTheme = uiModeFlags != UiMode.NightYes;

			if (Build.VERSION.SdkInt >= (BuildVersionCodes)36)
			{
				window.SetEdgeToEdge();
				window.SetStatusBarColorForEdgeToEdge(color);
				SetStatusBarAppearance(window, isLightTheme);
			}
			else
			{
				// Pre-API 36: Direct color setting
				window.SetStatusBarColor(color);
				
				if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
				{
					SetStatusBarAppearance(window, isLightTheme);
				}
			}
		}
		
		/// <summary>
		/// Sets status bar icon appearance (light/dark) based on theme.
		/// </summary>
		static void SetStatusBarAppearance(Window window, bool isLightTheme)
		{
			var windowInsetsController = WindowCompat.GetInsetsController(window, window.DecorView);
			if (windowInsetsController is not null)
			{
				windowInsetsController.AppearanceLightStatusBars = isLightTheme;
			}
		}
		
		/// <summary>
		/// Configures window insets handling to prevent content overlap with system bars.
		/// Applied to the content root view with margin-based positioning.
		/// </summary>
		static void SetWindowInsets(Window window)
		{
			if (window is null)
				return;

			var rootView = window.DecorView?.FindViewById(global::Android.Resource.Id.Content);
			
			if (rootView != null)
			{
				ViewCompat.SetOnApplyWindowInsetsListener(rootView, new WindowInsetsListener());
			}
		}
		
		/// <summary>
		/// Creates a colored overlay view positioned in the status bar area for edge-to-edge scenarios.
		/// Replaces any existing overlay to prevent stacking.
		/// </summary>
		static void SetStatusBarColorForEdgeToEdge(this Window window, AColor color)
		{
			if (window?.DecorView is not ViewGroup decorView)
				return;
			
			// Remove existing overlay if present
			var existingOverlay = decorView.FindViewById(StatusBarOverlayId);
			if (existingOverlay != null)
			{
				decorView.RemoveView(existingOverlay);
			}

			// Create new colored overlay
			var statusBarOverlay = new View(window.Context)
			{
				Id = StatusBarOverlayId,
				LayoutParameters = new ViewGroup.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.WrapContent)
			};
			statusBarOverlay.SetBackgroundColor(color);

			decorView.AddView(statusBarOverlay, 0); // Add behind content
			ViewCompat.SetOnApplyWindowInsetsListener(statusBarOverlay, new StatusBarOverlayInsetsListener());
		}
	}

	/// <summary>
	/// Applies system bar insets as margins to prevent content overlap in edge-to-edge scenarios.
	/// </summary>
	internal class WindowInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
	{
		public WindowInsetsCompat? OnApplyWindowInsets(View? view, WindowInsetsCompat? insets)
		{
			if (view is null || insets is null)
				return insets;

			var systemBarInsets = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
			
			if (view.LayoutParameters is ViewGroup.MarginLayoutParams marginParams && systemBarInsets != null)
			{
				marginParams.LeftMargin = systemBarInsets.Left;
				marginParams.BottomMargin = systemBarInsets.Bottom;
				marginParams.RightMargin = systemBarInsets.Right;
				marginParams.TopMargin = systemBarInsets.Top;
				
				view.LayoutParameters = marginParams;
			}

			return WindowInsetsCompat.Consumed;
		}
	}
	
	/// <summary>
	/// Positions the status bar overlay to match the system status bar height.
	/// </summary>
	internal class StatusBarOverlayInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
	{
		public WindowInsetsCompat? OnApplyWindowInsets(View? view, WindowInsetsCompat? insets)
		{
			if (view is null || insets is null)
				return insets;

			var statusBarInsets = insets.GetInsets(WindowInsetsCompat.Type.StatusBars());
			
			if (view.LayoutParameters is ViewGroup.LayoutParams layoutParams && statusBarInsets != null)
			{
				layoutParams.Height = statusBarInsets.Top;
				view.LayoutParameters = layoutParams;
			}

			return insets;
		}
	}
}