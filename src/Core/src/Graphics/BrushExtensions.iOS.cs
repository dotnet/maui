using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Foundation;

namespace Microsoft.Maui.Graphics
{
	public static partial class BrushExtensions
	{
		public static CALayer? CreateCALayer(this ISolidColorBrush solidColorBrush, CGRect frame = default)
		{
			var solidColorLayer = new CALayer
			{
				ContentsGravity = CALayer.GravityResizeAspectFill,
				Frame = frame,
				BackgroundColor = solidColorBrush.Color.ToCGColor()
			};

			return solidColorLayer;
		}

		public static CALayer? CreateCALayer(this IGradientBrush gradientBrush, CGRect frame = default)
		{
			if (gradientBrush is ILinearGradientBrush linearGradientBrush)
				return linearGradientBrush.CreateCALayer(frame);

			if (gradientBrush is IRadialGradientBrush radialGradientBrush)
				return radialGradientBrush.CreateCALayer(frame);

			return null;
		}

		public static CALayer? CreateCALayer(this ILinearGradientBrush linearGradientBrush, CGRect frame = default)
		{
			var p1 = linearGradientBrush.StartPoint;
			var p2 = linearGradientBrush.EndPoint;

			var linearGradientLayer = new CAGradientLayer
			{
				ContentsGravity = CALayer.GravityResizeAspectFill,
				Frame = frame,
				LayerType = CAGradientLayerType.Axial,
				StartPoint = new CGPoint(p1.X, p1.Y),
				EndPoint = new CGPoint(p2.X, p2.Y)
			};

			if (linearGradientBrush.GradientStops != null && linearGradientBrush.GradientStops.Count > 0)
			{
				var orderedStops = linearGradientBrush.GradientStops.OrderBy(x => x.Offset).ToList();
				linearGradientLayer.Colors = orderedStops.Select(x => x.Color.ToCGColor()).ToArray();
				linearGradientLayer.Locations = GetCAGradientLayerLocations(orderedStops);
			}

			return linearGradientLayer;
		}

		public static CALayer? CreateCALayer(this IRadialGradientBrush radialGradientBrush, CGRect frame = default)
		{
			var center = radialGradientBrush.Center;
			var radius = radialGradientBrush.Radius;

			var radialGradientLayer = new CAGradientLayer
			{
				ContentsGravity = CALayer.GravityResizeAspectFill,
				Frame = frame,
				LayerType = CAGradientLayerType.Radial,
				StartPoint = new CGPoint(center.X, center.Y),
				EndPoint = GetRadialGradientBrushEndPoint(center, radius),
				CornerRadius = (float)radius
			};

			if (radialGradientBrush.GradientStops != null && radialGradientBrush.GradientStops.Count > 0)
			{
				var orderedStops = radialGradientBrush.GradientStops.OrderBy(x => x.Offset).ToList();
				radialGradientLayer.Colors = orderedStops.Select(x => x.Color.ToCGColor()).ToArray();
				radialGradientLayer.Locations = GetCAGradientLayerLocations(orderedStops);
			}

			return radialGradientLayer;
		}

		static CGPoint GetRadialGradientBrushEndPoint(Point startPoint, double radius)
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

		static NSNumber[] GetCAGradientLayerLocations(List<IGradientStop> gradientStops)
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