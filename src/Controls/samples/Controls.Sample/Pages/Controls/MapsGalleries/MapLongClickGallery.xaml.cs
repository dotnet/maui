using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;
using Position = Microsoft.Maui.Devices.Sensors.Location;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public partial class MapLongClickGallery : ContentPage
	{
		int _pinCount = 0;

		public MapLongClickGallery()
		{
			InitializeComponent();

			// Center map on San Francisco
			map.MoveToRegion(MapSpan.FromCenterAndRadius(
				new Position(37.7749, -122.4194),
				Distance.FromMiles(5)));
		}

		void OnMapLongClicked(object sender, MapClickedEventArgs e)
		{
			_pinCount++;

			// Update status label
			StatusLabel.Text = $"Long press #{_pinCount} at ({e.Location.Latitude:F4}, {e.Location.Longitude:F4})";
			StatusLabel.TextColor = Colors.Green;

			// Add a pin at the long-pressed location
			var pin = new Pin
			{
				Label = $"Pin #{_pinCount}",
				Address = $"Added via long press",
				Location = e.Location,
				Type = PinType.Generic
			};

			map.Pins.Add(pin);
		}
	}
}
