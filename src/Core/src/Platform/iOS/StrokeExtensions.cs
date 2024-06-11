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

			if (backgroundLayer == null)
			{
				backgroundLayer = platformView.Layer?.Sublayers?
					.FirstOrDefault(x => x is MauiCALayer);

				if (backgroundLayer == null)
				{
					backgroundLayer = new MauiCALayer
					{
						Name = ViewExtensions.BackgroundLayerName
					};

					platformView.BackgroundColor = UIColor.Clear;
					platformView.InsertBackgroundLayer(backgroundLayer, 0);
				}
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

					if (sublayer.Name == ViewExtensions.BackgroundLayerName && sublayer.Frame != bounds)
						sublayer.Frame = bounds;
				}
			}
		}
	}
}