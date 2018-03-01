using Microsoft.Caboodle;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class Battery_Tests
    {
        [Fact]
        public void Charge_Level() =>
            Assert.Equal(1.0, Battery.ChargeLevel);

        [Fact]
        public void Charge_State() =>
            Assert.Equal(BatteryState.Full, Battery.State);

        [Fact]
        public void Charge_Power() =>
            Assert.Equal(BatteryPowerSource.AC, Battery.PowerSource);
    }
}
