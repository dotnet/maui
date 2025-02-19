using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	// TEST NOTES:
	//   - these tests require a gyroscope to be present
	[Category("Gyroscope")]
	public class Gyroscope_Tests
	{
		[Fact]
		public void IsSupported()
		{
			Assert.Equal(HardwareSupport.HasGyroscope, Gyroscope.IsSupported);
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Gyroscope, Traits.FeatureSupport.Supported)]
		public async Task Monitor(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasGyroscope)
				return;

			var tcs = new TaskCompletionSource<GyroscopeData>(TaskCreationOptions.RunContinuationsAsynchronously);
			Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
			Gyroscope.Start(sensorSpeed);

			void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}

			var d = await tcs.Task;

			Gyroscope.Stop();
			Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Gyroscope, Traits.FeatureSupport.Supported)]
		public async Task IsMonitoring(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasGyroscope)
				return;

			var tcs = new TaskCompletionSource<GyroscopeData>(TaskCreationOptions.RunContinuationsAsynchronously);
			Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
			Gyroscope.Start(sensorSpeed);

			void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}

			var d = await tcs.Task;
			Assert.True(Gyroscope.IsMonitoring);
			Gyroscope.Stop();
			Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;
		}

		[Theory]
		[InlineData(SensorSpeed.Fastest)]
		[Trait(Traits.Hardware.Gyroscope, Traits.FeatureSupport.Supported)]
		public async Task Stop_Monitor(SensorSpeed sensorSpeed)
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasGyroscope)
				return;

			var tcs = new TaskCompletionSource<GyroscopeData>(TaskCreationOptions.RunContinuationsAsynchronously);

			Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
			Gyroscope.Start(sensorSpeed);

			void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}

			var d = await tcs.Task;

			Gyroscope.Stop();
			Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;

			Assert.False(Gyroscope.IsMonitoring);
		}
	}
}
