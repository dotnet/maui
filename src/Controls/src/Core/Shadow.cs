using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class Shadow : Element, IShadow
	{
		public static readonly BindableProperty RadiusProperty = BindableProperty.Create(nameof(Radius), typeof(float), typeof(Shadow), 10f);

		public static readonly BindableProperty OpacityProperty = BindableProperty.Create(nameof(Opacity), typeof(float), typeof(Shadow), 1f);

		public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Brush), typeof(Shadow), null);

		public static readonly BindableProperty OffsetProperty = BindableProperty.Create(nameof(Offset), typeof(Point), typeof(Shadow), null);

		Paint IShadow.Paint => Color;

		public float Radius
		{
			get { return (float)GetValue(RadiusProperty); }
			set { SetValue(RadiusProperty, value); }
		}

		public float Opacity
		{
			get { return (float)GetValue(OpacityProperty); }
			set { SetValue(OpacityProperty, value); }
		}

		public Brush Color
		{
			get { return (Brush)GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}

		public Point Offset
		{
			get { return (Point)GetValue(OffsetProperty); }
			set { SetValue(OffsetProperty, value); }
		}
	}
}
