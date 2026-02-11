using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public class MapElementVisibilityGallery : ContentPage
	{
		readonly Polygon _polygon;
		readonly Polyline _polyline;
		readonly Circle _circle;
		readonly Label _statusLabel;

		public MapElementVisibilityGallery()
		{
			Title = "Element Visibility & ZIndex";

			var center = new Location(47.6062, -122.3321); // Seattle

			_polygon = new Polygon
			{
				StrokeColor = Colors.Blue,
				StrokeWidth = 3,
				FillColor = Color.FromRgba(0, 0, 255, 64),
				ZIndex = 1,
			};
			_polygon.Geopath.Add(new Location(47.615, -122.345));
			_polygon.Geopath.Add(new Location(47.615, -122.320));
			_polygon.Geopath.Add(new Location(47.600, -122.320));
			_polygon.Geopath.Add(new Location(47.600, -122.345));

			_polyline = new Polyline
			{
				StrokeColor = Colors.Red,
				StrokeWidth = 5,
				ZIndex = 2,
			};
			_polyline.Geopath.Add(new Location(47.610, -122.350));
			_polyline.Geopath.Add(new Location(47.610, -122.315));

			_circle = new Circle
			{
				Center = center,
				Radius = new Distance(500),
				StrokeColor = Colors.Green,
				StrokeWidth = 3,
				FillColor = Color.FromRgba(0, 255, 0, 64),
				ZIndex = 3,
			};

			var map = new Microsoft.Maui.Controls.Maps.Map(new MapSpan(center, 0.03, 0.03));
			map.MapElements.Add(_polygon);
			map.MapElements.Add(_polyline);
			map.MapElements.Add(_circle);

			_statusLabel = new Label
			{
				Text = "All elements visible. ZIndex: Polygon=1, Polyline=2, Circle=3",
				HorizontalTextAlignment = TextAlignment.Center,
				AutomationId = "StatusLabel"
			};

			var togglePolygonBtn = new Button { Text = "Toggle Polygon", AutomationId = "TogglePolygon" };
			togglePolygonBtn.Clicked += (s, e) =>
			{
				_polygon.IsVisible = !_polygon.IsVisible;
				UpdateStatus();
			};

			var togglePolylineBtn = new Button { Text = "Toggle Polyline", AutomationId = "TogglePolyline" };
			togglePolylineBtn.Clicked += (s, e) =>
			{
				_polyline.IsVisible = !_polyline.IsVisible;
				UpdateStatus();
			};

			var toggleCircleBtn = new Button { Text = "Toggle Circle", AutomationId = "ToggleCircle" };
			toggleCircleBtn.Clicked += (s, e) =>
			{
				_circle.IsVisible = !_circle.IsVisible;
				UpdateStatus();
			};

			var bringPolygonTopBtn = new Button { Text = "Polygon to Top (Z=10)", AutomationId = "PolygonTop" };
			bringPolygonTopBtn.Clicked += (s, e) =>
			{
				_polygon.ZIndex = 10;
				UpdateStatus();
			};

			var resetZIndexBtn = new Button { Text = "Reset ZIndex", AutomationId = "ResetZIndex" };
			resetZIndexBtn.Clicked += (s, e) =>
			{
				_polygon.ZIndex = 1;
				_polyline.ZIndex = 2;
				_circle.ZIndex = 3;
				UpdateStatus();
			};

			Content = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition(GridLength.Star),
					new RowDefinition(GridLength.Auto),
				},
				Children =
				{
					map,
					new VerticalStackLayout
					{
						Spacing = 4,
						Padding = new Thickness(8),
						Children =
						{
							_statusLabel,
							new HorizontalStackLayout
							{
								Spacing = 4,
								HorizontalOptions = LayoutOptions.Center,
								Children = { togglePolygonBtn, togglePolylineBtn, toggleCircleBtn }
							},
							new HorizontalStackLayout
							{
								Spacing = 4,
								HorizontalOptions = LayoutOptions.Center,
								Children = { bringPolygonTopBtn, resetZIndexBtn }
							}
						}
					}.Assign(out var controls)
				}
			};

			Grid.SetRow(controls, 1);
		}

		void UpdateStatus()
		{
			_statusLabel.Text = $"Polygon:{(_polygon.IsVisible ? "ON" : "OFF")}(Z={_polygon.ZIndex}) " +
				$"Polyline:{(_polyline.IsVisible ? "ON" : "OFF")}(Z={_polyline.ZIndex}) " +
				$"Circle:{(_circle.IsVisible ? "ON" : "OFF")}(Z={_circle.ZIndex})";
		}
	}
}
