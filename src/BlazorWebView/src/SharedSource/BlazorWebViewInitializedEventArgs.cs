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
		private object? _platformWebView;

		/// <summary>
		/// Initializes a new instance of <see cref="BlazorWebViewInitializedEventArgs"/>.
		/// </summary>
		public BlazorWebViewInitializedEventArgs()
		{
		}

#if WEBVIEW2_MAUI
		/// <summary>
		/// Initializes a new instance of <see cref="BlazorWebViewInitializedEventArgs"/> for the specified
		/// platform-native web view control.
		/// </summary>
		/// <param name="platformWebView">The platform-native web view control that was initialized.</param>
		/// <remarks>
		/// This is how a handler implementing <c>IBlazorWebViewHandler</c> for a platform without a built-in
		/// MAUI backend surfaces its native control to application code, on target frameworks where the
		/// strongly typed <c>WebView</c> property is not declared.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="platformWebView"/> is <see langword="null"/>.</exception>
		public BlazorWebViewInitializedEventArgs(object platformWebView)
		{
			ArgumentNullException.ThrowIfNull(platformWebView);

			_platformWebView = platformWebView;
		}

		/// <summary>
		/// Gets the platform-native web view control that was initialized, as an untyped object, or
		/// <see langword="null"/> when the handler did not supply one.
		/// </summary>
		/// <remarks>
		/// This is the platform-neutral form of the strongly typed <c>WebView</c> property, which is only
		/// declared for target frameworks that MAUI itself has a built-in BlazorWebView backend for. On those
		/// target frameworks both properties report the same instance; where the stored value is not of the
		/// platform's web view type, <c>WebView</c> reports <see langword="null"/> rather than throwing.
		/// <para>
		/// The value is write-once and can only be supplied by the handler that raises the event — either
		/// through <see cref="BlazorWebViewInitializedEventArgs(object)"/> or, for the built-in handlers,
		/// through the strongly typed <c>WebView</c> property. Event subscribers cannot change what later
		/// subscribers observe.
		/// </para>
		/// </remarks>
		public object? PlatformWebView => _platformWebView;
#endif

#nullable disable
#if WINDOWS
		/// <summary>
		/// Gets the <see cref="WebView2Control"/> instance that was initialized.
		/// </summary>
		public WebView2Control WebView
		{
			get => _platformWebView as WebView2Control;
			internal set => SetPlatformWebView(value);
		}
#elif ANDROID
		/// <summary>
		/// Gets the <see cref="AWebView"/> instance that was initialized.
		/// </summary>
		public AWebView WebView
		{
			get => _platformWebView as AWebView;
			internal set => SetPlatformWebView(value);
		}
#elif MACCATALYST || IOS
		/// <summary>
		/// Gets the <see cref="WKWebView"/> instance that was initialized.
		/// the default values to allow further configuring additional options.
		/// </summary>
		public WKWebView WebView
		{
			get => _platformWebView as WKWebView;
			internal set => SetPlatformWebView(value);
		}
#elif TIZEN
		/// <summary>
		/// Gets the <see cref="TWebView"/> instance that was initialized.
		/// </summary>
		public TWebView WebView
		{
			get => _platformWebView as TWebView;
			internal set => SetPlatformWebView(value);
		}
#endif
#nullable restore

#if WINDOWS || ANDROID || MACCATALYST || IOS || TIZEN
		private void SetPlatformWebView(object? value)
		{
			if (_platformWebView is not null)
			{
				throw new InvalidOperationException(
					$"The platform web view for this {nameof(BlazorWebViewInitializedEventArgs)} has already been set.");
			}

			_platformWebView = value;
		}
#endif
	}
}
