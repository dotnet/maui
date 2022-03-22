using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	// It's easier if this name is different than Maui.Platform.ColorExtensions
	public static partial class ControlsColorExtensions
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

