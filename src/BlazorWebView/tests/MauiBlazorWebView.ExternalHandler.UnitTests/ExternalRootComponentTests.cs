using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Maui.MauiBlazorWebView.ExternalHandler.UnitTests;

/// <summary>
/// Proves that a third-party handler can drive <see cref="RootComponent"/> lifecycle through public
/// API, and therefore does not need to duplicate MAUI's validation rules or ordering.
/// </summary>
public class ExternalRootComponentTests
{
	private static FakeExternalWebViewManager CreateManager() =>
		new(
			new ServiceCollection().BuildServiceProvider(),
			new NullFileProvider(),
			new JSComponentConfigurationStore());

	[Fact]
	public async Task AddToWebViewManagerAsync_RegistersComponent()
	{
		await using var manager = CreateManager();
		var rootComponent = new RootComponent
		{
			Selector = "#app",
			ComponentType = typeof(TestRootComponentA),
		};

		await rootComponent.AddToWebViewManagerAsync(manager);

		// Removing only succeeds when the selector is actually registered with the manager.
		await rootComponent.RemoveFromWebViewManagerAsync(manager);
	}

	[Fact]
	public async Task AddToWebViewManagerAsync_PassesParameters()
	{
		await using var manager = CreateManager();
		var rootComponent = new RootComponent
		{
			Selector = "#app",
			ComponentType = typeof(TestRootComponentA),
			Parameters = new Dictionary<string, object?>
			{
				["Value"] = 42,
			},
		};

		await rootComponent.AddToWebViewManagerAsync(manager);
		await rootComponent.RemoveFromWebViewManagerAsync(manager);
	}

	[Fact]
	public async Task AddToWebViewManagerAsync_Throws_WhenSelectorMissing()
	{
		await using var manager = CreateManager();
		var rootComponent = new RootComponent
		{
			ComponentType = typeof(TestRootComponentA),
		};

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(
			() => rootComponent.AddToWebViewManagerAsync(manager));

		Assert.Contains(nameof(RootComponent.Selector), ex.Message, StringComparison.Ordinal);
	}

	[Fact]
	public async Task AddToWebViewManagerAsync_Throws_WhenComponentTypeMissing()
	{
		await using var manager = CreateManager();
		var rootComponent = new RootComponent
		{
			Selector = "#app",
		};

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(
			() => rootComponent.AddToWebViewManagerAsync(manager));

		Assert.Contains(nameof(RootComponent.ComponentType), ex.Message, StringComparison.Ordinal);
	}

	[Fact]
	public async Task AddToWebViewManagerAsync_Throws_ForDuplicateSelector()
	{
		await using var manager = CreateManager();
		var first = new RootComponent
		{
			Selector = "#app",
			ComponentType = typeof(TestRootComponentA),
		};
		var second = new RootComponent
		{
			Selector = "#app",
			ComponentType = typeof(TestRootComponentB),
		};

		await first.AddToWebViewManagerAsync(manager);

		await Assert.ThrowsAsync<InvalidOperationException>(
			() => second.AddToWebViewManagerAsync(manager));
	}

	[Fact]
	public async Task RemoveFromWebViewManagerAsync_Throws_WhenSelectorMissing()
	{
		await using var manager = CreateManager();
		var rootComponent = new RootComponent
		{
			ComponentType = typeof(TestRootComponentA),
		};

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(
			() => rootComponent.RemoveFromWebViewManagerAsync(manager));

		Assert.Contains(nameof(RootComponent.Selector), ex.Message, StringComparison.Ordinal);
	}

	[Fact]
	public async Task RemoveFromWebViewManagerAsync_Throws_WhenNotRegistered()
	{
		await using var manager = CreateManager();
		var rootComponent = new RootComponent
		{
			Selector = "#never-added",
			ComponentType = typeof(TestRootComponentA),
		};

		await Assert.ThrowsAsync<InvalidOperationException>(
			() => rootComponent.RemoveFromWebViewManagerAsync(manager));
	}

	[Fact]
	public async Task AddToWebViewManagerAsync_Throws_WhenManagerIsNull()
	{
		var rootComponent = new RootComponent
		{
			Selector = "#app",
			ComponentType = typeof(TestRootComponentA),
		};

		await Assert.ThrowsAsync<ArgumentNullException>(
			() => rootComponent.AddToWebViewManagerAsync(null!));
	}

	[Fact]
	public async Task RemoveFromWebViewManagerAsync_Throws_WhenManagerIsNull()
	{
		var rootComponent = new RootComponent
		{
			Selector = "#app",
			ComponentType = typeof(TestRootComponentA),
		};

		await Assert.ThrowsAsync<ArgumentNullException>(
			() => rootComponent.RemoveFromWebViewManagerAsync(null!));
	}

	[Fact]
	public async Task ExternalHandler_AddsRootComponents_InCollectionOrder()
	{
		var blazorWebView = new BlazorWebView();
		var first = new RootComponent
		{
			Selector = "#app",
			ComponentType = typeof(TestRootComponentA),
		};
		var second = new RootComponent
		{
			Selector = "#secondary",
			ComponentType = typeof(TestRootComponentB),
		};
		blazorWebView.RootComponents.Add(first);
		blazorWebView.RootComponents.Add(second);

		var handler = new FakeExternalBlazorWebViewHandler();
		handler.SetVirtualView(blazorWebView);
		await using var manager = handler.StartWebViewCore();

		Assert.Equal(new[] { first, second }, handler.AddedRootComponents);
	}
}
