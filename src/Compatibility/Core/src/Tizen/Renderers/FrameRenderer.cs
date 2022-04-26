using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using Tizen.NUI;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class FrameRenderer : LayoutRenderer
	{
		static float s_borderWidth = 1.0f;
		static SKColor s_defaultColor = SKColors.Transparent;

		SKGLSurfaceView _backgroundCanvas;

		public FrameRenderer()
		{
			RegisterPropertyHandler(Frame.HasShadowProperty, UpdateShadowVisibility);
			RegisterPropertyHandler(Frame.CornerRadiusProperty, UpdateCornerRadius);
			RegisterPropertyHandler(Frame.BorderColorProperty, UpdateBorderColor);
		}

		new Frame Element => base.Element as Frame;

		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			base.OnElementChanged(e);
			NativeView.ClippingMode = ClippingModeType.ClipChildren;
			if (_backgroundCanvas == null)
			{
				Control.LayoutUpdated += OnLayoutUpdated;
				_backgroundCanvas = new SKGLSurfaceView();
				_backgroundCanvas.PaintSurface += OnPaint;
				Control.Add(_backgroundCanvas);
			}
		}

		void OnLayoutUpdated(object sender, global::Tizen.UIExtensions.Common.LayoutEventArgs e)
		{
			if (_backgroundCanvas != null)
			{
				var bound = Control.GetBounds();
				bound.X = 0;
				bound.Y = 0;
				_backgroundCanvas.UpdateBounds(bound);
			}
		}

		void OnPaint(object sender, SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var bound = e.Info.Rect;
			canvas.Clear();
			var bgColor = Element.BackgroundColor.IsDefault() ? s_defaultColor : SKColor.Parse(Element.BackgroundColor.ToArgbHex());
			var borderColor = Element.BorderColor.IsDefault() ? s_defaultColor : SKColor.Parse(Element.BorderColor.ToArgbHex());
			var roundRect = CreateRoundRect(bound);

			using (var paint = new SKPaint
			{
				IsAntialias = true,
			})
			{
				if (Element.HasShadow)
				{
					paint.Color = SKColors.White;
					paint.Style = SKPaintStyle.Stroke;
					// Draw shadow
					paint.ImageFilter = SKImageFilter.CreateDropShadowOnly(
						Forms.ConvertToScaledPixel(0),
						Forms.ConvertToScaledPixel(0),
						Forms.ConvertToScaledPixel(s_borderWidth * 2),
						Forms.ConvertToScaledPixel(s_borderWidth * 2),
						SKColors.Black);
					canvas.DrawRoundRect(roundRect, paint);
				}

				paint.ImageFilter = null;
				paint.Style = SKPaintStyle.Fill;
				paint.Color = bgColor;

				// Draw background color
				canvas.DrawRoundRect(roundRect, paint);

				paint.Style = SKPaintStyle.Stroke;
				paint.StrokeWidth = Forms.ConvertToScaledPixel(s_borderWidth);
				paint.Color = borderColor;

				// Draw Background (Brush)
				using (var brushPaint = Element.GetBackgroundPaint(bound))
				{
					if (brushPaint != null)
						canvas.DrawRoundRect(roundRect, brushPaint);
				}

				// Draw border
				canvas.DrawRoundRect(roundRect, paint);
			}
		}

		SKRoundRect CreateRoundRect(SKRect bounds)
		{
			var border = Forms.ConvertToScaledPixel(s_borderWidth) * (Element.HasShadow ? 4 : 0);
			var radius = Forms.ConvertToScaledPixel(Element.CornerRadius);
			var roundRect = new SKRoundRect(bounds, radius);
			roundRect.Deflate(border, border);
			return roundRect;
		}

		void UpdateShadowVisibility()
		{
			_backgroundCanvas?.Invalidate();
		}

		void UpdateCornerRadius(bool init)
		{
			_backgroundCanvas?.Invalidate();
		}

		void UpdateBorderColor()
		{
			_backgroundCanvas?.Invalidate();
		}
	}
}
