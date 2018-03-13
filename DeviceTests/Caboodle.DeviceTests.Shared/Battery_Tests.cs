using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class Battery_Tests
    {
        [Fact]
        [Trait(Traits.DeviceType, Traits.DeviceTypes.Physical)]
        public void Charge_Level()
        {
            // TODO: remove this as soon as the test harness can filter
            // the iOS simulator does not emulate a battery
            if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DeviceInfo.Platforms.iOS)
                return;

            Assert.InRange(Battery.ChargeLevel, 0.01, 100.0);
        }

        [Fact]
        [Trait(Traits.DeviceType, Traits.DeviceTypes.Physical)]
        public void Charge_State()
        {
            // TODO: remove this as soon as the test harness can filter
            // the iOS simulator does not emulate a battery
            if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DeviceInfo.Platforms.iOS)
                return;

            Assert.NotEqual(BatteryState.Unknown, Battery.State);
        }

        [Fact]
        [Trait(Traits.DeviceType, Traits.DeviceTypes.Physical)]
        public void Charge_Power()
        {
            // TODO: remove this as soon as the test harness can filter
            // the iOS simulator does not emulate a battery
            if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DeviceInfo.Platforms.iOS)
                return;

            Assert.NotEqual(BatteryPowerSource.Unknown, Battery.PowerSource);
        }
    }
}
