using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public static class ColorExtensions
	{
		public static bool IsDefault(this Graphics.Color color)
		{
			return color == KnownColor.Default;
		}

		public static bool IsNotDefault(this Graphics.Color color)
		{
			return !IsDefault(color);
		}
	}

	public static class KnownColor
	{
		public static Color Default => null;

		public static Color Transparent { get; } = new(255, 255, 255, 0);

		public static void SetAccent(Color value) => Accent = value;

		public static Color Accent { get; internal set; }
	}
}

