using Microsoft.Maui.Graphics;
using NColor = Tizen.NUI.Color;
using TColor = Tizen.UIExtensions.Common.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public static class ColorExtensions
	{
		/// <summary>
		/// Creates an instance of ElmSharp.Color class based on provided Microsoft.Maui.Controls.Compatibility.Color instance
		/// </summary>
		/// <returns>ElmSharp.Color instance representing a color which corresponds to the provided Microsoft.Maui.Controls.Compatibility.Color</returns>
		/// <param name="c">The Microsoft.Maui.Controls.Compatibility.Color instance which will be converted to a ElmSharp.Color</param>
		public static TColor ToNative(this Color c)
		{
			if (c == null)
			{
				// Trying to convert the default color, this may result in black color.
				return TColor.Default;
			}
			else
			{
				return new TColor(c.Red, c.Green, c.Blue, c.Alpha);
			}
		}
		public static NColor ToNativeNUI(this Color c)
		{
			return new NColor((float)c.Red, (float)c.Green, (float)c.Blue, (float)c.Alpha);
		}

		public static Color WithAlpha(this Color color, double alpha)
		{
			return new Color(color.Red, color.Green, color.Blue, (int)(255 * alpha));
		}

		public static Color WithPremultiplied(this Color color, double alpha)
		{
			return new Color((int)(color.Red * alpha), (int)(color.Green * alpha), (int)(color.Blue * alpha), color.Alpha);
		}
	}
}
