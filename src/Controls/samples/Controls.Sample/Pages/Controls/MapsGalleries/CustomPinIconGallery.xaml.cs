using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public partial class CustomPinIconGallery : ContentPage
	{
		public CustomPinIconGallery()
		{
			InitializeComponent();
			
			// Center on Seattle
			CustomPinMap.MoveToRegion(MapSpan.FromCenterAndRadius(
				new Location(47.6062, -122.3321),
				Distance.FromMiles(5)));
		}

		void OnAddCustomPinsClicked(object sender, EventArgs e)
		{
			// Add pin with custom icon from an image file in the app bundle
			var customPin1 = new Pin
			{
				Label = "Custom Icon Pin",
				Address = "Using embedded image",
				Location = new Location(47.6062, -122.3321),
				ImageSource = ImageSource.FromFile("dotnet_bot.png")
			};
			CustomPinMap.Pins.Add(customPin1);

			// Add another custom pin at different location
			var customPin2 = new Pin
			{
				Label = "Another Custom Pin",
				Address = "Also using custom image",
				Location = new Location(47.62, -122.35),
				ImageSource = ImageSource.FromFile("dotnet_bot.png")
			};
			CustomPinMap.Pins.Add(customPin2);
		}

		void OnAddDefaultPinClicked(object sender, EventArgs e)
		{
			// Add a regular pin without custom icon for comparison
			var defaultPin = new Pin
			{
				Label = "Default Pin",
				Address = "Standard marker icon",
				Location = new Location(47.59, -122.31),
				Type = PinType.Place
			};
			CustomPinMap.Pins.Add(defaultPin);
		}

		void OnClearClicked(object sender, EventArgs e)
		{
			CustomPinMap.Pins.Clear();
		}
	}
}
