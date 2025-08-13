using System;
using Android.Graphics;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;

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

	public Insets ToInsets() =>
		Insets.Of((int)Left, (int)Top, (int)Right, (int)Bottom);
}

internal static class WindowInsetsExtensions
{
	public static SafeAreaPadding ToSafeAreaInsets(this WindowInsetsCompat insets, Context context)
	{
		// Get system bars insets (status bar, navigation bar)
		var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
		
		// Get display cutout insets if available (API 28+)
		var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());
		
		// For safe area calculation, we need to ensure content avoids both system bars and display cutouts
		// Take the maximum of each edge to ensure we avoid both types of obstructions
		// Convert from pixels to device-independent units
		var density = context.GetDisplayDensity();
		
		// Check if we have valid display cutout insets and ensure they're properly applied
		// Sometimes display cutout insets might be 0 if the window isn't configured properly
		var effectiveLeft = Math.Max(systemBars.Left, displayCutout.Left);
		var effectiveRight = Math.Max(systemBars.Right, displayCutout.Right);
		var effectiveTop = Math.Max(systemBars.Top, displayCutout.Top);
		var effectiveBottom = Math.Max(systemBars.Bottom, displayCutout.Bottom);
		
		// For debugging: Log if we have display cutout insets
		if (displayCutout.Top > 0 || displayCutout.Bottom > 0 || displayCutout.Left > 0 || displayCutout.Right > 0)
		{
			System.Diagnostics.Debug.WriteLine($"SafeArea: Display cutout found - L={displayCutout.Left}, T={displayCutout.Top}, R={displayCutout.Right}, B={displayCutout.Bottom}");
		}
		else
		{
			System.Diagnostics.Debug.WriteLine("SafeArea: No display cutout insets detected");
		}
		
		return new(
			effectiveLeft / density,
			effectiveRight / density,
			effectiveTop / density,
			effectiveBottom / density
		);
	}

	public static SafeAreaPadding ToSafeAreaInsetsWithKeyboard(this WindowInsetsCompat insets, Context context)
	{
		// Get base safe area insets
		var safeArea = insets.ToSafeAreaInsets(context);
		
		// Get keyboard insets if available (API 30+)
		var keyboard = insets.GetInsets(WindowInsetsCompat.Type.Ime());
		
		// For keyboard, we only care about the bottom inset and take the maximum
		// Convert from pixels to device-independent units
		return new(
			safeArea.Left,
			safeArea.Right,
			safeArea.Top,
			Math.Max(safeArea.Bottom, keyboard.Bottom / context.GetDisplayDensity())
		);
	}

	public static SafeAreaPadding GetKeyboardInsets(this WindowInsetsCompat insets, Context context)
	{
		// Get keyboard insets if available (API 30+)
		var keyboard = insets.GetInsets(WindowInsetsCompat.Type.Ime());
		
		// Return only keyboard insets (typically only bottom)
		// Convert from pixels to device-independent units
		return new(0, 0, 0, keyboard.Bottom / context.GetDisplayDensity());
	}

	/// <summary>
	/// Debug method to get detailed insets information for troubleshooting display cutout issues
	/// </summary>
	public static string GetInsetsDebugInfo(this WindowInsetsCompat insets, Context context)
	{
		var density = context.GetDisplayDensity();
		var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
		var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());
		var keyboard = insets.GetInsets(WindowInsetsCompat.Type.Ime());
		
		return $"SystemBars: L={systemBars.Left}, T={systemBars.Top}, R={systemBars.Right}, B={systemBars.Bottom} | " +
		       $"DisplayCutout: L={displayCutout.Left}, T={displayCutout.Top}, R={displayCutout.Right}, B={displayCutout.Bottom} | " +
		       $"IME: L={keyboard.Left}, T={keyboard.Top}, R={keyboard.Right}, B={keyboard.Bottom} | " +
		       $"Density: {density}";
	}
}