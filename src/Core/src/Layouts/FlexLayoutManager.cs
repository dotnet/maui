#nullable enable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Layouts
{
	public class FlexLayoutManager : ILayoutManager
	{
		IFlexLayout FlexLayout { get; }

		public FlexLayoutManager(IFlexLayout flexLayout)
		{
			FlexLayout = flexLayout;
		}

		public Size ArrangeChildren(Rect bounds)
		{
			var padding = FlexLayout.Padding;

			double top = padding.Top + bounds.Top;
			double left = padding.Left + bounds.Left;
			double availableWidth = bounds.Width - padding.HorizontalThickness;
			double availableHeight = bounds.Height - padding.VerticalThickness;

			FlexLayout.Layout(availableWidth, availableHeight);

			foreach (var child in FlexLayout)
			{
				var frame = FlexLayout.GetFlexFrame(child);
				if (double.IsNaN(frame.X)
					|| double.IsNaN(frame.Y)
					|| double.IsNaN(frame.Width)
					|| double.IsNaN(frame.Height))
					throw new Exception("something is deeply wrong");

				frame = frame.Offset(left, top);
				child.Arrange(frame);
			}

			return bounds.Size;
		}

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			var padding = FlexLayout.Padding;

			var availableWidth = widthConstraint - padding.HorizontalThickness;
			var availableHeight = heightConstraint - padding.VerticalThickness;

			double measuredHeight = 0;
			double measuredWidth = 0;

			FlexLayout.Layout(availableWidth, availableHeight);

			foreach (var child in FlexLayout)
			{
				if (child.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var frame = FlexLayout.GetFlexFrame(child);
				measuredHeight = Math.Max(measuredHeight, frame.Bottom);
				measuredWidth = Math.Max(measuredWidth, frame.Right);
			}

			var finalHeight = LayoutManager.ResolveConstraints(heightConstraint, FlexLayout.Height, measuredHeight + padding.VerticalThickness,
				FlexLayout.MinimumHeight, FlexLayout.MaximumHeight);

			var finalWidth = LayoutManager.ResolveConstraints(widthConstraint, FlexLayout.Width, measuredWidth + padding.HorizontalThickness,
				FlexLayout.MinimumWidth, FlexLayout.MaximumWidth);

			return new Size(finalWidth, finalHeight);
		}
	}
}
