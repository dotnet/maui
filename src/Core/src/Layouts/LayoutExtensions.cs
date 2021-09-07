using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Layouts
{
	public static class LayoutExtensions
	{
		public static Size ComputeDesiredSize(this IView view, double widthConstraint, double heightConstraint)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));

			if (view.Handler == null)
			{
				return Size.Zero;
			}

			var margin = view.Margin;

			// Adjust the constraints to account for the margins
			widthConstraint -= margin.HorizontalThickness;
			heightConstraint -= margin.VerticalThickness;

			// Ask the handler to do the actual measuring
			var measureWithMargins = view.Handler.GetDesiredSize(widthConstraint, heightConstraint);

			// Account for the margins when reporting the desired size value
			return new Size(measureWithMargins.Width + margin.HorizontalThickness,
				measureWithMargins.Height + margin.VerticalThickness);
		}

		public static Rectangle ComputeFrame(this IView view, Rectangle bounds)
		{
			Thickness margin = view.Margin;

			// We need to determine the width the element wants to consume; normally that's the element's DesiredSize.Width
			var consumedWidth = view.DesiredSize.Width;

			if (view.HorizontalLayoutAlignment == LayoutAlignment.Fill && !IsExplicitSet(view.Width))
			{
				// But if the element is set to fill horizontally and it doesn't have an explicitly set width,
				// then we want the width of the entire bounds
				consumedWidth = bounds.Width;
			}

			// And the actual frame width needs to subtract the margins
			var frameWidth = Math.Max(0, consumedWidth - margin.HorizontalThickness);

			// We need to determine the height the element wants to consume; normally that's the element's DesiredSize.Height
			var consumedHeight = view.DesiredSize.Height;

			// But, if the element is set to fill vertically and it doesn't have an explicitly set height,
			// then we want the height of the entire bounds
			if (view.VerticalLayoutAlignment == LayoutAlignment.Fill && !IsExplicitSet(view.Height))
			{
				consumedHeight = bounds.Height;
			}

			// And the actual frame height needs to subtract the margins
			var frameHeight = Math.Max(0, consumedHeight - margin.VerticalThickness);

			var frameX = AlignHorizontal(view, bounds, margin);
			var frameY = AlignVertical(view, bounds, margin);

			return new Rectangle(frameX, frameY, frameWidth, frameHeight);
		}

		static double AlignHorizontal(IView view, Rectangle bounds, Thickness margin)
		{
			var alignment = view.HorizontalLayoutAlignment;
			var desiredWidth = view.DesiredSize.Width;
			var startX = bounds.X;

			if (view.FlowDirection == FlowDirection.LeftToRight)
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

					frameX = startX + ((boundsWidth - desiredWidth) / 2);
					var marginOffset = (startMargin - endMargin) / 2;
					frameX += marginOffset;

					break;
				case LayoutAlignment.End:

					frameX = startX + boundsWidth - desiredWidth + startMargin;
					break;
			}

			return frameX;
		}

		static double AlignVertical(IView view, Rectangle bounds, Thickness margin)
		{
			double frameY = 0;
			var startY = bounds.Y;

			switch (view.VerticalLayoutAlignment)
			{
				case LayoutAlignment.Fill:
				case LayoutAlignment.Start:

					frameY = startY + margin.Top;
					break;

				case LayoutAlignment.Center:

					frameY = startY + ((bounds.Height - view.DesiredSize.Height) / 2);
					var offset = (margin.Top - margin.Bottom) / 2;
					frameY += offset;
					break;

				case LayoutAlignment.End:

					frameY = startY + bounds.Height - view.DesiredSize.Height + margin.Top;
					break;
			}

			return frameY;
		}

		public static Size MeasureContent(this IContentView contentView, double widthConstraint, double heightConstraint) 
		{
			var content = contentView.PresentedContent;
			var padding = contentView.Padding;

			var contentSize = Size.Zero;

			if (content != null)
			{
				contentSize = content.Measure(widthConstraint - padding.HorizontalThickness,
					heightConstraint - padding.VerticalThickness);
			}

			return new Size(contentSize.Width + padding.HorizontalThickness, contentSize.Height + padding.VerticalThickness);
		}

		public static void ArrangeContent(this IContentView contentView, Rectangle bounds) 
		{
			if (contentView.PresentedContent == null)
			{
				return;
			}

			var padding = contentView.Padding;

			var targetBounds = new Rectangle(bounds.Left + padding.Left, bounds.Top + padding.Top,
				bounds.Width - padding.HorizontalThickness, bounds.Height - padding.VerticalThickness);

			_ = contentView.PresentedContent.Arrange(targetBounds);
		}
	}
}
