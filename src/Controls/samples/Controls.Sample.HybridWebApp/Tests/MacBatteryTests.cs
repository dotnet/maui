using Maui.HybridApp;
using Maui.HybridApp.Essentials.Battery;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public class MacBatteryTests
{
	[Theory]
	[InlineData(58, 100, 0.58)]
	[InlineData(60, 100, 0.60)]
	[InlineData(0, 100, 0)]
	[InlineData(100, 100, 1)]
	public void NativeCapacityDeterminesCharge(double current, double maximum, double expected)
	{
		var result = BatteryCapacitySnapshot.Read([new(current, maximum, true)], "AC", "Off");
		Assert.Equal(expected, result.ChargeLevel);
		Assert.Equal("Charging", result.State);
	}
	[Fact]
	public void MultipleBatteriesAreCapacityWeighted()
	{
		var result = BatteryCapacitySnapshot.Read([new(40, 80, false), new(20, 40, false)], "Battery", "Off");
		Assert.Equal(0.5, result.ChargeLevel);
		Assert.Equal("Discharging", result.State);
	}
	[Fact]
	public void MissingBatteryDoesNotInventFullCharge()
	{
		var result = BatteryCapacitySnapshot.Read([], "AC", "Off");
		Assert.Equal(-1, result.ChargeLevel);
		Assert.Equal("NotPresent", result.State);
		Assert.Throws<CommandException>(() => BatteryCapacitySnapshot.Read([new(1, 0, false)], "AC", "Off"));
	}
}
