using System.Linq;
using CoreAnimation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this UIView nativeView, IBorder border)
		{
			var borderShape = border.Shape;
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			if (backgroundLayer == null && borderShape == null)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStroke(this UIView nativeView, IBorder border)
		{
			var borderBrush = border.Stroke;
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			if (backgroundLayer == null && borderBrush.IsNullOrEmpty())
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeThickness(this UIView nativeView, IBorder border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeDashPattern(this UIView nativeView, IBorder border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeDashOffset(this UIView nativeView, IBorder border)
		{
			var strokeDashPattern = border.StrokeDashPattern;
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeMiterLimit(this UIView nativeView, IBorder border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeLineCap(this UIView nativeView, IBorder border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		public static void UpdateStrokeLineJoin(this UIView nativeView, IBorder border)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(border);
		}

		internal static void UpdateMauiCALayer(this UIView nativeView, IBorder border)
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
				mauiCALayer.SetBackground(border.Background);
				mauiCALayer.SetBorderBrush(border.Stroke);
				mauiCALayer.SetBorderWidth(border.StrokeThickness);
				mauiCALayer.SetBorderDash(border.StrokeDashPattern, border.StrokeDashOffset);
				mauiCALayer.SetBorderMiterLimit(border.StrokeMiterLimit);
				mauiCALayer.SetBorderLineJoin(border.StrokeLineJoin);
				mauiCALayer.SetBorderLineCap(border.StrokeLineCap);
				mauiCALayer.SetBorderShape(border.Shape);
			}
		}
	}
}