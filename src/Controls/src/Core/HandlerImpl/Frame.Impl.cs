using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using static Microsoft.Maui.Layouts.LayoutManager;

namespace Microsoft.Maui.Controls
{
	public partial class Frame
	{
		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			Thickness contentMargin = (Content as IView)?.Margin ?? Thickness.Zero;
			Thickness padding = Padding;
			Thickness margin = Margin;

			// Account for the Frame's margins and padding and use the rest of the available space to measure the actual Content
			var contentWidthConstraint = widthConstraint - margin.HorizontalThickness - padding.HorizontalThickness;
			var contentHeightConstraint = heightConstraint - margin.VerticalThickness - padding.VerticalThickness;
			var contentSize = (this as IContentView).CrossPlatformMeasure(contentWidthConstraint, contentHeightConstraint);

			// We'll use ResolveConstraints to make sure we're sticking within any explicit Height/Width values or externally
			// imposed constraints
			var width = (this as IView).Width;
			var height = (this as IView).Height;

			var desiredWidth = ResolveConstraints(widthConstraint, width, contentSize.Width + margin.HorizontalThickness);
			var desiredHeight = ResolveConstraints(heightConstraint, height, contentSize.Height + margin.VerticalThickness);

			DesiredSize = new Size(desiredWidth, desiredHeight);
			return DesiredSize;
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.NativeArrange(Frame);

			(this as IContentView).CrossPlatformArrange(new Rectangle(Point.Zero, Frame.Size));

			return Frame.Size;
		}
	}
}
