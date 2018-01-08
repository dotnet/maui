using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using TChromium = Tizen.WebView.Chromium;
using TWebView = Tizen.WebView.WebView;

namespace Xamarin.Forms.Platform.Tizen
{
	public class WebViewRenderer : VisualElementRenderer<WebView>, IWebViewDelegate
	{
		bool _updating;
		WebNavigationEvent _eventState;
		TWebView _control = null;

		IWebViewController ElementController => Element;

		public void LoadHtml(string html, string baseUrl)
		{
			_control.LoadHtml(html, baseUrl);
		}

		public void LoadUrl(string url)
		{
			if (!string.IsNullOrEmpty(url))
			{
				_control.LoadUrl(url);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_control != null)
				{
					_control.StopLoading();
					_control.LoadStarted -= OnLoadStarted;
					_control.LoadFinished -= OnLoadFinished;
					_control.LoadError -= OnLoadError;
				}

				if (Element != null)
				{
					Element.EvalRequested -= OnEvalRequested;
					Element.GoBackRequested -= OnGoBackRequested;
					Element.GoForwardRequested -= OnGoForwardRequested;
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			if (_control == null)
			{
				TChromium.Initialize();
				Forms.Context.Terminated += (sender, arg) => TChromium.Shutdown();
				_control = new TWebView(Forms.NativeParent);
				_control.LoadStarted += OnLoadStarted;
				_control.LoadFinished += OnLoadFinished;
				_control.LoadError += OnLoadError;
				SetNativeView(_control);
			}

			if (e.OldElement != null)
			{
				e.OldElement.EvalRequested -= OnEvalRequested;
				e.OldElement.GoBackRequested -= OnGoBackRequested;
				e.OldElement.GoForwardRequested -= OnGoForwardRequested;
			}

			if (e.NewElement != null)
			{
				e.NewElement.EvalRequested += OnEvalRequested;
				e.NewElement.GoForwardRequested += OnGoForwardRequested;
				e.NewElement.GoBackRequested += OnGoBackRequested;
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
			string url = _control.Url;
			if (!string.IsNullOrEmpty(url))
			{
				var args = new WebNavigatingEventArgs(_eventState, new UrlWebViewSource { Url = url }, url);
				Element.SendNavigating(args);

				if (args.Cancel)
				{
					_eventState = WebNavigationEvent.NewPage;
				}
			}
		}

		void OnLoadFinished(object sender, EventArgs e)
		{
			string url = _control.Url;
			if (!string.IsNullOrEmpty(url))
				SendNavigated(new UrlWebViewSource { Url = url }, _eventState, WebNavigationResult.Success);

			_control.SetFocus(true);
			UpdateCanGoBackForward();
		}

		void Load()
		{
			if (_updating)
				return;

			if (Element.Source != null)
			{
				Element.Source.Load(this);
			}

			UpdateCanGoBackForward();
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			_control.Eval(eventArg.Script);
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (_control.CanGoBack())
			{
				_eventState = WebNavigationEvent.Back;
				_control.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (_control.CanGoForward())
			{
				_eventState = WebNavigationEvent.Forward;
				_control.GoForward();
			}

			UpdateCanGoBackForward();
		}

		void SendNavigated(UrlWebViewSource source, WebNavigationEvent evnt, WebNavigationResult result)
		{
			_updating = true;
			((IElementController)Element).SetValueFromRenderer(WebView.SourceProperty, source);
			_updating = false;

			Element.SendNavigated(new WebNavigatedEventArgs(evnt, source, source.Url, result));

			UpdateCanGoBackForward();
			_eventState = WebNavigationEvent.NewPage;
		}

		void UpdateCanGoBackForward()
		{
			ElementController.CanGoBack = _control.CanGoBack();
			ElementController.CanGoForward = _control.CanGoForward();
		}
	}
}