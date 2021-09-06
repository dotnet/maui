using Microsoft.AspNetCore.Components.Web;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public class BlazorWebView : Microsoft.Maui.Controls.View, IBlazorWebView
	{
		private readonly JSComponentConfigurationStore _jSComponents = new();

		public BlazorWebView()
		{
			RootComponents = new RootComponentsCollection(_jSComponents);
		}

		JSComponentConfigurationStore IBlazorWebView.JSComponents => _jSComponents;

		public string? HostPage { get; set; }

		public RootComponentsCollection RootComponents { get; }
	}
}
