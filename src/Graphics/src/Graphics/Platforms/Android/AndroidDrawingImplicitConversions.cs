using System;
using Android.Graphics;

namespace Microsoft.Maui.Graphics
{
	public partial struct RectF
	{
		public static implicit operator global::Android.Graphics.Rect(RectF rect) => new global::Android.Graphics.Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
		public static implicit operator global::Android.Graphics.RectF(RectF rect) => new global::Android.Graphics.RectF(rect.X, rect.Y, rect.Width, rect.Height);
	}

	public partial struct Rect
	{
		public static implicit operator global::Android.Graphics.Rect(Rect rect) => new global::Android.Graphics.Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
		public static implicit operator global::Android.Graphics.RectF(Rect rect) => new global::Android.Graphics.RectF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
	}

	public partial struct PointF
	{
		public static implicit operator global::Android.Graphics.PointF(PointF size) => new global::Android.Graphics.PointF(size.X, size.Y);
		public static implicit operator global::Android.Graphics.Point(PointF size) => new global::Android.Graphics.Point((int)size.X, (int)size.Y);
	}

	public partial struct Point
	{
		public static implicit operator global::Android.Graphics.PointF(Point size) => new global::Android.Graphics.PointF((float)size.X, (float)size.Y);
		public static implicit operator global::Android.Graphics.Point(Point size) => new global::Android.Graphics.Point((int)size.X, (int)size.Y);
	}
}
