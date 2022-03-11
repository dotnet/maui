using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using static Microsoft.Maui.Layouts.LayoutManager;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Frame.xml" path="Type[@FullName='Microsoft.Maui.Controls.Frame']/Docs" />
	public partial class Frame : IView
	{
		IShadow IView.Shadow
		{
			get
			{
				if (!HasShadow)
					return null;

				if (base.Shadow != null)
					return base.Shadow;

#if IOS
				// The way the shadow is applied in .NET MAUI on iOS is the same way it was applied in Forms
				// so on iOS we just return the shadow that was hard coded into the renderer
				// On Android it sets the elevation on the CardView and on WinUI Forms just ignored HasShadow
				if(HasShadow)
					return new Shadow() { Radius = 5, Opacity = 0.8f, Offset = new Point(0, 0), Brush = SolidColorBrush.Black };
#endif

				return null;
			}
		}

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

		protected override Size ArrangeOverride(Rect bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.PlatformArrange(Frame);

			(this as IContentView).CrossPlatformArrange(new Rect(Point.Zero, Frame.Size));

			return Frame.Size;
		}
	}
}
