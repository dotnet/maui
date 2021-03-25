using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public static class BrushExtensions
	{
		internal const string BackgroundLayer = "BackgroundLayer";

		public static void UpdateBackground(this UIView control, IBrush brush)
		{
			if (control == null)
				return;

			// Remove previous background gradient layer if any
			RemoveBackgroundLayer(control);

			if (brush.IsNullOrEmpty())
				return;

			var backgroundLayer = GetBackgroundLayer(control, brush);

			if (backgroundLayer != null)
			{
				control.BackgroundColor = UIColor.Clear;
				control.InsertBackgroundLayer(backgroundLayer, 0);
			}
		}

		public static CALayer? GetBackgroundLayer(this UIView control, IBrush brush)
		{
			if (control == null)
				return null;

			if (brush is ISolidColorBrush solidColorBrush)
			{
				var solidColorLayer = new CALayer
				{
					Name = BackgroundLayer,
					ContentsGravity = CALayer.GravityResizeAspectFill,
					Frame = control.Bounds,
					BackgroundColor = solidColorBrush.Color.ToCGColor()
				};

				return solidColorLayer;
			}

			if (brush is ILinearGradientBrush linearGradientBrush)
			{
				var p1 = linearGradientBrush.StartPoint;
				var p2 = linearGradientBrush.EndPoint;

				var linearGradientLayer = new CAGradientLayer
				{
					Name = BackgroundLayer,
					ContentsGravity = CALayer.GravityResizeAspectFill,
					Frame = control.Bounds,
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

			if (brush is IRadialGradientBrush radialGradientBrush)
			{
				var center = radialGradientBrush.Center;
				var radius = radialGradientBrush.Radius;

				var radialGradientLayer = new CAGradientLayer
				{
					Name = BackgroundLayer,
					ContentsGravity = CALayer.GravityResizeAspectFill,
					Frame = control.Bounds,
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

			return null;
		}

		public static void InsertBackgroundLayer(this UIView view, CALayer backgroundLayer, int index = -1)
		{
			InsertBackgroundLayer(view.Layer, backgroundLayer, index);
		}

		public static void InsertBackgroundLayer(this CALayer layer, CALayer backgroundLayer, int index = -1)
		{
			RemoveBackgroundLayer(layer);

			if (backgroundLayer != null)
			{
				if (index > -1)
					layer.InsertSublayer(backgroundLayer, index);
				else
					layer.AddSublayer(backgroundLayer);
			}
		}

		public static void RemoveBackgroundLayer(this UIView view)
		{
			if (view != null)
				RemoveBackgroundLayer(view.Layer);
		}

		public static void RemoveBackgroundLayer(this CALayer layer)
		{
			if (layer != null)
			{
				if (layer.Name == BackgroundLayer)
					layer.RemoveFromSuperLayer();

				if (layer.Sublayers == null || layer.Sublayers.Count() == 0)
					return;

				foreach (var subLayer in layer.Sublayers)
				{
					if (subLayer.Name == BackgroundLayer)
						subLayer?.RemoveFromSuperLayer();
				}
			}
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