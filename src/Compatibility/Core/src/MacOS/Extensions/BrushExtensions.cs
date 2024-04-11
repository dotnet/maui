using System.Linq;
using AppKit;
using CoreAnimation;
using CoreGraphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
{
	public static partial class BrushExtensions
	{
		const string BackgroundLayer = "BackgroundLayer";
		const string SolidColorBrushLayer = "SolidColorBrushLayer";

		public static void UpdateBackground(this NSView control, Brush brush)
		{
			if (control == null)
				return;

			NSView view = ShouldUseParentView(control) ? control.Superview : control;

			// Clear previous background color
			if (control.Layer != null && control.Layer.Name == SolidColorBrushLayer)
				control.Layer.BackgroundColor = NSColor.Clear.CGColor;

			// Remove previous background gradient layer if any
			RemoveBackgroundLayer(view);

			if (Brush.IsNullOrEmpty(brush))
				return;

			control.WantsLayer = true;
			control.LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.BeforeViewResize;

			if (brush is SolidColorBrush solidColorBrush)
			{
				control.Layer.Name = SolidColorBrushLayer;

				var backgroundColor = solidColorBrush.Color;

				if (backgroundColor == Color.Default)
					control.Layer.BackgroundColor = NSColor.Clear.CGColor;
				else
					control.Layer.BackgroundColor = backgroundColor.ToCGColor();
			}
			else
			{
				var backgroundLayer = GetBackgroundLayer(control, brush);

				if (backgroundLayer != null)
					view.InsertBackgroundLayer(backgroundLayer, 0);
			}
		}

		public static CAGradientLayer GetBackgroundLayer(this NSView control, Brush brush)
		{
			if (control == null)
				return null;

			if (brush is LinearGradientBrush linearGradientBrush)
			{
				var p1 = linearGradientBrush.StartPoint;
				var p2 = linearGradientBrush.EndPoint;

				var linearGradientLayer = new CAGradientLayer
				{
					Name = BackgroundLayer,
					AutoresizingMask = CAAutoresizingMask.HeightSizable | CAAutoresizingMask.WidthSizable,
					ContentsGravity = CALayer.GravityResizeAspectFill,
					Frame = control.Bounds,
					LayerType = CAGradientLayerType.Axial,
					StartPoint = new CGPoint(p1.X, p1.Y),
					EndPoint = new CGPoint(p2.X, p2.Y),
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
					Frame = control.Bounds,
					AutoresizingMask = CAAutoresizingMask.HeightSizable | CAAutoresizingMask.WidthSizable,
					ContentsGravity = CALayer.GravityResizeAspectFill,
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

		public static NSImage GetBackgroundImage(this NSView control, Brush brush)
		{
			if (control == null || brush == null || brush.IsEmpty)
				return null;

			var backgroundLayer = control.GetBackgroundLayer(brush);

			if (backgroundLayer == null)
				return null;

			NSImage backgroundImage = new NSImage(new CGSize(backgroundLayer.Bounds.Width, backgroundLayer.Bounds.Height));
			backgroundImage.LockFocus();
			var context = NSGraphicsContext.CurrentContext.GraphicsPort;
			backgroundLayer.RenderInContext(context);
			backgroundImage.UnlockFocus();

			return backgroundImage;
		}

		public static void InsertBackgroundLayer(this NSView view, CAGradientLayer backgroundLayer, int index)
		{
			InsertBackgroundLayer(view.Layer, backgroundLayer, index);
		}

		public static void InsertBackgroundLayer(this CALayer layer, CAGradientLayer backgroundLayer, int index)
		{
			RemoveBackgroundLayer(layer);

			if (backgroundLayer != null)
				layer.InsertSublayer(backgroundLayer, index);
		}

		public static void RemoveBackgroundLayer(this NSView view)
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

		static bool ShouldUseParentView(NSView view)
		{
			if (view is NSButton || view is NSTextField || view is NSDatePicker || view is NSSlider || view is NSStepper)
				return true;

			return false;
		}


	}
}