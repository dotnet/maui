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

		public void SetBrush(ISolidColorBrush solidColorBrush)
		{
			var color = solidColorBrush.Color == null
				? (AColor?)null
				: solidColorBrush.Color.ToNative();

			SetColor(color);
		}

		public void SetBrush(ILinearGradientBrush linearGradientBrush)
		{
			var p1 = linearGradientBrush.StartPoint;
			var x1 = (float)p1.X;
			var y1 = (float)p1.Y;

			var p2 = linearGradientBrush.EndPoint;
			var x2 = (float)p2.X;
			var y2 = (float)p2.Y;

			var data = GetGradientBrushData(linearGradientBrush);
			var shader = new LinearGradientData(data.Colors, data.Offsets, x1, y1, x2, y2);

			_backgroundColor = null;
			SetShaderFactory(new LinearGradientShaderFactory(shader));
		}

		public void SetBrush(IRadialGradientBrush radialGradientBrush)
		{
			var center = radialGradientBrush.Center;
			float centerX = (float)center.X;
			float centerY = (float)center.Y;
			float radius = (float)radialGradientBrush.Radius;

			var data = GetGradientBrushData(radialGradientBrush);
			var shader = new RadialGradientData(data.Colors, data.Offsets, centerX, centerY, radius);

			_backgroundColor = null;
			SetShaderFactory(new RadialGradientShaderFactory(shader));
		}

		protected override void OnDraw(Shape? shape, Canvas? canvas, global::Android.Graphics.Paint? paint)
		{
			if (paint != null && _backgroundColor != null)
				paint.Color = _backgroundColor.Value;

			base.OnDraw(shape, canvas, paint);
		}

		static GradientData GetGradientBrushData(IGradientBrush gradientBrush)
		{
			var orderStops = gradientBrush.GradientStops;

			var data = new GradientData(orderStops.Count);

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