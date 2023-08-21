// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ShapeExtensions
	{
		internal static WindingMode GetPathWindingMode(this IDrawable drawable, IShapeView shapeView)
		{
			if (drawable is not ShapeDrawable || shapeView is null || shapeView.Shape is not Path path)
				return WindingMode.NonZero;

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
				fillRule = geometryGroup.FillRule;

			if (data is PathGeometry pathGeometry)
				fillRule = pathGeometry.FillRule;

			return fillRule == FillRule.EvenOdd ? WindingMode.EvenOdd : WindingMode.NonZero;
		}
	}
}
