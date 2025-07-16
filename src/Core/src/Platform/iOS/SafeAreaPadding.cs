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
}

internal static class SafeAreaInsetsExtensions
{
	public static SafeAreaPadding ToSafeAreaInsets(this UIEdgeInsets insets) =>
		new(insets.Left, insets.Right, insets.Top, insets.Bottom);
}