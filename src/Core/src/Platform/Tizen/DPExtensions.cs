using System;
using Microsoft.Maui.Graphics;
using DeviceInfo = Tizen.UIExtensions.Common.DeviceInfo;
using EPoint = ElmSharp.Point;
using ERect = ElmSharp.Rect;
using ESize = ElmSharp.Size;
using Point = Microsoft.Maui.Graphics.Point;
using TRect = Tizen.UIExtensions.Common.Rect;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Platform
{
	public static class DPExtensions
	{
		public static Rect ToDP(this ERect rect)
		{
			return new Rect(ConvertToScaledDP(rect.X), ConvertToScaledDP(rect.Y), ConvertToScaledDP(rect.Width), ConvertToScaledDP(rect.Height));
		}

		public static ERect ToEFLPixel(this Rect rect)
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

		public static Rect ToDP(this TRect rect)
		{
			return new Rect(ConvertToScaledDP(rect.X), ConvertToScaledDP(rect.Y), ConvertToScaledDP(rect.Width), ConvertToScaledDP(rect.Height));
		}

		public static TRect ToPixel(this Rect rect)
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

		public static float ToScaledDP(this float pixel)
		{
			return pixel / (float)DeviceInfo.ScalingFactor;
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
			if (pixel == int.MaxValue)
				return double.PositiveInfinity;
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

		public static Point ToPoint(this EPoint point)
		{
			return new Point(DPExtensions.ConvertToScaledDP(point.X), DPExtensions.ConvertToScaledDP(point.Y));
		}

		public static PointF ToPointF(this EPoint point)
		{
			return new PointF((float)DPExtensions.ConvertToScaledDP(point.X), (float)DPExtensions.ConvertToScaledDP(point.Y));
		}
	}
}
