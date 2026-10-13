using Microsoft.Maui.Devices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json.Serialization;

namespace Maui.HybridApp.Essentials.DeviceInfo;

/// <summary>Explicitly registers device metadata, independently of AppInfo and process diagnostics.</summary>
/// <param name="deviceInfo">The host's existing Essentials device API.</param>
/// <param name="dispatcher">The host's main-thread boundary.</param>
public sealed class DeviceInfoProvider(IDeviceInfo deviceInfo, NativeDispatcher dispatcher) : IHybridCommandProvider
{
	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = DeviceInfoJsonContext.Default;
		registry.Register("device.info", json.DeviceInfoArgs, json.DeviceInfoResult, (_, op) =>
			dispatcher.RunAsync(() => Task.FromResult(new DeviceInfoResult(deviceInfo.Platform.ToString(), deviceInfo.VersionString, deviceInfo.Model)),
				op.Cancellation));
	}
}

/// <summary>Explicit device-information provider composition.</summary>
public static class DeviceInfoServiceCollectionExtensions
{
	/// <summary>Registers a scoped provider and the real Essentials API unless the host already supplied one.</summary>
	/// <param name="services">The host's service registrations.</param>
	/// <returns>The same collection for further explicit registrations.</returns>
	public static IServiceCollection AddHybridDeviceInfo(this IServiceCollection services)
	{
		services.TryAddSingleton<IDeviceInfo>(_ => Microsoft.Maui.Devices.DeviceInfo.Current);
		services.TryAddScoped<NativeDispatcher>();
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, DeviceInfoProvider>());
		return services;
	}
}

sealed record DeviceInfoArgs;
sealed record DeviceInfoResult(string Platform, string OsVersion, string Model);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(DeviceInfoArgs))]
[JsonSerializable(typeof(DeviceInfoResult))]
partial class DeviceInfoJsonContext : JsonSerializerContext;
