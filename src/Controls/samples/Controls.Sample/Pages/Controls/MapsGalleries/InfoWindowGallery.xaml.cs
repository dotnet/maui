using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample.Pages.Controls.MapsGalleries;

public partial class InfoWindowGallery : ContentPage
{
	Pin _seattlePin;
	Pin _nycPin;

	public InfoWindowGallery()
	{
		InitializeComponent();

		_seattlePin = new Pin
		{
			Label = "Seattle",
			Address = "Washington, USA",
			Location = new Location(47.6062, -122.3321)
		};

		_nycPin = new Pin
		{
			Label = "New York City",
			Address = "New York, USA",
			Location = new Location(40.7128, -74.0060)
		};

		map.Pins.Add(_seattlePin);
		map.Pins.Add(_nycPin);
		map.Pins.Add(new Pin
		{
			Label = "Los Angeles",
			Address = "California, USA",
			Location = new Location(34.0522, -118.2437)
		});

		map.MoveToRegion(MapSpan.FromCenterAndRadius(
			new Location(39.8, -98.5), Distance.FromMiles(1500)));
	}

	void OnShowSeattle(object? sender, EventArgs e)
	{
		_seattlePin.ShowInfoWindow();
	}

	void OnShowNYC(object? sender, EventArgs e)
	{
		_nycPin.ShowInfoWindow();
	}

	void OnHideAll(object? sender, EventArgs e)
	{
		_seattlePin.HideInfoWindow();
		_nycPin.HideInfoWindow();
	}
}
