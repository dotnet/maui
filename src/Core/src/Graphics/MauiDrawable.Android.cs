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
			_backgroundColor = null;
			SetShaderFactory(PaintExtensions.GetLinearGradientShaderFactory(linearGradientPaint));
		}

		public void SetPaint(RadialGradientPaint radialGradientPaint)
		{
			_backgroundColor = null;
			SetShaderFactory(PaintExtensions.GetRadialGradientShaderFactory(radialGradientPaint));
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
	}
}