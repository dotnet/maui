using System.Collections.Generic;

namespace System.Maui.Core.Layout
{
	public static class Stack
	{
		public static SizeRequest MeasureStack(double widthConstraint, double heightConstraint, IEnumerable<IView> views, Orientation orientation) 
		{
			if (orientation == Orientation.Horizontal)
			{
				return MeasureStackHorizontal(heightConstraint, views);
			}

			return MeasureStackVertical(widthConstraint, views);
		}

		static SizeRequest MeasureStackVertical(double constraint, IEnumerable<IView> views) 
		{
			double totalRequestedHeight = 0;
			double totalMinimumHeight = 0;
			double requestedWidth = 0;
			double minimumWidth = 0;

			foreach (var child in views)
			{
				// TODO check child.IsVisible

				var measure = child.IsMeasureValid ? child.DesiredSize : child.Measure(constraint, double.PositiveInfinity);
				totalRequestedHeight += measure.Request.Height;
				totalMinimumHeight += measure.Minimum.Height;

				requestedWidth = Math.Max(requestedWidth, measure.Request.Width);
				minimumWidth = Math.Max(minimumWidth, measure.Minimum.Width);
			}

			return new SizeRequest(
				new Size(requestedWidth, totalRequestedHeight),
				new Size(minimumWidth, totalMinimumHeight));
		}

		static SizeRequest MeasureStackHorizontal(double constraint, IEnumerable<IView> views)
		{
			double totalRequestedWidth = 0;
			double totalMinimumWidth = 0;
			double requestedHeight = 0;
			double minimumHeight = 0;

			foreach (var child in views)
			{
				// TODO check child.IsVisible

				var measure = child.IsMeasureValid ? child.DesiredSize : child.Measure(double.PositiveInfinity, constraint);
				totalRequestedWidth += measure.Request.Width;
				totalMinimumWidth += measure.Minimum.Width;

				requestedHeight = Math.Max(requestedHeight, measure.Request.Height);
				minimumHeight = Math.Max(minimumHeight, measure.Minimum.Height);
			}

			return new SizeRequest(
				new Size(totalRequestedWidth, requestedHeight),
				new Size(totalMinimumWidth, minimumHeight));
		}

		public static void ArrangeStack(Rectangle bounds, IEnumerable<IView> views, Orientation orientation)
		{
			switch (orientation)
			{
				case Orientation.Vertical:
					ArrangeStackVertically(bounds.Width, views);
					break;
				case Orientation.Horizontal:
					ArrangeStackHorizontally(bounds.Height, views);
					break;
			}
		}

		static void ArrangeStackHorizontally(double heightConstraint, IEnumerable<IView> views)
		{
			double stackWidth = 0;

			foreach (var child in views)
			{
				var destination = new Rectangle(stackWidth, 0, child.DesiredSize.Request.Width, heightConstraint);
				child.Arrange(destination);

				stackWidth += destination.Width;
			}
		}

		static void ArrangeStackVertically(double widthConstraint, IEnumerable<IView> views) 
		{
			double stackHeight = 0;

			foreach (var child in views)
			{
				var destination = new Rectangle(0, stackHeight, widthConstraint, child.DesiredSize.Request.Height);
				child.Arrange(destination);

				stackHeight += destination.Height;
			}
		}
	}
}


// TODO Android GetDesiredSize needs to convert double.Infinity (or maxvalue?) to measurespec unspecified (rather than atmost)