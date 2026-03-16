using System;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Layouts
{
	public class VerticalStackLayoutManager : StackLayoutManager
	{
		public VerticalStackLayoutManager(IStackLayout stackLayout) : base(stackLayout)
		{
		}

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			var padding = Stack.Padding;

			double measuredHeight = 0;
			double measuredWidth = 0;
			double childWidthConstraint = widthConstraint - padding.HorizontalThickness;

			// When this stack has an explicit width (WidthRequest), children should be measured
			// within that width rather than the potentially wider parent constraint. Without this,
			// a word-wrap label child would measure at the parent's full width and report a
			// single-line desired size, causing it to overflow the stack's narrower arranged area.
			if (IsExplicitSet(Stack.Width))
			{
				childWidthConstraint = Math.Min(childWidthConstraint, Math.Max(0, Stack.Width - padding.HorizontalThickness));
			}

			int spacingCount = 0;

			for (int n = 0; n < Stack.Count; n++)
			{
				var child = Stack[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				spacingCount += 1;
				var measure = child.Measure(childWidthConstraint, double.PositiveInfinity);
				measuredHeight += measure.Height;
				measuredWidth = Math.Max(measuredWidth, measure.Width);
			}

			measuredHeight += MeasureSpacing(Stack.Spacing, spacingCount);
			measuredHeight += padding.VerticalThickness;
			measuredWidth += padding.HorizontalThickness;

			var finalHeight = ResolveConstraints(heightConstraint, Stack.Height, measuredHeight, Stack.MinimumHeight, Stack.MaximumHeight);
			var finalWidth = ResolveConstraints(widthConstraint, Stack.Width, measuredWidth, Stack.MinimumWidth, Stack.MaximumWidth);

			return new Size(finalWidth, finalHeight);
		}

		public override Size ArrangeChildren(Rect bounds)
		{
			var padding = Stack.Padding;

			double stackHeight = padding.Top + bounds.Y;
			double left = padding.Left + bounds.X;
			double width = Math.Max(0, bounds.Width - padding.HorizontalThickness);

			for (int n = 0; n < Stack.Count; n++)
			{
				var child = Stack[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var destination = new Rect(left, stackHeight, width, child.DesiredSize.Height);
				child.Arrange(destination);
				stackHeight += destination.Height + Stack.Spacing;
			}
			return new Size(bounds.Width, bounds.Height);
		}
	}
}
