using System.Text.Json.Serialization;
using Microsoft.Maui.Devices;

namespace Maui.HybridApp.Essentials.Display;

/// <summary>Read-only native main-display snapshots. This provider never changes KeepScreenOn.</summary>
public sealed class DisplayProvider(IDeviceDisplay display, NativeDispatcher dispatcher) : IHybridCommandProvider
{
	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = DisplayJsonContext.Default;
		registry.Register("display.info", json.DisplayArgs, json.DisplayResult, (_, op) => dispatcher.RunAsync(() =>
		{
			var info = display.MainDisplayInfo;
			return Task.FromResult(new DisplayResult(info.Width, info.Height, info.Density,
				info.Orientation.ToString(), info.Rotation.ToString(), info.RefreshRate));
		}, op.Cancellation));
	}
}

sealed record DisplayArgs;
sealed record DisplayResult(double Width, double Height, double Density, string Orientation, string Rotation, double RefreshRate);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(DisplayArgs))]
[JsonSerializable(typeof(DisplayResult))]
partial class DisplayJsonContext : JsonSerializerContext;
