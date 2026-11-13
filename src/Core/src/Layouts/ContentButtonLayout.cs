using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Layouts
{
	internal static class ContentButtonLayout
	{
		internal static Thickness GetInset(IContentButton button) => button.Padding + Math.Max(0, button.StrokeThickness);

		internal static Size Measure(IContentButton button, double widthConstraint, double heightConstraint)
		{
			var inset = GetInset(button);
			var width = GetContentMeasureConstraint(widthConstraint, button.Width, button.MinimumWidth, button.MaximumWidth);
			var height = GetContentMeasureConstraint(heightConstraint, button.Height, button.MinimumHeight, button.MaximumHeight);

			// The shared helper subtracts the inset. Supply at least the inset so tiny
			// buttons never pass negative constraints to their content.
			var size = button.MeasureContent(inset,
				Math.Max(inset.HorizontalThickness, width),
				Math.Max(inset.VerticalThickness, height), false, false);

			return new Size(
				Math.Max(0, LayoutManager.ResolveConstraints(widthConstraint, button.Width, size.Width, button.MinimumWidth, button.MaximumWidth)),
				Math.Max(0, LayoutManager.ResolveConstraints(heightConstraint, button.Height, size.Height, button.MinimumHeight, button.MaximumHeight)));
		}

		// Explicit sizes resolve with the same minimum and maximum used for the final size,
		// so content is measured at the width or height where it will be arranged.
		static double GetContentMeasureConstraint(double externalConstraint, double explicitLength, double minimum, double maximum) =>
			Dimension.IsExplicitSet(explicitLength)
				? LayoutManager.ResolveConstraints(externalConstraint, explicitLength, explicitLength, minimum, maximum)
				: Math.Min(externalConstraint, maximum);

		internal static Size Arrange(IContentButton button, Rect bounds)
		{
			button.ArrangeContent(GetInset(button), bounds);
			return bounds.Size;
		}
	}
}
