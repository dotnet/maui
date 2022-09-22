using System;
using Microsoft.Maui.Graphics;
using NRectangle = Tizen.NUI.Rectangle;
using TRect = Tizen.UIExtensions.Common.Rect;
using TSize = Tizen.UIExtensions.Common.Size;
using TPoint = Tizen.UIExtensions.Common.Point;
using DeviceInfo = Tizen.UIExtensions.Common.DeviceInfo;

namespace Microsoft.Maui.Platform
{
	public static class DPExtensions
	{
		internal static Rect ToDP(this NRectangle rect)
		{
			return new Rect(ConvertToScaledDP(rect.X), ConvertToScaledDP(rect.Y), ConvertToScaledDP(rect.Width), ConvertToScaledDP(rect.Height));
		}

		public static Rect ToDP(this TRect rect)
		{
			return new Rect(ConvertToScaledDP(rect.X), ConvertToScaledDP(rect.Y), ConvertToScaledDP(rect.Width), ConvertToScaledDP(rect.Height));
		}

		public static TRect ToPixel(this Rect rect)
		{
			return new TRect(ConvertToScaledPixel(rect.X), ConvertToScaledPixel(rect.Y), ConvertToScaledPixel(rect.Width), ConvertToScaledPixel(rect.Height));
		}

		public static TPoint ToPixel(this Point point)
		{
			return new TPoint(ConvertToScaledPixel(point.X), ConvertToScaledPixel(point.Y));
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
			if (double.IsPositiveInfinity(dp))
				return int.MaxValue;
			return (int)Math.Round(dp * DeviceInfo.ScalingFactor);
		}

		public static double ToScaledDP(this int pixel)
		{
			return pixel / DeviceInfo.ScalingFactor;
		}

		public static float ToScaledDP(this float pixel)
		{
			return pixel / (float)DeviceInfo.ScalingFactor;
		}

		public static double ToScaledDP(this double pixel)
		{
			return pixel / DeviceInfo.ScalingFactor;
		}

		public static double ToPoint(this double dp)
		{
			return dp * 72 / 160.0;
		}

		public static double ToScaledPoint(this double dp)
		{
			return dp.ToScaledPixel() * 72 / DeviceInfo.DPI;
		}

		public static int ConvertToPixel(double dp)
		{
			return (int)Math.Round(dp * DeviceInfo.DPI / 160.0);
		}

		public static int ConvertToScaledPixel(double dp)
		{
			if (double.IsPositiveInfinity(dp))
				return int.MaxValue;
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

		public static double ConvertToDPFont(int pt)
		{
			return ConvertToScaledDP(pt * DeviceInfo.DPI / 72.0);
		}
	}
}
