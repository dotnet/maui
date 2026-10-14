using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Xunit;

namespace Tests
{
	public class Battery_Tests
	{
		[Theory]
		[InlineData(58, 100, 0.58)]
		[InlineData(60, 100, 0.60)]
		[InlineData(0, 100, 0)]
		[InlineData(100, 100, 1)]
		public void Native_Capacity_Determines_Charge(double current, double maximum, double expected)
		{
			var result = BatteryCapacitySnapshot.Create(new[] { new BatteryCapacity(current, maximum, true) }, BatteryPowerSource.AC);
			Assert.Equal(expected, result.ChargeLevel);
			Assert.Equal(BatteryState.Charging, result.State);
			Assert.Equal(BatteryPowerSource.AC, result.PowerSource);
		}

		[Fact]
		public void Native_Capacities_Are_Weighted()
		{
			var result = BatteryCapacitySnapshot.Create(new[] { new BatteryCapacity(30, 60, false), new BatteryCapacity(10, 40, false) },
				BatteryPowerSource.Battery);
			Assert.Equal(0.4, result.ChargeLevel);
			Assert.Equal(BatteryState.Discharging, result.State);
		}

		[Fact]
		public void Missing_Battery_Does_Not_Report_Full()
		{
			var result = BatteryCapacitySnapshot.Create(Array.Empty<BatteryCapacity>(), BatteryPowerSource.AC);
			Assert.Equal(-1, result.ChargeLevel);
			Assert.Equal(BatteryState.NotPresent, result.State);
		}

		[Theory]
		[InlineData(100, 100, BatteryPowerSource.AC, BatteryState.Full)]
		[InlineData(60, 100, BatteryPowerSource.AC, BatteryState.NotCharging)]
		[InlineData(60, 100, BatteryPowerSource.Battery, BatteryState.Discharging)]
		[InlineData(60, 100, BatteryPowerSource.Unknown, BatteryState.Unknown)]
		public void Native_State_Uses_Actual_Charge_And_Power(double current, double maximum, BatteryPowerSource power, BatteryState state)
		{
			var result = BatteryCapacitySnapshot.Create(new[] { new BatteryCapacity(current, maximum, false) }, power);
			Assert.Equal(state, result.State);
		}

		[Theory]
		[InlineData(1, 0)]
		[InlineData(-1, 100)]
		[InlineData(101, 100)]
		[InlineData(double.NaN, 100)]
		[InlineData(1, double.PositiveInfinity)]
		public void Invalid_Native_Capacity_Is_Not_A_Successful_Reading(double current, double maximum) =>
			Assert.Throws<InvalidOperationException>(() => BatteryCapacitySnapshot.Create(
				new[] { new BatteryCapacity(current, maximum, false) }, BatteryPowerSource.AC));

		[Fact]
		public void Charge_Level_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Battery.ChargeLevel);

		[Fact]
		public void Charge_State_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Battery.State);

		[Fact]
		public void Charge_Power_Source_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Battery.PowerSource);

		[Fact]
		public void Battery_Changed_Event_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Battery.BatteryInfoChanged += Battery_BatteryInfoChanged);

		void Battery_BatteryInfoChanged(object sender, BatteryInfoChangedEventArgs e)
		{
		}
	}
}
