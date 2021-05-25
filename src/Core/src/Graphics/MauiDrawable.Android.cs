using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Graphics
{
	public class MauiDrawable : PaintDrawable
	{
		AColor? _backgroundColor;

		public MauiDrawable()
		{
			Shape = new RectShape();
		}

		public void SetColor(AColor? backgroundColor)
		{
			_backgroundColor = backgroundColor;
			SetShaderFactory(null);

			InvalidateSelf();
		}

		public void SetPaint(SolidPaint solidPaint)
		{
			var color = solidPaint.Color == null
				? (AColor?)null
				: solidPaint.Color.ToNative();

			SetColor(color);
		}

		public void SetPaint(LinearGradientPaint linearGradientPaint)
		{
			var p1 = linearGradientPaint.StartPoint;
			var x1 = (float)p1.X;
			var y1 = (float)p1.Y;

			var p2 = linearGradientPaint.EndPoint;
			var x2 = (float)p2.X;
			var y2 = (float)p2.Y;

			var data = GetGradientPaintData(linearGradientPaint);
			var shader = new LinearGradientData(data.Colors, data.Offsets, x1, y1, x2, y2);

			_backgroundColor = null;
			SetShaderFactory(new LinearGradientShaderFactory(shader));
		}

		public void SetPaint(RadialGradientPaint radialGradientPaint)
		{
			var center = radialGradientPaint.Center;
			float centerX = (float)center.X;
			float centerY = (float)center.Y;
			float radius = (float)radialGradientPaint.Radius;

			var data = GetGradientPaintData(radialGradientPaint);
			var shader = new RadialGradientData(data.Colors, data.Offsets, centerX, centerY, radius);

			_backgroundColor = null;
			SetShaderFactory(new RadialGradientShaderFactory(shader));
		}

		public void SetPaint(ImagePaint imagePaint)
		{
			throw new NotImplementedException();
		}

		public void SetPaint(PatternPaint patternPaint)
		{
			throw new NotImplementedException();
		}

		protected override void OnDraw(Shape? shape, Canvas? canvas, global::Android.Graphics.Paint? paint)
		{
			if (paint != null && _backgroundColor != null)
				paint.Color = _backgroundColor.Value;

			base.OnDraw(shape, canvas, paint);
		}

		static GradientData GetGradientPaintData(GradientPaint gradientPaint)
		{
			var orderStops = gradientPaint.GradientStops;

			var data = new GradientData(orderStops.Length);

			int count = 0;
			foreach (var orderStop in orderStops)
			{
				data.Colors[count] = orderStop.Color.ToNative().ToArgb();
				data.Offsets[count] = orderStop.Offset;
				count++;
			}

			return data;
		}
	}
}