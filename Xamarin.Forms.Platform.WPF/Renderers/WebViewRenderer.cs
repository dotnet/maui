using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	public class WebViewRenderer : ViewRenderer<WebView, WebBrowser>, IWebViewDelegate
	{
		WebNavigationEvent _eventState;
		bool _updating;
		
		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			if (e.OldElement != null) // Clear old element event
			{
				e.OldElement.EvalRequested -= OnEvalRequested;
				e.OldElement.GoBackRequested -= OnGoBackRequested;
				e.OldElement.GoForwardRequested -= OnGoForwardRequested;
			}

			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new WebBrowser());
					Control.Navigated += WebBrowserOnNavigated;
					Control.Navigating += WebBrowserOnNavigating;
				}

				// Update control property 
				Load();

				// Suscribe element event
				Element.EvalRequested += OnEvalRequested;
				Element.GoBackRequested += OnGoBackRequested;
				Element.GoForwardRequested += OnGoForwardRequested;
			}

			base.OnElementChanged(e);
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

		public async void LoadHtml(string html, string baseUrl)
		{
			if (html == null)
				return;

			string fileName = string.Format("formslocal_{0}.html", DateTime.Now.Ticks);

			await SaveToIsoStore(fileName, html);
			Control.Navigate(new Uri(fileName, UriKind.Relative));
		}

		public void LoadUrl(string url)
		{
			if (url == null)
				return;

			Control.Source = new Uri(url, UriKind.RelativeOrAbsolute);
		}


		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			Control.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Control.InvokeScript("eval", eventArg.Script)));
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

		async Task SaveToIsoStore(string fileName, string html)
		{
			IIsolatedStorageFile store = Device.PlatformServices.GetUserStoreForApplication();
			using (var file = await store.OpenFileAsync(fileName, FileMode.CreateNew, FileAccess.Write).ConfigureAwait(false))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(html);
				await file.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
			}
		}

		void SendNavigated(UrlWebViewSource source, WebNavigationEvent evnt, WebNavigationResult result)
		{
			Console.WriteLine("SendNavigated : " + source.Url);
			_updating = true;
			((IElementController)Element).SetValueFromRenderer(WebView.SourceProperty, source);
			_updating = false;

			Element.SendNavigated(new WebNavigatedEventArgs(evnt, source, source.Url, result));

			UpdateCanGoBackForward();
			_eventState = WebNavigationEvent.NewPage;
		}
		
		void UpdateCanGoBackForward()
		{
			((IWebViewController)Element).CanGoBack = Control.CanGoBack;
			((IWebViewController)Element).CanGoForward = Control.CanGoForward;
		}

		void WebBrowserOnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs navigationEventArgs)
		{
			Console.WriteLine("WebBrowserOnNavigated");

			string url = navigationEventArgs.Uri.IsAbsoluteUri ? navigationEventArgs.Uri.AbsoluteUri : navigationEventArgs.Uri.OriginalString;
		  	SendNavigated(new UrlWebViewSource { Url = url }, _eventState, WebNavigationResult.Success);

			UpdateCanGoBackForward();
		}

		void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs navigatingEventArgs)
		{
			string url = navigatingEventArgs.Uri.IsAbsoluteUri ? navigatingEventArgs.Uri.AbsoluteUri : navigatingEventArgs.Uri.OriginalString;
			var args = new WebNavigatingEventArgs(_eventState, new UrlWebViewSource { Url = url }, url);

			Element.SendNavigating(args);

			navigatingEventArgs.Cancel = args.Cancel;

			// reset in this case because this is the last event we will get
			if (args.Cancel)
				_eventState = WebNavigationEvent.NewPage;
		}

		void WebBrowserOnNavigationFailed(object sender, NavigationFailedEventArgs navigationFailedEventArgs)
		{
			string url = navigationFailedEventArgs.Uri.IsAbsoluteUri ? navigationFailedEventArgs.Uri.AbsoluteUri : navigationFailedEventArgs.Uri.OriginalString;
			SendNavigated(new UrlWebViewSource { Url = url }, _eventState, WebNavigationResult.Failure);
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.Navigated -= WebBrowserOnNavigated;
					Control.Navigating -= WebBrowserOnNavigating;
					Control.Source = null;
					Control.Dispose();
				}

				if (Element != null)
				{
					Element.EvalRequested -= OnEvalRequested;
					Element.GoBackRequested -= OnGoBackRequested;
					Element.GoForwardRequested -= OnGoForwardRequested;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}
