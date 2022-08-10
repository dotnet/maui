using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample
{

	public class MapElementsPage : ContentPage
	{

		public MapElementsPage()
		{
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			var myhouse = new Location(47.6368678, -122.137305);

			//var center = new 
			var map = new Microsoft.Maui.Controls.Maps.Map(new MapSpan(myhouse, 0.1, 0.1));
			grid.Children.Add(map);

			// instantiate a polygon
#pragma warning disable CS0618 // Type or member is obsolete
			Polygon polygon = new Polygon
			{
				StrokeWidth = 8,
				StrokeColor = Color.FromHex("#1BA1E2"),
				FillColor = Color.FromHex("#881BA1E2"),
				Geopath =
						{
							new Location(47.6368678, -122.137305),
							new Location(47.6368894, -122.134655),
							new Location(47.6359424, -122.134655),
							new Location(47.6359496, -122.1325521),
							new Location(47.6424124, -122.1325199),
							new Location(47.642463,  -122.1338932),
							new Location(47.6406414, -122.1344833),
							new Location(47.6384943, -122.1361248),
							new Location(47.6372943, -122.1376912)
						}
			};


			// add the polygon to the map's MapElements collection
			map.MapElements.Add(polygon);
			Polyline polyline = new Polyline
			{
				StrokeColor = Colors.Pink,
				StrokeWidth = 12,
				Geopath =
							{
									new Location(47.6381401, -122.1317367),
									new Location(47.6381473, -122.1350841),
									new Location(47.6382847, -122.1353094),
									new Location(47.6384582, -122.1354703),
									new Location(47.6401136, -122.1360819),
									new Location(47.6403883, -122.1364681),
									new Location(47.6407426, -122.1377019),
									new Location(47.6412558, -122.1404056),
									new Location(47.6414148, -122.1418647),
									new Location(47.6414654, -122.1432702)
							}
			};

			// add the polyline to the map's MapElements collection
			map.MapElements.Add(polyline);



			Circle circle = new Circle
			{
				Center = new Location(37.79752, -122.40183),
				Radius = new Distance(250),
				StrokeColor = Color.FromHex("#88FF0000"),
				StrokeWidth = 8,
				FillColor = Color.FromHex("#88FFC0CB")
			};

#pragma warning restore CS0618 // Type or member is obsolete

			// Add the Circle to the map's MapElements collection
			map.MapElements.Add(circle);

			Content = grid;
		}
	}
	public class MapPage : ContentPage
	{
		public MapPage()
		{
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition());

			var myhouse = new Location(41.0116556, -8.642892);

			var map = new Microsoft.Maui.Controls.Maps.Map(new MapSpan(myhouse, 0.1, 0.1));
			map.Pins.Add(new Pin
			{
				Position = myhouse,
				Label = "Casa"
			});
			map.Pins.Add(new Pin
			{
				Position = new Location(40.9930868, -8.6376671),
				Label = "Bairro"
			});
			map.Pins.Add(new Pin
			{
				Position = new Location(40.9846363, -8.6412593),
				Label = "Golfe"
			});
			map.Pins.Add(new Pin
			{
				Position = new Location(41.0000939, -8.6228158),
				Label = "Nave"
			});
			map.MapClicked += async (s, e) => { await DisplayAlert("Map Clicked", $"Clicked on Position:{e.Position}", "ok"); };

			Grid.SetColumnSpan(map, 2);

			var lbl = new Label { Text = "MapType" };
			var picker = new Picker
			{
				VerticalOptions = LayoutOptions.Start,
				ItemsSource = new string[] { nameof(MapType.Street), nameof(MapType.Satellite), nameof(MapType.Hybrid) }
			};
			picker.SelectedIndexChanged += (s, e) =>
			{
				var item = picker.SelectedItem.ToString();
				switch (item)
				{
					case nameof(MapType.Street):
						map.MapType = MapType.Street;
						break;
					case nameof(MapType.Satellite):
						map.MapType = MapType.Satellite;
						break;
					case nameof(MapType.Hybrid):
						map.MapType = MapType.Hybrid;
						break;
					default:
						break;
				}
			};
			picker.SelectedIndex = 0;

			Grid.SetRow(lbl, 1);
			Grid.SetRow(picker, 1);
			Grid.SetColumn(picker, 1);

			AddBoolMapOption(grid, nameof(map.IsShowingUser), 2, map.IsShowingUser, (bool b) => map.IsShowingUser = b);
			AddBoolMapOption(grid, nameof(map.HasScrollEnabled), 3, map.HasScrollEnabled, (bool b) => map.HasScrollEnabled = b);
			AddBoolMapOption(grid, nameof(map.HasTrafficEnabled), 4, map.HasTrafficEnabled, (bool b) => map.HasTrafficEnabled = b);
			AddBoolMapOption(grid, nameof(map.HasZoomEnabled), 5, map.HasZoomEnabled, (bool b) => map.HasZoomEnabled = b);

			grid.Children.Add(map);
			grid.Children.Add(lbl);
			grid.Children.Add(picker);

			var buttonGoTo = new Button
			{
				Text = "Go To Redmond",
				Command = new Command(() => map.MoveToRegion(new MapSpan(new Location(47.6434194, -122.1298166), 0.2, 0.2)))
			};

			Grid.SetRow(buttonGoTo, 6);
			grid.Children.Add(buttonGoTo);

			var hStack = new HorizontalStackLayout();

			var centerPin = new Pin
			{
				Position = map.VisibleRegion?.Center,
				Address = "THe map center",
				Label = "Center",
				Type = PinType.Place
			};
			var buttonAddPin = new Button
			{
				Text = "Add",
				Command = new Command(() =>
				{
					centerPin.Position = map.VisibleRegion?.Center;
					map.Pins.Add(centerPin);
				})
			};

			var buttonRemovePin = new Button
			{
				Text = "Remove",
				Command = new Command(() => map.Pins.Remove(centerPin))
			};

			var buttonClearPin = new Button
			{
				Text = "Clear",
				Command = new Command(() => map.Pins.Clear())
			};

			hStack.Children.Add(buttonAddPin);
			hStack.Children.Add(buttonRemovePin);
			hStack.Children.Add(buttonClearPin);

			Grid.SetRow(hStack, 6);
			Grid.SetColumn(hStack, 1);
			grid.Children.Add(hStack);

			var lblVisibleRegion = new Label();
			lblVisibleRegion.SetBinding(Label.TextProperty, new Binding(nameof(map.VisibleRegion), source: map));
			Grid.SetRow(lblVisibleRegion, 7);
			Grid.SetColumnSpan(lblVisibleRegion, 2);


			grid.Children.Add(lblVisibleRegion);
			Content = grid;
		}

		static void AddBoolMapOption(Grid grid, string name, int row, bool isToogled, Action<bool> toogled)
		{
			var lbl = new Label { Text = name };
			var swt = new Switch
			{
				IsToggled = isToogled,
				VerticalOptions = LayoutOptions.Start
			};

			swt.Toggled += (s, e) =>
			{
				toogled((s as Switch).IsToggled);
			};

			Grid.SetRow(lbl, row);
			Grid.SetRow(swt, row);
			Grid.SetColumn(swt, 1);

			grid.Children.Add(lbl);
			grid.Children.Add(swt);
		}
	}
}
