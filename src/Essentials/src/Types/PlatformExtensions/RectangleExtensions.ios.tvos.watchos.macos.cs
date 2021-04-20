using System;
using Microsoft.Maui.Graphics;
using iOSRectangle = CoreGraphics.CGRect;

namespace Microsoft.Maui.Essentials
{
	public static class RectangleExtensions
	{
		public static Rectangle ToMauiRectangle(this iOSRectangle rect)
		{
			if (rect.X > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(rect.X));

			if (rect.Y > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(rect.Y));

			if (rect.Width > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(rect.Width));

			if (rect.Height > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(rect.Height));

			return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static RectangleF ToMauiRectangleF(this iOSRectangle rect) =>
			new RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

		public static iOSRectangle ToPlatformRectangle(this Rectangle rect) =>
			new iOSRectangle((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height);

		public static iOSRectangle ToPlatformRectangle(this RectangleF rect) =>
			new iOSRectangle((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height);
	}
}
