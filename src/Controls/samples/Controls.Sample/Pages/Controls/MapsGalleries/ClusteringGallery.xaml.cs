using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Position = Microsoft.Maui.Devices.Sensors.Location;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public partial class ClusteringGallery : ContentPage
	{
		// Center on a location (e.g., San Francisco area) for demo
		private readonly Position _center = new Position(37.7749, -122.4194);
		private readonly Random _random = new Random();
		private int _clusterClickCount;

		public ClusteringGallery()
		{
			InitializeComponent();
			
			// Set initial map region
			var mapSpan = MapSpan.FromCenterAndRadius(_center, Distance.FromKilometers(20));
			clusterMap.MoveToRegion(mapSpan);
			
			// Add some initial pins
			AddPins(30);
			UpdateStatus();
		}

		void OnAdd50PinsClicked(object sender, EventArgs e)
		{
			AddPins(50);
			UpdateStatus();
		}

		void OnAdd100PinsClicked(object sender, EventArgs e)
		{
			AddPins(100);
			UpdateStatus();
		}

		void OnClearPinsClicked(object sender, EventArgs e)
		{
			clusterMap.Pins.Clear();
			_clusterClickCount = 0;
			UpdateStatus();
		}

		void OnClusteringToggled(object sender, ToggledEventArgs e)
		{
			clusterMap.IsClusteringEnabled = e.Value;
			UpdateStatus();
		}

		void OnClusterClicked(object sender, ClusterClickedEventArgs e)
		{
			_clusterClickCount++;
			
			// Show info about the cluster
			var pinLabels = new List<string>();
			foreach (var pin in e.Pins)
			{
				pinLabels.Add(pin.Label);
			}

			DisplayAlert(
				$"Cluster Clicked ({e.Pins.Count} pins)",
				$"Location: {e.Location.Latitude:F4}, {e.Location.Longitude:F4}\n\nPins: {string.Join(", ", pinLabels.GetRange(0, Math.Min(5, pinLabels.Count)))}{(pinLabels.Count > 5 ? "..." : "")}",
				"OK");
			
			// Set Handled to true to prevent default zoom behavior
			// Uncomment to test: e.Handled = true;
			
			UpdateStatus();
		}

		void OnMapClicked(object sender, MapClickedEventArgs e)
		{
			// Add a pin where user clicks
			var pin = new Pin
			{
				Label = $"Clicked Pin",
				Address = $"Lat: {e.Location.Latitude:F4}, Lon: {e.Location.Longitude:F4}",
				Location = e.Location,
				Type = PinType.Generic
			};
			clusterMap.Pins.Add(pin);
			UpdateStatus();
		}

		void AddPins(int count)
		{
			// Add pins in a cluster around the center
			for (int i = 0; i < count; i++)
			{
				// Random offset within ~20km radius
				double latOffset = (_random.NextDouble() - 0.5) * 0.3;
				double lonOffset = (_random.NextDouble() - 0.5) * 0.3;
				
				// Alternate between two clustering identifiers for variety
				string clusterId = i % 3 == 0 ? "restaurants" : Pin.DefaultClusteringIdentifier;
				
				var pin = new Pin
				{
					Label = $"Pin {clusterMap.Pins.Count + 1}",
					Address = $"Address {clusterMap.Pins.Count + 1}",
					Location = new Position(_center.Latitude + latOffset, _center.Longitude + lonOffset),
					Type = i % 2 == 0 ? PinType.Place : PinType.Generic,
					ClusteringIdentifier = clusterId
				};
				clusterMap.Pins.Add(pin);
			}
		}

		void UpdateStatus()
		{
			statusLabel.Text = $"Pins: {clusterMap.Pins.Count} | Cluster clicks: {_clusterClickCount}";
		}
	}
}
