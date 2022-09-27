using System;
using System.Collections.ObjectModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Dispatching;
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

			var myhouse = new Location(47.6368678, -122.137305);

			//var center = new 
			var map = new Map(new MapSpan(myhouse, 0.1, 0.1));
			grid.Children.Add(map);

			// add the polygon to the map's MapElements collection
			map.MapElements.Add(AddPolygon());

			// add the polyline to the map's MapElements collection
			var polyline = AddPolyline();
			map.MapElements.Add(polyline);
			//UpdatePolyLine(Dispatcher,polyline);

			// add the Circle to the map's MapElements collection
			map.MapElements.Add(AddCircle());

			Content = grid;
		}

		static Polygon AddPolygon()
		{
			return new Polygon
			{
				StrokeWidth = 8,
				StrokeColor = Colors.Blue,
				FillColor = Colors.Blue.WithAlpha(0.6f),
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
		}

		static Polyline AddPolyline()
		{
			return new Polyline
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
		}

		static Circle AddCircle()
		{
			return new Circle
			{
				Center = new Location(47.6381401, -122.1317367),
				Radius = new Distance(250),
				StrokeColor = Colors.Red,
				StrokeWidth = 8,
				FillColor = Colors.Red.WithAlpha(0.6f)
			};
		}

		static void UpdatePolyLine(IDispatcher dispatcher, Polyline polyline)
		{
			int count = 0;
			dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				polyline.StrokeWidth = count;
				if (count == 0)
				{
					(polyline.Geopath as ObservableCollection<Location>).Add(new Location(47.6381401, -122.1317367));
					polyline.StrokeColor = Colors.Blue;
				}
				if (count == 1)
				{
					(polyline.Geopath as ObservableCollection<Location>).Add(new Location(47.6381473, -122.1350841));
					polyline.StrokeColor = Colors.Yellow;
				}
				if (count == 2)
				{
					(polyline.Geopath as ObservableCollection<Location>).Add(new Location(47.6382847, -122.1353094));
					polyline.StrokeColor = Colors.Green;
				}
				if (count == 3)
				{
					(polyline.Geopath as ObservableCollection<Location>).Add(new Location(47.6384582, -122.1354703));
					polyline.StrokeColor = Colors.GreenYellow;
				}
				if (count == 4)
				{
					(polyline.Geopath as ObservableCollection<Location>).Add(new Location(47.6401136, -122.1360819));
					polyline.StrokeColor = Colors.Black;
				}
				if (count == 5)
				{
					(polyline.Geopath as ObservableCollection<Location>).Add(new Location(47.6403883, -122.1364681));
					polyline.StrokeColor = Colors.Brown;
				}
				if (count == 6)
				{
					(polyline.Geopath as ObservableCollection<Location>).Add(new Location(47.6407426, -122.1377019));
					polyline.StrokeColor = Colors.Red;
				}
				if (count == 7)
				{
					(polyline.Geopath as ObservableCollection<Location>).Add(new Location(47.6412558, -122.1404056));
					polyline.StrokeColor = Colors.Silver;
				}
				if (count == 8)
				{
					(polyline.Geopath as ObservableCollection<Location>).Add(new Location(47.6414148, -122.1418647));
					polyline.StrokeColor = Colors.SteelBlue;
				}
				if (count == 9)
				{
					(polyline.Geopath as ObservableCollection<Location>).Add(new Location(47.6414654, -122.1432702));
					polyline.StrokeColor = Colors.Turquoise;
				}
				count++;

				return (count < 10);
			});
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
				Location = myhouse,
				Label = "Casa"
			});
			map.Pins.Add(new Pin
			{
				Location = new Location(40.9930868, -8.6376671),
				Label = "Bairro"
			});
			map.Pins.Add(new Pin
			{
				Location = new Location(40.9846363, -8.6412593),
				Label = "Golfe"
			});
			map.Pins.Add(new Pin
			{
				Location = new Location(41.0000939, -8.6228158),
				Label = "Nave"
			});
			map.MapClicked += async (s, e) => { await DisplayAlert("Map Clicked", $"Clicked on Position:{e.Location}", "ok"); };

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
			AddBoolMapOption(grid, nameof(map.IsScrollEnabled), 3, map.IsScrollEnabled, (bool b) => map.IsScrollEnabled = b);
			AddBoolMapOption(grid, nameof(map.IsTrafficEnabled), 4, map.IsTrafficEnabled, (bool b) => map.IsTrafficEnabled = b);
			AddBoolMapOption(grid, nameof(map.IsZoomEnabled), 5, map.IsZoomEnabled, (bool b) => map.IsZoomEnabled = b);

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
				Location = map.VisibleRegion?.Center,
				Address = "THe map center",
				Label = "Center",
				Type = PinType.Place
			};
			var buttonAddPin = new Button
			{
				Text = "Add",
				Command = new Command(() =>
				{
					centerPin.Location = map.VisibleRegion?.Center;
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
