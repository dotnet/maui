#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class Shadow : Element, IShadow
	{
		public static readonly BindableProperty RadiusProperty = BindableProperty.Create(nameof(Radius), typeof(float), typeof(Shadow), 10f);

		public static readonly BindableProperty OpacityProperty = BindableProperty.Create(nameof(Opacity), typeof(float), typeof(Shadow), 1f);

		public static readonly BindableProperty BrushProperty = BindableProperty.Create(nameof(Brush), typeof(Brush), typeof(Shadow), Brush.Black);

		public static readonly BindableProperty OffsetProperty = BindableProperty.Create(nameof(Offset), typeof(Point), typeof(Shadow), null);

		Paint IShadow.Paint => Brush;

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

		public Brush Brush
		{
			get { return (Brush)GetValue(BrushProperty); }
			set { SetValue(BrushProperty, value); }
		}

		public Point Offset
		{
			get { return (Point)GetValue(OffsetProperty); }
			set { SetValue(OffsetProperty, value); }
		}
	}
}
