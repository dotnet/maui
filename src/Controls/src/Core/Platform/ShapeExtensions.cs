using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ShapeExtensions
	{
		internal static WindingMode GetPathWindingMode(this IDrawable drawable, IShapeView shapeView)
		{
			if (drawable is not ShapeDrawable || shapeView is null || shapeView.Shape is not Path path)

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				return WindingMode.NonZero;

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
				fillRule = geometryGroup.FillRule;

			if (data is PathGeometry pathGeometry)
				fillRule = pathGeometry.FillRule;
After:
			{
				return WindingMode.NonZero;
			}

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
			{
				fillRule = geometryGroup.FillRule;
			}

			if (data is PathGeometry pathGeometry)
			{
				fillRule = pathGeometry.FillRule;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				return WindingMode.NonZero;

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
				fillRule = geometryGroup.FillRule;

			if (data is PathGeometry pathGeometry)
				fillRule = pathGeometry.FillRule;
After:
			{
				return WindingMode.NonZero;
			}

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
			{
				fillRule = geometryGroup.FillRule;
			}

			if (data is PathGeometry pathGeometry)
			{
				fillRule = pathGeometry.FillRule;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				return WindingMode.NonZero;

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
				fillRule = geometryGroup.FillRule;

			if (data is PathGeometry pathGeometry)
				fillRule = pathGeometry.FillRule;
After:
			{
				return WindingMode.NonZero;
			}

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
			{
				fillRule = geometryGroup.FillRule;
			}

			if (data is PathGeometry pathGeometry)
			{
				fillRule = pathGeometry.FillRule;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				return WindingMode.NonZero;

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
				fillRule = geometryGroup.FillRule;

			if (data is PathGeometry pathGeometry)
				fillRule = pathGeometry.FillRule;
After:
			{
				return WindingMode.NonZero;
			}

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
			{
				fillRule = geometryGroup.FillRule;
			}

			if (data is PathGeometry pathGeometry)
			{
				fillRule = pathGeometry.FillRule;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				return WindingMode.NonZero;

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
				fillRule = geometryGroup.FillRule;

			if (data is PathGeometry pathGeometry)
				fillRule = pathGeometry.FillRule;
After:
			{
				return WindingMode.NonZero;
			}

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
			{
				fillRule = geometryGroup.FillRule;
			}

			if (data is PathGeometry pathGeometry)
			{
				fillRule = pathGeometry.FillRule;
			}
*/
			{
				return WindingMode.NonZero;
			}

			var data = path.Data;

			FillRule fillRule = FillRule.EvenOdd;

			if (data is GeometryGroup geometryGroup)
			{
				fillRule = geometryGroup.FillRule;
			}

			if (data is PathGeometry pathGeometry)
			{
				fillRule = pathGeometry.FillRule;
			}

			return fillRule == FillRule.EvenOdd ? WindingMode.EvenOdd : WindingMode.NonZero;
		}
	}
}
