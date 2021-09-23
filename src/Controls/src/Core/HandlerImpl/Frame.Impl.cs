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

			// Account for the Frame's margins and use the rest of the available space to measure the actual Content
			var contentWidthConstraint = widthConstraint - Margin.HorizontalThickness;
			var contentHeightConstraint = heightConstraint - Margin.VerticalThickness;
			(this as IContentView).CrossPlatformMeasure(contentWidthConstraint, contentHeightConstraint);

			// Now measure the Frame itself 
			var defaultSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);

			// The value from ComputeDesiredSize won't account for any margins on the Content; we'll need to do that manually
			// And we'll use ResolveConstraints to make sure we're sticking within and explicit Height/Width values or externally
			// imposed constraints
			var width = (this as IView).Width;
			var height = (this as IView).Height;

			var desiredWidth = ResolveConstraints(widthConstraint, width, defaultSize.Width + contentMargin.HorizontalThickness + padding.HorizontalThickness);
			var desiredHeight = ResolveConstraints(heightConstraint, height, defaultSize.Height + contentMargin.VerticalThickness + padding.VerticalThickness);

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
