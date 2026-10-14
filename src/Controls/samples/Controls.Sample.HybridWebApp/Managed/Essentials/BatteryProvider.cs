using System.Text.Json.Serialization;
using Microsoft.Maui.Devices;

namespace Maui.HybridApp.Essentials.Battery;

/// <summary>Read-only native battery snapshots, without subscriptions or browser emulation.</summary>
public sealed class BatteryProvider(IBattery battery, NativeDispatcher dispatcher) : IHybridCommandProvider
{
	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = BatteryJsonContext.Default;
		registry.Register("battery.info", json.BatteryArgs, json.BatteryResult, (_, op) => dispatcher.RunAsync(
			() => Task.FromResult(Read()), op.Cancellation));
	}
	BatteryResult Read()
	{
#if MACCATALYST
		return MacBatterySnapshot.Read(battery.EnergySaverStatus.ToString());
#else
		return new(battery.ChargeLevel, battery.State.ToString(), battery.PowerSource.ToString(), battery.EnergySaverStatus.ToString());
#endif
	}
}

internal readonly record struct BatteryCapacity(double Current, double Maximum, bool Charging);
internal static class BatteryCapacitySnapshot
{
	internal static BatteryResult Read(IEnumerable<BatteryCapacity> capacities, string power, string saver)
	{
		var batteries = capacities.ToArray();
		if (batteries.Length == 0) return new(-1, "NotPresent", power, saver);
		if (batteries.Any(value => !double.IsFinite(value.Current) || !double.IsFinite(value.Maximum) ||
			value.Maximum <= 0 || value.Current < 0 || value.Current > value.Maximum))
			throw new CommandException("native_error", "The native battery capacity information is invalid.");
		var current = batteries.Sum(value => value.Current);
		var maximum = batteries.Sum(value => value.Maximum);
		if (!double.IsFinite(current) || !double.IsFinite(maximum))
			throw new CommandException("native_error", "The native aggregate battery capacity is invalid.");
		var charge = current / maximum;
		var state = batteries.Any(value => value.Charging) ? "Charging" : charge >= 1 ? "Full" :
			power == "Battery" ? "Discharging" : power == "AC" ? "NotCharging" : "Unknown";
		return new(charge, state, power, saver);
	}
}

sealed record BatteryArgs;
sealed record BatteryResult(double ChargeLevel, string State, string PowerSource, string EnergySaverStatus);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(BatteryArgs))]
[JsonSerializable(typeof(BatteryResult))]
partial class BatteryJsonContext : JsonSerializerContext;
