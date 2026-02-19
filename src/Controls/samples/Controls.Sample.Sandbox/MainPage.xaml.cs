using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	bool _trafficEnabled;

	public MainPage()
	{
		InitializeComponent();

		Loaded += (s, e) =>
		{
			var london = new Location(51.5074, -0.1278);
			map.MoveToRegion(MapSpan.FromCenterAndRadius(london, Distance.FromKilometers(10)));
			statusLabel.Text = "London | Street";
		};
	}

	void OnLondonClicked(object? sender, EventArgs e)
	{
		map.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(51.5074, -0.1278), Distance.FromKilometers(10)));
		statusLabel.Text = $"London | {map.MapType} | Pins: {map.Pins.Count}";
	}

	void OnNewYorkClicked(object? sender, EventArgs e)
	{
		map.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(40.7128, -74.0060), Distance.FromKilometers(15)));
		statusLabel.Text = $"New York | {map.MapType} | Pins: {map.Pins.Count}";
	}

	void OnTokyoClicked(object? sender, EventArgs e)
	{
		map.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(35.6762, 139.6503), Distance.FromKilometers(20)));
		statusLabel.Text = $"Tokyo | {map.MapType} | Pins: {map.Pins.Count}";
	}

	int _pinCounter;

	void OnAddPinClicked(object? sender, EventArgs e)
	{
		// Place each pin at a slightly different offset so they're visually distinguishable
		var center = map.VisibleRegion?.Center ?? new Location(51.5074, -0.1278);
		double offset = _pinCounter * 0.01;
		var pinLocation = new Location(center.Latitude + offset, center.Longitude + offset);
		_pinCounter++;

		var pin = new Pin
		{
			Label = $"Pin {map.Pins.Count + 1}",
			Location = pinLocation
		};
		map.Pins.Add(pin);
		statusLabel.Text = $"Pin added (total: {map.Pins.Count})";
	}

	void OnClearPinsClicked(object? sender, EventArgs e)
	{
		var countBefore = map.Pins.Count;
		map.Pins.Clear();
		_pinCounter = 0;
		statusLabel.Text = $"Pins cleared ({countBefore} removed)";
	}

	void OnStreetClicked(object? sender, EventArgs e)
	{
		map.MapType = MapType.Street;
		statusLabel.Text = "MapType: Street";
	}

	void OnSatelliteClicked(object? sender, EventArgs e)
	{
		map.MapType = MapType.Satellite;
		statusLabel.Text = "MapType: Satellite";
	}

	void OnHybridClicked(object? sender, EventArgs e)
	{
		map.MapType = MapType.Hybrid;
		statusLabel.Text = "MapType: Hybrid";
	}

	void OnTrafficClicked(object? sender, EventArgs e)
	{
		_trafficEnabled = !_trafficEnabled;
		map.IsTrafficEnabled = _trafficEnabled;
		statusLabel.Text = $"Traffic: {(_trafficEnabled ? "ON" : "OFF")}";
	}

	void OnToggleScrollClicked(object? sender, EventArgs e)
	{
		map.IsScrollEnabled = !map.IsScrollEnabled;
		statusLabel.Text = $"Scroll: {(map.IsScrollEnabled ? "ON" : "OFF")} | Zoom: {(map.IsZoomEnabled ? "ON" : "OFF")}";
	}

	void OnToggleZoomClicked(object? sender, EventArgs e)
	{
		map.IsZoomEnabled = !map.IsZoomEnabled;
		statusLabel.Text = $"Scroll: {(map.IsScrollEnabled ? "ON" : "OFF")} | Zoom: {(map.IsZoomEnabled ? "ON" : "OFF")}";
	}
}