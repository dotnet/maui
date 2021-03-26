using System.Linq;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
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
					linearGradientLayer.Colors = orderedStops.Select(x => x.Color.ToCGColor()).ToArray();
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

				if (layer.Sublayers == null || layer.Sublayers.Count() == 0)
					return;

				foreach (var subLayer in layer.Sublayers)
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
			if (layer != null && layer.Sublayers != null)
			{
				foreach (var sublayer in layer.Sublayers)
				{
					UpdateBackgroundLayer(sublayer, bounds);

					if (sublayer.Name == BackgroundLayer && sublayer.Frame != bounds)
						sublayer.Frame = bounds;
				}
			}
		}

		static bool ShouldUseParentView(UIView view)
		{
			if (view is UILabel)
				return true;

			return false;
		}
	}
}