using System;
using Xamarin.Forms;

namespace Xamarin.Platform.Layouts
{
	public static class LayoutExtensions
	{
		public static Size ComputeDesiredSize(this IFrameworkElement frameworkElement, double widthConstraint, double heightConstraint)
		{
			_ = frameworkElement ?? throw new ArgumentNullException(nameof(frameworkElement));

			if (frameworkElement.Handler == null)
			{
				return Size.Zero;
			}

			var margin = frameworkElement.Margin;

			// Adjust the constraints to account for the margins
			widthConstraint -= margin.HorizontalThickness;
			heightConstraint -= margin.VerticalThickness;

			// Determine whether the external constraints or the requested size values will determine the measurements
			widthConstraint = LayoutManager.ResolveConstraints(widthConstraint, frameworkElement.Width);
			heightConstraint = LayoutManager.ResolveConstraints(heightConstraint, frameworkElement.Height);

			// Ask the handler to do the actual measuring								
			var measureWithMargins = frameworkElement.Handler.GetDesiredSize(widthConstraint, heightConstraint);

			// Account for the margins when reporting the desired size value
			return new Size(measureWithMargins.Width + margin.HorizontalThickness,
				measureWithMargins.Height + margin.VerticalThickness);
		}

		public static Rectangle ComputeFrame(this IFrameworkElement frameworkElement, Rectangle bounds)
		{
			var margin = frameworkElement.Margin;

			// If the margins are too big for the bounds, then simply collapse them to zero
			var frameWidth = Math.Max(0, bounds.Width - margin.HorizontalThickness);
			var frameHeight = Math.Max(0, bounds.Height - margin.VerticalThickness);

			return new Rectangle(bounds.X + margin.Left, bounds.Y + margin.Top,
				frameWidth, frameHeight);
		}
	}
}
