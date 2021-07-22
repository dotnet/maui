﻿using System;
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

			var finalWidth = ResolveConstraints(widthConstraint, Stack.Width, measuredWidth);
			var finalHeight = ResolveConstraints(heightConstraint, Stack.Height, measuredHeight);

			return new Size(finalWidth, finalHeight);
		}

		public override void ArrangeChildren(Rectangle bounds)
		{
			var padding = Stack.Padding;
			var height = bounds.Height - padding.VerticalThickness;

			if (Stack.FlowDirection == FlowDirection.LeftToRight)
			{
				ArrangeLeftToRight(height, padding.Left, padding.Top, Stack.Spacing, Stack);
			}
			else
			{
				// We _could_ simply reverse the list of child views when arranging from right to left, 
				// but this way we avoid extra list and enumerator allocations
				ArrangeRightToLeft(height, padding.Left, padding.Top, Stack.Spacing, Stack);
			}
		}

		static void ArrangeLeftToRight(double height, double left, double top, double spacing, IList<IView> children)
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
		}

		static void ArrangeRightToLeft(double height, double left, double top, double spacing, IList<IView> children)
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
		}

		static double ArrangeChild(IView child, double height, double top, double spacing, double x)
		{
			var destination = new Rectangle(x, top, child.DesiredSize.Width, height);
			child.Frame = child.ComputeFrame(destination);
			child.Arrange(child.Frame);
			return destination.Width + spacing;
		}
	}
}
