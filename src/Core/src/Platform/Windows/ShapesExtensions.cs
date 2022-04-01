using System;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Platform
{
	public static class ShapesExtensions
	{
		public static void UpdateShape(this W2DGraphicsView platformView, IShapeView shapeView)
		{
			Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362));
			platformView.Drawable = new ShapeDrawable(shapeView);
		}

		public static void InvalidateShape(this W2DGraphicsView platformView, IShapeView shapeView)
		{
			Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362));
			platformView.Invalidate();
		}
	}
}
