using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using static Microsoft.Maui.Layouts.LayoutManager;

namespace Microsoft.Maui.Controls
{
	public partial class ScrollView : IScrollView, IContentView
	{
		object IContentView.Content => Content;
		IView IContentView.PresentedContent => Content;

		double IScrollView.HorizontalOffset
		{
			get => ScrollX;
			set
			{
				if (ScrollX != value)
				{
					SetScrolledPosition(value, ScrollY);
				}
			}
		}

		double IScrollView.VerticalOffset
		{
			get => ScrollY;
			set
			{
				if (ScrollY != value)
				{
					SetScrolledPosition(ScrollX, value);
				}
			}
		}

		void IScrollView.RequestScrollTo(double horizontalOffset, double verticalOffset, bool instant)
		{
			var request = new ScrollToRequest(horizontalOffset, verticalOffset, instant);
			Handler?.Invoke(nameof(IScrollView.RequestScrollTo), request);
		}

		void IScrollView.ScrollFinished() => SendScrollFinished();

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			Thickness contentMargin = (Content as IView)?.Margin ?? Thickness.Zero;

			// Account for the ScrollView's margins and use the rest of the available space to measure the actual Content
			var contentWidthConstraint = widthConstraint - Margin.HorizontalThickness;
			var contentHeightConstraint = heightConstraint - Margin.VerticalThickness;
			(this as IContentView).CrossPlatformMeasure(contentWidthConstraint, contentHeightConstraint);

			// Now measure the ScrollView itself (ComputeDesiredSize will account for the ScrollView margins)
			var defaultSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);

			// The value from ComputeDesiredSize won't account for any margins on the Content; we'll need to do that manually
			// And we'll use ResolveConstraints to make sure we're sticking within and explicit Height/Width values or externally
			// imposed constraints
			var width = (this as IView).Width;
			var height = (this as IView).Height;

			var desiredWidth = ResolveConstraints(widthConstraint, width, defaultSize.Width + contentMargin.HorizontalThickness);
			var desiredHeight = ResolveConstraints(heightConstraint, height, defaultSize.Height + contentMargin.VerticalThickness);

			DesiredSize = new Size(desiredWidth, desiredHeight);
			return DesiredSize;
		}

		Size IContentView.CrossPlatformMeasure(double contentWidthConstraint, double contentHeightConstraint)
		{
			if (Content is not IView content)
			{
				return Content.DesiredSize;
			}

			switch (Orientation)
			{
				case ScrollOrientation.Horizontal:
					contentWidthConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Neither:
				case ScrollOrientation.Both:
					contentHeightConstraint = double.PositiveInfinity;
					contentWidthConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Vertical:
				default:
					contentHeightConstraint = double.PositiveInfinity;
					break;
			}

			content.Measure(contentWidthConstraint, contentHeightConstraint);
			ContentSize = content.DesiredSize;

			return ContentSize;
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.NativeArrange(Frame);

			(this as IContentView).CrossPlatformArrange(Frame);

			return Frame.Size;
		}

		Size IContentView.CrossPlatformArrange(Rectangle bounds)
		{
			if (Content is IView content)
			{
				// Normally we'd just want the content to be arranged within the ContentView's Frame,
				// but ScrollView content might be larger than the ScrollView itself (for obvious reasons)
				// So in each dimension, we assume the larger of the two values.
				var width = Math.Max(Frame.Width, content.DesiredSize.Width);
				var height = Math.Max(Frame.Height, content.DesiredSize.Height);

				content.Arrange(new Rectangle(0, 0, width, height));
			}

			return bounds.Size;
		}
	}
}
