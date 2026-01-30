using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public partial class UserLocationGallery : ContentPage
	{
		private int _updateCount = 0;
		private readonly Queue<string> _eventLog = new();
		private const int MaxLogEntries = 5;

		public UserLocationGallery()
		{
			InitializeComponent();
			
			// Set initial map region to somewhere visible
			var initialLocation = new Location(37.7749, -122.4194); // San Francisco
			userLocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(initialLocation, Distance.FromMiles(10)));
		}

		private void OnUserLocationChanged(object sender, UserLocationChangedEventArgs e)
		{
			_updateCount++;
			
			// Update the labels
			LatitudeLabel.Text = $"Latitude: {e.Location.Latitude:F6}";
			LongitudeLabel.Text = $"Longitude: {e.Location.Longitude:F6}";
			UpdateCountLabel.Text = $"Updates received: {_updateCount}";
			
			// Add to event log
			var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
			var logEntry = $"[{timestamp}] ({e.Location.Latitude:F4}, {e.Location.Longitude:F4})";
			
			_eventLog.Enqueue(logEntry);
			while (_eventLog.Count > MaxLogEntries)
			{
				_eventLog.Dequeue();
			}
			
			EventLogLabel.Text = string.Join("\n", _eventLog);
			
			// Also verify the LastUserLocation property matches
			var lastLocation = userLocationMap.LastUserLocation;
			if (lastLocation != null)
			{
				System.Diagnostics.Debug.WriteLine($"LastUserLocation: ({lastLocation.Latitude:F6}, {lastLocation.Longitude:F6})");
			}
		}

		private void OnMapClicked(object sender, MapClickedEventArgs e)
		{
			// Just for additional verification - show where user clicked
			System.Diagnostics.Debug.WriteLine($"Map clicked at: ({e.Location.Latitude:F6}, {e.Location.Longitude:F6})");
		}
	}
}
