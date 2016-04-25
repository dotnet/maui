using System;
using System.ComponentModel;
using Android.Webkit;
using Android.Widget;
using Xamarin.Forms.Internals;
using AWebView = Android.Webkit.WebView;

namespace Xamarin.Forms.Platform.Android
{
	public class WebViewRenderer : ViewRenderer<WebView, AWebView>, IWebViewDelegate
	{
		bool _ignoreSourceChanges;
		FormsWebChromeClient _webChromeClient;

		public WebViewRenderer()
		{
			AutoPackage = false;
		}

		public void LoadHtml(string html, string baseUrl)
		{
			Control.LoadDataWithBaseURL(baseUrl == null ? "file:///android_asset/" : baseUrl, html, "text/html", "UTF-8", null);
		}

		public void LoadUrl(string url)
		{
			Control.LoadUrl(url);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					if (Control != null)
						Control.StopLoading();
					Element.EvalRequested -= OnEvalRequested;
					Element.GoBackRequested -= OnGoBackRequested;
					Element.GoForwardRequested -= OnGoForwardRequested;

					_webChromeClient?.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		protected virtual FormsWebChromeClient GetFormsWebChromeClient()
		{
			return new FormsWebChromeClient();
		}

		protected override Size MinimumSize()
		{
			return new Size(Context.ToPixels(40), Context.ToPixels(40));
		}

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				var webView = new AWebView(Context);
#pragma warning disable 618 // This can probably be replaced with LinearLayout(LayoutParams.MatchParent, LayoutParams.MatchParent); just need to test that theory
				webView.LayoutParameters = new global::Android.Widget.AbsoluteLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 0, 0);
#pragma warning restore 618
				webView.SetWebViewClient(new WebClient(this));

				_webChromeClient = GetFormsWebChromeClient();
				_webChromeClient.SetContext(Context as IStartActivityForResult);
				webView.SetWebChromeClient(_webChromeClient);

				webView.Settings.JavaScriptEnabled = true;
				webView.Settings.DomStorageEnabled = true;
				SetNativeControl(webView);
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
				e.NewElement.GoBackRequested += OnGoBackRequested;
				e.NewElement.GoForwardRequested += OnGoForwardRequested;
			}

			Load();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			switch (e.PropertyName)
			{
				case "Source":
					Load();
					break;
			}
		}

		void Load()
		{
			if (_ignoreSourceChanges)
				return;

			if (Element.Source != null)
				Element.Source.Load(this);

			UpdateCanGoBackForward();
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			LoadUrl("javascript:" + eventArg.Script);
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoBack())
				Control.GoBack();

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoForward())
				Control.GoForward();

			UpdateCanGoBackForward();
		}

		void UpdateCanGoBackForward()
		{
			if (Element == null || Control == null)
				return;
			Element.CanGoBack = Control.CanGoBack();
			Element.CanGoForward = Control.CanGoForward();
		}

		class WebClient : WebViewClient
		{
			WebNavigationResult _navigationResult = WebNavigationResult.Success;
			WebViewRenderer _renderer;

			public WebClient(WebViewRenderer renderer)
			{
				if (renderer == null)
					throw new ArgumentNullException("renderer");
				_renderer = renderer;
			}

			public override void OnPageFinished(AWebView view, string url)
			{
				if (_renderer.Element == null || url == "file:///android_asset/")
					return;

				var source = new UrlWebViewSource { Url = url };
				_renderer._ignoreSourceChanges = true;
				((IElementController)_renderer.Element).SetValueFromRenderer(WebView.SourceProperty, source);
				_renderer._ignoreSourceChanges = false;

				var args = new WebNavigatedEventArgs(WebNavigationEvent.NewPage, source, url, _navigationResult);

				_renderer.Element.SendNavigated(args);

				_renderer.UpdateCanGoBackForward();

				base.OnPageFinished(view, url);
			}

			[Obsolete("This method was deprecated in API level 23.")]
			public override void OnReceivedError(AWebView view, ClientError errorCode, string description, string failingUrl)
			{
				_navigationResult = WebNavigationResult.Failure;
				if (errorCode == ClientError.Timeout)
					_navigationResult = WebNavigationResult.Timeout;
#pragma warning disable 618
				base.OnReceivedError(view, errorCode, description, failingUrl);
#pragma warning restore 618
			}

			public override void OnReceivedError(AWebView view, IWebResourceRequest request, WebResourceError error)
			{
				_navigationResult = WebNavigationResult.Failure;
				if (error.ErrorCode == ClientError.Timeout)
					_navigationResult = WebNavigationResult.Timeout;
				base.OnReceivedError(view, request, error);
			}

			public override bool ShouldOverrideUrlLoading(AWebView view, string url)
			{
				if (_renderer.Element == null)
					return true;

				var args = new WebNavigatingEventArgs(WebNavigationEvent.NewPage, new UrlWebViewSource { Url = url }, url);

				_renderer.Element.SendNavigating(args);
				_navigationResult = WebNavigationResult.Success;

				_renderer.UpdateCanGoBackForward();
				return args.Cancel;
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
					_renderer = null;
			}
		}
	}
}