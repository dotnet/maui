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
        public void Charge_Level() =>
            Assert.True(Battery.ChargeLevel > 0.0);

        [Fact]
        public void Charge_State() =>
            Assert.True(Battery.State != BatteryState.Unknown);

        [Fact]
        public void Charge_Power() =>
            Assert.True(Battery.PowerSource != BatteryPowerSource.Unknown);
    }
}
