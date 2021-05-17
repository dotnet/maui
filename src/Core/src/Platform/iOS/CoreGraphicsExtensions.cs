using System;
using CoreGraphics;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class CoreGraphicsExtensions
	{
		public static Point ToPoint(this CGPoint size)
		{
			return new Point((float)size.X, (float)size.Y);
		}

		public static Size ToSize(this CGSize size)
		{
			return new Size((float)size.Width, (float)size.Height);
		}

		public static CGSize ToCGSize(this Size size)
		{
			return new CGSize(size.Width, size.Height);
		}

		public static Rectangle ToRectangle(this CGRect rect)
		{
			return new Rectangle((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
		}

		public static CGRect ToCGRect(this Rectangle rect)
		{
			return new CGRect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static bool IsCloseTo(this CGSize size0, CGSize size1, nfloat tolerance)
		{
			var diff = size0 - size1;
			return Math.Abs(diff.Width) < tolerance && Math.Abs(diff.Height) < tolerance;
		}
	}
}
