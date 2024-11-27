#nullable disable
using System;
using System.ComponentModel;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using APaint = Android.Graphics.Paint;
using GPaint = Microsoft.Maui.Graphics.Paint;

namespace Microsoft.Maui.Controls.Platform
{
	public class GradientStrokeDrawable : PaintDrawable
	{
		readonly APaint _strokePaint;
		AColor? _backgroundColor;

		public GradientStrokeDrawable()
		{
			_strokePaint = new APaint
			{
				Dither = true,
				AntiAlias = true
			};
			_strokePaint.SetStyle(APaint.Style.Stroke);
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
			var paint = (GPaint)brush;
			if (paint is LinearGradientPaint linearGradientBrush)
			{
				var factory = linearGradientBrush.GetGradientShaderFactory(null);
				SetShaderFactory(factory);
			}
			else if (paint is RadialGradientPaint radialGradientBrush)
			{
				var factory = radialGradientBrush.GetGradientShaderFactory(null);
				SetShaderFactory(factory);
			}
		}

		internal void SetBrush(Brush brush)
		{
			if (brush is SolidColorBrush solidColorBrush)
			{
				var color = solidColorBrush.Color ?? Colors.Transparent;
				SetColor(color.ToPlatform());
			}
			else if (brush is GradientBrush gradientBrush)
			{
				SetGradient(gradientBrush);
			}
		}

		protected override void OnDraw(Shape shape, Canvas canvas, APaint paint)
		{
			base.OnDraw(shape, canvas, paint);

			if (_backgroundColor is not null)
				paint.Color = _backgroundColor.Value;

			shape.Draw(canvas, _strokePaint);
		}

		/// <summary>
		/// Represents a gradient.
		/// This type is not meant to be used anywhere and is for internal use only. This type will be removed in the future.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use Microsoft.Maui.Graphics.GradientData instead.")]
		public abstract class GradientShader
		{
			public int[] Colors { get; set; }
			public float[] Offsets { get; set; }
		}

		/// <summary>
		/// Represents a linear gradient.
		/// This type is not meant to be used anywhere and is for internal use only. This type will be removed in the future.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use Microsoft.Maui.Graphics.LinearGradientData instead.")]
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

		/// <summary>
		/// Represents a radial gradient.
		/// This type is not meant to be used anywhere and is for internal use only. This type will be removed in the future.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use Microsoft.Maui.Graphics.RadialGradientData instead.")]
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
	}
}