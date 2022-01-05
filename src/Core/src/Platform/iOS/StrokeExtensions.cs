using System.Linq;
using CoreAnimation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this UIView nativeView, IBorderStroke border)
		{
			var borderShape = border.Shape;
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			if (backgroundLayer == null && borderShape == null)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStroke(this UIView nativeView, IBorderStroke border)
		{
			var borderBrush = border.Stroke;
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			if (backgroundLayer == null && borderBrush.IsNullOrEmpty())
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeThickness(this UIView nativeView, IBorderStroke border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeDashPattern(this UIView nativeView, IBorderStroke border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeDashOffset(this UIView nativeView, IBorderStroke border)
		{
			var strokeDashPattern = border.StrokeDashPattern;
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeMiterLimit(this UIView nativeView, IBorderStroke border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeLineCap(this UIView nativeView, IBorderStroke border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeLineJoin(this UIView nativeView, IBorderStroke border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		internal static void UpdateMauiCALayer(this UIView nativeView, IBorderStroke? border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			if (backgroundLayer == null)
			{
				backgroundLayer = nativeView.Layer?.Sublayers?
					.FirstOrDefault(x => x is MauiCALayer);

				if (backgroundLayer == null)
				{
					backgroundLayer = new MauiCALayer
					{
						Name = ViewExtensions.BackgroundLayerName
					};

					nativeView.BackgroundColor = UIColor.Clear;
					nativeView.InsertBackgroundLayer(backgroundLayer, 0);
				}
			}

			if (backgroundLayer is MauiCALayer mauiCALayer)
			{
				backgroundLayer.Frame = nativeView.Bounds;
				if (border is IView v)
					mauiCALayer.SetBackground(v.Background);
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
		}
	}
}