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
			double left = padding.Left + bounds.Left;
			var height = bounds.Height - padding.VerticalThickness;
			double stackWidth;

			if (Stack.FlowDirection == FlowDirection.LeftToRight)
			{
				stackWidth = ArrangeLeftToRight(height, left, top, Stack.Spacing, Stack);
			}
			else
			{
				// We _could_ simply reverse the list of child views when arranging from right to left, 
				// but this way we avoid extra list and enumerator allocations
				stackWidth = ArrangeRightToLeft(height, left, top, Stack.Spacing, Stack);
			}

			var actual = new Size(stackWidth, height);

			return actual.AdjustForFill(bounds, Stack);
		}

		static double ArrangeLeftToRight(double height, double left, double top, double spacing, IList<IView> children)
		{
			double xPosition = left;

			for (int n = 0; n < children.Count; n++)
			{
				var child = children[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				xPosition += ArrangeChild(child, height, top, spacing, xPosition);
			}

			return xPosition;
		}

		static double ArrangeRightToLeft(double height, double left, double top, double spacing, IList<IView> children)
		{
			double xPostition = left;

			for (int n = children.Count - 1; n >= 0; n--)
			{
				var child = children[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				xPostition += ArrangeChild(child, height, top, spacing, xPostition);
			}

			return xPostition;
		}

		static double ArrangeChild(IView child, double height, double top, double spacing, double x)
		{
			var destination = new Rect(x, top, child.DesiredSize.Width, height);
			child.Arrange(destination);
			return destination.Width + spacing;
		}
	}
}
