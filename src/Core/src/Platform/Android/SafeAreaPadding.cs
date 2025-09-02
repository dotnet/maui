using System;
using Android.Content;
using AndroidX.Core.View;
using ARect = Android.Graphics.Rect;
using AInsets = AndroidX.Core.Graphics.Insets;

namespace Microsoft.Maui.Platform;

internal readonly record struct SafeAreaPadding(double Left, double Right, double Top, double Bottom)
{
	public static SafeAreaPadding Empty { get; } = new(0, 0, 0, 0);

	public bool IsEmpty { get; } = Left == 0 && Right == 0 && Top == 0 && Bottom == 0;
	public double HorizontalThickness { get; } = Left + Right;
	public double VerticalThickness { get; } = Top + Bottom;

	public ARect InsetRect(ARect bounds)
	{
		if (IsEmpty)
		{
			return bounds;
		}

		return new ARect(
			(int)(bounds.Left + Left),
			(int)(bounds.Top + Top),
			(int)(bounds.Right - Right),
			(int)(bounds.Bottom - Bottom));
	}

	public Graphics.Rect InsetRectF(Graphics.Rect bounds)
	{
		if (IsEmpty)
		{
			return bounds;
		}

		return new Graphics.Rect(
			bounds.X + Left,
			bounds.Y + Top,
			bounds.Width - HorizontalThickness,
			bounds.Height - VerticalThickness);
	}
}

internal static class WindowInsetsExtensions
{
	public static SafeAreaPadding ToSafeAreaInsets(this WindowInsetsCompat insets, Context? context)
	{
		if (context == null)
			return SafeAreaPadding.Empty;

		// Get system bars insets (status bar, navigation bar)
		var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());

		// Get display cutout insets if available (API 28+)
		var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

		// Combine insets, taking the maximum for each edge
		// Convert from pixels to device-independent units
		var density = context.GetDisplayDensity();
		return new(
			Math.Max(systemBars?.Left ?? 0, displayCutout?.Left ?? 0) / density,
			Math.Max(systemBars?.Right ?? 0, displayCutout?.Right ?? 0) / density,
			Math.Max(systemBars?.Top ?? 0, displayCutout?.Top ?? 0) / density,
			Math.Max(systemBars?.Bottom ?? 0, displayCutout?.Bottom ?? 0) / density
		);
	}

	public static SafeAreaPadding GetKeyboardInsets(this WindowInsetsCompat insets, Context? context)
	{
		if (context == null)
			return SafeAreaPadding.Empty;

		// Get keyboard insets if available (API 30+)
		var keyboard = insets.GetInsets(WindowInsetsCompat.Type.Ime());

		// Return only keyboard insets (typically only bottom)
		// Convert from pixels to device-independent units
		var density = context.GetDisplayDensity();
		var keyboardHeight = (keyboard?.Bottom ?? 0) / density;

		// The keyboard inset should represent the distance from the bottom of the screen
		// to the top of the keyboard. This is the correct value to use for bottom padding.
		return new(0, 0, 0, keyboardHeight);
	}
}