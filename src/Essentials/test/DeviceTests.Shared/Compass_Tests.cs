using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
	// TEST NOTES:
	//   - these tests require a compass to be present
	public class Compass_Tests
	{
		[Fact]
		public void IsSupported()
		{
			Assert.Equal(HardwareSupport.HasCompass, Compass.IsSupported);
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Compass, Traits.FeatureSupport.Supported)]
		public async Task Monitor(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasCompass)
				return;

			var tcs = new TaskCompletionSource<CompassData>();

			Compass.ReadingChanged += Compass_ReadingChanged;
			void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}
			Compass.Start(sensorSpeed);

			var d = await tcs.Task;

			Assert.True(d.HeadingMagneticNorth >= 0);
			Compass.Stop();
			Compass.ReadingChanged -= Compass_ReadingChanged;
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Compass, Traits.FeatureSupport.Supported)]
		public async Task IsMonitoring(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasCompass)
				return;

			var tcs = new TaskCompletionSource<CompassData>();
			Compass.ReadingChanged += Compass_ReadingChanged;
			void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}
			Compass.Start(sensorSpeed);

			var d = await tcs.Task;
			Assert.True(Compass.IsMonitoring);

			Compass.Stop();
			Compass.ReadingChanged -= Compass_ReadingChanged;
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Compass, Traits.FeatureSupport.Supported)]
		public async Task Stop_Monitor(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasCompass)
				return;

			var tcs = new TaskCompletionSource<CompassData>();
			Compass.ReadingChanged += Compass_ReadingChanged;
			void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}
			Compass.Start(sensorSpeed);

			var d = await tcs.Task;

			Compass.Stop();
			Compass.ReadingChanged -= Compass_ReadingChanged;

			Assert.False(Compass.IsMonitoring);
		}
	}
}
