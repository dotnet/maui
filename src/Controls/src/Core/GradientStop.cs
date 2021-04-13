using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class GradientStop : Element
	{
		public static readonly BindableProperty ColorProperty = BindableProperty.Create(
			nameof(Color), typeof(Color), typeof(GradientStop), null);

		public Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		public static readonly BindableProperty OffsetProperty = BindableProperty.Create(
			nameof(Offset), typeof(float), typeof(GradientStop), 0f);

		public float Offset
		{
			get => (float)GetValue(OffsetProperty);
			set => SetValue(OffsetProperty, value);
		}

		public GradientStop() { }

		public GradientStop(Color color, float offset)
		{
			Color = color;
			Offset = offset;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is GradientStop dest))
				return false;

			return Color == dest.Color && global::System.Math.Abs(Offset - dest.Offset) < 0.00001;
		}

		public override int GetHashCode()
		{
			return -1234567890 + Color.GetHashCode();
		}
	}
}