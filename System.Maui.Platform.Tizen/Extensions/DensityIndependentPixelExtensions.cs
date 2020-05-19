using ERect = ElmSharp.Rect;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Extension class, provides DIP convert functionalities
	/// </summary>
	public static class DensityIndependentPixelExtensions
	{
		public static Rectangle ToDP(this ERect rect)
		{
			return new Rectangle(Forms.ConvertToScaledDP(rect.X), Forms.ConvertToScaledDP(rect.Y), Forms.ConvertToScaledDP(rect.Width), Forms.ConvertToScaledDP(rect.Height));
		}

		public static ERect ToPixel(this Rectangle rect)
		{
			return new ERect(Forms.ConvertToScaledPixel(rect.X), Forms.ConvertToScaledPixel(rect.Y), Forms.ConvertToScaledPixel(rect.Width), Forms.ConvertToScaledPixel(rect.Height));
		}

		public static Size ToDP(this ESize size)
		{
			return new Size(Forms.ConvertToScaledDP(size.Width), Forms.ConvertToScaledDP(size.Height));
		}

		public static ESize ToPixel(this Size size)
		{
			return new ESize(Forms.ConvertToScaledPixel(size.Width), Forms.ConvertToScaledPixel(size.Height));
		}
	}
}
