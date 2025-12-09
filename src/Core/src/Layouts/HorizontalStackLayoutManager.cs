using System;
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
			int spacingCount = 0;

			for (int n = 0; n < Stack.Count; n++)
			{
				var child = Stack[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				spacingCount += 1;
				var measure = child.Measure(double.PositiveInfinity, heightConstraint - padding.VerticalThickness);
				measuredWidth += measure.Width;
				measuredHeight = Math.Max(measuredHeight, measure.Height);
			}

			measuredWidth += MeasureSpacing(Stack.Spacing, spacingCount);
			measuredWidth += padding.HorizontalThickness;
			measuredHeight += padding.VerticalThickness;

			var finalHeight = ResolveConstraints(heightConstraint, Stack.Height, measuredHeight, Stack.MinimumHeight, Stack.MaximumHeight);
			var finalWidth = ResolveConstraints(widthConstraint, Stack.Width, measuredWidth, Stack.MinimumWidth, Stack.MaximumWidth);

			return new Size(finalWidth, finalHeight);
		}

		public override Size ArrangeChildren(Rect bounds)
		{
			var padding = Stack.Padding;
			double spacing = Stack.Spacing;
			var childCount = Stack.Count;

			double top = padding.Top + bounds.Top;

			var height = Math.Max(0, bounds.Height - padding.VerticalThickness);

			// Figure out where we're starting from 
			double xPosition = padding.Left + bounds.Left;

			for (int n = 0; n < Stack.Count; n++)
			{
				var child = Stack[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				xPosition += ArrangeChild(child, height, top, xPosition);

				if (n < childCount - 1)
				{
					// If we have more than one child and we're not on the last one, add spacing
					xPosition += spacing;
				}
			}

			return new Size(bounds.Width, bounds.Height);
		}

		static double ArrangeChild(IView child, double height, double top, double x)
		{
			var destination = new Rect(x, top, child.DesiredSize.Width, height);
			child.Arrange(destination);
			return destination.Width;
		}
	}
}
