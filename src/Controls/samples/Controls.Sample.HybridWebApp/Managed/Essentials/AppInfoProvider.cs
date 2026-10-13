using Microsoft.Maui.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json.Serialization;

namespace Maui.HybridApp.Essentials.AppInfo;

/// <summary>Explicitly registers application metadata using the existing Essentials interface.</summary>
/// <param name="appInfo">The host's existing Essentials application API.</param>
/// <param name="dispatcher">The host's main-thread boundary.</param>
public sealed class AppInfoProvider(IAppInfo appInfo, NativeDispatcher dispatcher) : IHybridCommandProvider
{
	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = AppInfoJsonContext.Default;
		registry.Register("app.info", json.AppInfoArgs, json.AppInfoResult, (_, op) =>
			dispatcher.RunAsync(() => Task.FromResult(new AppInfoResult(appInfo.Name, appInfo.VersionString, appInfo.BuildString)),
				op.Cancellation));
	}
}

/// <summary>Explicit application-information provider composition.</summary>
public static class AppInfoServiceCollectionExtensions
{
	/// <summary>Registers a scoped provider and the real Essentials API unless the host already supplied one.</summary>
	/// <param name="services">The host's service registrations.</param>
	/// <returns>The same collection for further explicit registrations.</returns>
	public static IServiceCollection AddHybridAppInfo(this IServiceCollection services)
	{
		services.TryAddSingleton<IAppInfo>(_ => Microsoft.Maui.ApplicationModel.AppInfo.Current);
		services.TryAddScoped<NativeDispatcher>();
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, AppInfoProvider>());
		return services;
	}
}

sealed record AppInfoArgs;
sealed record AppInfoResult(string AppName, string Version, string Build);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(AppInfoArgs))]
[JsonSerializable(typeof(AppInfoResult))]
partial class AppInfoJsonContext : JsonSerializerContext;
