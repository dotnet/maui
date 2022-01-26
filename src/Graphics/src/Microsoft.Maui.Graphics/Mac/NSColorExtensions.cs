using AppKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class NSColorExtensions
	{
		public static Color AsColor(this NSColor color)
		{
			var convertedColorspace = color.UsingColorSpace(NSColorSpace.GenericRGBColorSpace);
			convertedColorspace.GetRgba(out var red, out var green, out var blue, out var alpha);
			return new Color((float) red, (float) green, (float) blue, (float) alpha);
		}

		public static string ToHex(this NSColor color)
		{
			if (color == null)
				return null;

			var red = (float)color.RedComponent;
			var green = (float)color.GreenComponent;
			var blue = (float)color.BlueComponent;
			var alpha = (float)color.AlphaComponent;

			if (alpha < 1)
				return "#" + ToHexString(red) + ToHexString(green) + ToHexString(blue) + ToHexString(alpha);

			return "#" + ToHexString(red) + ToHexString(green) + ToHexString(blue);
		}

		private static string ToHexString(float value)
		{
			var intValue = (int)(255f * value);
			var stringValue = intValue.ToString("X");
			if (stringValue.Length == 1)
				return "0" + stringValue;

			return stringValue;
		}
	}
}
