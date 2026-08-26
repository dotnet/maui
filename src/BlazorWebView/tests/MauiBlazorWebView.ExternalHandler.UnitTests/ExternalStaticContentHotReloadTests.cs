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
			new JSComponentConfigurationStore());

	[Fact]
	public async Task AttachToWebViewManagerIfEnabled_MatchesHotReloadAvailability()
	{
		await using var manager = CreateManager();

		BlazorWebViewStaticContentHotReload.AttachToWebViewManagerIfEnabled(manager);

		if (MetadataUpdater.IsSupported)
		{
			// The notifier root component is registered under a fixed selector, so removing it succeeds.
			await manager.RemoveRootComponentAsync("body::after");
		}
		else
		{
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => manager.RemoveRootComponentAsync("body::after"));
		}
	}

	[Fact]
	public void AttachToWebViewManagerIfEnabled_Throws_WhenManagerIsNull()
	{
		Assert.Throws<ArgumentNullException>(
			() => BlazorWebViewStaticContentHotReload.AttachToWebViewManagerIfEnabled(null!));
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
	}

	[Fact]
	public void TryReplaceResponseContent_ReturnsFalse_ForUnknownContent()
	{
		var originalContent = new MemoryStream(Encoding.UTF8.GetBytes("original"));
		Stream content = originalContent;
		var statusCode = 200;
		var headers = new Dictionary<string, string>(StringComparer.Ordinal);

		var replaced = BlazorWebViewStaticContentHotReload.TryReplaceResponseContent(
			FakeExternalWebViewManager.ContentRootRelativeToAppRoot,
			new Uri(FakeExternalWebViewManager.AppBaseUri, "css/app.css").AbsoluteUri,
			ref statusCode,
			ref content,
			headers);

		Assert.False(replaced);
		Assert.Same(originalContent, content);
		Assert.Equal(200, statusCode);
		Assert.Empty(headers);
	}

	[Fact]
	public void TryReplaceResponseContent_ServesHotReloadScript_WhenSupported()
	{
		var originalContent = new MemoryStream(Encoding.UTF8.GetBytes("original"));
		Stream content = originalContent;
		var statusCode = 404;
		var headers = new Dictionary<string, string>(StringComparer.Ordinal);

		var replaced = BlazorWebViewStaticContentHotReload.TryReplaceResponseContent(
			FakeExternalWebViewManager.ContentRootRelativeToAppRoot,
			new Uri(FakeExternalWebViewManager.AppBaseUri, HotReloadScriptPath).AbsoluteUri,
			ref statusCode,
			ref content,
			headers);

		if (!MetadataUpdater.IsSupported)
		{
			Assert.False(replaced);
			return;
		}

		Assert.True(replaced);
		Assert.Equal(200, statusCode);
		Assert.NotSame(originalContent, content);
		Assert.Equal("text/javascript", headers["Content-Type"]);

		using var reader = new StreamReader(content);
		Assert.Contains("notifyCssUpdated", reader.ReadToEnd(), StringComparison.Ordinal);
	}

	[Theory]
	[InlineData(null, "https://0.0.0.0/css/app.css")]
	[InlineData("wwwroot", null)]
	public void TryReplaceResponseContent_Throws_ForNullStringArguments(string? contentRoot, string? requestUri)
	{
		var headers = new Dictionary<string, string>(StringComparer.Ordinal);

		Assert.Throws<ArgumentNullException>(() =>
		{
			Stream content = new MemoryStream();
			var statusCode = 200;
			BlazorWebViewStaticContentHotReload.TryReplaceResponseContent(
				contentRoot!,
				requestUri!,
				ref statusCode,
				ref content,
				headers);
		});
	}

	[Fact]
	public void TryReplaceResponseContent_Throws_WhenHeadersAreNull()
	{
		Assert.Throws<ArgumentNullException>(() =>
		{
			Stream content = new MemoryStream();
			var statusCode = 200;
			BlazorWebViewStaticContentHotReload.TryReplaceResponseContent(
				FakeExternalWebViewManager.ContentRootRelativeToAppRoot,
				new Uri(FakeExternalWebViewManager.AppBaseUri, "css/app.css").AbsoluteUri,
				ref statusCode,
				ref content,
				null!);
		});
	}

	[Fact]
	public void TryReplaceResponseContent_Throws_WhenContentIsNull()
	{
		Assert.Throws<ArgumentNullException>(() =>
		{
			Stream? content = null;
			var statusCode = 200;
			BlazorWebViewStaticContentHotReload.TryReplaceResponseContent(
				FakeExternalWebViewManager.ContentRootRelativeToAppRoot,
				new Uri(FakeExternalWebViewManager.AppBaseUri, "css/app.css").AbsoluteUri,
				ref statusCode,
				ref content!,
				new Dictionary<string, string>(StringComparer.Ordinal));
		});
	}
}
