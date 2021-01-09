using System.Globalization;

namespace System.Graphics
{
    public class Color
	{
		public readonly float Red;
		public readonly float Green;
		public readonly float Blue;
		public readonly float Alpha = 1;

		public Color(float red, float green, float blue)
		{
			Red = red;
			Green = green;
			Blue = blue;
		}

		public Color(float red, float green, float blue, float alpha)
		{
			Red = red;
			Green = green;
			Blue = blue;
			Alpha = alpha;
		}

		public Color(string colorAsHex)
		{
			//Remove # if present
			if (colorAsHex.IndexOf('#') != -1)
				colorAsHex = colorAsHex.Replace("#", "");

			int red = 0;
			int green = 0;
			int blue = 0;
			int alpha = 255;

			if (colorAsHex.Length == 6)
			{
				//#RRGGBB
				red = int.Parse(colorAsHex.Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse(colorAsHex.Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse(colorAsHex.Substring(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}
			else if (colorAsHex.Length == 3)
			{
				//#RGB
				red = int.Parse($"{colorAsHex[0]}{colorAsHex[0]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse($"{colorAsHex[1]}{colorAsHex[1]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse($"{colorAsHex[2]}{colorAsHex[2]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}
			else if (colorAsHex.Length == 4)
			{
				//#RGBA
				red = int.Parse($"{colorAsHex[0]}{colorAsHex[0]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse($"{colorAsHex[1]}{colorAsHex[1]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse($"{colorAsHex[2]}{colorAsHex[2]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				alpha = int.Parse($"{colorAsHex[3]}{colorAsHex[3]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}
			else if (colorAsHex.Length == 8)
			{
				//#RRGGBBAA
				red = int.Parse(colorAsHex.Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse(colorAsHex.Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse(colorAsHex.Substring(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				alpha = int.Parse(colorAsHex.Substring(6, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}

			Red = red / 255f;
			Green = green / 255f;
			Blue = blue / 255f;
			Alpha = alpha / 255f;
		}

		public static Color FromBytes(byte red, byte green, byte blue) => Color.FromBytes(red, green, blue, 255);
		
		public static Color FromBytes(byte red, byte green, byte blue, byte alpha)
			=> new Color(red / 255f, green / 255f, blue / 255f, alpha / 255f);

		public override int GetHashCode()
		{
			return ((int)Red ^ (int)Blue) ^ ((int)Green ^ (int)Alpha);
		}

		public string ToHexString()
		{
			return "#" + ToHexString(Red) + ToHexString(Green) + ToHexString(Blue);
		}

		public Paint AsPaint()
		{
			return new Paint()
			{
				PaintType = PaintType.Solid,
				StartColor = this
			};
		}
		
		public Color WithAlpha(float alpha)
		{
			if (Math.Abs(alpha - Alpha) < Geometry.Epsilon)
				return this;
			
			return new Color(Red,Green,Blue, alpha);
		}

		public Color MultiplyAlpha(float multiplyBy)
		{
			return new Color(Red,Green,Blue, Alpha * multiplyBy);
		}
		
		public string ToHexStringIncludingAlpha()
		{
			if (Alpha < 1)
				return ToHexString() + ToHexString(Alpha);

			return ToHexString();
		}

		public static string ToHexString(float r, float g, float b)
		{
			return "#" + ToHexString(r) + ToHexString(g) + ToHexString(b);
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