using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler
	{
		public static PropertyMapper<IBlazorWebView, BlazorWebViewHandler> WebViewMapper = new PropertyMapper<IBlazorWebView, BlazorWebViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IBlazorWebView.HostPage)] = MapHostPage,
			[nameof(IBlazorWebView.RootComponents)] = MapRootComponents,
			[nameof(IBlazorWebView.Services)] = MapServices,
		};

		public BlazorWebViewHandler() : base(WebViewMapper)
		{
		}

		public BlazorWebViewHandler(PropertyMapper mapper) : base(mapper ?? WebViewMapper)
		{
		}
	}
}
