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

		public static Rect ComputeFrame(this IView view, Rect bounds)
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

			return new Rect(frameX, frameY, frameWidth, frameHeight);
		}

		static double AlignHorizontal(IView view, Rect bounds, Thickness margin)
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
			double frameX = startX + startMargin;

			switch (horizontalLayoutAlignment)
			{
				case LayoutAlignment.Center:
					frameX += (boundsWidth - desiredWidth) / 2;
					break;

				case LayoutAlignment.End:
					frameX += boundsWidth - desiredWidth;
					break;
			}

			return frameX;
		}

		static double AlignVertical(IView view, Rect bounds, Thickness margin)
		{
			double frameY = bounds.Y + margin.Top;

			switch (view.VerticalLayoutAlignment)
			{
				case LayoutAlignment.Center:
					frameY += (bounds.Height - view.DesiredSize.Height) / 2;
					break;

				case LayoutAlignment.End:
					frameY += bounds.Height - view.DesiredSize.Height;
					break;
			}

			return frameY;
		}

		public static Size MeasureContent(this IContentView contentView, double widthConstraint, double heightConstraint)
		{
			return contentView.MeasureContent(contentView.Padding, widthConstraint, heightConstraint);
		}

		public static Size MeasureContent(this IContentView contentView, Thickness inset, double widthConstraint, double heightConstraint)
		{
			var content = contentView.PresentedContent;

			var contentSize = Size.Zero;

			if (content != null)
			{
				contentSize = content.Measure(widthConstraint - inset.HorizontalThickness,
					heightConstraint - inset.VerticalThickness);
			}

			return new Size(contentSize.Width + inset.HorizontalThickness, contentSize.Height + inset.VerticalThickness);
		}

		public static void ArrangeContent(this IContentView contentView, Rect bounds)
		{
			if (contentView.PresentedContent == null)
			{
				return;
			}

			var padding = contentView.Padding;

			var targetBounds = new Rect(bounds.Left + padding.Left, bounds.Top + padding.Top,
				bounds.Width - padding.HorizontalThickness, bounds.Height - padding.VerticalThickness);

			_ = contentView.PresentedContent.Arrange(targetBounds);
		}

		public static Size AdjustForFill(this Size size, Rect bounds, IView view)
		{
			if (view.HorizontalLayoutAlignment == LayoutAlignment.Fill)
			{
				size.Width = Math.Max(bounds.Width, size.Width);
			}

			if (view.VerticalLayoutAlignment == LayoutAlignment.Fill)
			{
				size.Height = Math.Max(bounds.Height, size.Height);
			}

			return size;
		}
	}
}
