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
		public abstract void ArrangeChildren(Rectangle childBounds);

		public static double ResolveConstraints(double externalConstraint, double explicitLength, double measuredLength)
		{
			if (explicitLength == -1)
			{
				// No user-specified length, so the measured value will be limited by the external constraint
				return Math.Min(measuredLength, externalConstraint);
			}

			// User-specified length wins, subject to external constraints
			return Math.Min(explicitLength, externalConstraint);
		}
	}
}
