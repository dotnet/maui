using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler
	{
		public static PropertyMapper<IBlazorWebView, BlazorWebViewHandler> BlazorWebViewMapper = new PropertyMapper<IBlazorWebView, BlazorWebViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IBlazorWebView.HostPage)] = MapHostPage,
			[nameof(IBlazorWebView.RootComponents)] = MapRootComponents,
			[nameof(IBlazorWebView.Services)] = MapServices,
		};

		public BlazorWebViewHandler() : base(BlazorWebViewMapper)
		{
		}

		public BlazorWebViewHandler(PropertyMapper mapper) : base(mapper ?? BlazorWebViewMapper)
		{
		}
	}
}
