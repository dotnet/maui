using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	// TEST NOTES:
	//   - these tests require a battery to be present
	[Category("Battery")]
	public class Battery_Tests
	{
		[Fact
#if WINDOWS
			(Skip = "Somehow reports -1 on the CI test runner")
#endif
			]
		[Trait(Traits.Hardware.Battery, Traits.FeatureSupport.Supported)]
		public void Charge_Level()
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasBattery)
				return;

			if (Battery.State == BatteryState.Unknown || Battery.State == BatteryState.NotPresent)
				Assert.Equal(-1.0, Battery.ChargeLevel);
			else
				Assert.InRange(Battery.ChargeLevel, 0, 1.0);
		}

		[Fact]
		[Trait(Traits.Hardware.Battery, Traits.FeatureSupport.Supported)]
		public void Charge_State()
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasBattery)
				return;

			Assert.NotEqual(BatteryState.Unknown, Battery.State);
		}

		[Fact]
		[Trait(Traits.Hardware.Battery, Traits.FeatureSupport.Supported)]
		public void Charge_Power()
		{
			// TODO: the test runner app (UI version) should do this, until then...
			if (!HardwareSupport.HasBattery)
				return;

			Assert.NotEqual(BatteryPowerSource.Unknown, Battery.PowerSource);
		}

		[Fact]
		public void App_Is_Not_Lower_Power_mode()
		{
			Assert.Equal(EnergySaverStatus.Off, Battery.EnergySaverStatus);
		}
	}
}
