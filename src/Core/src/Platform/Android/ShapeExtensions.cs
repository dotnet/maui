using Android.Graphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public static class ShapeExtensions
	{
		public static Path ToPlatform(this IShape shape, Graphics.Rect bounds, float strokeThickness, float density, bool innerPath = false)
		{
			return shape.ToPlatform(bounds, strokeThickness, density, innerPath, includeShapeStroke: false);
		}

		internal static Path ToPlatform(this IShape shape, Graphics.Rect bounds, float strokeThickness, float density, bool innerPath, bool includeShapeStroke)
		{
			Graphics.Rect pathBounds;
			PathF path;

			if (innerPath)
			{
				if (shape is IRoundRectangle roundRectangle)
				{
					path = roundRectangle.InnerPathForBounds(bounds, strokeThickness, includeShapeStroke);
					return path.AsAndroidPath(scaleX: density, scaleY: density);
				}

				float x = (float)bounds.X + strokeThickness / 2;
				float y = (float)bounds.Y + strokeThickness / 2;
				float width = (float)bounds.Width - strokeThickness;
				float height = (float)bounds.Height - strokeThickness;

				pathBounds = new Graphics.Rect(x, y, width, height);
			}
			else
			{
				pathBounds = bounds;
			}

			path = shape is IShapeWithStroke shapeWithStroke
				? shapeWithStroke.PathForBounds(pathBounds, includeShapeStroke)
				: shape.PathForBounds(pathBounds);

			return path.AsAndroidPath(scaleX: density, scaleY: density);
		}
	}
}