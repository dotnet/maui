using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	[TypeConverter(typeof(BrushTypeConverter))]
	public class SolidColorBrush : Brush
	{
		public SolidColorBrush()
		{

		}

		public SolidColorBrush(Color color)
		{
			Color = color;
		}

		public override bool IsEmpty
		{
			get
			{
				var solidColorBrush = this;
				return solidColorBrush == null || solidColorBrush.Color == null;
			}
		}

		public static readonly BindableProperty ColorProperty = BindableProperty.Create(
			nameof(Color), typeof(Color), typeof(SolidColorBrush), null);

		public Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is SolidColorBrush dest))
				return false;

			return Color == dest.Color;
		}

		public override int GetHashCode()
		{
			return -1234567890 + Color.GetHashCode();
		}
	}
}