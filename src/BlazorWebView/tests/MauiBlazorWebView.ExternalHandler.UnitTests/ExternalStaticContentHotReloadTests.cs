using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Maui.MauiBlazorWebView.ExternalHandler.UnitTests;

/// <summary>
/// Proves that a third-party handler can participate in MAUI Blazor static content hot reload
/// through <see cref="BlazorWebViewStaticContentHotReload"/> without any access to internals.
/// </summary>
public class ExternalStaticContentHotReloadTests
{
	private const string HotReloadScriptPath = "_framework/static-content-hot-reload.js";

	// Static content hot reload is gated on MetadataUpdater.IsSupported, which is only true when the
	// runtime is started with DOTNET_MODIFIABLE_ASSEMBLIES=debug. CI sets that for every xUnit work
	// item (see eng/helix.proj), so the enabled branch of these tests is the one that runs there.
	private static FakeExternalWebViewManager CreateManager() =>
		new(
			new ServiceCollection().BuildServiceProvider(),
			new NullFileProvider(),
			new JSComponentConfigurationStore(),
			new FakeExternalDispatcher());

	private static string RequestUri(string relativePath) =>
		new Uri(FakeExternalWebViewManager.AppBaseUri, relativePath).AbsoluteUri;

	[Fact]
	public async Task TryAttachToWebViewManager_ReportsWhetherItAttached()
	{
		await using var manager = CreateManager();

		var attachTask = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);

		if (MetadataUpdater.IsSupported)
		{
			Assert.NotNull(attachTask);
			await attachTask!;

			// The notifier root component is registered under a fixed selector, so removing it succeeds.
			await manager.RemoveRootComponentAsync("body::after");
		}
		else
		{
			Assert.Null(attachTask);
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => manager.RemoveRootComponentAsync("body::after"));
		}
	}

	[Fact]
	public async Task TryAttachToWebViewManager_IsIdempotentForTheSameManager()
	{
		await using var manager = CreateManager();

		var first = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);
		var second = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);

		// A second attach must not throw "there is already a root component with selector 'body::after'".
		Assert.Same(first, second);
	}

	[Fact]
	public async Task TryAttachToWebViewManager_AttachesEachManagerIndependently()
	{
		await using var first = CreateManager();
		await using var second = CreateManager();

		var firstTask = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(first);
		var secondTask = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(second);

		if (!MetadataUpdater.IsSupported)
		{
			Assert.Null(firstTask);
			Assert.Null(secondTask);
			return;
		}

		Assert.NotNull(firstTask);
		Assert.NotNull(secondTask);

		// Idempotence is scoped to a single manager, so each one really did get the notifier registered.
		await first.RemoveRootComponentAsync("body::after");
		await second.RemoveRootComponentAsync("body::after");
	}

	[Fact]
	public void TryAttachToWebViewManager_Throws_WhenManagerIsNull()
	{
		Assert.Throws<ArgumentNullException>(() =>
		{
			_ = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(null!);
		});
	}

	[Fact]
	public void ExternalHandler_AttachesHotReload_DuringStartup()
	{
		var blazorWebView = new BlazorWebView();
		var handler = new FakeExternalBlazorWebViewHandler();
		handler.SetVirtualView(blazorWebView);

		var manager = handler.StartWebViewCore();

		Assert.NotNull(manager);
		Assert.Same(manager, handler.WebViewManager);
		Assert.Equal(MetadataUpdater.IsSupported, handler.HotReloadAttachTask is not null);
	}

	[Fact]
	public void TryGetUpdatedStaticContent_ReturnsFalse_ForUnknownContent()
	{
		var found = BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
			FakeExternalWebViewManager.ContentRootRelativeToAppRoot,
			RequestUri("css/app.css"),
			out var content,
			out var contentType);

		Assert.False(found);
		Assert.Null(content);
		Assert.Null(contentType);
	}

	[Fact]
	public void TryGetUpdatedStaticContent_ServesHotReloadScript_WhenSupported()
	{
		var found = BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
			FakeExternalWebViewManager.ContentRootRelativeToAppRoot,
			RequestUri(HotReloadScriptPath),
			out var content,
			out var contentType);

		if (!MetadataUpdater.IsSupported)
		{
			Assert.False(found);
			Assert.Null(content);
			return;
		}

		Assert.True(found);
		Assert.Equal("text/javascript", contentType);
		Assert.NotNull(content);

		// The caller owns the stream, so it must be readable and independently disposable.
		using (content!)
		{
			using var reader = new StreamReader(content, Encoding.UTF8);
			Assert.Contains("notifyCssUpdated", reader.ReadToEnd(), StringComparison.Ordinal);
		}
	}

	[Fact]
	public void TryGetUpdatedStaticContent_ReturnsAFreshStreamPerCall()
	{
		if (!MetadataUpdater.IsSupported)
		{
			return;
		}

		var uri = RequestUri(HotReloadScriptPath);

		Assert.True(BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
			FakeExternalWebViewManager.ContentRootRelativeToAppRoot, uri, out var first, out _));
		Assert.True(BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
			FakeExternalWebViewManager.ContentRootRelativeToAppRoot, uri, out var second, out _));

		Assert.NotSame(first, second);

		// Disposing one must not affect the other; the caller owns each stream.
		first!.Dispose();
		Assert.NotEqual(0, second!.ReadByte());
		second.Dispose();
	}

	[Theory]
	[InlineData(null, "https://0.0.0.0/css/app.css")]
	[InlineData("wwwroot", null)]
	public void TryGetUpdatedStaticContent_Throws_ForNullArguments(string? contentRoot, string? requestUri)
	{
		Assert.Throws<ArgumentNullException>(() =>
			BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
				contentRoot!, requestUri!, out _, out _));
	}
}
