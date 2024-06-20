using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this UIView platformView, IBorderStroke border)
		{
			var borderShape = border.Shape;
			CALayer? backgroundLayer = platformView.Layer as MauiCALayer;

			if (backgroundLayer == null && borderShape == null)
				return;

			platformView.UpdateMauiCALayer(border);
		}

		public static void UpdateStroke(this UIView platformView, IBorderStroke border)
		{
			var borderBrush = border.Stroke;
			CALayer? backgroundLayer = platformView.Layer as MauiCALayer;

			if (backgroundLayer == null && borderBrush.IsNullOrEmpty())
				return;

			platformView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeThickness(this UIView platformView, IBorderStroke border)
		{
			CALayer? backgroundLayer = platformView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			platformView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeDashPattern(this UIView platformView, IBorderStroke border)
		{
			CALayer? backgroundLayer = platformView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			platformView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeDashOffset(this UIView platformView, IBorderStroke border)
		{
			var strokeDashPattern = border.StrokeDashPattern;
			CALayer? backgroundLayer = platformView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
				return;

			platformView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeMiterLimit(this UIView platformView, IBorderStroke border)
		{
			CALayer? backgroundLayer = platformView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			platformView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeLineCap(this UIView platformView, IBorderStroke border)
		{
			CALayer? backgroundLayer = platformView.Layer as MauiCALayer;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			platformView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeLineJoin(this UIView platformView, IBorderStroke border)
		{
			CALayer? backgroundLayer = platformView.Layer as MauiCALayer;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			platformView.UpdateMauiCALayer(border);
		}

		internal static void UpdateMauiCALayer(this UIView platformView, IBorderStroke? border)
		{
			CALayer? backgroundLayer = platformView.Layer as MauiCALayer;

			var initialRender = false;
			if (backgroundLayer == null)
			{
				backgroundLayer = platformView.Layer?.Sublayers?
					.FirstOrDefault(x => x is MauiCALayer);

				if (backgroundLayer == null)
				{
					initialRender = true;
					backgroundLayer = new MauiCALayer
					{
						Name = ViewExtensions.BackgroundLayerName
					};

					platformView.BackgroundColor = UIColor.Clear;
					platformView.InsertBackgroundLayer(backgroundLayer, 0);
				}
			}

			// While we're in the process of connecting the handler properties will not change
			// So it's useless to update the layer many times with the same value
			if (platformView is ContentView { View: null } && !initialRender)
			{
				return;
			}

			if (backgroundLayer is MauiCALayer mauiCALayer)
			{
				backgroundLayer.Frame = platformView.Bounds;

				if (border is IView view)
					mauiCALayer.SetBackground(view.Background);
				else
					mauiCALayer.SetBackground(new SolidPaint(Colors.Transparent));

				mauiCALayer.SetBorderBrush(border?.Stroke);
				mauiCALayer.SetBorderWidth(border?.StrokeThickness ?? 0);
				mauiCALayer.SetBorderDash(border?.StrokeDashPattern, border?.StrokeDashOffset ?? 0);
				mauiCALayer.SetBorderMiterLimit(border?.StrokeMiterLimit ?? 0);

				if (border != null)
				{
					mauiCALayer.SetBorderLineJoin(border.StrokeLineJoin);
					mauiCALayer.SetBorderLineCap(border.StrokeLineCap);
				}

				mauiCALayer.SetBorderShape(border?.Shape);
			}

			if (platformView is ContentView contentView)
				contentView.Clip = border;
		}

		internal static void UpdateMauiCALayer(this UIView view)
		{
			if (view.Frame.IsEmpty)
			{
				return;
			}

			var layer = view.Layer;
			if (layer?.Sublayers is { Length: > 0 } sublayers)
			{
				var bounds = view.Bounds;
				var backgroundLayers = GetBackgroundLayersNeedingUpdate(sublayers, bounds);
				backgroundLayers.UpdateBackgroundLayers(bounds);
			}
		}

		static IEnumerable<CALayer> GetBackgroundLayersNeedingUpdate(this CALayer[] layers, CGRect bounds)
		{
			foreach (var layer in layers)
			{
				if (layer.Sublayers is { Length: > 0 } sublayers)
				{
					foreach (var sublayer in GetBackgroundLayersNeedingUpdate(sublayers, bounds))
					{
						yield return sublayer;
					}
				}

				if (layer.Name == ViewExtensions.BackgroundLayerName && layer.Frame != bounds)
				{
					yield return layer;
				}
			}
		}
		
		static void UpdateBackgroundLayers(this IEnumerable<CALayer> backgroundLayers, CGRect bounds)
		{
			using var backgroundLayerEnumerator = backgroundLayers.GetEnumerator();

			if (backgroundLayerEnumerator.MoveNext())
			{
				// iOS by default adds animations to certain actions such as layer resizing (setting the Frame property).
				// This can result in the background layer not keeping up with animations controlled by MAUI.
				// To prevent this undesired effect, native animations will be turned off for the duration of the operation.
				CATransaction.Begin();
				CATransaction.AnimationDuration = 0;
				
				do
				{
					var backgroundLayer = backgroundLayerEnumerator.Current;
					backgroundLayer.Frame = bounds;
				}
				while (backgroundLayerEnumerator.MoveNext());
				
				CATransaction.Commit();
			}
		}
	}
}