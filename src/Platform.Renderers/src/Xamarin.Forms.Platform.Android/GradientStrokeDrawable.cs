using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using AColor = Android.Graphics.Color;

namespace Xamarin.Forms.Platform.Android
{
	public class GradientStrokeDrawable : PaintDrawable
	{
		readonly Paint _strokePaint;
		AColor _backgroundColor;

		public GradientStrokeDrawable()
		{
			_strokePaint = new Paint
			{
				Dither = true,
				AntiAlias = true
			};
			_strokePaint.SetStyle(Paint.Style.Stroke);
		}

		public void SetColor(AColor backgroundColor)
		{
			_backgroundColor = backgroundColor;
			InvalidateSelf();
		}

		public void SetStroke(int strokeWidth, AColor strokeColor)
		{
			_strokePaint.StrokeWidth = strokeWidth;
			_strokePaint.Color = strokeColor;
			InvalidateSelf();
		}

		public void SetGradient(Brush brush)
		{
			if (brush is LinearGradientBrush linearGradientBrush)
			{
				var p1 = linearGradientBrush.StartPoint;
				var x1 = (float)p1.X;
				var y1 = (float)p1.Y;

				var p2 = linearGradientBrush.EndPoint;
				var x2 = (float)p2.X;
				var y2 = (float)p2.Y;

				var gradientBrushData = linearGradientBrush.GetGradientBrushData();
				var colors = gradientBrushData.Item1;
				var offsets = gradientBrushData.Item2;

				var linearGradientShader = new LinearGradientShader(colors, offsets, x1, y1, x2, y2);
				SetShaderFactory(new GradientShaderFactory(linearGradientShader));
			}

			if (brush is RadialGradientBrush radialGradientBrush)
			{
				var center = radialGradientBrush.Center;
				float centerX = (float)center.X;
				float centerY = (float)center.Y;
				float radius = (float)radialGradientBrush.Radius;

				var gradientBrushData = radialGradientBrush.GetGradientBrushData();
				var colors = gradientBrushData.Item1;
				var offsets = gradientBrushData.Item2;

				var radialGradientShader = new RadialGradientShader(colors, offsets, centerX, centerY, radius);
				SetShaderFactory(new GradientShaderFactory(radialGradientShader));
			}
		}

		protected override void OnDraw(Shape shape, Canvas canvas, Paint paint)
		{
			base.OnDraw(shape, canvas, paint);

			if (_backgroundColor != null)
				paint.Color = _backgroundColor;

			shape.Draw(canvas, _strokePaint);
		}

		public abstract class GradientShader
		{
			public int[] Colors { get; set; }
			public float[] Offsets { get; set; }
		}

		public class LinearGradientShader : GradientShader
		{
			public LinearGradientShader()
			{

			}

			public LinearGradientShader(int[] colors, float[] offsets, float x1, float y1, float x2, float y2)
			{
				Colors = colors;
				Offsets = offsets;
				X1 = x1;
				Y1 = y1;
				X2 = x2;
				Y2 = y2;
			}

			public float X1 { get; set; }
			public float Y1 { get; set; }
			public float X2 { get; set; }
			public float Y2 { get; set; }
		}

		public class RadialGradientShader : GradientShader
		{
			public RadialGradientShader()
			{

			}

			public RadialGradientShader(int[] colors, float[] offsets, float centerX, float centerY, float radius)
			{
				Colors = colors;
				Offsets = offsets;
				CenterX = centerX;
				CenterY = centerY;
				Radius = radius;
			}

			public float CenterX { get; set; }
			public float CenterY { get; set; }
			public float Radius { get; set; }
		}

		internal class GradientShaderFactory : ShaderFactory
		{
			readonly GradientShader _gradientShader;

			public GradientShaderFactory(GradientShader gradientShader)
			{
				_gradientShader = gradientShader;
			}

			public override Shader Resize(int width, int height)
			{
				if (width == 0 && height == 0)
					return null;

				if (_gradientShader is LinearGradientShader linearGradientShader)
				{
					if (linearGradientShader.Colors.Length < 2)
						return null;

					return new LinearGradient(
						width * linearGradientShader.X1,
						height * linearGradientShader.Y1,
						width * linearGradientShader.X2,
						height * linearGradientShader.Y2,
						linearGradientShader.Colors,
						linearGradientShader.Offsets,
						Shader.TileMode.Clamp);
				}

				if (_gradientShader is RadialGradientShader radialGradientShader)
				{
					if (radialGradientShader.Colors.Length < 2)
						return null;

					return new RadialGradient(
						width * radialGradientShader.CenterX,
						height * radialGradientShader.CenterY,
						Math.Max(height, width) * radialGradientShader.Radius,
						radialGradientShader.Colors,
						radialGradientShader.Offsets,
						Shader.TileMode.Clamp);
				}
				return null;
			}
		}
	}
}