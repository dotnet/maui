using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	internal static class GraphicsExtensions
	{
		public static Rectangle ExpandTo(this Rectangle geometry, Thickness shadowMargin)
		{
			var canvasGeometry = new Rectangle(
			geometry.X - shadowMargin.Left,
			geometry.Y - shadowMargin.Top,
			geometry.Width + shadowMargin.HorizontalThickness,
			geometry.Height + shadowMargin.VerticalThickness);

			return canvasGeometry;
		}
	}
}