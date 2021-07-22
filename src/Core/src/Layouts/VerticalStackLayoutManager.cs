using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Layouts
{
	public class VerticalStackLayoutManager : StackLayoutManager
	{
		public VerticalStackLayoutManager(IStackLayout stackLayout) : base(stackLayout)
		{
		}

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			var children = Stack.Children;
			var padding = Stack.Padding;

			double measuredHeight = 0;
			double measuredWidth = 0;

			for (int n = 0; n < children.Count; n++)
			{
				var child = children[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var measure = child.Measure(widthConstraint, double.PositiveInfinity);
				measuredHeight += measure.Height;
				measuredWidth = Math.Max(measuredWidth, measure.Width);
			}

			measuredHeight += MeasureSpacing(Stack.Spacing, children.Count);
			measuredHeight += padding.VerticalThickness;
			measuredWidth += padding.HorizontalThickness;

			var finalHeight = ResolveConstraints(heightConstraint, Stack.Height, measuredHeight);
			var finalWidth = ResolveConstraints(widthConstraint, Stack.Width, measuredWidth);

			return new Size(finalWidth, finalHeight);
		}

		public override void ArrangeChildren(Rectangle bounds) 
		{
			var padding = Stack.Padding;

			double stackHeight = padding.Top;
			double left = padding.Left;
			double width = bounds.Width - padding.HorizontalThickness;

			foreach (var child in Stack.Children)
			{
				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var destination = new Rectangle(left, stackHeight, width, child.DesiredSize.Height);
				child.Frame = child.ComputeFrame(destination);
				child.Arrange(child.Frame);
				stackHeight += destination.Height + Stack.Spacing;
			}
		}
	}
}
