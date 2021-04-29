using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	static class ColorComparison
	{
		public const int ColorPrecision = 1;

		public static bool IsEquivalent(this AColor actualColor, AColor expectedColor, int precision = ColorPrecision)
		{
			var red = actualColor.R <= expectedColor.R + precision && actualColor.R >= expectedColor.R - precision;
			var green = actualColor.G <= expectedColor.G + precision && actualColor.G >= expectedColor.G - precision;
			var blue = actualColor.B <= expectedColor.B + precision && actualColor.B >= expectedColor.B - precision;
			var alpha = actualColor.A <= expectedColor.A + precision && actualColor.A >= expectedColor.A - precision;

			return red && green && blue && alpha;
		}
	}
}