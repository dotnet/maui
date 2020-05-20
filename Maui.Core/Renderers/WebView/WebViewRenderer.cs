namespace System.Maui.Platform
{
	public partial class WebViewRenderer
	{
		public static PropertyMapper<IWebView> WebViewMapper = new PropertyMapper<IWebView>(ViewRenderer.ViewMapper)
		{
			[nameof(IWebView.Source)] = MapPropertySource,
			Actions = {
				["GoBack"] = MapGoBack
			}
		};

		public WebViewRenderer() : base(WebViewMapper)
		{

		}

		public WebViewRenderer(PropertyMapper mapper) : base(mapper ?? WebViewMapper)
		{

		}
		public static void MapGoBack(IViewRenderer renderer, IWebView webView) { }
	}
}