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
			var measure = Measure(heightConstraint, Stack.Spacing, Stack.Children);

			var finalWidth = ResolveConstraints(widthConstraint, Stack.Width, measure.Width);

			return new Size(finalWidth, measure.Height);
		}

		public override void ArrangeChildren(Rectangle bounds)
		{
			if (Stack.FlowDirection == FlowDirection.LeftToRight)
			{
				ArrangeLeftToRight(bounds.Height, Stack.Spacing, Stack.Children);
			}
			else
			{
				// We _could_ simply reverse the list of child views when arranging from right to left, 
				// but this way we avoid extra list and enumerator allocations
				ArrangeRightToLeft(bounds.Height, Stack.Spacing, Stack.Children);
			}
		}

		static Size Measure(double heightConstraint, int spacing, IReadOnlyList<IView> views)
		{
			double totalRequestedWidth = 0;
			double requestedHeight = 0;

			for (int n = 0; n < views.Count; n++)
			{
				var child = views[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var measure = child.Measure(double.PositiveInfinity, heightConstraint);
				totalRequestedWidth += measure.Width;
				requestedHeight = Math.Max(requestedHeight, measure.Height);
			}

			var accountForSpacing = MeasureSpacing(spacing, views.Count);
			totalRequestedWidth += accountForSpacing;

			return new Size(totalRequestedWidth, requestedHeight);
		}

		static void ArrangeLeftToRight(double height, int spacing, IReadOnlyList<IView> views)
		{
			double xPosition = 0;

			for (int n = 0; n < views.Count; n++)
			{
				var child = views[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				xPosition += ArrangeChild(child, height, spacing, xPosition);
			}
		}

		static void ArrangeRightToLeft(double height, int spacing, IReadOnlyList<IView> views)
		{
			double xPostition = 0;

			for (int n = views.Count - 1; n >= 0; n--)
			{
				var child = views[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				xPostition += ArrangeChild(child, height, spacing, xPostition);
			}
		}

		static double ArrangeChild(IView child, double height, int spacing, double x)
		{
			var destination = new Rectangle(x, 0, child.DesiredSize.Width, height);
			child.Arrange(destination);
			return destination.Width + spacing;
		}
	}
}
