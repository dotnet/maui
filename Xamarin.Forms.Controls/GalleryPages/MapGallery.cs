using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.Controls
{
	public class MapGallery : ContentPage
	{
		readonly StackLayout _stack;

		public MapGallery ()
		{
			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var map = MakeMap ();

			map.MoveToRegion (MapSpan.FromCenterAndRadius (new Position (41.890202, 12.492049), Distance.FromMiles (0.5)));

			var searchAddress = new SearchBar { Placeholder = "Search Address" };

			searchAddress.SearchButtonPressed += async (e, a) => {
				var addressQuery = searchAddress.Text;
				searchAddress.Text = "";
				searchAddress.Unfocus ();

				var positions = (await (new Geocoder ()).GetPositionsForAddressAsync (addressQuery)).ToList ();
				if (!positions.Any ())
					return;

				var position = positions.First ();
				map.MoveToRegion (MapSpan.FromCenterAndRadius (position,
					Distance.FromMeters (4000)));
				map.Pins.Add (new Pin {
					Label = addressQuery,
					Position = position,
					Address = addressQuery
				});
			};

			var buttonAddressFromPosition = new Button { Text = "Address From Position" };
			buttonAddressFromPosition.Clicked += async (e, a) => {
				var addresses = (await (new Geocoder ()).GetAddressesForPositionAsync (new Position (41.8902, 12.4923))).ToList ();
				foreach (var ad in addresses)
					Debug.WriteLine (ad);
			};

			var buttonZoomIn = new Button { Text = "Zoom In" };
			buttonZoomIn.Clicked += (e, a) => map.MoveToRegion (map.VisibleRegion.WithZoom (5f));

			var buttonZoomOut = new Button { Text = "Zoom Out" };
			buttonZoomOut.Clicked += (e, a) => map.MoveToRegion (map.VisibleRegion.WithZoom (1 / 3f));

			var mapTypeButton = new Button { Text = "Map Type" };
			mapTypeButton.Clicked += async (e, a) => {
				var result = await DisplayActionSheet ("Select Map Type", null, null, "Street", "Satellite", "Hybrid");
				switch (result) {
				case "Street":
					map.MapType = MapType.Street;
					break;
				case "Satellite":
					map.MapType = MapType.Satellite;
					break;
				case "Hybrid":
					map.MapType = MapType.Hybrid;
					break;
				}
			};

			var buttonHome = new Button { Text = "Home" };
			buttonHome.Clicked += (a, e) => {
				map.MoveToRegion (MapSpan.FromCenterAndRadius (new Position (41.890202, 12.492049), Distance.FromMiles (0.5)));
			};

			var buttonRemove = new Button { Text = "Remove Pin" };
			buttonRemove.Clicked += (a, e) =>
			{
				map.Pins.RemoveAt(0);
			};

			_stack = new StackLayout {
				Spacing = 0,
				Padding = new Thickness (30, 0)
			};
			//stack.SetRowSpacing (1, 0);

			Title = "Map Gallery";

			var buttonZoomPin = new Button { Text = "Zoom Pin" };
			buttonZoomPin.Clicked += (a, e) => {
				var pos = new Position (41.011995, -8.642995);
				map.Pins.Clear ();
				map.Pins.Add (new Pin { Position = pos, Label = "Rui" });
				map.MoveToRegion (MapSpan.FromCenterAndRadius (pos, Distance.FromMiles (0.5)));
			};

			var buttonEditPin = new Button { Text = "Edit Pin" };
			buttonEditPin.Clicked += (a, e) =>
			{
				var pin = map.Pins.First();

				pin.Label += " Edited";
				pin.Address = "Edited";

				var pos = new Position(pin.Position.Latitude + 1, pin.Position.Longitude + 1);
				pin.Position = pos;
				map.MoveToRegion(MapSpan.FromCenterAndRadius(pos, Distance.FromMiles(0.5)));
			};

			map.VerticalOptions = LayoutOptions.FillAndExpand;
			_stack.Children.Add (searchAddress);
			_stack.Children.Add (map);
			_stack.Children.Add (mapTypeButton);
			_stack.Children.Add (buttonZoomIn);
			_stack.Children.Add (buttonZoomOut);
			_stack.Children.Add (buttonAddressFromPosition);
			_stack.Children.Add (buttonHome);
			_stack.Children.Add (buttonZoomPin);
			_stack.Children.Add (buttonEditPin);
			_stack.Children.Add (buttonRemove);

			Content = _stack;
		}

		public static Map MakeMap ()
		{
			Pin colosseum = null;
			Pin pantheon = null;
			Pin chapel = null;

			var map = new Map {
				IsShowingUser = false,
				Pins = {
					(colosseum = new Pin {
						Type = PinType.Place,
						Position = new Position (41.890202, 12.492049),
						Label = "Colosseum",
						Address = "Piazza del Colosseo, 00184 Rome, Province of Rome, Italy"
					}),
					(pantheon = new Pin {
						Type = PinType.Place,
						Position = new Position (41.898652, 12.476831),
						Label = "Pantheon",
						Address = "Piazza della Rotunda, 00186 Rome, Province of Rome, Italy"
					}),
					(chapel = new Pin {
						Type = PinType.Place,
						Position = new Position (41.903209, 12.454545),
						Label = "Sistine Chapel",
						Address = "Piazza della Rotunda, 00186 Rome, Province of Rome, Italy"
					})
				}
			};

			colosseum.Clicked += PinClicked;
			pantheon.Clicked += PinClicked;
			chapel.Clicked += PinClicked;
			return map;
		}

		static void PinClicked (object sender, EventArgs e)
		{
			Pin pin = (Pin)sender;
			Application.Current.MainPage.DisplayAlert("Pin Click",
				$"You clicked the {pin.Label} pin, located at {pin.Address}, or coordinates ({pin.Position.Latitude}, {pin.Position.Longitude})",
				"OK");
		}
	}
}
