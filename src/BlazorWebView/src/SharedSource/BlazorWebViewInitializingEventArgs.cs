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
#endif

namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Allows configuring the underlying web view when the application is initializing.
	/// </summary>
	public partial class BlazorWebViewInitializingEventArgs : EventArgs
	{
#if WINDOWS
#nullable disable
		/// <summary>
		/// Gets or sets the browser executable folder path for the <see cref="WebView2Control"/>.
		/// </summary>
		public string BrowserExecutableFolder { get; set; }

		/// <summary>
		/// Gets or sets the user data folder path for the <see cref="WebView2Control"/>.
		/// </summary>
		public string UserDataFolder { get; set; }

		/// <summary>
		/// Gets or sets the environment options for the <see cref="WebView2Control"/>.
		/// </summary>
		public CoreWebView2EnvironmentOptions EnvironmentOptions { get; set; }

		/// <summary>
		/// Gets or sets a function that will be invoked once the WebView has been initialized with
		/// the default values to allow further configuring additional options.
		/// </summary>
		public Func<WebView2Control, Task> OnWebViewInitialized { get; set; }
#endif
	}
}
