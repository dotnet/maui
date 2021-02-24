using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	internal class ColorComparison : IEqualityComparer<UIColor>
	{
		public static bool ARGBEquivalent(UIColor color1, UIColor color2)
		{
			color1.GetRGBA(out nfloat red1, out nfloat green1, out nfloat blue1, out nfloat alpha1);
			color2.GetRGBA(out nfloat red2, out nfloat green2, out nfloat blue2, out nfloat alpha2);

			const double tolerance = 0.000001;

			return Equal(red1, red2, tolerance)
				&& Equal(green1, green2, tolerance)
				&& Equal(blue1, blue2, tolerance)
				&& Equal(alpha1, alpha2, tolerance);
		}

		static bool Equal(nfloat v1, nfloat v2, double tolerance)
		{
			return Math.Abs(v1 - v2) <= tolerance;
		}

		public bool Equals(UIColor x, UIColor y) => ARGBEquivalent(x, y);

		public int GetHashCode(UIColor obj) => obj.GetHashCode();
	}
}