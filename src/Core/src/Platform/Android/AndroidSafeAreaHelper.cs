using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Modern Android insets handling helper following Android 15+ edge-to-edge best practices.
	/// Uses proper inset listener patterns and inset consumption for optimal compatibility.
	/// </summary>
	internal static class AndroidSafeAreaHelper
	{
		/// <summary>
		/// Sets up modern window insets handling for a view based on whether it should respect safe areas.
		/// Uses ViewCompat.setOnApplyWindowInsetsListener for proper inset handling patterns.
		/// </summary>
		/// <param name="view">The Android view to configure insets handling for</param>
		/// <param name="layout">The cross-platform layout to check for safe area preferences</param>
		public static void SetupWindowInsetsHandling(View view, ICrossPlatformLayout? layout)
		{
			if (layout is not ISafeAreaView safeAreaView)
			{
				// If not a safe area view, clear any existing listener and let insets pass through
				ViewCompat.SetOnApplyWindowInsetsListener(view, null);
				return;
			}

			// Set up modern insets listener using ViewCompat for compatibility
			ViewCompat.SetOnApplyWindowInsetsListener(view, (v, insets) =>
			{
				return HandleWindowInsets(v, insets, safeAreaView.IgnoreSafeArea);
			});

			// Request insets to be applied initially
			ViewCompat.RequestApplyInsets(view);
		}

		/// <summary>
		/// Modern window insets handler that follows Android 15+ best practices.
		/// Properly consumes insets when handled and passes through when ignored.
		/// </summary>
		/// <param name="view">The view receiving the insets</param>
		/// <param name="insets">The window insets to handle</param>
		/// <param name="ignoreSafeArea">Whether to ignore safe areas (allow edge-to-edge)</param>
		/// <returns>The remaining insets after processing</returns>
		private static WindowInsetsCompat HandleWindowInsets(View view, WindowInsetsCompat insets, bool ignoreSafeArea)
		{
			if (ignoreSafeArea)
			{
				// When ignoring safe areas, reset any margins and let content go edge-to-edge
				ResetViewMargins(view);
				// Don't consume insets - pass them through to allow edge-to-edge behavior
				return insets;
			}

			// When respecting safe areas, apply them as margins for modern, flexible layout
			ApplySafeAreaAsMargins(view, insets);

			// Return the original insets since we've handled them by applying margins
			// This allows proper inset dispatch to children while indicating we've processed them
			return insets;
		}

		/// <summary>
		/// Applies safe area insets as margins rather than padding for more flexible layouts.
		/// This is the modern approach that works better with complex view hierarchies.
		/// </summary>
		/// <param name="view">The view to apply margins to</param>
		/// <param name="insets">The window insets containing safe area information</param>
		private static void ApplySafeAreaAsMargins(View view, WindowInsetsCompat insets)
		{
			// Get the safe area insets by combining system bars and display cutouts
			var systemInsets = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
			var cutoutInsets = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

			// Use the maximum of system and cutout insets for comprehensive safe area
			var safeInsets = MaxInsets(systemInsets, cutoutInsets);

			// Apply as margins which are more flexible than padding for complex layouts
			if (view.LayoutParameters is ViewGroup.MarginLayoutParams marginParams)
			{
				marginParams.SetMargins(
					safeInsets.Left,
					safeInsets.Top,
					safeInsets.Right,
					safeInsets.Bottom
				);
				view.LayoutParameters = marginParams;
			}
		}

		/// <summary>
		/// Resets view margins when safe areas should be ignored.
		/// </summary>
		/// <param name="view">The view to reset margins for</param>
		private static void ResetViewMargins(View view)
		{
			if (view.LayoutParameters is ViewGroup.MarginLayoutParams marginParams)
			{
				marginParams.SetMargins(0, 0, 0, 0);
				view.LayoutParameters = marginParams;
			}
		}

		/// <summary>
		/// Gets safe area insets in device-independent units for cross-platform use.
		/// </summary>
		/// <param name="view">The view to get insets from</param>
		/// <param name="context">Android context for unit conversion</param>
		/// <returns>Safe area insets in device-independent units, or Thickness.Zero if unavailable</returns>
		public static Thickness GetSafeAreaInsets(View view, Context context)
		{
			var insets = ViewCompat.GetRootWindowInsets(view);
			if (insets == null)
				return Thickness.Zero;

			var systemInsets = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
			var cutoutInsets = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());
			var safeInsets = MaxInsets(systemInsets, cutoutInsets);

			return new Thickness(
				context.FromPixels(safeInsets.Left),
				context.FromPixels(safeInsets.Top),
				context.FromPixels(safeInsets.Right),
				context.FromPixels(safeInsets.Bottom)
			);
		}

		/// <summary>
		/// Checks if the given layout should handle window insets based on safe area preferences.
		/// </summary>
		/// <param name="layout">The cross-platform layout to check</param>
		/// <returns>True if the layout implements ISafeAreaView</returns>
		public static bool ShouldHandleWindowInsets(ICrossPlatformLayout? layout)
		{
			return layout is ISafeAreaView;
		}

		/// <summary>
		/// Calculates the maximum insets from two Android.Graphics.Insets objects.
		/// Uses the built-in Android.Graphics.Insets.Max() method when available.
		/// </summary>
		/// <param name="first">The first insets object</param>
		/// <param name="second">The second insets object</param>
		/// <returns>Insets containing the maximum values from both inputs</returns>
		private static Android.Graphics.Insets MaxInsets(Android.Graphics.Insets first, Android.Graphics.Insets second)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(29))
			{
				// Use the built-in Max method available in API 29+
				return Android.Graphics.Insets.Max(first, second);
			}
			else
			{
				// For older Android versions, calculate manually
				return Android.Graphics.Insets.Of(
					Math.Max(first.Left, second.Left),
					Math.Max(first.Top, second.Top),
					Math.Max(first.Right, second.Right),
					Math.Max(first.Bottom, second.Bottom)
				);
			}
		}
	}
}