using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	internal static class GraphicsExtensions
	{
		public static Rect ExpandTo(this Rect geometry, Thickness shadowMargin)
		{
			var canvasGeometry = new Rect(
			geometry.X - shadowMargin.Left,
			geometry.Y - shadowMargin.Top,
			geometry.Width + shadowMargin.HorizontalThickness,
			geometry.Height + shadowMargin.VerticalThickness);

			return canvasGeometry;
		}
	}
}