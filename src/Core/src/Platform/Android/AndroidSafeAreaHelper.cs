using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;
using Rectangle = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Platform
{
	internal static class AndroidSafeAreaHelper
	{
		/// <summary>
		/// Adjusts the given bounds to account for safe areas if the layout implements ISafeAreaView
		/// and has IgnoreSafeArea set to false.
		/// </summary>
		/// <param name="view">The Android view to get window insets from</param>
		/// <param name="context">The Android context for unit conversion</param>
		/// <param name="bounds">The bounds to adjust</param>
		/// <param name="layout">The cross-platform layout to check for safe area preferences</param>
		/// <returns>Adjusted bounds with safe area insets applied, or original bounds if no adjustment needed</returns>
		public static Rectangle AdjustForSafeArea(View view, Context? context, Rectangle bounds, ICrossPlatformLayout? layout)
		{
			if (layout is not ISafeAreaView sav || sav.IgnoreSafeArea || context == null)
			{
				return bounds;
			}

			var insets = ViewCompat.GetRootWindowInsets(view);
			if (insets == null)
			{
				return bounds;
			}

			// Get system window insets (status bar, navigation bar, etc.)
			var systemInsets = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
			var safeAreaInsets = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

			// Use the maximum of system insets and cutout insets for true safe area
			var left = Math.Max(systemInsets?.Left ?? 0, safeAreaInsets?.Left ?? 0);
			var top = Math.Max(systemInsets?.Top ?? 0, safeAreaInsets?.Top ?? 0);
			var right = Math.Max(systemInsets?.Right ?? 0, safeAreaInsets?.Right ?? 0);
			var bottom = Math.Max(systemInsets?.Bottom ?? 0, safeAreaInsets?.Bottom ?? 0);

			// Convert Android pixels to device-independent units
			var leftDip = context.FromPixels(left);
			var topDip = context.FromPixels(top);
			var rightDip = context.FromPixels(right);
			var bottomDip = context.FromPixels(bottom);

			// Apply safe area insets to bounds
			return new Rectangle(
				bounds.X + leftDip,
				bounds.Y + topDip,
				bounds.Width - leftDip - rightDip,
				bounds.Height - topDip - bottomDip);
		}

		/// <summary>
		/// Determines if the given layout should trigger a layout update when window insets change.
		/// </summary>
		/// <param name="layout">The cross-platform layout to check</param>
		/// <returns>True if the layout cares about safe areas and should update on inset changes</returns>
		public static bool ShouldHandleWindowInsets(ICrossPlatformLayout? layout)
		{
			return layout is ISafeAreaView sav && !sav.IgnoreSafeArea;
		}
	}
}