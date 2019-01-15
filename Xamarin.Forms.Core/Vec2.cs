using System;
using System.ComponentModel;

namespace Xamarin.Forms
{
	[Obsolete("This is no longer used, and might be removed at some point ")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public struct Vec2
	{
		public double X;
		public double Y;

		public Vec2(double x, double y)
		{
			X = x;
			Y = y;
		}
	}
}