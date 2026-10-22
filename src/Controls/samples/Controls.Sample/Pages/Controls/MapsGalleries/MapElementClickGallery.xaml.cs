using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;
using Position = Microsoft.Maui.Devices.Sensors.Location;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public partial class MapElementClickGallery : ContentPage
	{
		public MapElementClickGallery()
		{
			InitializeComponent();
			AddMapElements();
		}

		void AddMapElements()
		{
			// Add a circle
			var circle = new Circle
			{
				Center = new Position(37.79752, -122.40183),
				Radius = new Distance(200),
				StrokeColor = Color.FromArgb("#88FF0000"),
				StrokeWidth = 8,
				FillColor = Color.FromArgb("#88FFC0CB")
			};
			circle.CircleClicked += OnCircleClicked;
			map.MapElements.Add(circle);

			// Add a polygon (triangle)
			var polygon = new Polygon
			{
				StrokeColor = Color.FromArgb("#880000FF"),
				StrokeWidth = 8,
				FillColor = Color.FromArgb("#8800FF00")
			};
			polygon.Geopath.Add(new Position(37.7997, -122.4050));
			polygon.Geopath.Add(new Position(37.7997, -122.3980));
			polygon.Geopath.Add(new Position(37.7950, -122.4015));
			polygon.PolygonClicked += OnPolygonClicked;
			map.MapElements.Add(polygon);

			// Add a polyline
			var polyline = new Polyline
			{
				StrokeColor = Color.FromArgb("#FF6600"),
				StrokeWidth = 10
			};
			polyline.Geopath.Add(new Position(37.7930, -122.4100));
			polyline.Geopath.Add(new Position(37.7940, -122.4050));
			polyline.Geopath.Add(new Position(37.7935, -122.4000));
			polyline.Geopath.Add(new Position(37.7950, -122.3950));
			polyline.PolylineClicked += OnPolylineClicked;
			map.MapElements.Add(polyline);
		}

		void OnCircleClicked(object? sender, EventArgs e)
		{
			StatusLabel.Text = "Circle clicked!";
			StatusLabel.TextColor = Colors.Red;
		}

		void OnPolygonClicked(object? sender, EventArgs e)
		{
			StatusLabel.Text = "Polygon clicked!";
			StatusLabel.TextColor = Colors.Green;
		}

		void OnPolylineClicked(object? sender, EventArgs e)
		{
			StatusLabel.Text = "Polyline clicked!";
			StatusLabel.TextColor = Colors.Orange;
		}
	}
}
