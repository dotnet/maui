using System;
using Microsoft.Maui;

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

		public static double ResolveConstraints(double externalConstraint, double desiredLength)
		{
			if (desiredLength == -1)
			{
				return externalConstraint;
			}

			return Math.Min(externalConstraint, desiredLength);
		}

		public static double ResolveConstraints(double externalConstraint, double desiredLength, double measuredLength)
		{
			if (desiredLength == -1)
			{
				// No user-specified length, so the measured value will be limited by the external constraint
				return Math.Min(measuredLength, externalConstraint);
			}

			// User-specified length wins, subject to external constraints
			return Math.Min(desiredLength, externalConstraint);
		}
	}
}
