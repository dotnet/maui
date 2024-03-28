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

		public override Shader? Resize(int width, int height) =>
			_data.ToShader(width, height);
	}

	class RadialGradientShaderFactory : ShaderFactory
	{
		readonly RadialGradientData _data;

		public RadialGradientShaderFactory(RadialGradientData data)
		{
			_data = data;
		}

		public override Shader? Resize(int width, int height) =>
			_data.ToShader(width, height);
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

		public GradientData(GradientData data)
		{
			Colors = data.Colors;
			Offsets = data.Offsets;
		}

		public int[] Colors { get; set; }
		public float[] Offsets { get; set; }

		public virtual Shader? ToShader(int width, int height) => null;
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

		public LinearGradientData(GradientData data, float x1, float y1, float x2, float y2)
			: base(data)
		{
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
		}

		public LinearGradientData(LinearGradientData data)
			: base(data)
		{
			X1 = data.X1;
			Y1 = data.Y1;
			X2 = data.X2;
			Y2 = data.Y2;
		}

		public float X1 { get; set; }
		public float Y1 { get; set; }
		public float X2 { get; set; }
		public float Y2 { get; set; }

		public override Shader? ToShader(int width, int height)
		{
			if (width == 0 && height == 0)
				return null;

			if (Colors == null || Colors.Length < 2)
				return null;

			return new LinearGradient(
				width * X1,
				height * Y1,
				width * X2,
				height * Y2,
				Colors,
				Offsets,
				Shader.TileMode.Clamp!);
		}
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

		public RadialGradientData(GradientData data, float centerX, float centerY, float radius)
			: base(data)
		{
			CenterX = centerX;
			CenterY = centerY;
			Radius = radius;
		}

		public RadialGradientData(RadialGradientData data)
			: base(data)
		{
			CenterX = data.CenterX;
			CenterY = data.CenterY;
			Radius = data.Radius;
		}

		public float CenterX { get; set; }
		public float CenterY { get; set; }
		public float Radius { get; set; }

		public override Shader? ToShader(int width, int height)
		{
			if (width == 0 && height == 0)
				return null;

			if (Colors == null || Colors.Length < 2)
				return null;

			return new RadialGradient(
				width * CenterX,
				height * CenterY,
				Math.Max(height, width) * Radius,
				Colors,
				Offsets,
				Shader.TileMode.Clamp!);
		}
	}
}
