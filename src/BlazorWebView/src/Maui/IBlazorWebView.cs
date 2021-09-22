using Microsoft.AspNetCore.Components.Web;
using Microsoft.Maui;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public interface IBlazorWebView : IView
	{
		string? HostPage { get; set; }
		RootComponentsCollection RootComponents { get; }
		JSComponentConfigurationStore JSComponents { get; }
	}
}
