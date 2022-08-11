using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Circle : MapElement
	{
		public static readonly BindableProperty CenterProperty = BindableProperty.Create(
			nameof(Center),
			typeof(Location),
			typeof(Circle),
			default(Location));

		public static readonly BindableProperty RadiusProperty = BindableProperty.Create(
			nameof(Radius),
			typeof(Distance),
			typeof(Circle),
			default(Distance));

		public static readonly BindableProperty FillColorProperty = BindableProperty.Create(
			nameof(FillColor),
			typeof(Color),
			typeof(Circle),
			null);

		public Location Center
		{
			get => (Location)GetValue(CenterProperty);
			set => SetValue(CenterProperty, value);
		}

		public Distance Radius
		{
			get => (Distance)GetValue(RadiusProperty);
			set => SetValue(RadiusProperty, value);
		}

		public Color FillColor
		{
			get => (Color)GetValue(FillColorProperty);
			set => SetValue(FillColorProperty, value);
		}
	}
}
