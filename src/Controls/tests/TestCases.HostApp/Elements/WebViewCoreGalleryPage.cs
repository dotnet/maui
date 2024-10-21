namespace Maui.Controls.Sample;

internal class WebViewCoreGalleryPage : CoreGalleryPage<WebView>
{
	protected override bool SupportsFocus => false;

	protected override void InitializeElement(WebView element)
	{
		element.HeightRequest = 200;

		element.Source = new UrlWebViewSource { Url = "https://github.com/dotnet/maui" };
	}

	protected override void Build()
	{
		base.Build();
	}
}
