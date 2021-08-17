using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Layouts
{
	public class AbsoluteLayoutManager : LayoutManager
	{
		public IAbsoluteLayout AbsoluteLayout { get; }

		static double AutoSize = -1;

		public AbsoluteLayoutManager(IAbsoluteLayout absoluteLayout) : base(absoluteLayout)
		{
			AbsoluteLayout = absoluteLayout;
		}

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			var padding = AbsoluteLayout.Padding;

			double measuredHeight = 0;
			double measuredWidth = 0;


			for (int n = 0; n < AbsoluteLayout.Count; n++)
			{
				var child = AbsoluteLayout[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var measure = child.Measure(widthConstraint, heightConstraint);

				var bounds = AbsoluteLayout.GetLayoutBounds(child);
				var width = bounds.Width;
				var height = bounds.Height;

				var flags = AbsoluteLayout.GetLayoutFlags(child);

				if (flags.HasFlag(AbsoluteLayoutFlags.WidthProportional))
				{
					if (!double.IsInfinity(widthConstraint))
					{
						width *= widthConstraint;
					}
				}
				else
				{
					if (width == -1)
					{
						width = measure.Width;
					}
				}

				if (flags.HasFlag(AbsoluteLayoutFlags.HeightProportional))
				{
					if (!double.IsInfinity(heightConstraint))
					{
						height *= heightConstraint;
					}
				}
				else
				{
					if (height == -1)
					{
						height = measure.Height;
					}
				}

				measuredHeight = Math.Max(measuredHeight, bounds.Top + height);
				measuredWidth = Math.Max(measuredWidth, bounds.Left + width);
			}

			var finalHeight = ResolveConstraints(heightConstraint, AbsoluteLayout.Height, measuredHeight);
			var finalWidth = ResolveConstraints(widthConstraint, AbsoluteLayout.Width, measuredWidth);

			return new Size(finalWidth, finalHeight);
		}

		public override Size ArrangeChildren(Rectangle bounds)
		{
			var padding = AbsoluteLayout.Padding;

			double top = padding.Top + bounds.Y;
			double left = padding.Left + bounds.X;
			double width = bounds.Width - padding.HorizontalThickness;
			double height = bounds.Height - padding.VerticalThickness;

			for (int n = 0; n < AbsoluteLayout.Count; n++)
			{
				var child = AbsoluteLayout[n];

				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var destination = AbsoluteLayout.GetLayoutBounds(child);
				var flags = AbsoluteLayout.GetLayoutFlags(child);

				if (flags.HasFlag(AbsoluteLayoutFlags.WidthProportional))
				{
					destination.Width *= bounds.Width;
				}
				else
				{
					if (destination.Width == AutoSize)
					{
						destination.Width = child.DesiredSize.Width;
					}
				}

				if (flags.HasFlag(AbsoluteLayoutFlags.HeightProportional))
				{
					destination.Height *= bounds.Height;
				}
				else
				{
					if (destination.Height == AutoSize)
					{
						destination.Height = child.DesiredSize.Height;
					}
				}

				if (flags.HasFlag(AbsoluteLayoutFlags.XProportional))
				{
					destination.X = (bounds.Width - destination.Width) * destination.X;
				}

				if (flags.HasFlag(AbsoluteLayoutFlags.YProportional))
				{
					destination.Y = (bounds.Height - destination.Height) * destination.Y;
				}

				child.Arrange(destination.Offset(left, top));
			}

			return new Size(width, height);
		}
	}
}
