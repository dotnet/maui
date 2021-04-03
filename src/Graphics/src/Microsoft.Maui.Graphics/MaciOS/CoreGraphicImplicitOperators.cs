using System;
using CoreGraphics;
namespace Microsoft.Maui.Graphics
{
    public partial struct RectangleF
    {
        public static implicit operator CGRect(RectangleF rect) => new CGRect(rect.X,rect.Y,rect.Width,rect.Height);
    }

    public partial struct Rectangle
    {
        public static implicit operator CGRect(Rectangle rect) => new CGRect(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public partial struct Size
    {
        public static implicit operator CGSize(Size size) => new CGSize(size.Width,size.Height);
        public static implicit operator CGPoint(Size size) => new CGPoint(size.Width, size.Height);
    }

    public partial struct SizeF
    {
        public static implicit operator CGSize(SizeF size) => new CGSize(size.Width, size.Height);
        public static implicit operator CGPoint(SizeF size) => new CGPoint(size.Width, size.Height);
    }

    public partial struct PointF
    {
        public static implicit operator CGSize(PointF size) => new CGSize(size.X, size.Y);
        public static implicit operator CGPoint(PointF size) => new CGPoint(size.X, size.Y);
    }

    public partial struct Point
    {
        public static implicit operator CGSize(Point size) => new CGSize(size.X, size.Y);
        public static implicit operator CGPoint(Point size) => new CGPoint(size.X, size.Y);
    }
}
