#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static partial class BrushExtensions
	{
		const string BackgroundLayer = "BackgroundLayer";

		public static void UpdateBackground(this UIView control, Brush brush)
		{
			if (control == null)
				return;

			UIView view = ShouldUseParentView(control) ? control.Superview : control;

			// Remove previous background gradient layer if any
			RemoveBackgroundLayer(view);

			if (Brush.IsNullOrEmpty(brush))
				return;

			var backgroundLayer = GetBackgroundLayer(control, brush);

			if (backgroundLayer != null)
			{
				control.BackgroundColor = UIColor.Clear;
				view.InsertBackgroundLayer(backgroundLayer, 0);
			}
		}

		public static CALayer GetBackgroundLayer(this UIView control, Brush brush)
		{
			if (control == null)
				return null;

			if (brush is SolidColorBrush solidColorBrush)
			{
				var linearGradientLayer = new CALayer
				{
					Name = BackgroundLayer,
					ContentsGravity = CALayer.GravityResizeAspectFill,
					Frame = control.Bounds,
					BackgroundColor = solidColorBrush.Color.ToCGColor()
				};

				return linearGradientLayer;
			}

			if (brush is LinearGradientBrush linearGradientBrush)
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
					linearGradientLayer.Colors = GetCAGradientLayerColors(orderedStops);
					linearGradientLayer.Locations = GetCAGradientLayerLocations(orderedStops);
				}

				return linearGradientLayer;
			}

			if (brush is RadialGradientBrush radialGradientBrush)
			{
				var center = radialGradientBrush.Center;
				var radius = radialGradientBrush.Radius;

				var radialGradientLayer = new CAGradientLayer
				{
					Name = BackgroundLayer,
					ContentsGravity = CALayer.GravityResizeAspectFill,
					Frame = control.Bounds,
#pragma warning disable CA1416 // TODO:  'CAGradientLayerType.Radial' is only supported on: 'ios' 12.0 and later
					LayerType = CAGradientLayerType.Radial,
#pragma warning restore CA1416
					StartPoint = new CGPoint(center.X, center.Y),
					EndPoint = GetRadialGradientBrushEndPoint(center, radius),
					CornerRadius = (float)radius
				};

				if (radialGradientBrush.GradientStops != null && radialGradientBrush.GradientStops.Count > 0)
				{
					var orderedStops = radialGradientBrush.GradientStops.OrderBy(x => x.Offset).ToList();
					radialGradientLayer.Colors = GetCAGradientLayerColors(orderedStops);
					radialGradientLayer.Locations = GetCAGradientLayerLocations(orderedStops);
				}

				return radialGradientLayer;
			}

			return null;
		}

		public static UIImage GetBackgroundImage(this UIView control, Brush brush)
		{
			if (control == null || brush == null || brush.IsEmpty)
				return null;

			var backgroundLayer = control.GetBackgroundLayer(brush);

			if (backgroundLayer == null)
				return null;

			UIGraphics.BeginImageContextWithOptions(backgroundLayer.Bounds.Size, false, UIScreen.MainScreen.Scale);

			if (UIGraphics.GetCurrentContext() == null)
				return null;

			backgroundLayer.RenderInContext(UIGraphics.GetCurrentContext());
			UIImage gradientImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return gradientImage;
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
					layer?.RemoveFromSuperLayer();

				var sublayers = layer.Sublayers;
				if (sublayers is null || sublayers.Length == 0)
					return;

				foreach (var subLayer in sublayers)
				{
					if (subLayer.Name == BackgroundLayer)
						subLayer?.RemoveFromSuperLayer();
				}
			}
		}

		public static void UpdateBackgroundLayer(this UIView view)
		{
			if (view == null || view.Frame.IsEmpty)
				return;

			var layer = view.Layer;

			UpdateBackgroundLayer(layer, view.Bounds);
		}

		static void UpdateBackgroundLayer(this CALayer layer, CGRect bounds)
		{
			var sublayers = layer?.Sublayers;
			if (sublayers is not null)
			{
				foreach (var sublayer in sublayers)
				{
					UpdateBackgroundLayer(sublayer, bounds);

					if (sublayer.Name == BackgroundLayer && sublayer.Frame != bounds)
						sublayer.Frame = bounds;
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

		static NSNumber[] GetCAGradientLayerLocations(List<GradientStop> gradientStops)
		{
			if (gradientStops == null || gradientStops.Count == 0)
				return Array.Empty<NSNumber>();

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

		static CGColor[] GetCAGradientLayerColors(List<GradientStop> gradientStops)
		{
			if (gradientStops == null || gradientStops.Count == 0)
				return Array.Empty<CGColor>();

			CGColor[] colors = new CGColor[gradientStops.Count];

			int index = 0;
			foreach (var gradientStop in gradientStops)
			{
				if (gradientStop.Color == Colors.Transparent)
				{
					var color = gradientStops[index == 0 ? index + 1 : index - 1].Color;
					CGColor nativeColor = color.ToPlatform().ColorWithAlpha(0.0f).CGColor;
					colors[index] = nativeColor;
				}
				else
					colors[index] = gradientStop.Color.ToCGColor();

				index++;
			}

			return colors;
		}

		static bool ShouldUseParentView(UIView view)
		{
			if (view is UILabel)
				return true;

			return false;
		}
	}
}