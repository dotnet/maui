using SkiaSharp;
using SkiaSharp.Views.Tizen;

namespace Xamarin.Forms.Platform.Tizen.SkiaSharp
{
	public class FrameRenderer : LayoutRenderer
	{
		static float s_borderWidth = 1.0f;
		static SKColor s_defaultColor = SKColors.Transparent;

		SKCanvasView _cliper;
		new Frame Element => base.Element as Frame;

		public FrameRenderer()
		{
			RegisterPropertyHandler(Frame.CornerRadiusProperty, UpdateCornerRadius);
			RegisterPropertyHandler(Frame.BorderColorProperty, UpdateBorderColor);
			RegisterPropertyHandler(Frame.HasShadowProperty, UpdateHasShadow);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			base.OnElementChanged(e);
			_cliper = new SKCanvasView(Forms.NativeParent);
			_cliper.Show();
			_cliper.PassEvents = true;
			_cliper.PaintSurface += OnCliperPaint;
			Control.Children.Add(_cliper);
			BackgroundCanvas?.StackAbove(_cliper);
		}

		protected override void UpdateBackgroundColor(bool initialize)
		{
			if (initialize && Element.BackgroundColor.IsDefault)
				return;
			else
				BackgroundCanvas.Invalidate();
		}

		protected override void OnBackgroundLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			base.OnBackgroundLayoutUpdated(sender, e);
			if (_cliper != null)
			{
				_cliper.Geometry = Control.Geometry;
				if (Element.Content != null)
				{
					var nativeView = Platform.GetOrCreateRenderer(Element.Content)?.NativeView;
					nativeView?.SetClip(null);
					nativeView?.SetClip(_cliper);
				}
			}
		}

		protected override void OnBackgroundPaint(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var bound = e.Info.Rect;
			canvas.Clear();
			var bgColor = Element.BackgroundColor == Color.Default ? SKColors.White : SKColor.Parse(Element.BackgroundColor.ToHex());
			var borderColor = Element.BorderColor == Color.Default ? s_defaultColor : SKColor.Parse(Element.BorderColor.ToHex());
			var roundRect = CreateRoundRect(bound);

			using (var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = bgColor,
			})
			{
				if (Element.HasShadow)
				{
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

		void OnCliperPaint(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var bound = e.Info.Rect;
			canvas.Clear();

			var bgColor = Element.BackgroundColor == Color.Default ? SKColors.White : SKColor.Parse(Element.BackgroundColor.ToHex());
			var roundRect = CreateRoundRect(bound);

			using (var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = bgColor,
			})
			{
				canvas.DrawRoundRect(roundRect, paint);
			}
		}

		void UpdateCornerRadius()
		{
			BackgroundCanvas.Invalidate();
			_cliper?.Invalidate();
		}

		void UpdateBorderColor()
		{
			BackgroundCanvas.Invalidate();
		}

		void UpdateHasShadow()
		{
			BackgroundCanvas.Invalidate();
			_cliper?.Invalidate();
		}

		SKRoundRect CreateRoundRect(SKRect bounds)
		{
			var border = Forms.ConvertToScaledPixel(s_borderWidth) * (Element.HasShadow ? 4 : 2);
			var radius = Forms.ConvertToScaledPixel(Element.CornerRadius);
			var roundRect = new SKRoundRect(bounds, radius);
			roundRect.Deflate(border, border);
			return roundRect;
		}
	}
}
