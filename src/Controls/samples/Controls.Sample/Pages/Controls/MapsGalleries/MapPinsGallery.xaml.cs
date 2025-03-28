using System;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Xaml;
using Position = Microsoft.Maui.Devices.Sensors.Location;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPinsGallery
	{
		readonly Random _locationRandomSeed = new();
		int _locationIncrement = 0;

		// TODO generate actual random pins
		private readonly Position[] _randomLocations =
		{
			new Position(51.8833333333333, 176.65),
			new Position(21.3166666666667, 157.833333333333),
			new Position(71.3, 156.766666666667),
			new Position(19.7, 155.083333333333),
			new Position(61.2166666666667, 149.9),
			new Position(70.2, 148.516666666667),
			new Position(64.85, 147.716666666667),
			new Position(57.05, 135.333333333333),
			new Position(60.7166666666667, 135.05),
			new Position(58.3, 134.416666666667),
			new Position(69.45, 133.033333333333),
			new Position(48.4333333333333, 123.366666666667),
			new Position(49.25, 123.1),
			new Position(45.5166666666667, 122.683333333333),
			new Position(37.7833333333333, 122.416666666667),
			new Position(47.6166666666667, 122.333333333333),
			new Position(38.55, 121.466666666667),
			new Position(50.6833333333333, 120.333333333333),
			new Position(39.5333333333333, 119.816666666667),
			new Position(34.4333333333333, 119.716666666667),
			new Position(49.8833333333333, 119.5),
			new Position(55.1666666666667, 118.8),
			new Position(34.05, 118.25),
			new Position(33.95, 117.4),
			new Position(32.7166666666667, 117.166666666667),
			new Position(32.5333333333333, 117.033333333333),
			new Position(31.85, 116.6),
			new Position(43.6166666666667, 116.2),
			new Position(32.6666666666667, 115.466666666667),
			new Position(36.1833333333333, 115.133333333333),
			new Position(62.45, 114.4),
			new Position(51.05, 114.066666666667),
			new Position(53.5333333333333, 113.5),
			new Position(33.45, 112.066666666667),
			new Position(46.6, 112.033333333333),
		};

		public MapPinsGallery()
		{
			InitializeComponent();

			var microsoftPin = new Pin()
			{
				Address = "One Microsoft Way, Redmond, USA",
				Label = "Microsoft Visitors Center",
				Location = new Position(47.64232, -122.13684),
			};

			microsoftPin.MarkerClicked += (sender, args) =>
			{
				DisplayAlertAsync("Marker", $"Marker Clicked: {((Pin)sender!).Label}", "OK");
			};

			// TODO this doesn't seem to work on iOS?
			microsoftPin.InfoWindowClicked += (sender, args) =>
			{
				DisplayAlertAsync("Info", $"Info Window Clicked: {((Pin)sender!).Label}", "OK");
			};

			pinsMap.Pins.Add(microsoftPin);
		}

		void OnAddPinClicked(object sender, EventArgs e)
		{
			AddPin();
		}

		void OnRemovePinClicked(object sender, EventArgs e)
		{
			if (pinsMap.Pins.Count > 0)
			{
				pinsMap.Pins.RemoveAt(pinsMap.Pins.Count - 1);
				_locationIncrement--;
			}
		}

		void OnAdd10PinsClicked(object sender, EventArgs e)
		{
			for (int i = 0; i <= 10; i++)
			{
				AddPin();
			}
		}

		void AddPin()
		{
			pinsMap.Pins.Add(new Pin()
			{
				Label = $"Location {_locationIncrement++}",
				Location = _randomLocations[_locationRandomSeed.Next(0, _randomLocations.Length)],
			});
		}

		void OnMapClicked(object sender, MapClickedEventArgs e)
		{
			DisplayAlertAsync("Map", $"Map {e.Location.Latitude}, {e.Location.Longitude} clicked.", "Ok");
		}
	}
}