using System;
using Android.Graphics;

namespace Microsoft.Maui.Graphics
{

    public partial struct RectangleF
    {
        public static implicit operator Rect(RectangleF rect) => new Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        public static implicit operator RectF(RectangleF rect) => new RectF(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public partial struct Rectangle
    {
        public static implicit operator Rect(Rectangle rect) => new Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        public static implicit operator RectF(Rectangle rect) => new RectF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
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
