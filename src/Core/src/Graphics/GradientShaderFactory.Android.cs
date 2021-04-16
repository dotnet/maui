using System;
using Android.Graphics;
using static Android.Graphics.Drawables.ShapeDrawable;

namespace Microsoft.Maui.Graphics
{
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
