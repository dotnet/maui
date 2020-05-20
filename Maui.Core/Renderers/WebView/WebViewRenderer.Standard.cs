namespace System.Maui.Platform
{
	public partial class WebViewRenderer : AbstractViewRenderer<IWebView, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public static void MapPropertySource(IViewRenderer renderer, IWebView webView) { }
	}
}