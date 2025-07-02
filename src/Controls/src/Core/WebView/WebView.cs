#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

using Microsoft.Maui.Devices;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="Type[@FullName='Microsoft.Maui.Controls.WebView']/Docs/*" />
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class WebView : View, IWebViewController, IElementConfiguration<WebView>, IWebView
	{
		/// <summary>Bindable property for <see cref="Source"/>.</summary>
		public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(WebViewSource), typeof(WebView), default(WebViewSource),
			propertyChanging: (bindable, oldvalue, newvalue) =>
			{
				var source = oldvalue as WebViewSource;
				if (source != null)
					source.SourceChanged -= ((WebView)bindable).OnSourceChanged;
			}, propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var source = newvalue as WebViewSource;
				var webview = (WebView)bindable;
				if (source != null)
				{
					source.SourceChanged += webview.OnSourceChanged;
					SetInheritedBindingContext(source, webview.BindingContext);
				}
			});

		static readonly BindablePropertyKey CanGoBackPropertyKey = BindableProperty.CreateReadOnly(nameof(CanGoBack), typeof(bool), typeof(WebView), false);

		/// <summary>Bindable property for <see cref="CanGoBack"/>.</summary>
		public static readonly BindableProperty CanGoBackProperty = CanGoBackPropertyKey.BindableProperty;

		static readonly BindablePropertyKey CanGoForwardPropertyKey = BindableProperty.CreateReadOnly(nameof(CanGoForward), typeof(bool), typeof(WebView), false);

		/// <summary>Bindable property for <see cref="CanGoForward"/>.</summary>
		public static readonly BindableProperty CanGoForwardProperty = CanGoForwardPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="UserAgent"/>.</summary>
		public static readonly BindableProperty UserAgentProperty = BindableProperty.Create(nameof(UserAgent), typeof(string), typeof(WebView), null);

		/// <summary>Bindable property for <see cref="Cookies"/>.</summary>
		public static readonly BindableProperty CookiesProperty = BindableProperty.Create(nameof(Cookies), typeof(CookieContainer), typeof(WebView), null);

		readonly Lazy<PlatformConfigurationRegistry<WebView>> _platformConfigurationRegistry;

		bool _canGoBack;
		bool _canGoForward;

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public WebView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<WebView>>(() => new PlatformConfigurationRegistry<WebView>(this));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool IWebViewController.CanGoBack
		{
			get { return CanGoBack; }
			set { SetValue(CanGoBackPropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='CanGoBack']/Docs/*" />
		public bool CanGoBack
		{
			get { return (bool)GetValue(CanGoBackProperty); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool IWebViewController.CanGoForward
		{
			get { return CanGoForward; }
			set { SetValue(CanGoForwardPropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='CanGoForward']/Docs/*" />
		public bool CanGoForward
		{
			get { return (bool)GetValue(CanGoForwardProperty); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='UserAgent']/Docs/*" />
		public string UserAgent
		{
			get { return (string)GetValue(UserAgentProperty); }
			set { SetValue(UserAgentProperty, value ?? UserAgent); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='Cookies']/Docs/*" />
		public CookieContainer Cookies
		{
			get { return (CookieContainer)GetValue(CookiesProperty); }
			set { SetValue(CookiesProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='Source']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(WebViewSourceTypeConverter))]
		public WebViewSource Source
		{
			get { return (WebViewSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='Eval']/Docs/*" />
		public void Eval(string script)
		{
			Handler?.Invoke(nameof(IWebView.Eval), script);
			_evalRequested?.Invoke(this, new EvalRequested(script));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='EvaluateJavaScriptAsync']/Docs/*" />
		public async Task<string> EvaluateJavaScriptAsync(string script)
		{
			if (script == null)
				return null;

			// Make all the platforms mimic Android's implementation, which is by far the most complete.
			if (DeviceInfo.Platform != DevicePlatform.Android)
			{
				script = WebViewHelper.EscapeJsString(script);

				if (DeviceInfo.Platform != DevicePlatform.WinUI)
				{
					// Use JSON.stringify() method to converts a JavaScript value to a JSON string
					script = "try{JSON.stringify(eval('" + script + "'))}catch(e){'null'};";
				}
				else
					script = "try{eval('" + script + "')}catch(e){'null'};";
			}

			string result;

			if (_evaluateJavaScriptRequested != null) // With Handlers we don't use events, if is null we are using a renderer and a handler otherwise.
			{
				// This is the WebViewRenderer subscribing to these requests; the handler stuff
				// doesn't use them.
				result = await _evaluateJavaScriptRequested?.Invoke(script);
			}
			else
			{
				// Use the handler command to evaluate the JS
				result = await Handler.InvokeAsync(nameof(IWebView.EvaluateJavaScriptAsync),
					new EvaluateJavaScriptAsyncRequest(script));
			}

			//if the js function errored or returned null/undefined treat it as null
			if (result == "null")
				result = null;

			//JSON.stringify wraps the result in literal quotes, we just want the actual returned result
			//note that if the js function returns the string "null" we will get here and not above
			else if (result != null)
				result = result.Trim('"');

			return result;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='GoBack']/Docs/*" />
		public void GoBack()
		{
			Handler?.Invoke(nameof(IWebView.GoBack));
			_goBackRequested?.Invoke(this, EventArgs.Empty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='GoForward']/Docs/*" />
		public void GoForward()
		{
			Handler?.Invoke(nameof(IWebView.GoForward));
			_goForwardRequested?.Invoke(this, EventArgs.Empty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='Reload']/Docs/*" />
		public void Reload()
		{
			Handler?.Invoke(nameof(IWebView.Reload));
			_reloadRequested?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Raised after web navigation completes.
		/// </summary>
		public event EventHandler<WebNavigatedEventArgs> Navigated;

		/// <summary>
		/// Raised after web navigation begins.
		/// </summary>
		public event EventHandler<WebNavigatingEventArgs> Navigating;

		/// <summary>
		///  Raised when a WebView process ends unexpectedly.
		/// </summary>
		public event EventHandler<WebViewProcessTerminatedEventArgs> ProcessTerminated;

		/// <inheritdoc/>
		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			WebViewSource source = Source;
			if (source != null)
			{
				SetInheritedBindingContext(source, BindingContext);
			}
		}

		/// <inheritdoc/>
		protected override void OnPropertyChanged(string propertyName)
		{
			if (propertyName == "BindingContext")
			{
				WebViewSource source = Source;
				if (source != null)
					SetInheritedBindingContext(source, BindingContext);
			}

			base.OnPropertyChanged(propertyName);
		}

		protected void OnSourceChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(SourceProperty.PropertyName);
		}

		event EventHandler<EvalRequested> _evalRequested;

		/// <inheritdoc/>
		event EventHandler<EvalRequested> IWebViewController.EvalRequested
		{
			add { _evalRequested += value; }
			remove { _evalRequested -= value; }
		}

		event EvaluateJavaScriptDelegate _evaluateJavaScriptRequested;

		/// <inheritdoc/>
		event EvaluateJavaScriptDelegate IWebViewController.EvaluateJavaScriptRequested
		{
			add { _evaluateJavaScriptRequested += value; }
			remove { _evaluateJavaScriptRequested -= value; }
		}

		event EventHandler _goBackRequested;

		/// <inheritdoc/>
		event EventHandler IWebViewController.GoBackRequested
		{
			add { _goBackRequested += value; }
			remove { _goBackRequested -= value; }
		}

		event EventHandler _goForwardRequested;

		/// <inheritdoc/>
		event EventHandler IWebViewController.GoForwardRequested
		{
			add { _goForwardRequested += value; }
			remove { _goForwardRequested -= value; }
		}

		void IWebViewController.SendNavigated(WebNavigatedEventArgs args)
		{
			Navigated?.Invoke(this, args);
		}

		void IWebViewController.SendNavigating(WebNavigatingEventArgs args)
		{
			Navigating?.Invoke(this, args);
		}

		event EventHandler _reloadRequested;

		/// <inheritdoc/>
		event EventHandler IWebViewController.ReloadRequested
		{
			add { _reloadRequested += value; }
			remove { _reloadRequested -= value; }
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, WebView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		/// <inheritdoc/>
		IWebViewSource IWebView.Source => Source;

		/// <inheritdoc/>
		bool IWebView.CanGoBack
		{
			get => _canGoBack;
			set
			{
				_canGoBack = value;
				((IWebViewController)this).CanGoBack = _canGoBack;
				Handler?.UpdateValue(nameof(IWebView.CanGoBack));
			}
		}

		/// <inheritdoc/>
		bool IWebView.CanGoForward
		{
			get => _canGoForward;
			set
			{
				_canGoForward = value;
				((IWebViewController)this).CanGoForward = _canGoForward;
				Handler?.UpdateValue(nameof(IWebView.CanGoForward));
			}
		}

		/// <inheritdoc/>
		bool IWebView.Navigating(WebNavigationEvent evnt, string url)
		{
			var args = new WebNavigatingEventArgs(evnt, new UrlWebViewSource { Url = url }, url);
			(this as IWebViewController)?.SendNavigating(args);

			return args.Cancel;
		}

		/// <inheritdoc/>
		void IWebView.Navigated(WebNavigationEvent evnt, string url, WebNavigationResult result)
		{
			var args = new WebNavigatedEventArgs(evnt, new UrlWebViewSource { Url = url }, url, result);
			(this as IWebViewController)?.SendNavigated(args);
		}

		void IWebView.ProcessTerminated(WebProcessTerminatedEventArgs args)
		{
#if ANDROID
			var platformArgs = new PlatformWebViewProcessTerminatedEventArgs(args.Sender, args.RenderProcessGoneDetail);
			var webViewProcessTerminatedEventArgs = new WebViewProcessTerminatedEventArgs(platformArgs);
#elif IOS || MACCATALYST
			var platformArgs = new PlatformWebViewProcessTerminatedEventArgs(args.Sender);
			var webViewProcessTerminatedEventArgs = new WebViewProcessTerminatedEventArgs(platformArgs);
#elif WINDOWS
			var platformArgs = new PlatformWebViewProcessTerminatedEventArgs(args.Sender, args.CoreWebView2ProcessFailedEventArgs);
			var webViewProcessTerminatedEventArgs = new WebViewProcessTerminatedEventArgs(platformArgs);
#else
			var webViewProcessTerminatedEventArgs = new WebViewProcessTerminatedEventArgs();
#endif
			ProcessTerminated?.Invoke(this, webViewProcessTerminatedEventArgs);
		}

		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(Source), Source);
			return $"{base.GetDebuggerDisplay()}, {debugText}";
		}
	}
}