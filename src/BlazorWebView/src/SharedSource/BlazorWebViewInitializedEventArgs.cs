using System;
using System.Threading.Tasks;
#if WEBVIEW2_WINFORMS
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;
#elif WEBVIEW2_WPF
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.Wpf.WebView2;
#elif WINDOWS && WEBVIEW2_MAUI
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;
#elif ANDROID
using AWebView = Android.Webkit.WebView;
#elif IOS || MACCATALYST
using WebKit;
#endif

namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Allows configuring the underlying web view after it has been initialized.
	/// </summary>
	public partial class BlazorWebViewInitializedEventArgs : EventArgs
	{
#nullable disable
#if WINDOWS
		/// <summary>
		/// Gets or sets a function that will be invoked once the web view has been initialized with
		/// the default values to allow further configuring additional options.
		/// </summary>
		public WebView2Control WebView { get; internal set; }
#elif ANDROID
		/// <summary>
		/// Gets or sets a function that will be invoked once the web view has been initialized with
		/// the default values to allow further configuring additional options.
		/// </summary>
		public AWebView WebView { get; internal set; }
#elif MACCATALYST || IOS
		/// <summary>
		/// Gets or sets a function that will be invoked once the web view has been initialized with
		/// the default values to allow further configuring additional options.
		/// </summary>
		public WKWebView WebView { get; internal set; }
#endif
	}
}
