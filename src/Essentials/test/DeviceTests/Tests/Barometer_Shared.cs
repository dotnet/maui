using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Barometer")]
	public class Barometer_Tests
	{
		[Fact]
		public void IsSupported()
			=> Assert.Equal(HardwareSupport.HasBarometer, Barometer.IsSupported);

		[Fact]
		[Trait(Traits.Hardware.Barometer, Traits.FeatureSupport.Supported)]
		public async Task Monitor()
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasBarometer)
				return;

			var tcs = new TaskCompletionSource<BarometerData>(TaskCreationOptions.RunContinuationsAsynchronously);

			Barometer.ReadingChanged += Barometer_ReadingChanged;
			void Barometer_ReadingChanged(object sender, BarometerChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}
			Barometer.Start(SensorSpeed.UI);

			var d = await tcs.Task;

			Assert.True(d.PressureInHectopascals >= 0);
			Barometer.Stop();
			Barometer.ReadingChanged -= Barometer_ReadingChanged;
		}

		[Fact]
		[Trait(Traits.Hardware.Barometer, Traits.FeatureSupport.Supported)]
		public async Task IsMonitoring()
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasBarometer)
				return;

			var tcs = new TaskCompletionSource<BarometerData>(TaskCreationOptions.RunContinuationsAsynchronously);
			Barometer.ReadingChanged += Barometer_ReadingChanged;
			void Barometer_ReadingChanged(object sender, BarometerChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}
			Barometer.Start(SensorSpeed.UI);

			var d = await tcs.Task;
			Assert.True(Barometer.IsMonitoring);

			Barometer.Stop();
			Barometer.ReadingChanged -= Barometer_ReadingChanged;
		}

		[Fact]
		[Trait(Traits.Hardware.Barometer, Traits.FeatureSupport.Supported)]
		public async Task Stop_Monitor()
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasBarometer)
				return;

			var tcs = new TaskCompletionSource<BarometerData>(TaskCreationOptions.RunContinuationsAsynchronously);
			Barometer.ReadingChanged += Barometer_ReadingChanged;
			void Barometer_ReadingChanged(object sender, BarometerChangedEventArgs e)
			{
				tcs.TrySetResult(e.Reading);
			}
			Barometer.Start(SensorSpeed.UI);

			var d = await tcs.Task;

			Barometer.Stop();
			Barometer.ReadingChanged -= Barometer_ReadingChanged;

			Assert.False(Barometer.IsMonitoring);
		}
	}
}
