using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    // TEST NOTES:
    //   - the battery is not emulated on iOS simulators
    public class Battery_Tests
    {
        [Fact]
#if __IOS__
        [Trait(Traits.DeviceType, Traits.DeviceTypes.Physical)]
#endif
        public void Charge_Level()
        {
            if (Battery.State == BatteryState.Unknown || Battery.State == BatteryState.NotPresent)
                Assert.Equal(-1.0, Battery.ChargeLevel);
            else
                Assert.InRange(Battery.ChargeLevel, 0, 1.0);
        }

        [Fact]
#if __IOS__
        [Trait(Traits.DeviceType, Traits.DeviceTypes.Physical)]
#endif
        public void Charge_State()
        {
           Assert.NotEqual(BatteryState.Unknown, Battery.State);
        }

        [Fact]
#if __IOS__
        [Trait(Traits.DeviceType, Traits.DeviceTypes.Physical)]
#endif
        public void Charge_Power()
        {
            Assert.NotEqual(BatteryPowerSource.Unknown, Battery.PowerSource);
        }
    }
}
