using System;
using Microsoft.Maui.Graphics;
using ERect = ElmSharp.Rect;
using ESize = ElmSharp.Size;
using TRect = Tizen.UIExtensions.Common.Rect;
using TSize = Tizen.UIExtensions.Common.Size;
using DeviceInfo = Tizen.UIExtensions.Common.DeviceInfo;

namespace Microsoft.Maui
{
	public static class DPExtensions
	{
		public static Rectangle ToDP(this ERect rect)
		{
			return new Rectangle(ConvertToScaledDP(rect.X), ConvertToScaledDP(rect.Y), ConvertToScaledDP(rect.Width), ConvertToScaledDP(rect.Height));
		}

		public static ERect ToEFLPixel(this Rectangle rect)
		{
			return new ERect(ConvertToScaledPixel(rect.X), ConvertToScaledPixel(rect.Y), ConvertToScaledPixel(rect.Width), ConvertToScaledPixel(rect.Height));
		}

		public static Size ToDP(this ESize size)
		{
			return new Size(ConvertToScaledDP(size.Width), ConvertToScaledDP(size.Height));
		}

		public static ESize ToEFLPixel(this Size size)
		{
			return new ESize(ConvertToScaledPixel(size.Width), ConvertToScaledPixel(size.Height));
		}

		public static Rectangle ToDP(this TRect rect)
		{
			return new Rectangle(ConvertToScaledDP(rect.X), ConvertToScaledDP(rect.Y), ConvertToScaledDP(rect.Width), ConvertToScaledDP(rect.Height));
		}

		public static TRect ToPixel(this Rectangle rect)
		{
			return new TRect(ConvertToScaledPixel(rect.X), ConvertToScaledPixel(rect.Y), ConvertToScaledPixel(rect.Width), ConvertToScaledPixel(rect.Height));
		}

		public static Size ToDP(this TSize size)
		{
			return new Size(ConvertToScaledDP(size.Width), ConvertToScaledDP(size.Height));
		}

		public static TSize ToPixel(this Size size)
		{
			return new TSize(ConvertToScaledPixel(size.Width), ConvertToScaledPixel(size.Height));
		}

		public static int ToPixel(this double dp)
		{
			return (int)Math.Round(dp * DeviceInfo.DPI / 160.0);
		}

		public static int ToScaledPixel(this double dp)
		{
			return (int)Math.Round(dp * DeviceInfo.ScalingFactor);
		}

		public static double ToScaledDP(this int pixel)
		{
			return pixel / DeviceInfo.ScalingFactor;
		}

		public static double ToScaledDP(this double pixel)
		{
			return pixel / DeviceInfo.ScalingFactor;
		}

		public static int ToEflFontPoint(this double sp)
		{
			return (int)Math.Round(ConvertToScaledPixel(sp) * DeviceInfo.ElmScale);
		}

		public static double ToDPFont(this int eflPt)
		{
			return ConvertToScaledDP(eflPt / DeviceInfo.ElmScale);
		}

		public static int ConvertToPixel(double dp)
		{
			return (int)Math.Round(dp * DeviceInfo.DPI / 160.0);
		}

		public static int ConvertToScaledPixel(double dp)
		{
			return (int)Math.Round(dp * DeviceInfo.ScalingFactor);
		}

		public static double ConvertToScaledDP(int pixel)
		{
			return pixel / DeviceInfo.ScalingFactor;
		}

		public static double ConvertToScaledDP(double pixel)
		{
			return pixel / DeviceInfo.ScalingFactor;
		}

		public static int ConvertToEflFontPoint(double sp)
		{
			return (int)Math.Round(ConvertToScaledPixel(sp) * DeviceInfo.ElmScale);
		}

		public static double ConvertToDPFont(int eflPt)
		{
			return ConvertToScaledDP(eflPt / DeviceInfo.ElmScale);
		}
	}
}
