using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class Shadow : Element, IShadow
	{
		public static readonly BindableProperty RadiusProperty = BindableProperty.Create(nameof(Radius), typeof(double), typeof(Shadow), 10d);

		public static readonly BindableProperty OpacityProperty = BindableProperty.Create(nameof(Opacity), typeof(double), typeof(Shadow), 1d);

		public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Brush), typeof(Shadow), null);

		public static readonly BindableProperty OffsetProperty = BindableProperty.Create(nameof(Offset), typeof(Size), typeof(Shadow), null);

		Paint IShadow.Paint => Color;

		public double Radius
		{
			get { return (double)GetValue(RadiusProperty); }
			set
			{
				SetValue(RadiusProperty, value);
			}
		}

		public double Opacity
		{
			get { return (double)GetValue(OpacityProperty); }
			set
			{
				SetValue(OpacityProperty, value);
			}
		}

		public Brush Color
		{
			get { return (Brush)GetValue(ColorProperty); }
			set 
			{ 
				SetValue(ColorProperty, value); 
			}
		}

		public Size Offset
		{
			get { return (Size)GetValue(OffsetProperty); }
			set { SetValue(OffsetProperty, value); }
		}
	}
}
