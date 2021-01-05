using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Xamarin.Platform.Layouts
{
	public class HorizontalStackLayoutManager : StackLayoutManager
	{
		public HorizontalStackLayoutManager(IStackLayout layout) : base(layout)
		{
		}

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			var heightMeasureConstraint = ResolveConstraints(heightConstraint, Stack.Height);

			var measure = Measure(heightMeasureConstraint, Stack.Spacing, Stack.Children);

			var finalWidth = ResolveConstraints(widthConstraint, Stack.Width, measure.Width);

			return new Size(finalWidth, measure.Height);
		}

		public override void Arrange(Rectangle bounds) => Arrange(Stack.Spacing, Stack.Children);

		static Size Measure(double heightConstraint, int spacing, IReadOnlyList<IView> views)
		{
			double totalRequestedWidth = 0;
			double requestedHeight = 0;

			foreach (var child in views)
			{
				var measure = child.IsMeasureValid ? child.DesiredSize : child.Measure(double.PositiveInfinity, heightConstraint);
				totalRequestedWidth += measure.Width;
				requestedHeight = Math.Max(requestedHeight, measure.Height);
			}

			var accountForSpacing = MeasureSpacing(spacing, views.Count);
			totalRequestedWidth += accountForSpacing;

			return new Size(totalRequestedWidth, requestedHeight);
		}

		static void Arrange(int spacing, IEnumerable<IView> views)
		{
			double stackWidth = 0;

			foreach (var child in views)
			{
				var destination = new Rectangle(stackWidth, 0, child.DesiredSize.Width, child.DesiredSize.Height);
				child.Arrange(destination);

				stackWidth += destination.Width + spacing;
			}
		}
	}
}
