using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample
{
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
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition());

			var map = new Microsoft.Maui.Controls.Maps.Map(new MapSpan(new Location(41.0116556, -8.642892), 0.2, 0.2));
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

			var lblVisibleRegion = new Label();

			lblVisibleRegion.SetBinding(Label.TextProperty, new Binding(nameof(map.VisibleRegion), source: map));
			Grid.SetRow(buttonGoTo, 7);
			Grid.SetRow(lblVisibleRegion, 7);
			Grid.SetColumn(lblVisibleRegion, 1);
			
			grid.Children.Add(buttonGoTo);
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
