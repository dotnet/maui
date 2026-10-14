using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Maui.HybridApp.Essentials.AppInfo;
using Maui.HybridApp.Essentials.Clipboard;
using Maui.HybridApp.Essentials.DeviceInfo;
using Maui.HybridApp.Essentials.Preferences;
using Maui.HybridApp.Essentials.Battery;
using Maui.HybridApp.Essentials.Display;
using Maui.HybridApp.Essentials.FilePicker;
using Maui.HybridApp.Essentials.Connectivity;
using Maui.HybridApp.Essentials.TextToSpeech;
using Maui.HybridApp.Essentials.Launcher;
using Maui.HybridApp.UI;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Media;
using Microsoft.Maui.ApplicationModel;

namespace Maui.HybridApp;

/// <summary>Minimal explicit DI composition for one hybrid hub per owning WebView scope.</summary>
public static class HybridAppServiceCollectionExtensions
{
	/// <summary>Registers the scoped hub and implemented Essentials and MAUI UI providers.</summary>
	/// <param name="services">The host's service registrations.</param>
	/// <param name="options">Immutable host policy with its explicitly chosen preference prefix and text limit.</param>
	/// <returns>The same collection for further explicit registrations.</returns>
	/// <remarks>
	/// Registers only implemented providers, not every Essentials or Controls feature.
	/// The WIP package now depends on MAUI Controls; UI and Essentials remain distinct provider modules.
	/// Existing host-provided Essentials interfaces, dispatcher and clock are preserved.
	/// Repeating identical policy is idempotent; conflicting policy is rejected.
	/// Resolve the hub from an owning scope, never the root provider, and dispose that scope with the WebView/window.
	/// Registration does not grant OS permissions or enable arbitrary native invocation.
	/// </remarks>
	public static IServiceCollection AddHybridApp(this IServiceCollection services, HybridAppOptions options)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(options);
		var existing = services.LastOrDefault(service => service.ServiceType == typeof(HybridAppOptions));
		if (existing is not null && (existing.ImplementationInstance is not HybridAppOptions previous ||
			previous.PreferenceNamespace != options.PreferenceNamespace || previous.MaximumTextLength != options.MaximumTextLength ||
			previous.MaximumWindows != options.MaximumWindows))
			throw new InvalidOperationException("HybridApp host policy is already registered. Configure its namespace and text limit once at startup.");
		services.TryAddSingleton(options);
		services.TryAddSingleton<TimeProvider>(TimeProvider.System);
		services.TryAddScoped<NativeDispatcher>();
		services.TryAddScoped<HybridAppTargetObject>();
		services.TryAddScoped<HybridViewContext>();
		services.TryAddSingleton<HybridWindowHost>();
		services.TryAddScoped<NativeNotifications>();
		services.TryAddScoped<HybridWindowActions>();
		services.TryAddScoped<HybridMenus>();
		services.TryAddSingleton<HybridFileLimits>();
		services.TryAddSingleton<IBattery>(_ => Microsoft.Maui.Devices.Battery.Default);
		services.TryAddSingleton<IDeviceDisplay>(_ => DeviceDisplay.Current);
		services.TryAddSingleton<IFilePicker>(_ => Microsoft.Maui.Storage.FilePicker.Default);
		services.TryAddSingleton<IConnectivity>(_ => Microsoft.Maui.Networking.Connectivity.Current);
		services.TryAddSingleton<ITextToSpeech>(_ => Microsoft.Maui.Media.TextToSpeech.Default);
		services.TryAddSingleton<ILauncher>(_ => Microsoft.Maui.ApplicationModel.Launcher.Default);
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, BatteryProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, DisplayProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, FilePickerProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, ConnectivityProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, TextToSpeechProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, LauncherProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, WindowProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, DialogProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, RemoteWebViewProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, MenuProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, WindowEventsProvider>());
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, ApplicationProvider>());
		return services
			.AddHybridAppInfo()
			.AddHybridDeviceInfo()
			.AddHybridClipboard()
			.AddHybridPreferences();
	}

	/// <summary>Adds an explicitly opted-in scoped provider to the collection consumed by the hub.</summary>
	/// <typeparam name="TProvider">A provider with public constructors whose dependencies are registered by its host or plugin.</typeparam>
	/// <param name="services">The host's service registrations.</param>
	/// <returns>The same collection for further explicit registrations.</returns>
	/// <remarks>Providers own their generated JSON metadata. Registering conflicting providers fails during hub construction, before its lease timer starts.</remarks>
	public static IServiceCollection AddHybridProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TProvider>(
		this IServiceCollection services) where TProvider : class, IHybridCommandProvider
	{
		ArgumentNullException.ThrowIfNull(services);
		services.AddScoped<IHybridCommandProvider, TProvider>();
		return services;
	}
}
