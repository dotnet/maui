using Microsoft.Maui.Graphics;
using NSize = Tizen.NUI.Size2D;
using Rect = Microsoft.Maui.Graphics.Rect;
using TRect = Tizen.UIExtensions.Common.Rect;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	/// <summary>
	/// Extension class, provides DIP convert functionalities
	/// </summary>
	public static class DensityIndependentPixelExtensions
	{
		public static Rect ToDP(this TRect rect)
		{
			return new Rect(Forms.ConvertToScaledDP(rect.X), Forms.ConvertToScaledDP(rect.Y), Forms.ConvertToScaledDP(rect.Width), Forms.ConvertToScaledDP(rect.Height));
		}

		public static TRect ToPixel(this Rect rect)
		{
			return new TRect(Forms.ConvertToScaledPixel(rect.X), Forms.ConvertToScaledPixel(rect.Y), Forms.ConvertToScaledPixel(rect.Width), Forms.ConvertToScaledPixel(rect.Height));
		}

		public static Size ToDP(this TSize size)
		{
			return new Size(Forms.ConvertToScaledDP(size.Width), Forms.ConvertToScaledDP(size.Height));
		}

		public static Size ToDP(this NSize size)
		{
			return new Size(Forms.ConvertToScaledDP(size.Width), Forms.ConvertToScaledDP(size.Height));
		}

		public static TSize ToPixel(this Size size)
		{
			return new TSize(Forms.ConvertToScaledPixel(size.Width), Forms.ConvertToScaledPixel(size.Height));
		}
	}
}
