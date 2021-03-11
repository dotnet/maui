using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
    [DebuggerDisplay("X={X}, Y={Y}, Width={Width}, Height={Height}")]
    public struct RectangleF
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public static RectangleF Zero = new RectangleF();

        public override string ToString()
        {
            return string.Format("{{X={0} Y={1} Width={2} Height={3}}}", X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture), Width.ToString(CultureInfo.InvariantCulture),
                Height.ToString(CultureInfo.InvariantCulture));
        }

        // constructors
        public RectangleF(float x, float y, float width, float height) : this()
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleF(PointF loc, SizeF sz) : this(loc.X, loc.Y, sz.Width, sz.Height)
        {
        }

        public static RectangleF FromLTRB(float left, float top, float right, float bottom)
        {
            return new RectangleF(left, top, right - left, bottom - top);
        }

        public bool Equals(RectangleF other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is RectangleF && Equals((RectangleF)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RectangleF r1, RectangleF r2)
        {
            return (r1.Location == r2.Location) && (r1.Size == r2.Size);
        }

        public static bool operator !=(RectangleF r1, RectangleF r2)
        {
            return !(r1 == r2);
        }

        // Hit Testing / Intersection / Union
        public bool Contains(RectangleF rect)
        {
            return X <= rect.X && Right >= rect.Right && Y <= rect.Y && Bottom >= rect.Bottom;
        }

        public bool Contains(PointF pt)
        {
            return Contains(pt.X, pt.Y);
        }

        public bool Contains(float x, float y)
        {
            return (x >= Left) && (x < Right) && (y >= Top) && (y < Bottom);
        }

        public bool IntersectsWith(RectangleF r)
        {
            return !((Left >= r.Right) || (Right <= r.Left) || (Top >= r.Bottom) || (Bottom <= r.Top));
        }

        public RectangleF Union(RectangleF r)
        {
            return Union(this, r);
        }

        public static RectangleF Union(RectangleF r1, RectangleF r2)
        {
            return FromLTRB(Math.Min(r1.Left, r2.Left), Math.Min(r1.Top, r2.Top), Math.Max(r1.Right, r2.Right), Math.Max(r1.Bottom, r2.Bottom));
        }

        public RectangleF Intersect(RectangleF r)
        {
            return Intersect(this, r);
        }

        public static RectangleF Intersect(RectangleF r1, RectangleF r2)
        {
            float x = Math.Max(r1.X, r2.X);
            float y = Math.Max(r1.Y, r2.Y);
            float width = Math.Min(r1.Right, r2.Right) - x;
            float height = Math.Min(r1.Bottom, r2.Bottom) - y;

            if (width < 0 || height < 0)
            {
                return Zero;
            }
            return new RectangleF(x, y, width, height);
        }

        // Position/Size
        public float Top
        {
            get => Y;
            set => Y = value;
        }

        public float Bottom
        {
            get => Y + Height;
            set => Height = value - Y;
        }

        public float Right
        {
            get => X + Width;
            set => Width = value - X;
        }

        public float Left
        {
            get => X;
            set => X = value;
        }

        public bool IsEmpty => (Width <= 0) || (Height <= 0);

        public SizeF Size
        {
            get => new SizeF(Width, Height);
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }

        public PointF Location
        {
            get => new PointF(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public PointF Center => new PointF(X + Width / 2, Y + Height / 2);

        // Inflate and Offset
        public RectangleF Inflate(SizeF sz)
        {
            return Inflate(sz.Width, sz.Height);
        }

        public RectangleF Inflate(float width, float height)
        {
            RectangleF r = this;
            r.X -= width;
            r.Y -= height;
            r.Width += width * 2;
            r.Height += height * 2;
            return r;
        }

        public RectangleF Offset(float dx, float dy)
        {
            RectangleF r = this;
            r.X += dx;
            r.Y += dy;
            return r;
        }

        public RectangleF Offset(PointF dr)
        {
            return Offset(dr.X, dr.Y);
        }

        public RectangleF Round()
        {
            return new RectangleF((float)Math.Round(X), (float)Math.Round(Y), (float)Math.Round(Width), (float)Math.Round(Height));
        }

        public void Deconstruct(out float x, out float y, out float width, out float height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }
    }
}