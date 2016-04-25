using System;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Internals;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class WebViewRenderer : ViewRenderer<WebView, Windows.UI.Xaml.Controls.WebView>, IWebViewDelegate
	{
		WebNavigationEvent _eventState;
		bool _updating;

		public void LoadHtml(string html, string baseUrl)
		{
			/*
			 * FIXME: If baseUrl is a file URL, set the Base property to its path.
			 * Otherwise, it doesn't seem as if WebBrowser can handle it.
			 */
			Control.NavigateToString(html);
		}

		public void LoadUrl(string url)
		{
			Control.Source = new Uri(url);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.NavigationStarting -= OnNavigationStarted;
					Control.NavigationCompleted -= OnNavigationCompleted;
					Control.NavigationFailed -= OnNavigationFailed;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				e.OldElement.EvalRequested -= OnEvalRequested;
				e.OldElement.GoBackRequested -= OnGoBackRequested;
				e.OldElement.GoForwardRequested -= OnGoForwardRequested;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var webView = new Windows.UI.Xaml.Controls.WebView();
					webView.NavigationStarting += OnNavigationStarted;
					webView.NavigationCompleted += OnNavigationCompleted;
					webView.NavigationFailed += OnNavigationFailed;
					SetNativeControl(webView);
				}

				e.NewElement.EvalRequested += OnEvalRequested;
				e.NewElement.GoForwardRequested += OnGoForwardRequested;
				e.NewElement.GoBackRequested += OnGoBackRequested;

				Load();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == WebView.SourceProperty.PropertyName)
			{
				if (!_updating)
					Load();
			}
		}

		void Load()
		{
			if (Element.Source != null)
				Element.Source.Load(this);

			UpdateCanGoBackForward();
		}

		async void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			await Control.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await Control.InvokeScriptAsync("eval", new[] { eventArg.Script }));
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoBack)
			{
				_eventState = WebNavigationEvent.Back;
				Control.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoForward)
			{
				_eventState = WebNavigationEvent.Forward;
				Control.GoForward();
			}

			UpdateCanGoBackForward();
		}

		void OnNavigationCompleted(Windows.UI.Xaml.Controls.WebView sender, WebViewNavigationCompletedEventArgs e)
		{
			if (e.Uri != null)
				SendNavigated(new UrlWebViewSource { Url = e.Uri.AbsoluteUri }, _eventState, WebNavigationResult.Success);

			UpdateCanGoBackForward();
		}

		void OnNavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
		{
			if (e.Uri != null)
				SendNavigated(new UrlWebViewSource { Url = e.Uri.AbsoluteUri }, _eventState, WebNavigationResult.Failure);
		}

		void OnNavigationStarted(Windows.UI.Xaml.Controls.WebView sender, WebViewNavigationStartingEventArgs e)
		{
			Uri uri = e.Uri;

			if (uri != null)
			{
				var args = new WebNavigatingEventArgs(_eventState, new UrlWebViewSource { Url = uri.AbsoluteUri }, uri.AbsoluteUri);

				Element.SendNavigating(args);
				e.Cancel = args.Cancel;

				// reset in this case because this is the last event we will get
				if (args.Cancel)
					_eventState = WebNavigationEvent.NewPage;
			}
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

		// Nasty hack because we cant bind this because OneWayToSource isn't a thing in WP8, yay
		void UpdateCanGoBackForward()
		{
			Element.CanGoBack = Control.CanGoBack;
			Element.CanGoForward = Control.CanGoForward;
		}
	}
}