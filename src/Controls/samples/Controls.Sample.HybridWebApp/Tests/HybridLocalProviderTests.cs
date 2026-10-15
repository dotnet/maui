using Maui.HybridApp;
using Maui.HybridApp.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System.Text.Json;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class HybridLocalProviderTests
{
	[Fact]
	public void BorrowedNativeOverridesRetainTheirHostTransientLifetime()
	{
		var services = new ServiceCollection();
		services.AddTransient<Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard>(_ => new FakeClipboard());
		services.AddHybridApp(new());
		using var root = services.BuildServiceProvider();
		using var scope = root.CreateScope();
		using var local = scope.ServiceProvider.CreateHybridAppProvider();
		Assert.NotSame(local.GetRequiredService<Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard>(),
			local.GetRequiredService<Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard>());
	}

	[Fact]
	public void ViewAndSessionServicesCannotAccidentallyBecomeGlobalSingletons()
	{
		foreach (var service in new object[] { new NativeNotifications(), new Maui.HybridApp.UI.HybridViewContext() })
		{
			var services = new ServiceCollection();
			services.AddHybridApp(new());
			services.AddSingleton(service.GetType(), service);
			using var root = services.BuildServiceProvider();
			using var scope = root.CreateScope();
			Assert.Throws<InvalidOperationException>(() => scope.ServiceProvider.CreateHybridAppProvider());
			((IDisposable)service).Dispose();
		}
	}

	[Fact]
	public async Task SameHostScopeSharesOnlyExplicitBusinessTargetWhileLocalTargetsAndHubsStayIsolatedAcrossReload()
	{
		var services = TestComposition.Services();
		using var host = services.BuildServiceProvider();
		using var hostScope = host.CreateScope();
		using var first = hostScope.ServiceProvider.CreateHybridAppProvider();
		using var second = hostScope.ServiceProvider.CreateHybridAppProvider();
		var hub1 = first.GetRequiredService<HybridAppTargetObject>();
		var hub2 = second.GetRequiredService<HybridAppTargetObject>();
		Assert.NotSame(hub1, hub2);
		Assert.NotSame(first.GetRequiredService<NativeNotifications>(), second.GetRequiredService<NativeNotifications>());
		Assert.NotSame(first.GetRequiredService<CommandRegistry>(), second.GetRequiredService<CommandRegistry>());
		var one = first.GetRequiredService<CommandRegistry>();
		var two = second.GetRequiredService<CommandRegistry>();
		async Task<JsonElement> Count(CommandRegistry registry, string action, int increment) =>
			await registry.DispatchAsync(action, JsonSerializer.SerializeToElement(new { increment }),
				new Operation("session", "request", default, (_, _) => { }));
		var local1 = await Count(one, "sample.localCounter", 1);
		var local2 = await Count(two, "sample.localCounter", 0);
		Assert.NotEqual(local1.GetProperty("instanceId").GetString(), local2.GetProperty("instanceId").GetString());
		Assert.Equal(0, local2.GetProperty("count").GetInt32());
		var shared1 = await Count(one, "sample.sharedCounter", 1);
		var shared2 = await Count(two, "sample.sharedCounter", 0);
		Assert.Equal(shared1.GetRawText(), shared2.GetRawText());
		var old = Guid.NewGuid().ToString("D");
		var next = Guid.NewGuid().ToString("D");
		string Connect(string session) => JsonSerializer.Serialize(new Request(1, session, Guid.NewGuid().ToString("D"), "core.connect", JsonSerializer.SerializeToElement(new { })), ProtocolJsonContext.Default.Request);
		await hub1.Invoke(Connect(old));
		await hub1.Invoke(Connect(next));
		Assert.False(hub1.IsCurrent(old));
		Assert.True(hub1.IsCurrent(next));
		Assert.Equal(local1.GetRawText(), (await Count(one, "sample.localCounter", 0)).GetRawText());
		first.Dispose();
		Assert.Equal(1, (await Count(two, "sample.sharedCounter", 0)).GetProperty("count").GetInt32());
	}

	[Fact]
	public async Task HostOwnsBusinessScopesWhileGeneratedConstructorsUseTheLocalProxy()
	{
		var services = TestComposition.Services();
		services.AddScoped<BusinessProbe>();
		services.AddKeyedScoped<BusinessProbe>("proxy-key", (_, _) => new());
		using var host = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
		using var hostScope = host.CreateScope();
		var normal = hostScope.ServiceProvider.GetRequiredService<BusinessProbe>();
		var keyed = hostScope.ServiceProvider.GetRequiredKeyedService<BusinessProbe>("proxy-key");
		using var first = hostScope.ServiceProvider.CreateHybridAppProvider();
		using var second = hostScope.ServiceProvider.CreateHybridAppProvider();
		var target = first.GetRequiredService<ProxyOwnedTarget>();
		Assert.Same(first, target.Provider);
		Assert.Same(first, first.GetRequiredService<IServiceProvider>());
		Assert.Same(normal, target.Normal);
		Assert.Same(keyed, target.Keyed);
		Assert.Same(keyed, first.GetRequiredKeyedService<BusinessProbe>("proxy-key"));
		Assert.Same(normal, second.GetRequiredService<BusinessProbe>());
		using var args = JsonDocument.Parse("{}");
		Assert.True((await first.GetRequiredService<CommandRegistry>().DispatchAsync("proxy.method", args.RootElement,
			new Operation("proxy-session", "proxy-request", default, (_, _) => { }))).GetBoolean());
		Assert.NotSame(target, second.GetRequiredService<ProxyOwnedTarget>());
		Assert.Null(first.GetService(typeof(IComparable)));
		Assert.Null(first.GetKeyedService(typeof(BusinessProbe), "missing"));
		Assert.Throws<InvalidOperationException>(() => first.GetRequiredKeyedService(typeof(BusinessProbe), "missing"));
		first.Dispose();
		Assert.Equal(1, target.Disposals);
		Assert.Equal(0, normal.Disposals);
		Assert.Equal(0, keyed.Disposals);
		Assert.Throws<ObjectDisposedException>(() => first.GetRequiredService<BusinessProbe>());
		hostScope.Dispose();
		Assert.Equal(1, normal.Disposals);
		Assert.Equal(1, keyed.Disposals);
	}

	[Fact]
	public void TypedFactoryOverridesUseProxyAndExternalSharedTargetsAreNeverDisposed()
	{
		var services = TestComposition.Services();
		var external = new ProxyOwnedTarget(null!, new(), new());
		services.AddSingleton(external);
		HybridActionAssemblies.Target<OwnedAlias>(services, _ => new());
		services.AddScoped<OwnedAlias>(provider => new OwnedAlias { Provider = provider });
		using var host = services.BuildServiceProvider();
		using var scope = host.CreateScope();
		using var first = scope.ServiceProvider.CreateHybridAppProvider();
		using var second = scope.ServiceProvider.CreateHybridAppProvider();
		Assert.Same(external, first.GetRequiredService<ProxyOwnedTarget>());
		Assert.Same(external, second.GetRequiredService<ProxyOwnedTarget>());
		var owned = first.GetRequiredService<OwnedAlias>();
		Assert.Same(first, owned.Provider);
		first.Dispose(); first.Dispose();
		Assert.Equal(1, owned.Disposals);
		Assert.Equal(0, external.Disposals);
	}

	[Fact]
	public async Task HubDisconnectsBeforeLaterCreatedResourceTargetsAreDisposed()
	{
		var services = TestComposition.Services();
		var session = Guid.NewGuid().ToString("D");
		HybridActionAssemblies.Target(services, provider =>
		{
			var hub = provider.GetRequiredService<HybridAppTargetObject>();
			return new DisposalProbe(() => !hub.IsCurrent(session));
		}, perViewOnly: true);
		using var host = services.BuildServiceProvider();
		using var scope = host.CreateScope();
		using var local = scope.ServiceProvider.CreateHybridAppProvider();
		var hub = local.GetRequiredService<HybridAppTargetObject>();
		await hub.Invoke(JsonSerializer.Serialize(new Request(1, session, Guid.NewGuid().ToString("D"), "core.connect",
			JsonSerializer.SerializeToElement(new { })), ProtocolJsonContext.Default.Request));
		Assert.True(hub.IsCurrent(session));
		var probe = local.GetRequiredService<DisposalProbe>();
		local.Dispose();
		Assert.True(probe.DisconnectedDuringDispose);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void LocalFactoryCannotRetireItsProviderDuringConstruction(bool asynchronous)
	{
		var services = new ServiceCollection();
		services.AddHybridApp(new());
		var allocated = false;
		HybridActionAssemblies.Target<OwnedAlias>(services, provider =>
		{
			if (asynchronous) ((HybridServiceProvider)provider).DisposeAsync().GetAwaiter().GetResult();
			else ((HybridServiceProvider)provider).Dispose();
			allocated = true;
			return new();
		});
		using var host = services.BuildServiceProvider();
		using var scope = host.CreateScope();
		using var local = scope.ServiceProvider.CreateHybridAppProvider();
		Assert.Throws<InvalidOperationException>(() => local.GetRequiredService<OwnedAlias>());
		Assert.False(allocated);
		Assert.NotNull(local.GetRequiredService<Maui.HybridApp.UI.HybridViewContext>());
	}

	[Fact]
	public async Task ConcurrentSyncAndAsyncDisposalShareTheActualCleanupCompletion()
	{
		var services = new ServiceCollection();
		services.AddHybridApp(new());
		var resource = new BlockingResource();
		HybridActionAssemblies.Target(services, _ => resource);
		using var host = services.BuildServiceProvider();
		using var scope = host.CreateScope();
		var local = scope.ServiceProvider.CreateHybridAppProvider();
		Assert.Same(resource, local.GetRequiredService<BlockingResource>());
		var synchronous = Task.Run(local.Dispose);
		Assert.True(resource.Entered.Wait(TimeSpan.FromSeconds(3)));
		try
		{
			var asynchronous = local.DisposeAsync().AsTask();
			Assert.False(asynchronous.IsCompleted);
			Assert.Same(asynchronous, local.DisposeAsync().AsTask());
			Assert.Throws<InvalidOperationException>(local.Dispose);
			resource.Release.Set();
			await Task.WhenAll(synchronous, asynchronous).WaitAsync(TimeSpan.FromSeconds(3));
			Assert.Equal(1, resource.Disposals);
		}
		finally
		{
			resource.Release.Set();
			await synchronous;
			await local.DisposeAsync();
			resource.Entered.Dispose();
			resource.Release.Dispose();
		}
	}

	[Fact]
	public async Task AsyncOwnedCleanupIsAwaitedAndReentrantResolutionCannotLeakInstances()
	{
		var services = new ServiceCollection();
		services.AddHybridApp(new());
		var asyncResource = new AsyncResource();
		HybridActionAssemblies.Target(services, _ => asyncResource);
		HybridActionAssemblies.Target<OwnedAlias>(services, provider => { _ = provider.GetRequiredService<OwnedAlias>(); return new(); });
		using var host = services.BuildServiceProvider();
		using var scope = host.CreateScope();
		var local = scope.ServiceProvider.CreateHybridAppProvider();
		Assert.Throws<InvalidOperationException>(() => local.GetRequiredService<OwnedAlias>());
		Assert.Same(asyncResource, local.GetRequiredService<AsyncResource>());
		Assert.Throws<InvalidOperationException>(() => local.Dispose());
		var disposing = local.DisposeAsync().AsTask();
		Assert.False(disposing.IsCompleted);
		Assert.Same(disposing, local.DisposeAsync().AsTask());
		Assert.Throws<InvalidOperationException>(() => local.Dispose());
		Assert.Throws<ObjectDisposedException>(() => local.GetService(typeof(AsyncResource)));
		asyncResource.Completion.SetResult();
		await disposing;
		await local.DisposeAsync();
		Assert.Equal(1, asyncResource.Disposals);
	}

	[Fact]
	public void ContextIsInitiallyUnboundAndFollowsOnlyTheLiveLogicalOwner()
	{
		var services = new ServiceCollection();
		services.AddHybridApp(new());
		using var host = services.BuildServiceProvider();
		using var scope = host.CreateScope();
		using var local = scope.ServiceProvider.CreateHybridAppProvider();
		Assert.Equal("context_unavailable", Assert.Throws<CommandException>(() => local.GetRequiredService<Page>()).Code);
		var context = local.GetRequiredService<Maui.HybridApp.UI.HybridViewContext>();
		var view = new HybridWebView();
		var layout = context.CreateLayout(view);
		var page = new ContentPage { Content = layout };
		var window = new Window(new NavigationPage(page));
		context.Bind(page, window, "index.html");
		Assert.Same(page, local.GetRequiredService<Page>());
		Assert.Same(view, local.GetRequiredService<HybridWebView>());
		Assert.Same(window, local.GetRequiredService<Microsoft.Maui.IWindow>());
		page.Content = null;
		Assert.Equal("context_unavailable", Assert.Throws<CommandException>(() => local.GetRequiredService<Page>()).Code);
		var replacement = new ContentPage { Content = layout };
		window.Page = replacement;
		Assert.Same(replacement, local.GetRequiredService<Page>());
		Assert.Same(window, local.GetRequiredService<Window>());
		replacement.Content = null;
		var foreign = new ContentPage { Content = layout };
		_ = new Window(foreign);
		Assert.Equal("context_unavailable", Assert.Throws<CommandException>(() => local.GetRequiredService<Page>()).Code);
	}

	public sealed class BusinessProbe : IDisposable
	{
		public int Disposals { get; private set; }
		public void Dispose() => Disposals++;
	}
	public sealed class ProxyOwnedTarget(IServiceProvider provider, BusinessProbe normal,
		[FromKeyedServices("proxy-key")] BusinessProbe keyed) : IDisposable
	{
		public IServiceProvider Provider => provider;
		public BusinessProbe Normal => normal;
		public BusinessProbe Keyed => keyed;
		public int Disposals { get; private set; }
		[HybridAction("proxy.method")]
		public bool Method([FromServices] IServiceProvider current,
			[FromKeyedServices("proxy-key")] BusinessProbe currentKeyed) => ReferenceEquals(provider, current) && ReferenceEquals(keyed, currentKeyed);
		public void Dispose() => Disposals++;
	}
	internal sealed class OwnedAlias : IDisposable
	{
		internal IServiceProvider? Provider;
		internal int Disposals;
		public void Dispose() => Disposals++;
	}
	internal sealed class AsyncResource : IAsyncDisposable
	{
		internal TaskCompletionSource Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
		internal int Disposals;
		public async ValueTask DisposeAsync() { Disposals++; await Completion.Task; }
	}
	internal sealed class BlockingResource : IDisposable
	{
		internal readonly ManualResetEventSlim Entered = new();
		internal readonly ManualResetEventSlim Release = new();
		internal int Disposals;
		public void Dispose()
		{
			Interlocked.Increment(ref Disposals);
			Entered.Set();
			if (!Release.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Test cleanup was not released.");
		}
	}
	internal sealed class DisposalProbe(Func<bool> isDisconnected) : IDisposable
	{
		internal bool DisconnectedDuringDispose;
		public void Dispose() => DisconnectedDuringDispose = isDisconnected();
	}
}
