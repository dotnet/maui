using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	// TEST NOTES:
	//   - these tests require a magnetometer to be present
	[Category("Magnetometer")]
	public class Magnetometer_Tests
	{
		[Fact]
		public void IsSupported()
		{
			Assert.Equal(HardwareSupport.HasMagnetometer, Magnetometer.IsSupported);
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Magnetometer, Traits.FeatureSupport.Supported)]
		public async Task Monitor(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasMagnetometer)
				return;

			var tcs = new TaskCompletionSource<MagnetometerData>(TaskCreationOptions.RunContinuationsAsynchronously);
			Magnetometer.ReadingChanged += Magnetometer_ReadingChanged;
			Magnetometer.Start(sensorSpeed);

			void Magnetometer_ReadingChanged(object sender, MagnetometerChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}

			var d = await tcs.Task;

			Magnetometer.Stop();
			Magnetometer.ReadingChanged -= Magnetometer_ReadingChanged;
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Magnetometer, Traits.FeatureSupport.Supported)]
		public async Task IsMonitoring(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasMagnetometer)
				return;

			var tcs = new TaskCompletionSource<MagnetometerData>(TaskCreationOptions.RunContinuationsAsynchronously);
			Magnetometer.ReadingChanged += Magnetometer_ReadingChanged;
			Magnetometer.Start(sensorSpeed);

			void Magnetometer_ReadingChanged(object sender, MagnetometerChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}

			var d = await tcs.Task;
			Assert.True(Magnetometer.IsMonitoring);
			Magnetometer.Stop();
			Magnetometer.ReadingChanged -= Magnetometer_ReadingChanged;
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Magnetometer, Traits.FeatureSupport.Supported)]
		public async Task Stop_Monitor(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasMagnetometer)
				return;

			var tcs = new TaskCompletionSource<MagnetometerData>(TaskCreationOptions.RunContinuationsAsynchronously);

			Magnetometer.ReadingChanged += Magnetometer_ReadingChanged;
			Magnetometer.Start(sensorSpeed);

			void Magnetometer_ReadingChanged(object sender, MagnetometerChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}

			var d = await tcs.Task;

			Magnetometer.Stop();
			Magnetometer.ReadingChanged -= Magnetometer_ReadingChanged;

			Assert.False(Magnetometer.IsMonitoring);
		}
	}
}
