using System;
using System.Drawing;
using Android.Graphics;

namespace Microsoft.Maui.Essentials
{
	public static class RectangleExtensions
	{
		public static Rectangle ToSystemRectangle(this Rect rect) =>
			new Rectangle(rect.Left, rect.Top, rect.Width(), rect.Height());

		public static RectangleF ToSystemRectangleF(this RectF rect) =>
			new RectangleF(rect.Left, rect.Top, rect.Width(), rect.Height());

		public static Rect ToPlatformRectangle(this Rectangle rect) =>
			new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);

		public static RectF ToPlatformRectangleF(this RectangleF rect) =>
			new RectF(rect.Left, rect.Top, rect.Right, rect.Bottom);
	}
}
