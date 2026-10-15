using System.Runtime.CompilerServices;
using HybridApp.ExternalProvider;
using Maui.Controls.Sample.HybridWebApp.Playground;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls.HybridWebApps;
using Microsoft.Maui.Controls.HybridWebApps.Generated;
using Microsoft.Maui.Media;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

// These fixtures opt into actual generated assemblies; no test-only command registration adapter.
static class TestComposition
{
	internal static IServiceCollection AddPlayground(this IServiceCollection services, string startupId, DateTimeOffset startedAt) =>
		services.AddPlayground(new SampleStartupIdentity(startupId, startedAt));

	internal static IServiceCollection AddPlayground(this IServiceCollection services, SampleStartupIdentity startup)
	{
		services.TryAddSingleton(startup);
		services.TryAddSingleton(new SharedCounterTarget());
		services.TryAddScoped<ActionState>();
		services.TryAddScoped<DependencyInjectionTests.ScopeProbeState>();
		services.TryAddScoped<InteractiveWait>();
		return services.AddMicrosoftMauiControlsHybridWebAppsUnitTestsActions();
	}
	sealed class Owner { internal HybridAppTargetObject? Hub; }
	static readonly ConditionalWeakTable<CommandRegistry, Owner> Owners = new();

	internal static ServiceCollection Services(HybridAppOptions? options = null)
	{
		var services = new ServiceCollection();
		services.AddSingleton<TimeProvider>(TimeProvider.System);
		services.AddSingleton(new NativeDispatcher(action => action()));
		services.AddSingleton<PluginActionState>();
		services.AddKeyedSingleton("action-key", new PluginActionState());
		services.AddSingleton(new SampleStartupIdentity("test-startup", DateTimeOffset.UtcNow));
		services.AddHybridApp(options ?? new HybridAppOptions());
		services.AddPlayground("test-startup", DateTimeOffset.UtcNow);
		services.AddExternalProviderActions();
		return services;
	}

	internal sealed class InteractiveWait
	{
		internal TaskCompletionSource<Ack> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
		[HybridAction("test.wait")]
		internal Task<Ack> Wait() => Completion.Task;
		[HybridAction("test.interactiveWait", Interactive = true)]
		internal Task<Ack> Interactive() => Completion.Task;
	}

	internal static CommandRegistry CreateRegistry(params object[] plugins) => CreateRegistryWithOptions(new(), plugins);

	internal static CommandRegistry CreateRegistryWithOptions(HybridAppOptions options, params object[] plugins)
	{
		var services = Services(options);
		foreach (var plugin in plugins)
			if (plugin is IClipboard clipboard)
				services.AddSingleton<IClipboard>(clipboard);
			else if (plugin is Microsoft.Maui.ApplicationModel.IAppInfo appInfo)
				services.AddSingleton(appInfo);
			else if (plugin is Microsoft.Maui.Devices.IDeviceInfo deviceInfo)
				services.AddSingleton(deviceInfo);
			else if (plugin is Microsoft.Maui.Devices.IBattery battery)
				services.AddSingleton(battery);
			else if (plugin is Microsoft.Maui.Devices.IDeviceDisplay display)
				services.AddSingleton(display);
			else if (plugin is Microsoft.Maui.Networking.IConnectivity connectivity)
				services.AddSingleton(connectivity);
			else if (plugin is Microsoft.Maui.ApplicationModel.ILauncher launcher)
				services.AddSingleton(launcher);
			else if (plugin is ITextToSpeech speech)
				services.AddSingleton<ITextToSpeech>(speech);
			else if (plugin is IPluginEventContract events)
				services.AddSingleton<IPluginEventContract>(events);
			else if (HybridActionAssemblies.Configuration(services).PerViewOnly.Contains(plugin.GetType()))
				services.AddScoped(plugin.GetType(), _ => plugin);
			else
				services.AddSingleton(plugin.GetType(), plugin);
		var owner = new Owner();
		services.AddScoped<HybridAppTargetObject>(_ => owner.Hub ?? throw new InvalidOperationException("Bind the test document before invoking core actions."));
		var scope = new TestHost(services).CreateScope();
		var registry = scope.ServiceProvider.GetRequiredService<CommandRegistry>();
		Owners.Add(registry, owner);
		return registry;
	}

	internal sealed class TestHost : IServiceProvider, IDisposable
	{
		readonly ServiceProvider _root;
		internal TestHost(IServiceCollection services) => _root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
		public object? GetService(Type type) => _root.GetService(type);
		internal IServiceScope CreateScope() => new LocalScope(_root.CreateScope());
		public void Dispose() => _root.Dispose();
		sealed class LocalScope : IServiceScope
		{
			readonly IServiceScope _host;
			readonly HybridServiceProvider _local;
			internal LocalScope(IServiceScope host) { _host = host; _local = host.ServiceProvider.CreateHybridAppProvider(); }
			public IServiceProvider ServiceProvider => _local;
			public void Dispose() { try { _local.Dispose(); } finally { _host.Dispose(); } }
		}
	}

	internal static HybridAppTargetObject CreateHub(CommandRegistry registry, Func<ReadyResult> ready, Action<NativeEvent>? send, TimeProvider? time = null)
	{
		var hub = new HybridAppTargetObject(registry, ready, send, time);
		Owners.GetValue(registry, _ => new Owner()).Hub = hub;
		return hub;
	}

	internal static HybridAppTargetObject CreateHub(object[] plugins, HybridAppOptions options,
		TimeProvider? time = null, NativeNotifications? notifications = null)
	{
		var registry = CreateRegistry(notifications is null ? plugins : [.. plugins, notifications]);
		var hub = new HybridAppTargetObject(registry, options, time, notifications: notifications);
		Owners.GetValue(registry, _ => new Owner()).Hub = hub;
		return hub;
	}
}
