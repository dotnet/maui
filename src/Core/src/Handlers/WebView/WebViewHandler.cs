namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler
	{
		public static PropertyMapper<IWebView, WebViewHandler> WebViewMapper = new PropertyMapper<IWebView, WebViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IWebView.Source)] = MapSource
		};

		public WebViewHandler() : base(WebViewMapper)
		{

		}

		public WebViewHandler(PropertyMapper mapper) : base(mapper ?? WebViewMapper)
		{

		}
	}
}