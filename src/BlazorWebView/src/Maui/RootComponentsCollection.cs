using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components.Web;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public class RootComponentsCollection : ObservableCollection<RootComponent>, IJSComponentConfiguration
	{
		private readonly BlazorWebView _blazorWebView;

		public RootComponentsCollection(BlazorWebView blazorWebView)
		{
			_blazorWebView = blazorWebView;
		}

		public JSComponentConfigurationStore JSComponents => _blazorWebView.WebViewManager?.JSComponentConfiguration!;
	}
}
