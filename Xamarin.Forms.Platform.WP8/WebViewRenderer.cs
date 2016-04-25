using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class WebViewRenderer : ViewRenderer<WebView, WebBrowser>, IWebViewDelegate
	{
		WebNavigationEvent _eventState;
		bool _updating;

		public async void LoadHtml(string html, string baseUrl)
		{
			string fileName = string.Format("formslocal_{0}.html", DateTime.Now.Ticks);
			
			await SaveToIsoStore(fileName, html);
			Control.Navigate(new Uri(fileName, UriKind.Relative));
		}

		public void LoadUrl(string url)
		{
			Control.Source = new Uri(url, UriKind.RelativeOrAbsolute);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				var webBrowser = new WebBrowser();
				webBrowser.IsScriptEnabled = true;
				webBrowser.Navigated += WebBrowserOnNavigated;
				webBrowser.Navigating += WebBrowserOnNavigating;
				webBrowser.NavigationFailed += WebBrowserOnNavigationFailed;
				SetNativeControl(webBrowser);
			}

			if (e.OldElement != null)
			{
				e.OldElement.EvalRequested -= OnEvalRequested;
				e.OldElement.GoBackRequested -= OnGoBackRequested;
				e.OldElement.GoForwardRequested -= OnGoForwardRequested;
				Control.DataContext = null;
			}

			if (e.NewElement != null)
			{
				e.NewElement.EvalRequested += OnEvalRequested;
				e.NewElement.GoBackRequested += OnGoBackRequested;
				e.NewElement.GoForwardRequested += OnGoForwardRequested;
				Control.DataContext = e.NewElement;
			}

			Load();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			switch (e.PropertyName)
			{
				case "Source":
					if (!_updating)
						Load();
					break;
			}
		}

		void Load()
		{
			if (Element.Source != null)
				Element.Source.Load(this);

			UpdateCanGoBackForward();
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			Control.Dispatcher.BeginInvoke(() => Control.InvokeScript("eval", eventArg.Script));
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
			using (Stream file = await store.OpenFileAsync(fileName, FileMode.CreateNew, FileAccess.Write).ConfigureAwait(false))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(html);
				await file.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
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

		void WebBrowserOnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs navigationEventArgs)
		{
			string url = navigationEventArgs.Uri.IsAbsoluteUri ? navigationEventArgs.Uri.AbsoluteUri : navigationEventArgs.Uri.OriginalString;
			SendNavigated(new UrlWebViewSource { Url = url }, _eventState, WebNavigationResult.Success);

			UpdateCanGoBackForward();
		}

		void WebBrowserOnNavigating(object sender, NavigatingEventArgs navigatingEventArgs)
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
	}
}