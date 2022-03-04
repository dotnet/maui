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
			FlexLayout.Layout(bounds.Width, bounds.Height);

			foreach (var child in FlexLayout)
			{
				var frame = FlexLayout.GetFlexFrame(child);
				if (double.IsNaN(frame.X)
					|| double.IsNaN(frame.Y)
					|| double.IsNaN(frame.Width)
					|| double.IsNaN(frame.Height))
					throw new Exception("something is deeply wrong");
				frame = frame.Offset(bounds.Left, bounds.Top);
				child.Arrange(frame);
			}

			return bounds.Size;
		}

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			double width = 0;
			double height = 0;

			if (!double.IsInfinity(widthConstraint))
			{
				width = widthConstraint;
			}

			if (!double.IsInfinity(heightConstraint))
			{
				height = heightConstraint;
			}

			height = LayoutManager.ResolveConstraints(height, FlexLayout.Height, height, FlexLayout.MinimumHeight, FlexLayout.MaximumHeight);
			width = LayoutManager.ResolveConstraints(width, FlexLayout.Width, width, FlexLayout.MinimumWidth, FlexLayout.MaximumWidth);

			return new Size(width, height);
		}
	}
}
