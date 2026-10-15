using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.HybridWebApps.Essentials.Battery;

/// <summary>Read-only Essentials battery snapshots, without browser emulation.</summary>
/// <param name="battery">The host's cross-platform Essentials battery API.</param>
public sealed class BatteryProvider(IBattery battery)
{
	[HybridAction("battery.info", MainThread = true)]
	internal BatteryResult Read() =>
		new(battery.ChargeLevel, battery.State, battery.PowerSource, battery.EnergySaverStatus);
}

sealed record BatteryResult(double ChargeLevel, BatteryState State, BatteryPowerSource PowerSource, EnergySaverStatus EnergySaverStatus);
