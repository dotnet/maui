using System;
using System.ComponentModel;
using AppKit;
using Foundation;
using System.Maui.Internals;
using System.Threading.Tasks;

using WebKit;

namespace System.Maui.Platform.MacOS
{
	public class WebViewRenderer : ViewRenderer<WebView, WebKit.WebView>, IWebViewDelegate, WebKit.IWebPolicyDelegate
	{
		bool _disposed;
		bool _ignoreSourceChanges;
		bool _sentNavigating;
		WebNavigationEvent _lastBackForwardEvent;
		WebNavigationEvent _lastEvent;

		void IWebViewDelegate.LoadHtml(string html, string baseUrl)
		{
			if (html != null)
				Control.MainFrame.LoadHtmlString(html,
					baseUrl == null ? new NSUrl(NSBundle.MainBundle.BundlePath, true) : new NSUrl(baseUrl, true));
		}

		void IWebViewDelegate.LoadUrl(string url)
		{
			Control.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl(url)));
		}

		[Export("webView:decidePolicyForNavigationAction:request:frame:decisionListener:")]
		public void DecidePolicyForNavigation(WebKit.WebView webView, NSDictionary actionInformation, NSUrlRequest request, WebKit.WebFrame frame, WebKit.IWebPolicyDecisionListener decisionToken)
		{
			var navEvent = WebNavigationEvent.NewPage;
			if(actionInformation.ContainsKey(WebPolicyDelegate.WebActionNavigationTypeKey))
			{
				var navigationType = ((WebNavigationType)((NSNumber)actionInformation[WebPolicyDelegate.WebActionNavigationTypeKey]).Int32Value);
				switch (navigationType)
				{
					case WebNavigationType.BackForward:
						navEvent = _lastBackForwardEvent;
						break;
					case WebNavigationType.Reload:
						navEvent = WebNavigationEvent.Refresh;
						break;
					case WebNavigationType.FormResubmitted:
					case WebNavigationType.FormSubmitted:
					case WebNavigationType.LinkClicked:
					case WebNavigationType.Other:
						navEvent = WebNavigationEvent.NewPage;
						break;
				}
			}
			
			if (!_sentNavigating)
			{
				_lastEvent = navEvent;
				_sentNavigating = true;
				var lastUrl = request.Url.ToString();
				var args = new WebNavigatingEventArgs(navEvent, new UrlWebViewSource { Url = lastUrl }, lastUrl);

				Element.SendNavigating(args);
				UpdateCanGoBackForward();
				if (!args.Cancel)
				{
					decisionToken.Use();
				}
				
				_sentNavigating = false;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new WebKit.WebView
					{
						AutoresizingMask = NSViewResizingMask.WidthSizable,
						AutoresizesSubviews = true
					});
				
					Element.EvalRequested += OnEvalRequested;
					Element.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
					Element.GoBackRequested += OnGoBackRequested;
					Element.GoForwardRequested += OnGoForwardRequested;
					Element.ReloadRequested += OnReloadRequested;

					Control.FrameLoadDelegate = new FormsWebFrameDelegate(this);
					Control.PolicyDelegate = this;
					Layer.BackgroundColor = NSColor.Clear.CGColor;
				}
			}

			Load();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == WebView.SourceProperty.PropertyName)
				Load();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				Element.EvalRequested -= OnEvalRequested;
				Element.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
				Element.GoBackRequested -= OnGoBackRequested;
				Element.GoForwardRequested -= OnGoForwardRequested;
				Element.ReloadRequested -= OnReloadRequested;

				if (Control?.FrameLoadDelegate is FormsWebFrameDelegate frameDelegate)
					frameDelegate.Renderer = null;

				Control.FrameLoadDelegate = null;
				Control.PolicyDelegate = null;
			}
			base.Dispose(disposing);
		}

		void Load()
		{
			if (_ignoreSourceChanges)
				return;

			Element?.Source?.Load(this);

			UpdateCanGoBackForward();
		}

		void UpdateCanGoBackForward()
		{
			if (Element == null)
				return;
			((IWebViewController)Element).CanGoBack = Control.CanGoBack();
			((IWebViewController)Element).CanGoForward = Control.CanGoForward();
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			Control?.StringByEvaluatingJavaScriptFromString(eventArg?.Script);
		}

		async Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			var tcr = new TaskCompletionSource<string>();
			var task = tcr.Task;

			Device.BeginInvokeOnMainThread(() => {
				tcr.SetResult(Control?.StringByEvaluatingJavaScriptFromString(script));
			});

			return await task.ConfigureAwait(false);
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoBack())
			{
				_lastBackForwardEvent = WebNavigationEvent.Back;
				Control.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoForward())
			{
				_lastBackForwardEvent = WebNavigationEvent.Forward;
				Control.GoForward();
			}

			UpdateCanGoBackForward();
		}

		void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			Control.Reload(this);
		}

		internal class FormsWebFrameDelegate : WebKit.WebFrameLoadDelegate
		{
			internal WebViewRenderer Renderer { private get; set; }
			internal FormsWebFrameDelegate(WebViewRenderer renderer)
			{
				Renderer = renderer;
			}

			public override void FinishedLoad(WebKit.WebView sender, WebFrame forFrame)
			{
				Renderer._sentNavigating = false;
				
				if (Renderer.Control.IsLoading)
					return;

				if (Renderer.Control.MainFrameUrl == $"file://{NSBundle.MainBundle.BundlePath}/")
					return;

				Renderer._ignoreSourceChanges = true;
				Renderer.Element?.SetValueFromRenderer(WebView.SourceProperty, new UrlWebViewSource { Url = Renderer.Control.MainFrameUrl });
				Renderer._ignoreSourceChanges = false;

				Renderer._lastEvent = Renderer._lastBackForwardEvent;
				Renderer.Element?.SendNavigated(new WebNavigatedEventArgs(Renderer._lastEvent, Renderer.Element?.Source, Renderer.Control.MainFrameUrl, WebNavigationResult.Success));

				Renderer.UpdateCanGoBackForward();
			}

			public override void FailedLoadWithError(WebKit.WebView sender, NSError error, WebFrame forFrame)
			{
				Renderer._sentNavigating = false;
				
				Renderer._lastEvent = Renderer._lastBackForwardEvent;

				Renderer.Element?.SendNavigated(new WebNavigatedEventArgs(Renderer._lastEvent, new UrlWebViewSource { Url = Renderer.Control.MainFrameUrl }, Renderer.Control.MainFrameUrl, WebNavigationResult.Failure));

				Renderer.UpdateCanGoBackForward();
			}
		}
	}
}
