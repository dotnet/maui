using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Layouts
{
	public abstract class LayoutManager : ILayoutManager
	{
		public LayoutManager(ILayout layout)
		{
			Layout = layout;
		}

		public ILayout Layout { get; }

		public abstract Size Measure(double widthConstraint, double heightConstraint);
		public abstract Size ArrangeChildren(Rectangle bounds);

		public static double ResolveConstraints(double externalConstraint, double explicitLength, double measuredLength, double min = -1, double max = -1)
		{
			var length = explicitLength >= 0 ? explicitLength : measuredLength;

			if (max >= 0 && max < length)
			{
				length = max;
			}

			if (min >= 0 && min > length)
			{
				length = min;
			}

			return Math.Min(length, externalConstraint);
		}
	}
}
