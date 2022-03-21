using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Layouts
{
	public class HorizontalStackLayoutManager : StackLayoutManager
	{
		public HorizontalStackLayoutManager(IStackLayout layout) : base(layout)
		{
		}

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			var padding = Stack.Padding;

			double measuredWidth = 0;
			double measuredHeight = 0;

			for (int n = 0; n < Stack.Count; n++)
			{
				var child = Stack[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var measure = child.Measure(double.PositiveInfinity, heightConstraint);
				measuredWidth += measure.Width;
				measuredHeight = Math.Max(measuredHeight, measure.Height);
			}

			measuredWidth += MeasureSpacing(Stack.Spacing, Stack.Count);
			measuredWidth += padding.HorizontalThickness;
			measuredHeight += padding.VerticalThickness;

			var finalHeight = ResolveConstraints(heightConstraint, Stack.Height, measuredHeight, Stack.MinimumHeight, Stack.MaximumHeight);
			var finalWidth = ResolveConstraints(widthConstraint, Stack.Width, measuredWidth, Stack.MinimumWidth, Stack.MaximumWidth);

			return new Size(finalWidth, finalHeight);
		}

		public override Size ArrangeChildren(Rect bounds)
		{
			var padding = Stack.Padding;
			double top = padding.Top + bounds.Top;
			
			var height = bounds.Height - padding.VerticalThickness;
			double stackWidth;

			bool leftToRight = Stack.ShouldArrangeLeftToRight();

			// Figure out where we're starting from (the left edge of the padded area, or the right edge)
			double xPosition = leftToRight ? padding.Left + bounds.Left : bounds.Right - padding.Right;

			// If we're arranging from the right, spacing will be added to the left
			double spacingDelta = leftToRight ? Stack.Spacing : -Stack.Spacing;

			for (int n = 0; n < Stack.Count; n++)
			{
				var child = Stack[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				xPosition += leftToRight 
					? ArrangeChildFromLeftEdge(child, height, top, xPosition) 
					: ArrangeChildFromRightEdge(child, height, top, xPosition);

				if (n < Stack.Count - 1)
				{
					// If we have more than one child and we're not on the last one, add spacing
					xPosition += spacingDelta;
				}
			}

			// If we started from the left, the total width is the current x position;
			// If we started from the right, it's the difference between the right edge and the current x position
			stackWidth = leftToRight ? xPosition : bounds.Right - xPosition;

			var actual = new Size(stackWidth, height);

			return actual.AdjustForFill(bounds, Stack);
		}

		static double ArrangeChildFromLeftEdge(IView child, double height, double top, double x)
		{
			var destination = new Rect(x, top, child.DesiredSize.Width, height);
			child.Arrange(destination);
			return destination.Width;
		}

		static double ArrangeChildFromRightEdge(IView child, double height, double top, double x)
		{
			var width = child.DesiredSize.Width;
			var destination = new Rect(x - width, top, width, height);
			child.Arrange(destination);
			return -destination.Width;
		}
	}
}
