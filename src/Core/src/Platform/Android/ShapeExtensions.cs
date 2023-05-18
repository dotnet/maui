using Android.Graphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public static class ShapeExtensions
	{
		public static Path ToPlatform(this IShape shape, Graphics.Rect bounds, float strokeThickness, bool innerPath = false)
		{
			float x = (float)bounds.X + strokeThickness / 2;
			float y = (float)bounds.Y + strokeThickness / 2;
			float width = (float)bounds.Width - strokeThickness;
			float height = (float)bounds.Height - strokeThickness;

			var pathBounds = new Graphics.Rect(x, y, width, height);

			PathF path;

			if (innerPath && shape is IRoundRectangle roundRectangle)
				path = roundRectangle.InnerPathForBounds(pathBounds, strokeThickness);
			else
				path = shape.PathForBounds(pathBounds);

			return path.AsAndroidPath();
		}
	}
}