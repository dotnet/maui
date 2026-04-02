using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform;

internal readonly record struct SafeAreaPadding(double Left, double Right, double Top, double Bottom)
{
	public static SafeAreaPadding Empty { get; } = new(0, 0, 0, 0);

	public bool IsEmpty { get; } = Left == 0 && Right == 0 && Top == 0 && Bottom == 0;
	public double HorizontalThickness { get; } = Left + Right;
	public double VerticalThickness { get; } = Top + Bottom;

	public CGRect InsetRect(CGRect bounds)
	{
		if (IsEmpty)
		{
			return bounds;
		}

		return new CGRect(
			bounds.Left + Left,
			bounds.Top + Top,
			bounds.Width - HorizontalThickness,
			bounds.Height - VerticalThickness);
	}

	public CGRect ToCGRect() =>
		new((nfloat)Top, (nfloat)Left, (nfloat)Bottom, (nfloat)Right);

	/// <summary>
	/// Compares two SafeAreaPadding values at device-pixel resolution.
	/// Sub-pixel differences (e.g., 0.001pt from animation noise) that map to the same
	/// physical pixel are treated as equal, preventing unnecessary layout invalidation cycles.
	/// </summary>
	public bool EqualsAtPixelLevel(SafeAreaPadding other)
	{
		var scale = (double)UIScreen.MainScreen.Scale;
		return RoundToPixel(Left, scale) == RoundToPixel(other.Left, scale)
			&& RoundToPixel(Right, scale) == RoundToPixel(other.Right, scale)
			&& RoundToPixel(Top, scale) == RoundToPixel(other.Top, scale)
			&& RoundToPixel(Bottom, scale) == RoundToPixel(other.Bottom, scale);
	}

	static double RoundToPixel(double value, double scale)
		=> Math.Round(value * scale, MidpointRounding.AwayFromZero);
}

internal static class SafeAreaInsetsExtensions
{
	public static SafeAreaPadding ToSafeAreaInsets(this UIEdgeInsets insets)
	{
		// Filters out negligible floating-point values from UIKit that may cause layout issues (e.g., 3.5527136788005009e-15).
		const double tolerance = 1e-14;

		static double ApplyTolerance(double value) => Math.Abs(value) < tolerance ? 0 : value;

		return new(
			ApplyTolerance(insets.Left),
			ApplyTolerance(insets.Right),
			ApplyTolerance(insets.Top),
			ApplyTolerance(insets.Bottom)
		);
	}
}