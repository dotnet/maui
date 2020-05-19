using ERect = ElmSharp.Rect;
using ESize = ElmSharp.Size;

namespace System.Maui.Platform.Tizen
{
	/// <summary>
	/// Extension class, provides DIP convert functionalities
	/// </summary>
	public static class DensityIndependentPixelExtensions
	{
		public static Rectangle ToDP(this ERect rect)
		{
			return new Rectangle(System.Maui.Maui.ConvertToScaledDP(rect.X), System.Maui.Maui.ConvertToScaledDP(rect.Y), System.Maui.Maui.ConvertToScaledDP(rect.Width), System.Maui.Maui.ConvertToScaledDP(rect.Height));
		}

		public static ERect ToPixel(this Rectangle rect)
		{
			return new ERect(System.Maui.Maui.ConvertToScaledPixel(rect.X), System.Maui.Maui.ConvertToScaledPixel(rect.Y), System.Maui.Maui.ConvertToScaledPixel(rect.Width), System.Maui.Maui.ConvertToScaledPixel(rect.Height));
		}

		public static Size ToDP(this ESize size)
		{
			return new Size(System.Maui.Maui.ConvertToScaledDP(size.Width), System.Maui.Maui.ConvertToScaledDP(size.Height));
		}

		public static ESize ToPixel(this Size size)
		{
			return new ESize(System.Maui.Maui.ConvertToScaledPixel(size.Width), System.Maui.Maui.ConvertToScaledPixel(size.Height));
		}
	}
}
