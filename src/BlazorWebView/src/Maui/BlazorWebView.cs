using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public class BlazorWebView : Microsoft.Maui.Controls.View, IBlazorWebView
	{
		internal static readonly string AppHostAddress = "0.0.0.0";

		private readonly JSComponentConfigurationStore _jSComponents = new();

		public BlazorWebView()
		{
			RootComponents = new RootComponentsCollection(_jSComponents);
		}

		JSComponentConfigurationStore IBlazorWebView.JSComponents => _jSComponents;

		public string? HostPage { get; set; }

		public RootComponentsCollection RootComponents { get; }

		/// <summary>
		/// Specify whether links should be opened in the external
		/// system default browser, or within the webview.
		/// </summary>
		/// <value>Defaults to opening links in the system default browser.</value>
		public ExternalLinkMode ExternalLinkMode { get; set; } = ExternalLinkMode.OpenInExternalBrowser;

		/// <inheritdoc/>
		public virtual IFileProvider CreateFileProvider(string contentRootDir)
		{
			// Call into the platform-specific code to get that platform's asset file provider
			return ((BlazorWebViewHandler)(Handler!)).CreateFileProvider(contentRootDir);
		}
	}
}
