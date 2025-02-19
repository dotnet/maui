using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.ControlGallery
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapGallery : ContentPage
	{
		readonly Map Map;

		public MapGallery()
		{
			InitializeComponent();

			Map = MakeMap();
			Map.Pins.ToList().ForEach(pin =>
			{
				pin.MarkerClicked += MarkerClicked;
				pin.InfoWindowClicked += InfoWindowClicked;
			});
			Map.MapClicked += MapClicked;

			((Grid)Content).Children.Add(Map, 0, 1);

		}

		public static Map MakeMap()
		{
			return new Map(MapSpan.FromCenterAndRadius(new Devices.Sensors.Location(41.890202, 12.492049), Distance.FromMiles(0.5)))
			{
				IsShowingUser = false,
				Pins =
				{
					new Pin
					{
						Type = PinType.Place,
						Location = new Location(41.890202, 12.492049),
						Label = "Colosseum",
						Address = "Piazza del Colosseo, 00184 Rome, Province of Rome, Italy"
					},
					new Pin
					{
						Type = PinType.Place,
						Location = new Location (41.898652, 12.476831),
						Label = "Pantheon",
						Address = "Piazza della Rotunda, 00186 Rome, Province of Rome, Italy"
					},
					new Pin
					{
						Type = PinType.Place,
						Location = new Location (41.903209, 12.454545),
						Label = "Sistine Chapel",
						Address = "Piazza della Rotunda, 00186 Rome, Province of Rome, Italy"
					}
				}
			};
		}


		void GetMapRegionClicked(object sender, EventArgs e)
		{
			if (Map.VisibleRegion == null)
				DisplayAlert(":(", "VisibleRegion is null, move the map to get it!", "OK");
			else
				DisplayAlert(":)", $"Lat: {Map.VisibleRegion.Center.Latitude}, Long: {Map.VisibleRegion.Center.Longitude}, move the map to get it!", "OK");
		}

		void MarkerClicked(object sender, PinClickedEventArgs e)
		{
			LastMarkerClickLabel.Text = $"Last Marker Clicked: {((Pin)sender).Label}";
		}

		void InfoWindowClicked(object sender, PinClickedEventArgs e)
		{
			LastInfoWindowClickLabel.Text = $"Last Info Window Clicked: {((Pin)sender).Label}";
			e.HideInfoWindow = true;
		}

		void MapClicked(object sender, MapClickedEventArgs e)
		{
			LastMapClickLabel.Text = $"Last MapClick: {e.Location.Latitude}, {e.Location.Longitude}";
		}

		async void MapTypeClicked(object sender, EventArgs e)
		{
			var result = await DisplayActionSheet("Select Map Type", null, null, "Street", "Satellite", "Hybrid");
			switch (result)
			{
				case "Street":
					Map.MapType = MapType.Street;
					break;
				case "Satellite":
					Map.MapType = MapType.Satellite;
					break;
				case "Hybrid":
					Map.MapType = MapType.Hybrid;
					break;
			}
		}

		void ZoomInClicked(object sender, EventArgs e)
		{
			Map.MoveToRegion(Map.VisibleRegion.WithZoom(5f));
		}

		void ZoomOutClicked(object sender, EventArgs e)
		{
			Map.MoveToRegion(Map.VisibleRegion.WithZoom(1.0 / 3));
		}

		void HomeClicked(object sender, EventArgs e)
		{
			Map.MoveToRegion(MapSpan.FromCenterAndRadius(new Devices.Sensors.Location(41.890202, 12.492049), Distance.FromMiles(0.5)));
		}

		void ZoomPinClicked(object sender, EventArgs e)
		{
			var pos = new Devices.Sensors.Location(41.011995, -8.642995);
			Map.Pins.Clear();
			Map.Pins.Add(new Pin { Location = pos, Label = "Rui" });
			Map.MoveToRegion(MapSpan.FromCenterAndRadius(pos, Distance.FromMiles(0.5)));
		}

		void EditPinClicked(object sender, EventArgs e)
		{
			var pin = (Pin)Map.Pins.First();

			pin.Label += " Edited";
			pin.Address = "Edited";

			var pos = new Devices.Sensors.Location(pin.Location.Latitude + 1, pin.Location.Longitude + 1);
			pin.Location = pos;
			Map.MoveToRegion(MapSpan.FromCenterAndRadius(pos, Distance.FromMiles(0.5)));
		}

		void RemovePinClicked(object sender, EventArgs e)
		{
			Map.Pins.RemoveAt(0);
		}

		void ShowTrafficToggled(object sender, ToggledEventArgs e)
		{
			var control = (Switch)sender;
			Map.IsTrafficEnabled = control.IsToggled;
		}
	}
}