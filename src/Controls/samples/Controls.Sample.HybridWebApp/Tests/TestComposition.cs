using System.Runtime.CompilerServices;
using Maui.HybridApp;
using Maui.HybridApp.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HybridApp.ExternalProvider;
using Maui.Controls.Sample.HybridWebApp.Playground;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

// These fixtures opt into actual generated assemblies; no test-only command registration adapter.
static class TestComposition
{
	internal static IServiceCollection AddPlayground(this IServiceCollection services, string startupId, DateTimeOffset startedAt) =>
		services.AddPlayground(new SampleStartupIdentity(startupId, startedAt));

	internal static IServiceCollection AddPlayground(this IServiceCollection services, SampleStartupIdentity startup)
	{
		services.TryAddSingleton(startup);
		services.TryAddScoped<ActionState>();
		services.TryAddScoped<DependencyInjectionTests.ScopeProbeState>();
		services.TryAddScoped<InteractiveWait>();
		return services.AddPlaygroundUnitTestsActions();
	}
	sealed class Owner { internal HybridAppTargetObject? Hub; }
	static readonly ConditionalWeakTable<CommandRegistry, Owner> Owners = new();

	internal static ServiceCollection Services()
	{
		var services = new ServiceCollection();
		services.AddSingleton<TimeProvider>(TimeProvider.System);
		services.AddSingleton(new NativeDispatcher(action => action()));
		services.AddSingleton<NativeNotifications>();
		services.AddSingleton<PluginActionState>();
		services.AddKeyedSingleton("action-key", new PluginActionState());
		services.AddSingleton(new SampleStartupIdentity("test-startup", DateTimeOffset.UtcNow));
		services.AddHybridApp(new HybridAppOptions("test."));
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

	internal static CommandRegistry CreateRegistry(params object[] plugins)
	{
		var services = Services();
		foreach (var plugin in plugins) services.AddSingleton(plugin.GetType(), plugin);
		var owner = new Owner();
		services.AddSingleton<HybridAppTargetObject>(_ => owner.Hub ?? throw new InvalidOperationException("Bind the test document before invoking core actions."));
		var scope = services.BuildServiceProvider().CreateScope();
		var registry = scope.ServiceProvider.GetRequiredService<CommandRegistry>();
		Owners.Add(registry, owner);
		return registry;
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
		var registry = CreateRegistry(plugins);
		var hub = new HybridAppTargetObject(registry, options, time, notifications: notifications);
		Owners.GetValue(registry, _ => new Owner()).Hub = hub;
		return hub;
	}
}
