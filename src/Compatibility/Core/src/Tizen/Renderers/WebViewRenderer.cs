using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using TChromium = Tizen.WebView.Chromium;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class WebViewRenderer : ViewRenderer<WebView, WebViewContainer>, IWebViewDelegate
	{
		bool _isUpdating;
		WebNavigationEvent _eventState;

		TWebView NativeWebView => Control.WebView;

		IWebViewController ElementController => Element;

		void IWebViewDelegate.LoadHtml(string html, string baseUrl)
		{
			NativeWebView.LoadHtml(html, baseUrl);
		}

		void IWebViewDelegate.LoadUrl(string url)
		{
			if (!string.IsNullOrEmpty(url))
			{
				NativeWebView.LoadUrl(url);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					NativeWebView.StopLoading();
					NativeWebView.LoadStarted -= OnLoadStarted;
					NativeWebView.LoadFinished -= OnLoadFinished;
					NativeWebView.LoadError -= OnLoadError;
				}

				if (Element != null)
				{
					((IWebViewController)Element).EvalRequested -= OnEvalRequested;
					((IWebViewController)Element).EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
					((IWebViewController)Element).GoBackRequested -= OnGoBackRequested;
					((IWebViewController)Element).GoForwardRequested -= OnGoForwardRequested;
					((IWebViewController)Element).ReloadRequested -= OnReloadRequested;
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			if (Control == null)
			{
				TChromium.Initialize();
				Forms.Context.Terminated += (sender, arg) => TChromium.Shutdown();
				SetNativeControl(new WebViewContainer(Forms.NativeParent));
				NativeWebView.LoadStarted += OnLoadStarted;
				NativeWebView.LoadFinished += OnLoadFinished;
				NativeWebView.LoadError += OnLoadError;
			}

			if (e.OldElement != null)
			{
				((IWebViewController)e.OldElement).EvalRequested -= OnEvalRequested;
				((IWebViewController)e.OldElement).GoBackRequested -= OnGoBackRequested;
				((IWebViewController)e.OldElement).GoForwardRequested -= OnGoForwardRequested;
				((IWebViewController)e.OldElement).ReloadRequested -= OnReloadRequested;
			}

			if (e.NewElement != null)
			{
				((IWebViewController)e.NewElement).EvalRequested += OnEvalRequested;
				((IWebViewController)e.NewElement).EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
				((IWebViewController)e.NewElement).GoForwardRequested += OnGoForwardRequested;
				((IWebViewController)e.NewElement).GoBackRequested += OnGoBackRequested;
				((IWebViewController)e.NewElement).ReloadRequested += OnReloadRequested;
				Load();
			}
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == WebView.SourceProperty.PropertyName)
				Load();

			base.OnElementPropertyChanged(sender, e);
		}

		void OnLoadError(object sender, global::Tizen.WebView.SmartCallbackLoadErrorArgs e)
		{
			string url = e.Url;
			if (!string.IsNullOrEmpty(url))
				SendNavigated(new UrlWebViewSource { Url = url }, _eventState, WebNavigationResult.Failure);
		}

		void OnLoadStarted(object sender, EventArgs e)
		{
			string url = NativeWebView.Url;
			if (!string.IsNullOrEmpty(url))
			{
				var args = new WebNavigatingEventArgs(_eventState, new UrlWebViewSource { Url = url }, url);
				ElementController.SendNavigating(args);

				if (args.Cancel)
				{
					_eventState = WebNavigationEvent.NewPage;
				}
			}
		}

		void OnLoadFinished(object sender, EventArgs e)
		{
			string url = NativeWebView.Url;
			if (!string.IsNullOrEmpty(url))
				SendNavigated(new UrlWebViewSource { Url = url }, _eventState, WebNavigationResult.Success);

			NativeWebView.SetFocus(true);
			UpdateCanGoBackForward();
		}

		void Load()
		{
			if (_isUpdating)
				return;

			if (Element.Source != null)
			{
				Element.Source.Load(this);
			}

			UpdateCanGoBackForward();
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			NativeWebView.Eval(eventArg.Script);
		}

		Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			NativeWebView.Eval(script);
			return null;
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (NativeWebView.CanGoBack())
			{
				_eventState = WebNavigationEvent.Back;
				NativeWebView.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (NativeWebView.CanGoForward())
			{
				_eventState = WebNavigationEvent.Forward;
				NativeWebView.GoForward();
			}

			UpdateCanGoBackForward();
		}

		void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			NativeWebView.Reload();
		}

		void SendNavigated(UrlWebViewSource source, WebNavigationEvent evnt, WebNavigationResult result)
		{
			_isUpdating = true;
			ElementController.SetValueFromRenderer(WebView.SourceProperty, source);
			_isUpdating = false;

			ElementController.SendNavigated(new WebNavigatedEventArgs(evnt, source, source.Url, result));

			UpdateCanGoBackForward();
			_eventState = WebNavigationEvent.NewPage;
		}

		[PortHandler]
		void UpdateCanGoBackForward()
		{
			ElementController.CanGoBack = NativeWebView.CanGoBack();
			ElementController.CanGoForward = NativeWebView.CanGoForward();
		}
	}
}