using Maui.HybridApp.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Maui.HybridApp.Essentials.FilePicker;
using Maui.HybridApp.UI;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Media;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Dispatching;

namespace Maui.HybridApp;

/// <summary>Explicit typed local composition over the owning WebView's host scope.</summary>
public static class HybridAppServiceCollectionExtensions
{
	/// <summary>Configures local hub/provider factories and borrowed native defaults without globally registering view services.</summary>
	/// <param name="services">The host's service registrations.</param>
	/// <param name="options">Immutable host clipboard/share text and window resource limits.</param>
	/// <returns>The same collection for further explicit registrations.</returns>
	/// <remarks>
	/// Registers only implemented providers, not every Essentials or Controls feature.
	/// The WIP package now depends on MAUI Controls; UI and Essentials remain distinct provider modules.
	/// Existing host-provided Essentials interfaces, dispatcher and clock are preserved.
	/// Repeating identical policy is idempotent; conflicting policy is rejected.
	/// Create a local provider over the owning host scope, and dispose the local provider before that scope.
	/// Registration does not grant OS permissions or enable arbitrary native invocation.
	/// </remarks>
	public static IServiceCollection AddHybridApp(this IServiceCollection services, HybridAppOptions options)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(options);
		var existing = services.LastOrDefault(service => service.ServiceType == typeof(HybridAppOptions));
		if (existing is not null && (existing.ImplementationInstance is not HybridAppOptions previous ||
			previous.MaximumTextLength != options.MaximumTextLength ||
			previous.MaximumWindows != options.MaximumWindows))
			throw new InvalidOperationException("HybridApp host policy is already registered. Configure its resource limits once at startup.");
		services.TryAddSingleton(options);
		services.TryAddSingleton<TimeProvider>(TimeProvider.System);
		services.TryAddSingleton<HybridWindowHost>();
		services.TryAddSingleton(_ => new Essentials.HybridShareCache(FileSystem.CacheDirectory, TimeProvider.System));
		HybridActionAssemblies.Target(services, _ => new HybridViewContext(), perViewOnly: true);
		HybridActionAssemblies.Target(services, provider =>
		{
			var context = provider.GetRequiredService<HybridViewContext>();
			return new NativeDispatcher(action => context.Page.Dispatcher.DispatchAsync(action),
				action => context.TeardownDispatcher.DispatchAsync(action));
		});
		HybridActionAssemblies.Target(services, _ => new NativeNotifications(), perViewOnly: true);
		HybridActionAssemblies.Target(services, _ => new HybridFileLimits());
		Default<IBattery>(services, () => Microsoft.Maui.Devices.Battery.Default);
		Default<IDeviceDisplay>(services, () => DeviceDisplay.Current);
		Default<IFilePicker>(services, () => Microsoft.Maui.Storage.FilePicker.Default);
		Default<IConnectivity>(services, () => Microsoft.Maui.Networking.Connectivity.Current);
		Default<ITextToSpeech>(services, () => Microsoft.Maui.Media.TextToSpeech.Default);
		Default<ILauncher>(services, () => Microsoft.Maui.ApplicationModel.Launcher.Default);
		Default<IAppInfo>(services, () => Microsoft.Maui.ApplicationModel.AppInfo.Current);
		Default<IDeviceInfo>(services, () => Microsoft.Maui.Devices.DeviceInfo.Current);
		Default<Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard>(services, () => Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default);
		Default<IPreferences>(services, () => Microsoft.Maui.Storage.Preferences.Default);
		Default<Microsoft.Maui.Devices.Sensors.IGeolocation>(services, () => Microsoft.Maui.Devices.Sensors.Geolocation.Default);
		Default<Microsoft.Maui.ApplicationModel.DataTransfer.IShare>(services, () => Microsoft.Maui.ApplicationModel.DataTransfer.Share.Default);
		return services.AddMauiHybridAppActions();
	}

	static void Default<T>(IServiceCollection services, Func<T> factory) where T : class
	{
		HybridActionAssemblies.Target(services, _ => factory());
		HybridActionAssemblies.Configuration(services).Borrowed.Add(typeof(T));
	}

	/// <summary>Creates one local overlay over the owning host scope; the caller retains ownership of that scope.</summary>
	/// <param name="services">The owning host/page/window service provider, not a handler's alternate scope.</param>
	/// <returns>A disposable per-view hybrid provider.</returns>
	public static HybridServiceProvider CreateHybridAppProvider(this IServiceProvider services) =>
		new(services, services.GetRequiredService<HybridLocalConfiguration>());

	/// <summary>Prunes expired app-owned share files at native startup without creating any view hub.</summary>
	/// <param name="services">The configured native host provider.</param>
	public static void PruneHybridShareCache(this IServiceProvider services) =>
		services.GetRequiredService<Essentials.HybridShareCache>().Prune();
}
