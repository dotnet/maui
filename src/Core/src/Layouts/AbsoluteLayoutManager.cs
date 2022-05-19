using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Layouts
{
	public class AbsoluteLayoutManager : LayoutManager
	{
		public IAbsoluteLayout AbsoluteLayout { get; }

		const double AutoSize = -1;

		public AbsoluteLayoutManager(IAbsoluteLayout absoluteLayout) : base(absoluteLayout)
		{
			AbsoluteLayout = absoluteLayout;
		}

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			var padding = AbsoluteLayout.Padding;

			var availableWidth = widthConstraint - padding.HorizontalThickness;
			var availableHeight = heightConstraint - padding.VerticalThickness;

			double measuredHeight = 0;
			double measuredWidth = 0;

			for (int n = 0; n < AbsoluteLayout.Count; n++)
			{
				var child = AbsoluteLayout[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var bounds = AbsoluteLayout.GetLayoutBounds(child);
				var flags = AbsoluteLayout.GetLayoutFlags(child);
				bool isWidthProportional = HasFlag(flags, AbsoluteLayoutFlags.WidthProportional);
				bool isHeightProportional = HasFlag(flags, AbsoluteLayoutFlags.HeightProportional);

				var measureWidth = ResolveChildMeasureConstraint(bounds.Width, isWidthProportional, widthConstraint);
				var measureHeight = ResolveChildMeasureConstraint(bounds.Height, isHeightProportional, heightConstraint);

				var measure = child.Measure(measureWidth, measureHeight);

				var width = ResolveDimension(isWidthProportional, bounds.Width, availableWidth, measure.Width);
				var height = ResolveDimension(isHeightProportional, bounds.Height, availableHeight, measure.Height);

				measuredHeight = Math.Max(measuredHeight, bounds.Top + height);
				measuredWidth = Math.Max(measuredWidth, bounds.Left + width);
			}

			var finalHeight = ResolveConstraints(heightConstraint, AbsoluteLayout.Height, measuredHeight, AbsoluteLayout.MinimumHeight, AbsoluteLayout.MaximumHeight);
			var finalWidth = ResolveConstraints(widthConstraint, AbsoluteLayout.Width, measuredWidth, AbsoluteLayout.MinimumWidth, AbsoluteLayout.MaximumWidth);

			return new Size(finalWidth, finalHeight);
		}

		public override Size ArrangeChildren(Rect bounds)
		{
			var padding = AbsoluteLayout.Padding;

			double top = padding.Top + bounds.Y;
			double left = padding.Left + bounds.X;
			double availableWidth = bounds.Width - padding.HorizontalThickness;
			double availableHeight = bounds.Height - padding.VerticalThickness;

			for (int n = 0; n < AbsoluteLayout.Count; n++)
			{
				var child = AbsoluteLayout[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var destination = AbsoluteLayout.GetLayoutBounds(child);
				var flags = AbsoluteLayout.GetLayoutFlags(child);

				bool isWidthProportional = HasFlag(flags, AbsoluteLayoutFlags.WidthProportional);
				bool isHeightProportional = HasFlag(flags, AbsoluteLayoutFlags.HeightProportional);

				destination.Width = ResolveDimension(isWidthProportional, destination.Width, availableWidth, child.DesiredSize.Width);
				destination.Height = ResolveDimension(isHeightProportional, destination.Height, availableHeight, child.DesiredSize.Height);

				if (HasFlag(flags, AbsoluteLayoutFlags.XProportional))
				{
					destination.X = (availableWidth - destination.Width) * destination.X;
				}

				if (HasFlag(flags, AbsoluteLayoutFlags.YProportional))
				{
					destination.Y = (availableHeight - destination.Height) * destination.Y;
				}

				destination.X += left;
				destination.Y += top;

				child.Arrange(destination);
			}

			return new Size(availableWidth, availableHeight);
		}

		static bool HasFlag(AbsoluteLayoutFlags a, AbsoluteLayoutFlags b)
		{
			// Avoiding Enum.HasFlag here for performance reasons; we don't need the type check
			return (a & b) == b;
		}

		static double ResolveDimension(bool isProportional, double fromBounds, double available, double measured)
		{
			// By default, we use the absolute value from LayoutBounds
			var value = fromBounds;

			if (isProportional && !double.IsInfinity(available))
			{
				// If this dimension is marked proportional, then the value is a percentage of the available space
				// Multiple it by the available space to figure out the final value
				value *= available;
			}
			else if (value == AutoSize)
			{
				// No absolute or proportional value specified, so we use the measured value
				value = measured;
			}

			return value;
		}

		static double ResolveChildMeasureConstraint(double boundsValue, bool proportional, double constraint)
		{
			if (boundsValue < 0)
			{
				// If the child view doesn't have bounds set by the AbsoluteLayout, then we'll let it auto-size
				return double.PositiveInfinity;
			}

			if (proportional)
			{
				return boundsValue * constraint;
			}

			return boundsValue;
		}
	}
}
