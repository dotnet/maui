using System;
using System.Drawing;
using ERect = ElmSharp.Rect;

namespace Microsoft.Maui.Essentials
{
	public static class RectangleExtensions
	{
		public static Rectangle ToSystemRectangle(this ERect rect) =>
			new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height);

		public static RectangleF ToSystemRectangleF(this ERect rect) =>
			new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height);

		public static ERect ToPlatformRectangle(this Rectangle rect) =>
			new ERect(rect.Left, rect.Top, rect.Right, rect.Bottom);

		public static ERect ToPlatformRectangle(this RectangleF rect) =>
			ToPlatformRectangleF(rect);

		public static ERect ToPlatformRectangleF(this RectangleF rect) =>
			new ERect((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
	}
}
