using System;
using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Foundation;

namespace Microsoft.Maui.Graphics
{
	public static partial class PaintExtensions
	{
		public static CALayer? ToCALayer(this Paint paint, CGRect frame = default)
		{
			if (paint is SolidPaint solidPaint)
				return solidPaint.CreateCALayer(frame);

			if (paint is LinearGradientPaint linearGradientPaint)
				return linearGradientPaint.CreateCALayer(frame);

			if (paint is RadialGradientPaint radialGradientPaint)
				return radialGradientPaint.CreateCALayer(frame);

			if (paint is ImagePaint imagePaint)
				return imagePaint.CreateCALayer(frame);

			if (paint is PatternPaint patternPaint)
				return patternPaint.CreateCALayer(frame);

			return null;
		}

		public static CALayer? CreateCALayer(this SolidPaint solidPaint, CGRect frame = default)
		{
			var solidColorLayer = new CALayer
			{
				ContentsGravity = CALayer.GravityResizeAspectFill,
				Frame = frame,
				BackgroundColor = solidPaint.Color.ToCGColor()
			};

			return solidColorLayer;
		}

		public static CALayer? CreateCALayer(this GradientPaint gradientPaint, CGRect frame = default)
		{
			if (gradientPaint is LinearGradientPaint linearGradientPaint)
				return linearGradientPaint.CreateCALayer(frame);

			if (gradientPaint is RadialGradientPaint radialGradientPaint)
				return radialGradientPaint.CreateCALayer(frame);

			return null;
		}

		public static CALayer? CreateCALayer(this LinearGradientPaint linearGradientPaint, CGRect frame = default)
		{
			var p1 = linearGradientPaint.StartPoint;
			var p2 = linearGradientPaint.EndPoint;

			var linearGradientLayer = new CAGradientLayer
			{
				ContentsGravity = CALayer.GravityResizeAspectFill,
				Frame = frame,
				LayerType = CAGradientLayerType.Axial,
				StartPoint = new CGPoint(p1.X, p1.Y),
				EndPoint = new CGPoint(p2.X, p2.Y)
			};

			if (linearGradientPaint.GradientStops != null && linearGradientPaint.GradientStops.Length > 0)
			{
				var orderedStops = linearGradientPaint.GradientStops.OrderBy(x => x.Offset).ToList();
				linearGradientLayer.Colors = orderedStops.Select(x => x.Color.ToCGColor()).ToArray();
				linearGradientLayer.Locations = GetCAGradientLayerLocations(orderedStops);
			}

			return linearGradientLayer;
		}

		public static CALayer? CreateCALayer(this RadialGradientPaint radialGradientPaint, CGRect frame = default)
		{
			var center = radialGradientPaint.Center;
			var radius = radialGradientPaint.Radius;

			var radialGradientLayer = new CAGradientLayer
			{
				ContentsGravity = CALayer.GravityResizeAspectFill,
				Frame = frame,
				LayerType = CAGradientLayerType.Radial,
				StartPoint = new CGPoint(center.X, center.Y),
				EndPoint = GetRadialGradientPaintEndPoint(center, radius),
				CornerRadius = (float)radius
			};

			if (radialGradientPaint.GradientStops != null && radialGradientPaint.GradientStops.Length > 0)
			{
				var orderedStops = radialGradientPaint.GradientStops.OrderBy(x => x.Offset).ToList();
				radialGradientLayer.Colors = orderedStops.Select(x => x.Color.ToCGColor()).ToArray();
				radialGradientLayer.Locations = GetCAGradientLayerLocations(orderedStops);
			}

			return radialGradientLayer;
		}

		public static CALayer? CreateCALayer(this ImagePaint imagePaint, CGRect frame = default)
		{
			throw new NotImplementedException();
		}

		public static CALayer? CreateCALayer(this PatternPaint patternPaint, CGRect frame = default)
		{
			throw new NotImplementedException();
		}

		static CGPoint GetRadialGradientPaintEndPoint(Point startPoint, double radius)
		{
			double x = startPoint.X == 1 ? (startPoint.X - radius) : (startPoint.X + radius);

			if (x < 0)
				x = 0;

			if (x > 1)
				x = 1;

			double y = startPoint.Y == 1 ? (startPoint.Y - radius) : (startPoint.Y + radius);

			if (y < 0)
				y = 0;

			if (y > 1)
				y = 1;

			return new CGPoint(x, y);
		}

		static NSNumber[] GetCAGradientLayerLocations(List<GradientStop> gradientStops)
		{
			if (gradientStops == null || gradientStops.Count == 0)
				return new NSNumber[0];

			if (gradientStops.Count > 1 && gradientStops.Any(gt => gt.Offset != 0))
				return gradientStops.Select(x => new NSNumber(x.Offset)).ToArray();
			else
			{
				int itemCount = gradientStops.Count;
				int index = 0;
				float step = 1.0f / itemCount;

				NSNumber[] locations = new NSNumber[itemCount];

				foreach (var gradientStop in gradientStops)
				{
					float location = step * index;
					bool setLocation = !gradientStops.Any(gt => gt.Offset > location);

					if (gradientStop.Offset == 0 && setLocation)
						locations[index] = new NSNumber(location);
					else
						locations[index] = new NSNumber(gradientStop.Offset);

					index++;
				}

				return locations;
			}
		}
	}
}