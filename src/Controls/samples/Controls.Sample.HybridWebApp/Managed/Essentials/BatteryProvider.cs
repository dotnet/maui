using Microsoft.Maui.Devices;

namespace Maui.HybridApp.Essentials.Battery;

/// <summary>Read-only Essentials battery snapshots, without browser emulation.</summary>
/// <param name="battery">The host's cross-platform Essentials battery API.</param>
public sealed class BatteryProvider(IBattery battery)
{
	[HybridAction("battery.info", MainThread = true)]
	internal BatteryResult Read() =>
		new(battery.ChargeLevel, battery.State.ToString(), battery.PowerSource.ToString(), battery.EnergySaverStatus.ToString());
}

sealed record BatteryResult(double ChargeLevel, string State, string PowerSource, string EnergySaverStatus);
