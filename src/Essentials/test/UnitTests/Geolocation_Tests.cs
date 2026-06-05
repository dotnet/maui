using System;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Tests
{
	public class Geolocation_Tests
	{
		[Fact]
		public void ListeningRequest_DefaultsToNoMinimumDistance()
		{
			var request = new GeolocationListeningRequest();

			Assert.Equal(0, request.MinimumDistance);
		}

		[Fact]
		public void ListeningRequest_AccuracyConstructorDefaultsToNoMinimumDistance()
		{
			var request = new GeolocationListeningRequest(GeolocationAccuracy.Best);

			Assert.Equal(0, request.MinimumDistance);
		}

		[Fact]
		public void ListeningRequest_TimeConstructorDefaultsToNoMinimumDistance()
		{
			var request = new GeolocationListeningRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));

			Assert.Equal(0, request.MinimumDistance);
		}

		[Fact]
		public void ListeningRequest_MinimumDistanceCanBeSet()
		{
			var request = new GeolocationListeningRequest
			{
				MinimumDistance = 10.5
			};

			Assert.Equal(10.5, request.MinimumDistance);
		}
	}
}
