using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Vector2']/Docs" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public struct Vector2
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public Vector2(double x, double y)
			: this()
		{
			X = x;
			Y = y;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public Vector2(Point p)
			: this()
		{
			X = p.X;
			Y = p.Y;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public Vector2(double angle)
			: this()
		{
			X = Math.Cos(Math.PI * angle / 180);
			Y = Math.Sin(Math.PI * angle / 180);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="//Member[@MemberName='X']/Docs" />
		public double X { private set; get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="//Member[@MemberName='Y']/Docs" />
		public double Y { private set; get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="//Member[@MemberName='LengthSquared']/Docs" />
		public double LengthSquared
		{
			get { return X * X + Y * Y; }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="//Member[@MemberName='Length']/Docs" />
		public double Length
		{
			get { return Math.Sqrt(LengthSquared); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="//Member[@MemberName='Normalized']/Docs" />
		public Vector2 Normalized
		{
			get
			{
				double length = Length;

				if (length != 0)
				{
					return new Vector2(X / length, Y / length);
				}
				return new Vector2();
			}
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="//Member[@MemberName='AngleBetween']/Docs" />
		public static double AngleBetween(Vector2 v1, Vector2 v2)
		{
			return 180 * (Math.Atan2(v2.Y, v2.X) - Math.Atan2(v1.Y, v1.X)) / Math.PI;
		}

		public static Vector2 operator +(Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
		}

		public static Point operator +(Vector2 v, Point p)
		{
			return new Point(v.X + p.X, v.Y + p.Y);
		}

		public static Point operator +(Point p, Vector2 v)
		{
			return new Point(v.X + p.X, v.Y + p.Y);
		}

		public static Vector2 operator -(Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
		}

		public static Point operator -(Point p, Vector2 v)
		{
			return new Point(p.X - v.X, p.Y - v.Y);
		}

		public static Vector2 operator *(Vector2 v, double d)
		{
			return new Vector2(d * v.X, d * v.Y);
		}

		public static Vector2 operator *(double d, Vector2 v)
		{
			return new Vector2(d * v.X, d * v.Y);
		}

		public static Vector2 operator /(Vector2 v, double d)
		{
			return new Vector2(v.X / d, v.Y / d);
		}

		public static Vector2 operator -(Vector2 v)
		{
			return new Vector2(-v.X, -v.Y);
		}

		public static explicit operator Point(Vector2 v)
		{
			return new Point(v.X, v.Y);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Vector2.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString()
		{
			return string.Format("({0} {1})", X, Y);
		}
	}
}