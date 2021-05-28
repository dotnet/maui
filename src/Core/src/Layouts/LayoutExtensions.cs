using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Layouts
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

			var margin = frameworkElement.GetMargin();

			// Adjust the constraints to account for the margins
			widthConstraint -= margin.HorizontalThickness;
			heightConstraint -= margin.VerticalThickness;

			// Ask the handler to do the actual measuring								
			var measureWithMargins = frameworkElement.Handler.GetDesiredSize(widthConstraint, heightConstraint);

			// Account for the margins when reporting the desired size value
			return new Size(measureWithMargins.Width + margin.HorizontalThickness,
				measureWithMargins.Height + margin.VerticalThickness);
		}

		public static Rectangle ComputeFrame(this IFrameworkElement frameworkElement, Rectangle bounds)
		{
			Thickness margin = frameworkElement.GetMargin();

			// Normally the frame's width will be the element's desired width
			var frameWidth = frameworkElement.DesiredSize.Width;

			// But, if the element is set to fill horizontally and it doesn't have an explicitly set width,
			// the frame width will be large enough to fill the space
			if (frameworkElement.HorizontalLayoutAlignment == LayoutAlignment.Fill && frameworkElement.Width == -1)
			{
				frameWidth = Math.Max(0, bounds.Width - margin.HorizontalThickness);
			}

			// Normally the frame's height will be the element's desired height
			var frameHeight = frameworkElement.DesiredSize.Height;

			// But, if the element is set to fill vertically and it doesn't have an explicitly set height,
			// the frame height will be large enough to fill the space
			if (frameworkElement.VerticalLayoutAlignment == LayoutAlignment.Fill && frameworkElement.Height == -1)
			{
				frameHeight = Math.Max(0, bounds.Height - margin.VerticalThickness);
			}

			var frameX = AlignHorizontal(frameworkElement, bounds, margin);
			var frameY = AlignVertical(frameworkElement, bounds, margin);

			return new Rectangle(frameX, frameY, frameWidth, frameHeight);
		}

		static Thickness GetMargin(this IFrameworkElement frameworkElement)
		{
			if (frameworkElement is IView view)
				return view.Margin;

			return Thickness.Zero;
		}

		static double AlignHorizontal(IFrameworkElement frameworkElement, Rectangle bounds, Thickness margin)
		{
			var alignment = frameworkElement.HorizontalLayoutAlignment;
			var desiredWidth = frameworkElement.DesiredSize.Width;
			var startX = bounds.X;

			if (frameworkElement.FlowDirection == FlowDirection.LeftToRight)
			{
				return AlignHorizontal(startX, margin.Left, margin.Right, bounds.Width, desiredWidth, alignment);
			}

			// If the flowdirection is RTL, then we can use the same logic to determine the X position of the Frame;
			// we just have to flip a few parameters. First we flip the alignment if it's start or end:

			if (alignment == LayoutAlignment.End)
			{
				alignment = LayoutAlignment.Start;
			}
			else if (alignment == LayoutAlignment.Start)
			{
				alignment = LayoutAlignment.End;
			}

			// And then we swap the left and right margins: 
			return AlignHorizontal(startX, margin.Right, margin.Left, bounds.Width, desiredWidth, alignment);
		}

		static double AlignHorizontal(double startX, double startMargin, double endMargin, double boundsWidth,
			double desiredWidth, LayoutAlignment horizontalLayoutAlignment)
		{
			double frameX = 0;

			switch (horizontalLayoutAlignment)
			{
				case LayoutAlignment.Fill:
				case LayoutAlignment.Start:
					frameX = startX + startMargin;
					break;

				case LayoutAlignment.Center:

					frameX = (boundsWidth - desiredWidth) / 2;
					var marginOffset = (startMargin - endMargin) / 2;
					frameX += marginOffset;

					break;
				case LayoutAlignment.End:

					frameX = boundsWidth - endMargin - desiredWidth;
					break;
			}

			return frameX;
		}

		static double AlignVertical(IFrameworkElement frameworkElement, Rectangle bounds, Thickness margin)
		{
			double frameY = 0;

			switch (frameworkElement.VerticalLayoutAlignment)
			{
				case LayoutAlignment.Fill:

					frameY = bounds.Y + margin.Top;
					break;

				case LayoutAlignment.Start:

					frameY = bounds.Y + margin.Top;
					break;

				case LayoutAlignment.Center:

					frameY = (bounds.Height - frameworkElement.DesiredSize.Height) / 2;
					var offset = (margin.Top - margin.Bottom) / 2;
					frameY += offset;
					break;

				case LayoutAlignment.End:

					frameY = bounds.Height - margin.Bottom - frameworkElement.DesiredSize.Height;
					break;
			}

			return frameY;
		}
	}
}
