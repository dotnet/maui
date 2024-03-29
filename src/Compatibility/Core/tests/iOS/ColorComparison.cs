using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	internal static class ColorComparison
	{
		private const double MinTolerance = 0.000001;

		public static bool ARGBEquivalent(UIColor color1, UIColor color2) =>
			ARGBEquivalent(color1, color2, tolerance: null);

		public static bool ARGBEquivalent(UIColor color1, UIColor color2, double? tolerance = null)
		{
			if (tolerance is null)
				tolerance = MinTolerance;

			color1.GetRGBA(out nfloat red1, out nfloat green1, out nfloat blue1, out nfloat alpha1);
			color2.GetRGBA(out nfloat red2, out nfloat green2, out nfloat blue2, out nfloat alpha2);

			return Equal(red1, red2, tolerance.Value)
				&& Equal(green1, green2, tolerance.Value)
				&& Equal(blue1, blue2, tolerance.Value)
				&& Equal(alpha1, alpha2, tolerance.Value);
		}

		static bool Equal(nfloat v1, nfloat v2, double tolerance)
		{
			return Math.Abs(v1 - v2) <= tolerance;
		}
	}
}