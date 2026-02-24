using System;
using Android.Content;
using AndroidX.Core.View;
using ARect = Android.Graphics.Rect;

namespace Microsoft.Maui.Platform;

internal readonly record struct SafeAreaPadding(double Left, double Right, double Top, double Bottom)
{
	public static SafeAreaPadding Empty { get; } = new(0, 0, 0, 0);
	public bool IsEmpty { get; } = Left == 0 && Right == 0 && Top == 0 && Bottom == 0;
}

internal static class WindowInsetsExtensions
{
	public static SafeAreaPadding ToSafeAreaInsetsPx(this WindowInsetsCompat insets, Context? context)
	{
		if (context == null)
			return SafeAreaPadding.Empty;

		// Get system bars insets (status bar, navigation bar)
		var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());

		// Get display cutout insets if available (API 28+)
		var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

		return new(
			Math.Max(systemBars?.Left ?? 0, displayCutout?.Left ?? 0),
			Math.Max(systemBars?.Right ?? 0, displayCutout?.Right ?? 0),
			Math.Max(systemBars?.Top ?? 0, displayCutout?.Top ?? 0),
			Math.Max(systemBars?.Bottom ?? 0, displayCutout?.Bottom ?? 0)
		);
	}

	public static SafeAreaPadding GetKeyboardInsetsPx(this WindowInsetsCompat insets, Context? context)
	{
		if (context == null)
			return SafeAreaPadding.Empty;

		// Get keyboard insets if available (API 30+)
		var keyboard = insets.GetInsets(WindowInsetsCompat.Type.Ime());
		var keyboardHeight = keyboard?.Bottom ?? 0;

		// The keyboard inset should represent the distance from the bottom of the screen
		// to the top of the keyboard. This is the correct value to use for bottom padding.
		return new(0, 0, 0, keyboardHeight);
	}
}