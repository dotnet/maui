using System;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices.Sensors;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPinsGallery
	{
		private readonly Random _locationRandomSeed = new();
		private int _locationIncrement = 0;

		// TODO generate actual random pins
		private readonly Location[] _randomLocations =
		{
			new Location(51.8833333333333, 176.65),
			new Location(21.3166666666667, 157.833333333333),
			new Location(71.3, 156.766666666667),
			new Location(19.7, 155.083333333333),
			new Location(61.2166666666667, 149.9),
			new Location(70.2, 148.516666666667),
			new Location(64.85, 147.716666666667),
			new Location(57.05, 135.333333333333),
			new Location(60.7166666666667, 135.05),
			new Location(58.3, 134.416666666667),
			new Location(69.45, 133.033333333333),
			new Location(48.4333333333333, 123.366666666667),
			new Location(49.25, 123.1),
			new Location(45.5166666666667, 122.683333333333),
			new Location(37.7833333333333, 122.416666666667),
			new Location(47.6166666666667, 122.333333333333),
			new Location(38.55, 121.466666666667),
			new Location(50.6833333333333, 120.333333333333),
			new Location(39.5333333333333, 119.816666666667),
			new Location(34.4333333333333, 119.716666666667),
			new Location(49.8833333333333, 119.5),
			new Location(55.1666666666667, 118.8),
			new Location(34.05, 118.25),
			new Location(33.95, 117.4),
			new Location(32.7166666666667, 117.166666666667),
			new Location(32.5333333333333, 117.033333333333),
			new Location(31.85, 116.6),
			new Location(43.6166666666667, 116.2),
			new Location(32.6666666666667, 115.466666666667),
			new Location(36.1833333333333, 115.133333333333),
			new Location(62.45, 114.4),
			new Location(51.05, 114.066666666667),
			new Location(53.5333333333333, 113.5),
			new Location(33.45, 112.066666666667),
			new Location(46.6, 112.033333333333),
		};

		public MapPinsGallery()
		{
			InitializeComponent();

			var microsoftPin = new Pin()
			{
				Address = "One Microsoft Way, Redmond, USA",
				Label = "Microsoft Visitors Center",
				Location = new Location(47.64232, -122.13684),
			};

			microsoftPin.MarkerClicked += (s, a) =>
			{
				DisplayAlert("Marker", "OK", "OK");
			};

			// TODO this doesn't seem to work on iOS?
			microsoftPin.InfoWindowClicked += (s, a) =>
			{
				DisplayAlert("Info", "OK", "OK");
			};

			pinsMap.Pins.Add(microsoftPin);
		}

		private void AddPin_Clicked(object sender, EventArgs e)
		{
			AddPin();
		}

		private void RemovePin_Clicked(object sender, EventArgs e)
		{
			if (pinsMap.Pins.Count > 0)
			{
				pinsMap.Pins.RemoveAt(pinsMap.Pins.Count - 1);
				_locationIncrement--;
			}
		}

		private void Add10Pins_Clicked(object sender, EventArgs e)
		{
			for (int i = 0; i <= 10; i++)
			{
				AddPin();
			}
		}

		private void AddPin()
		{
			pinsMap.Pins.Add(new Pin()
			{
				Label = $"Location {_locationIncrement++}",
				Location = _randomLocations[_locationRandomSeed.Next(0, _randomLocations.Length)],
			});
		}
	}
}