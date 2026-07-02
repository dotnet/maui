using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public partial class CustomPinIconGallery : ContentPage
	{
		bool _altIcon;
		int _moveCount;

		public CustomPinIconGallery()
		{
			InitializeComponent();

			// Center on Seattle
			CustomPinMap.MoveToRegion(MapSpan.FromCenterAndRadius(
				new Microsoft.Maui.Devices.Sensors.Location(47.6062, -122.3321),
				Distance.FromMiles(5)));
		}

		void OnAddCustomPinsClicked(object? sender, EventArgs e)
		{
			// Add pin with custom icon from an image file in the app bundle
			var customPin1 = new Pin
			{
				Label = "Custom Icon Pin",
				Address = "Using app bundle image",
				Location = new Microsoft.Maui.Devices.Sensors.Location(47.6062, -122.3321),
				ImageSource = ImageSource.FromFile("dotnet_bot.png")
			};
			CustomPinMap.Pins.Add(customPin1);

			// Add another custom pin at different location
			var customPin2 = new Pin
			{
				Label = "Another Custom Pin",
				Address = "Also using custom image",
				Location = new Microsoft.Maui.Devices.Sensors.Location(47.62, -122.35),
				ImageSource = ImageSource.FromFile("dotnet_bot.png")
			};
			CustomPinMap.Pins.Add(customPin2);
		}

		void OnAddDefaultPinClicked(object? sender, EventArgs e)
		{
			// Add a regular pin without custom icon for comparison
			var defaultPin = new Pin
			{
				Label = "Default Pin",
				Address = "Standard marker icon",
				Location = new Microsoft.Maui.Devices.Sensors.Location(47.59, -122.31),
				Type = PinType.Place
			};
			CustomPinMap.Pins.Add(defaultPin);
		}

		// Swaps ImageSource on every existing pin at runtime, to test whether the marker icon
		// refreshes live. Toggles between two distinct bundled images so the change is obvious.
		void OnToggleIconClicked(object? sender, EventArgs e)
		{
			_altIcon = !_altIcon;
			var file = _altIcon ? "coffee.png" : "dotnet_bot.png";

			foreach (var pin in CustomPinMap.Pins)
			{
				pin.ImageSource = ImageSource.FromFile(file);
			}
		}

		// Control case: Location and Label have real per-platform mappers, so these should update
		// live on every platform (unlike ImageSource) — used to contrast the ImageSource behavior.
		void OnMoveRenameClicked(object? sender, EventArgs e)
		{
			_moveCount++;

			foreach (var pin in CustomPinMap.Pins)
			{
				pin.Location = new Microsoft.Maui.Devices.Sensors.Location(
					pin.Location.Latitude + 0.01,
					pin.Location.Longitude);
				pin.Label = $"Moved x{_moveCount}";
			}
		}

		void OnClearClicked(object? sender, EventArgs e)
		{
			CustomPinMap.Pins.Clear();
		}
	}
}
