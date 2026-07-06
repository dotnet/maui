using System;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Tests
{
	public class GeolocationListeningRequest_Tests
	{
		[Fact]
		public void DefaultConstructorInitializesMinimumDistanceToZero()
		{
			var request = new GeolocationListeningRequest();

			Assert.Equal(0, request.MinimumDistance);
		}

		[Fact]
		public void AccuracyConstructorInitializesMinimumDistanceToZero()
		{
			var request = new GeolocationListeningRequest(GeolocationAccuracy.Best);

			Assert.Equal(0, request.MinimumDistance);
		}

		[Fact]
		public void AccuracyAndMinimumTimeConstructorInitializesMinimumDistanceToZero()
		{
			var request = new GeolocationListeningRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));

			Assert.Equal(0, request.MinimumDistance);
		}

		[Fact]
		public void MinimumDistanceCanBeSet()
		{
			var request = new GeolocationListeningRequest
			{
				MinimumDistance = 25,
			};

			Assert.Equal(25, request.MinimumDistance);
		}

		[Fact]
		public void ValidateListeningRequestAllowsZeroMinimumDistance()
		{
			var request = new GeolocationListeningRequest
			{
				MinimumDistance = 0,
			};

			GeolocationImplementation.ValidateListeningRequest(request);
		}

		[Fact]
		public void ValidateListeningRequestThrowsForNegativeMinimumDistance()
		{
			var request = new GeolocationListeningRequest
			{
				MinimumDistance = -double.Epsilon,
			};

			Assert.Throws<ArgumentOutOfRangeException>(() =>
				GeolocationImplementation.ValidateListeningRequest(request));
		}

		[Fact]
		public void ValidateListeningRequestThrowsForNegativeMinimumTime()
		{
			var request = new GeolocationListeningRequest
			{
				MinimumTime = TimeSpan.FromMilliseconds(-1),
			};

			Assert.Throws<ArgumentOutOfRangeException>(() =>
				GeolocationImplementation.ValidateListeningRequest(request));
		}
	}
}
