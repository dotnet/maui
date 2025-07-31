using System;
using Android.Graphics;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;

namespace Microsoft.Maui.Platform;

internal readonly record struct SafeAreaPadding(double Left, double Right, double Top, double Bottom)
{
	public static SafeAreaPadding Empty { get; } = new(0, 0, 0, 0);

	public bool IsEmpty { get; } = Left == 0 && Right == 0 && Top == 0 && Bottom == 0;
	public double HorizontalThickness { get; } = Left + Right;
	public double VerticalThickness { get; } = Top + Bottom;

	public Rect InsetRect(Rect bounds)
	{
		if (IsEmpty)
		{
			return bounds;
		}

		return new Rect(
			(int)(bounds.Left + Left),
			(int)(bounds.Top + Top),
			(int)(bounds.Right - Right),
			(int)(bounds.Bottom - Bottom));
	}

	public Insets ToInsets() =>
		Insets.Of((int)Left, (int)Top, (int)Right, (int)Bottom);
}

internal static class WindowInsetsExtensions
{
	public static SafeAreaPadding ToSafeAreaInsets(this WindowInsetsCompat insets)
	{
		// Get system bars insets (status bar, navigation bar)
		var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
		
		// Get display cutout insets if available (API 28+)
		var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());
		
		// Combine insets, taking the maximum for each edge
		return new(
			Math.Max(systemBars.Left, displayCutout.Left),
			Math.Max(systemBars.Right, displayCutout.Right),
			Math.Max(systemBars.Top, displayCutout.Top),
			Math.Max(systemBars.Bottom, displayCutout.Bottom)
		);
	}

	public static SafeAreaPadding ToSafeAreaInsetsWithKeyboard(this WindowInsetsCompat insets)
	{
		// Get base safe area insets
		var safeArea = insets.ToSafeAreaInsets();
		
		// Get keyboard insets if available (API 30+)
		var keyboard = insets.GetInsets(WindowInsetsCompat.Type.Ime());
		
		// For keyboard, we only care about the bottom inset and take the maximum
		return new(
			safeArea.Left,
			safeArea.Right,
			safeArea.Top,
			Math.Max(safeArea.Bottom, keyboard.Bottom)
		);
	}

	public static SafeAreaPadding GetKeyboardInsets(this WindowInsetsCompat insets)
	{
		// Get keyboard insets if available (API 30+)
		var keyboard = insets.GetInsets(WindowInsetsCompat.Type.Ime());
		
		// Return only keyboard insets (typically only bottom)
		return new(0, 0, 0, keyboard.Bottom);
	}
}