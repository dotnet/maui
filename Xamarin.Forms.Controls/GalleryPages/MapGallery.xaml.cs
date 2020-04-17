using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapGallery : ContentPage
	{
		readonly Geocoder _geocoder = new Geocoder();
		readonly Map Map;

		public MapGallery()
		{
			InitializeComponent();

			Map = MakeMap();
			Map.Pins.ForEach(pin =>
			{
				pin.MarkerClicked += MarkerClicked;
				pin.InfoWindowClicked += InfoWindowClicked;
			});
			Map.MapClicked += MapClicked;

			((Grid)Content).Children.Add(Map, 0, 1);

			_btnToggleMoveToLastRegionOnLayoutChange.Text = Map.MoveToLastRegionOnLayoutChangeProperty.DefaultValue.ToString();
		}

		public static Map MakeMap()
		{
			return new Map(MapSpan.FromCenterAndRadius(new Position(41.890202, 12.492049), Distance.FromMiles(0.5)))
			{
				IsShowingUser = false,
				Pins =
				{
					new Pin
					{
						Type = PinType.Place,
						Position = new Position (41.890202, 12.492049),
						Label = "Colosseum",
						Address = "Piazza del Colosseo, 00184 Rome, Province of Rome, Italy"
					},
					new Pin
					{
						Type = PinType.Place,
						Position = new Position (41.898652, 12.476831),
						Label = "Pantheon",
						Address = "Piazza della Rotunda, 00186 Rome, Province of Rome, Italy"
					},
					new Pin
					{
						Type = PinType.Place,
						Position = new Position (41.903209, 12.454545),
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

		async void SearchForAddress(object sender, EventArgs e)
		{
			var searchAddress = (SearchBar)sender;
			var addressQuery = searchAddress.Text;
			searchAddress.Text = "";
			searchAddress.Unfocus();

			var positions = (await _geocoder.GetPositionsForAddressAsync(addressQuery)).ToList();
			if (!positions.Any())
				return;

			var position = positions.First();
			Map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMeters(4000)));
			Map.Pins.Add(new Pin
			{
				Label = addressQuery,
				Position = position,
				Address = addressQuery
			});
		}

		void MapClicked(object sender, MapClickedEventArgs e)
		{
			LastMapClickLabel.Text = $"Last MapClick: {e.Position.Latitude}, {e.Position.Longitude}";
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

		async void ReverseGeocodeClicked(object sender, EventArgs e)
		{
			var addresses = await _geocoder.GetAddressesForPositionAsync(new Position(41.8902, 12.4923));
			foreach (var ad in addresses)
				Debug.WriteLine(ad);
		}

		void HomeClicked(object sender, EventArgs e)
		{
			Map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(41.890202, 12.492049), Distance.FromMiles(0.5)));
		}

		void ZoomPinClicked(object sender, EventArgs e)
		{
			var pos = new Position(41.011995, -8.642995);
			Map.Pins.Clear();
			Map.Pins.Add(new Pin { Position = pos, Label = "Rui" });
			Map.MoveToRegion(MapSpan.FromCenterAndRadius(pos, Distance.FromMiles(0.5)));
		}

		void EditPinClicked(object sender, EventArgs e)
		{
			var pin = Map.Pins.First();

			pin.Label += " Edited";
			pin.Address = "Edited";

			var pos = new Position(pin.Position.Latitude + 1, pin.Position.Longitude + 1);
			pin.Position = pos;
			Map.MoveToRegion(MapSpan.FromCenterAndRadius(pos, Distance.FromMiles(0.5)));
		}

		void RemovePinClicked(object sender, EventArgs e)
		{
			Map.Pins.RemoveAt(0);
		}

		void ToggleMoveToLastRegionOnLayoutChange(object sender, EventArgs e)
		{
			Map.MoveToLastRegionOnLayoutChange = !Map.MoveToLastRegionOnLayoutChange;
			((Button)sender).Text = Map.MoveToLastRegionOnLayoutChange.ToString();
		}
	}
}