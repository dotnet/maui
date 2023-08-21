// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Android.Graphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public static class ShapeExtensions
	{
		public static Path ToPlatform(this IShape shape, Graphics.Rect bounds, float strokeThickness, bool innerPath = false)
		{
			Graphics.Rect pathBounds;
			PathF path;

			if (innerPath)
			{
				if (shape is IRoundRectangle roundRectangle)
				{
					path = roundRectangle.InnerPathForBounds(bounds, strokeThickness);
					return path.AsAndroidPath();
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

			path = shape.PathForBounds(pathBounds);

			return path.AsAndroidPath();
		}
	}
}