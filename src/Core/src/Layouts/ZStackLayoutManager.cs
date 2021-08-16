using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Layouts
{
	public class ZStackLayoutManager : StackLayoutManager 
	{
		public ZStackLayoutManager(IStackLayout stackLayout) : base(stackLayout)
		{
		}

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			var padding = Stack.Padding;

			double measuredHeight = 0;
			double measuredWidth = 0;

			for (int n = 0; n < Stack.Count; n++)
			{
				var child = Stack[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var measure = child.Measure(double.PositiveInfinity, double.PositiveInfinity);
				measuredHeight = Math.Max(measuredHeight, measure.Height);
				measuredWidth = Math.Max(measuredWidth, measure.Width);
			}

			measuredHeight += padding.VerticalThickness;
			measuredWidth += padding.HorizontalThickness;

			var finalHeight = ResolveConstraints(heightConstraint, Stack.Height, measuredHeight);
			var finalWidth = ResolveConstraints(widthConstraint, Stack.Width, measuredWidth);

			return new Size(finalWidth, finalHeight);
		}

		public override Size ArrangeChildren(Rectangle bounds)
		{
			var padding = Stack.Padding;

			double stackHeight = padding.Top + bounds.Y;
			double left = padding.Left + bounds.X;
			double width = bounds.Width - padding.HorizontalThickness;
			double height = bounds.Height - padding.VerticalThickness;

			for (int n = 0; n < Stack.Count; n++)
			{
				var child = Stack[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var destination = new Rectangle(left, stackHeight, width, height);
				child.Arrange(destination);
			}

			return new Size(width, height);
		}
	}
}
