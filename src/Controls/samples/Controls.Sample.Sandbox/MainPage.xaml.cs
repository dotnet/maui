using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		Loaded += (s, e) =>
		{
			var london = new Location(51.5074, -0.1278);
			map.MoveToRegion(MapSpan.FromCenterAndRadius(london, Distance.FromKilometers(10)));
			statusLabel.Text = "London";
		};
	}

	void OnLondonClicked(object? sender, EventArgs e)
	{
		map.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(51.5074, -0.1278), Distance.FromKilometers(10)));
		statusLabel.Text = $"London | Pins: {map.Pins.Count}";
	}

	void OnNewYorkClicked(object? sender, EventArgs e)
	{
		map.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(40.7128, -74.0060), Distance.FromKilometers(15)));
		statusLabel.Text = $"New York | Pins: {map.Pins.Count}";
	}

	void OnTokyoClicked(object? sender, EventArgs e)
	{
		map.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(35.6762, 139.6503), Distance.FromKilometers(20)));
		statusLabel.Text = $"Tokyo | Pins: {map.Pins.Count}";
	}

	void OnAddPinClicked(object? sender, EventArgs e)
	{
		var center = map.VisibleRegion?.Center ?? new Location(51.5074, -0.1278);
		var pin = new Pin
		{
			Label = $"Pin {map.Pins.Count + 1}",
			Location = center
		};
		map.Pins.Add(pin);
		statusLabel.Text = $"Pin added (total: {map.Pins.Count})";
	}

	void OnClearPinsClicked(object? sender, EventArgs e)
	{
		var countBefore = map.Pins.Count;
		map.Pins.Clear();
		statusLabel.Text = $"Pins cleared ({countBefore} removed)";
	}
}