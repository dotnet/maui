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

		public void SetBrush(ISolidColorBrush solidColorBrush)
		{
			var color = solidColorBrush.Color.IsDefault
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

		protected override void OnDraw(Shape? shape, Canvas? canvas, Paint? paint)
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

		class LinearGradientShaderFactory : ShaderFactory
		{
			readonly LinearGradientData _data;

			public LinearGradientShaderFactory(LinearGradientData data)
			{
				_data = data;
			}

			public override Shader? Resize(int width, int height)
			{
				if (width == 0 && height == 0)
					return null;

				if (_data.Colors == null || _data.Colors.Length < 2)
					return null;

				return new LinearGradient(
					width * _data.X1,
					height * _data.Y1,
					width * _data.X2,
					height * _data.Y2,
					_data.Colors,
					_data.Offsets,
					Shader.TileMode.Clamp!);
			}
		}

		class RadialGradientShaderFactory : ShaderFactory
		{
			readonly RadialGradientData _data;

			public RadialGradientShaderFactory(RadialGradientData data)
			{
				_data = data;
			}

			public override Shader? Resize(int width, int height)
			{
				if (width == 0 && height == 0)
					return null;

				if (_data.Colors == null || _data.Colors.Length < 2)
					return null;

				return new RadialGradient(
					width * _data.CenterX,
					height * _data.CenterY,
					Math.Max(height, width) * _data.Radius,
					_data.Colors,
					_data.Offsets,
					Shader.TileMode.Clamp!);
			}
		}

		class GradientData
		{
			public GradientData(int count)
			{
				Colors = new int[count];
				Offsets = new float[count];
			}

			public GradientData(int[] colors, float[] offsets)
			{
				Colors = colors;
				Offsets = offsets;
			}

			public int[] Colors { get; set; }
			public float[] Offsets { get; set; }
		}

		class LinearGradientData : GradientData
		{
			public LinearGradientData(int[] colors, float[] offsets, float x1, float y1, float x2, float y2)
				: base(colors, offsets)
			{
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

		class RadialGradientData : GradientData
		{
			public RadialGradientData(int[] colors, float[] offsets, float centerX, float centerY, float radius)
				: base(colors, offsets)
			{
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