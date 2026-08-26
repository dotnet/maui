using System;
using System.Threading.Tasks;
#if WEBVIEW2_WINFORMS
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;
#elif WEBVIEW2_WPF
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.Wpf.IWebView2;
#elif WINDOWS && WEBVIEW2_MAUI
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;
#elif ANDROID
using AWebView = Android.Webkit.WebView;
#elif IOS || MACCATALYST
using WebKit;
#elif TIZEN
using TWebView = Tizen.NUI.BaseComponents.WebView;
#endif

namespace Microsoft.AspNetCore.Components.WebView
{
	/// <summary>
	/// Allows configuring the underlying web view after it has been initialized.
	/// </summary>
	public class BlazorWebViewInitializedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the platform-native web view control that was initialized, as an untyped object.
		/// </summary>
		/// <remarks>
		/// This is the platform-neutral form of the strongly typed <c>WebView</c> property, which is only
		/// declared for target frameworks that MAUI itself has a built-in BlazorWebView backend for. Handlers
		/// implementing <c>IBlazorWebViewHandler</c> for a platform without a built-in backend should set this
		/// property before raising <c>IBlazorWebView.BlazorWebViewInitialized</c>, so that application code has
		/// a supported way to reach the native control.
		/// <para>
		/// On target frameworks where the strongly typed <c>WebView</c> property exists, both properties are
		/// backed by the same value: setting <c>WebView</c> sets this property, and this property is what
		/// <c>WebView</c> returns. If the stored value is not of the platform's web view type, <c>WebView</c>
		/// returns <see langword="null"/> rather than throwing.
		/// </para>
		/// </remarks>
		public object? NativeWebView { get; set; }

#nullable disable
#if WINDOWS
		/// <summary>
		/// Gets the <see cref="WebView2Control"/> instance that was initialized.
		/// </summary>
		public WebView2Control WebView
		{
			get => NativeWebView as WebView2Control;
			internal set => NativeWebView = value;
		}
#elif ANDROID
		/// <summary>
		/// Gets the <see cref="AWebView"/> instance that was initialized.
		/// </summary>
		public AWebView WebView
		{
			get => NativeWebView as AWebView;
			internal set => NativeWebView = value;
		}
#elif MACCATALYST || IOS
		/// <summary>
		/// Gets the <see cref="WKWebView"/> instance that was initialized.
		/// the default values to allow further configuring additional options.
		/// </summary>
		public WKWebView WebView
		{
			get => NativeWebView as WKWebView;
			internal set => NativeWebView = value;
		}
#elif TIZEN
		/// <summary>
		/// Gets the <see cref="TWebView"/> instance that was initialized.
		/// </summary>
		public TWebView WebView
		{
			get => NativeWebView as TWebView;
			internal set => NativeWebView = value;
		}
#endif
	}
}
