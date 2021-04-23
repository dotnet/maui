using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public static partial class ColorExtensions
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
}

