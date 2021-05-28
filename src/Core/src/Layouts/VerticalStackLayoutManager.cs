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
			var measure = Measure(widthConstraint, Stack.Spacing, Stack.Children);

			var finalHeight = ResolveConstraints(heightConstraint, Stack.Height, measure.Height);

			return new Size(measure.Width, finalHeight);
		}

		public override void ArrangeChildren(Rectangle bounds) => Arrange(bounds.Width, Stack.Spacing, Stack.Children);

		static Size Measure(double widthConstraint, int spacing, IReadOnlyList<IView> views)
		{
			double totalRequestedHeight = 0;
			double requestedWidth = 0;

			foreach (var child in views)
			{
				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var measure = child.Measure(widthConstraint, double.PositiveInfinity);
				totalRequestedHeight += measure.Height;
				requestedWidth = Math.Max(requestedWidth, measure.Width);
			}

			var accountForSpacing = MeasureSpacing(spacing, views.Count);
			totalRequestedHeight += accountForSpacing;

			return new Size(requestedWidth, totalRequestedHeight);
		}

		static void Arrange(double width, int spacing, IEnumerable<IView> views)
		{
			double stackHeight = 0;

			foreach (var child in views)
			{
				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var destination = new Rectangle(0, stackHeight, width, child.DesiredSize.Height);
				child.Arrange(destination);
				stackHeight += destination.Height + spacing;
			}
		}
	}
}
