using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.MauiBlazorWebView.ExternalHandler.UnitTests;

/// <summary>
/// Stands in for a third-party BlazorWebView backend handler. It is compiled in an assembly that has
/// no <c>InternalsVisibleTo</c> access to <c>Microsoft.AspNetCore.Components.WebView.Maui</c>, so it
/// can only use the public extensibility surface.
/// </summary>
internal sealed class FakeExternalBlazorWebViewHandler : ViewHandler<IBlazorWebView, object>, IBlazorWebViewHandler
{
	public static readonly IPropertyMapper<IBlazorWebView, FakeExternalBlazorWebViewHandler> ExternalMapper =
		new PropertyMapper<IBlazorWebView, FakeExternalBlazorWebViewHandler>(ViewMapper);

	public FakeExternalBlazorWebViewHandler()
		: base(ExternalMapper)
	{
	}

	/// <summary>
	/// The native control this fake backend "owns". A real backend would use its platform web view.
	/// </summary>
	public object NativeControl { get; } = new FakeNativeWebView();

	public FakeExternalWebViewManager? WebViewManager { get; private set; }

	/// <summary>
	/// The task returned by the hot reload seam, or <see langword="null"/> when hot reload is disabled.
	/// </summary>
	public Task? HotReloadAttachTask { get; private set; }

	/// <summary>
	/// The task returned by the hot reload detach seam, or <see langword="null"/> when nothing was attached.
	/// </summary>
	public Task? HotReloadDetachTask { get; private set; }

	/// <summary>
	/// A real backend resolves this from <c>Services</c>; the fake supplies its own so the tests can run
	/// without a <c>MauiContext</c>.
	/// </summary>
	public FakeExternalDispatcher Dispatcher { get; } = new();

	public List<RootComponent> AddedRootComponents { get; } = new();

	public IFileProvider CreateFileProvider(string contentRootDir) => new NullFileProvider();

	public Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem)
	{
		ArgumentNullException.ThrowIfNull(workItem);
		return Task.FromResult(false);
	}

	protected override object CreatePlatformView() => NativeControl;

	/// <summary>
	/// Mirrors what a real backend does in its <c>StartWebViewCoreIfPossible</c>: create the manager,
	/// opt into static content hot reload, raise the initializing/initialized events, and add the
	/// configured root components in collection order.
	/// </summary>
	public FakeExternalWebViewManager StartWebViewCore()
	{
		var virtualView = VirtualView
			?? throw new InvalidOperationException("The handler must be connected to a virtual view first.");

		var services = new ServiceCollection().BuildServiceProvider();
		var manager = new FakeExternalWebViewManager(
			services,
			virtualView.CreateFileProvider(FakeExternalWebViewManager.ContentRootRelativeToAppRoot),
			virtualView.JSComponents,
			Dispatcher);

		HotReloadAttachTask = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);

		virtualView.BlazorWebViewInitializing(new BlazorWebViewInitializingEventArgs());
		virtualView.BlazorWebViewInitialized(new BlazorWebViewInitializedEventArgs(NativeControl));

		foreach (var rootComponent in virtualView.RootComponents)
		{
			// Since the page isn't loaded yet, this always completes synchronously.
			_ = rootComponent.AddToWebViewManagerAsync(manager);
			AddedRootComponents.Add(rootComponent);
		}

		WebViewManager = manager;
		manager.Navigate(virtualView.StartPath);
		return manager;
	}

	/// <summary>
	/// Mirrors what a real backend does in its <c>DisconnectHandler</c>: detach the hot reload notifier
	/// before disposing the manager, so a reconnected handler can attach again.
	/// </summary>
	public async Task StopWebViewCoreAsync()
	{
		if (WebViewManager is null)
		{
			return;
		}

		HotReloadDetachTask = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(WebViewManager);
		if (HotReloadDetachTask is not null)
		{
			await HotReloadDetachTask;
		}

		await WebViewManager.DisposeAsync();
		WebViewManager = null;
	}
}

internal sealed class FakeNativeWebView
{
	public string Url { get; set; } = string.Empty;
}
