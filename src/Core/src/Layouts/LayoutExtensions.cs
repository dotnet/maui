using System;
using System.Runtime.CompilerServices;
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
			var measureWithoutMargins = view.Handler.GetDesiredSize(widthConstraint, heightConstraint);

			// Account for the margins when reporting the desired size value
			return new Size(measureWithoutMargins.Width + margin.HorizontalThickness,
				measureWithoutMargins.Height + margin.VerticalThickness);
		}

		public static Rect ComputeFrame(this IView view, Rect bounds)
		{
			Thickness margin = view.Margin;

			// We need to determine the width the element wants to consume; normally that's the element's DesiredSize.Width
			var consumedWidth = view.DesiredSize.Width;

			// But if the element is set to fill horizontally and it doesn't have an explicitly set width,
			// then we want the minimum between its MaximumWidth and the bounds' width
			// MaximumWidth is always positive infinity if not defined by the user
			if (view.HorizontalLayoutAlignment == LayoutAlignment.Fill && !IsExplicitSet(view.Width))
			{
				consumedWidth = Math.Min(bounds.Width, view.MaximumWidth);
			}

			// And the actual frame width needs to subtract the margins
			var frameWidth = Math.Max(0, consumedWidth - margin.HorizontalThickness);

			// We need to determine the height the element wants to consume; normally that's the element's DesiredSize.Height
			var consumedHeight = view.DesiredSize.Height;

			// But, if the element is set to fill vertically and it doesn't have an explicitly set height,
			// then we want the minimum between its MaximumHeight  and the bounds' height
			// MaximumHeight is always positive infinity if not defined by the user
			if (view.VerticalLayoutAlignment == LayoutAlignment.Fill && !IsExplicitSet(view.Height))
			{
				consumedHeight = Math.Min(bounds.Height, view.MaximumHeight);
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

			if (alignment == LayoutAlignment.Fill && (IsExplicitSet(view.Width) || !double.IsInfinity(view.MaximumWidth)))
			{
				// If the view has an explicit width (or non-infinite MaxWidth) set and the layout alignment is Fill,
				// we just treat the view as centered within the space it "fills"
				alignment = LayoutAlignment.Center;

				// If the width is not set, we use the minimum between the MaxWidth or the bound's width
				desiredWidth = IsExplicitSet(view.Width) ? desiredWidth : Math.Min(bounds.Width, view.MaximumWidth);
			}

			return AlignHorizontal(bounds.X, margin.Left, margin.Right, bounds.Width, desiredWidth, alignment);
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
			var alignment = view.VerticalLayoutAlignment;
			var desiredHeight = view.DesiredSize.Height;

			if (alignment == LayoutAlignment.Fill && (IsExplicitSet(view.Height) || !double.IsInfinity(view.MaximumHeight)))
			{
				// If the view has an explicit height (or non-infinite MaxHeight) set and the layout alignment is Fill,
				// we just treat the view as centered within the space it "fills"
				alignment = LayoutAlignment.Center;

				// If the height is not set, we use the minimum between the MaxHeight or the bound's height
				desiredHeight = IsExplicitSet(view.Height) ? desiredHeight : Math.Min(bounds.Height, view.MaximumHeight);
			}

			double frameY = bounds.Y + margin.Top;

			switch (alignment)
			{
				case LayoutAlignment.Center:
					frameY += (bounds.Height - desiredHeight) / 2;
					break;

				case LayoutAlignment.End:
					frameY += bounds.Height - desiredHeight;
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

			if (Dimension.IsExplicitSet(contentView.Width))
			{
				widthConstraint = contentView.Width;
			}

			if (Dimension.IsExplicitSet(contentView.Height))
			{
				heightConstraint = contentView.Height;
			}

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

		/// <summary>
		/// Arranges content which can exceed the bounds of the IContentView.
		/// </summary>
		/// <remarks>
		/// Useful for arranging content where the IContentView provides a viewport to a portion of the content (e.g, 
		/// the content of an IScrollView).
		/// </remarks>
		/// <param name="contentView"></param>
		/// <param name="bounds"></param>
		/// <returns>The Size of the arranged content</returns>
		public static Size ArrangeContentUnbounded(this IContentView contentView, Rect bounds)
		{
			var presentedContent = contentView.PresentedContent;

			if (presentedContent == null)
			{
				return bounds.Size;
			}

			var padding = contentView.Padding;

			// Normally we'd just want the content to be arranged within the ContentView's Frame,
			// but in this case the content may exceed the size of the Frame.
			// So in each dimension, we assume the larger of the two values.

			bounds.Width = Math.Max(bounds.Width, presentedContent.DesiredSize.Width + padding.HorizontalThickness);
			bounds.Height = Math.Max(bounds.Height, presentedContent.DesiredSize.Height + padding.VerticalThickness);

			contentView.ArrangeContent(bounds);

			return bounds.Size;
		}
	}
}
