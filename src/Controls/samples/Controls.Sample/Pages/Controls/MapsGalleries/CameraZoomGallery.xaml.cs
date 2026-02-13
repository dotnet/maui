using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CameraZoomGallery
	{
		readonly Location[] _cities =
		{
			new(47.6062, -122.3321),  // Seattle
			new(37.7749, -122.4194),  // San Francisco
			new(34.0522, -118.2437),  // Los Angeles
			new(40.7128, -74.0060),   // New York
			new(41.8781, -87.6298),   // Chicago
		};

		public CameraZoomGallery()
		{
			InitializeComponent();

			foreach (var city in _cities)
			{
				map.Pins.Add(new Pin
				{
					Label = $"City ({city.Latitude:F2}, {city.Longitude:F2})",
					Location = city,
					Type = PinType.Place,
				});
			}
		}

		void OnFitAllPins(object? sender, EventArgs e)
		{
			var span = MapSpan.FromLocations(_cities);
			map.MoveToRegion(span, true);
			InfoLabel.Text = $"Best fit: center={span.Center.Latitude:F2},{span.Center.Longitude:F2} span={span.LatitudeDegrees:F2}x{span.LongitudeDegrees:F2}";
		}

		void OnMoveAnimated(object? sender, EventArgs e)
		{
			// Move to Seattle with animation
			var seattle = new MapSpan(new Location(47.6062, -122.3321), 0.1, 0.1);
			map.MoveToRegion(seattle, true);
			InfoLabel.Text = "Animated move to Seattle";
		}

		void OnMoveInstant(object? sender, EventArgs e)
		{
			// Move to New York instantly
			var newYork = new MapSpan(new Location(40.7128, -74.0060), 0.1, 0.1);
			map.MoveToRegion(newYork, false);
			InfoLabel.Text = "Instant move to New York";
		}
	}
}
