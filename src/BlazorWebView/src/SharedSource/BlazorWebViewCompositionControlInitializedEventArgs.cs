using System;
using WebView2Control = Microsoft.Web.WebView2.Wpf.WebView2CompositionControl;

namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Allows configuring the underlying web view after it has been initialized.
	/// </summary>
	public class BlazorWebViewCompositionControlInitializedEventArgs : EventArgs, IBlazorWebViewInitializedEventArgs<WebView2Control>
	{
#nullable disable
		/// <summary>
		/// Gets the <see cref="WebView2Control"/> instance that was initialized.
		/// </summary>
		public WebView2Control WebView { get; internal set; }
	}
}
