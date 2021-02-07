using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
	// TEST NOTES:
	//   - these tests require an accelerometer to be present
	public class Accelerometer_Tests
	{
		[Fact]
		public void IsSupported()
		{
			Assert.Equal(HardwareSupport.HasAccelerometer, Accelerometer.IsSupported);
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Accelerometer, Traits.FeatureSupport.Supported)]
		public async Task Monitor(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasAccelerometer)
				return;

			var tcs = new TaskCompletionSource<AccelerometerData>();
			Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
			Accelerometer.Start(sensorSpeed);

			void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}

			var d = await tcs.Task;

			Accelerometer.Stop();
			Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Accelerometer, Traits.FeatureSupport.Supported)]
		public async Task IsMonitoring(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasAccelerometer)
				return;

			var tcs = new TaskCompletionSource<AccelerometerData>();
			Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
			Accelerometer.Start(sensorSpeed);

			void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}

			var d = await tcs.Task;
			Assert.True(Accelerometer.IsMonitoring);
			Accelerometer.Stop();
			Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Accelerometer, Traits.FeatureSupport.Supported)]
		public async Task Stop_Monitor(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasAccelerometer)
				return;

			var tcs = new TaskCompletionSource<AccelerometerData>();

			Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
			Accelerometer.Start(sensorSpeed);

			void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}

			var d = await tcs.Task;

			Accelerometer.Stop();
			Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;

			Assert.False(Accelerometer.IsMonitoring);
		}
	}
}
