using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class UIViewExtensions
	{
		public static PointF[] GetPointsInView(this UIView target, UIEvent touchEvent)
		{
			var touchSet = touchEvent.TouchesForView(target);
			if (touchSet.Count == 0)
			{
				return Array.Empty<PointF>();
			}

			var touches = touchSet.ToArray<UITouch>();
			var points = new PointF[touches.Length];
			for (int i = 0; i < touches.Length; i++)
			{
				var touch = touches[i];
				var point = touch.LocationInView(target);
				points[i] = new PointF((float)point.X, (float)point.Y);
			}

			return points;
		}

		public static PointF[] GetPointsInView(this UIView target, NSSet touchSet)
		{
			var touches = touchSet.ToArray<UITouch>();
			var points = new PointF[touches.Length];
			for (int i = 0; i < touches.Length; i++)
			{
				var touch = touches[i];
				var point = touch.LocationInView(target);
				points[i] = new PointF((float)point.X, (float)point.Y);
			}

			return points;
		}
	}
}
