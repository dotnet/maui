using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Maui.MauiBlazorWebView.ExternalHandler.UnitTests;

/// <summary>
/// Stands in for the <see cref="WebViewManager"/> an out-of-repo BlazorWebView backend would write.
/// It only uses public API from <c>Microsoft.AspNetCore.Components.WebView</c>.
/// </summary>
internal sealed class FakeExternalWebViewManager : WebViewManager
{
	public static readonly Uri AppBaseUri = new("https://0.0.0.0/");

	public const string ContentRootRelativeToAppRoot = "wwwroot";

	public FakeExternalWebViewManager(
		IServiceProvider services,
		IFileProvider fileProvider,
		JSComponentConfigurationStore jsComponents)
		: base(services, Dispatcher.CreateDefault(), AppBaseUri, fileProvider, jsComponents, "index.html")
	{
	}

	public List<Uri> Navigations { get; } = new();

	public List<string> SentMessages { get; } = new();

	protected override void NavigateCore(Uri absoluteUri) => Navigations.Add(absoluteUri);

	protected override void SendMessage(string message) => SentMessages.Add(message);
}
