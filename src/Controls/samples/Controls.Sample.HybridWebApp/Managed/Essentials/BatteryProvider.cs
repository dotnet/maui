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
			() => Task.FromResult(new BatteryResult(battery.ChargeLevel, battery.State.ToString(),
				battery.PowerSource.ToString(), battery.EnergySaverStatus.ToString())), op.Cancellation));
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
