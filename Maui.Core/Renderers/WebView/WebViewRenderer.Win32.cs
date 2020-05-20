using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace System.Maui.Platform
{
	public partial class WebViewRenderer : AbstractViewRenderer<IWebView, WebBrowser>, IWebViewDelegate
	{
		WebNavigationEvent _eventState;
		bool _updating;

		protected override WebBrowser CreateView()
		{
			var webBrowser = new WebBrowser();

			webBrowser.Navigated += WebBrowserOnNavigated;
			webBrowser.Navigating += WebBrowserOnNavigating;

			VirtualView.EvalRequested += OnEvalRequested;
			VirtualView.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
			VirtualView.GoBackRequested += OnGoBackRequested;
			VirtualView.GoForwardRequested += OnGoForwardRequested;
			VirtualView.ReloadRequested += OnReloadRequested;

			return webBrowser;
		}

		protected override void DisposeView(WebBrowser webBrowser)
		{
			webBrowser.Navigated -= WebBrowserOnNavigated;
			webBrowser.Navigating -= WebBrowserOnNavigating;

			VirtualView.EvalRequested -= OnEvalRequested;
			VirtualView.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
			VirtualView.GoBackRequested -= OnGoBackRequested;
			VirtualView.GoForwardRequested -= OnGoForwardRequested;
			VirtualView.ReloadRequested -= OnReloadRequested;

			base.DisposeView(webBrowser);
		}

		public static void MapPropertySource(IViewRenderer renderer, IWebView webView)
		{
			(renderer as WebViewRenderer)?.Load();
		}

		public void LoadHtml(string html, string baseUrl)
		{
			if (html == null)
				return;

			TypedNativeView.NavigateToString(html);
		}

		public void LoadUrl(string url)
		{
			if (url == null)
				return;

			TypedNativeView.Source = new Uri(url, UriKind.RelativeOrAbsolute);
		}

		protected internal void UpdateCanGoBackForward()
		{
			VirtualView.CanGoBack = TypedNativeView.CanGoBack;
			VirtualView.CanGoForward = TypedNativeView.CanGoForward;
		}

		void Load()
		{
			if (_updating)
				return;

			if (VirtualView.Source != null)
				VirtualView.Source.Load(this);

			UpdateCanGoBackForward();
		}

		void WebBrowserOnNavigated(object sender, NavigationEventArgs navigationEventArgs)
		{
			if (navigationEventArgs.Uri == null)
				return;

			string url = navigationEventArgs.Uri.IsAbsoluteUri ? navigationEventArgs.Uri.AbsoluteUri : navigationEventArgs.Uri.OriginalString;
			SendNavigated(new UrlWebViewSource { Url = url }, _eventState, WebNavigationResult.Success);
			UpdateCanGoBackForward();
		}

		void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs navigatingEventArgs)
		{
			if (navigatingEventArgs.Uri == null)
				return;

			string url = navigatingEventArgs.Uri.IsAbsoluteUri ? navigatingEventArgs.Uri.AbsoluteUri : navigatingEventArgs.Uri.OriginalString;
			var args = new WebNavigatingEventArgs(_eventState, new UrlWebViewSource { Url = url }, url);

			VirtualView.Navigating(args);

			navigatingEventArgs.Cancel = args.Cancel;

			// Reset in this case because this is the last event we will get
			if (args.Cancel)
				_eventState = WebNavigationEvent.NewPage;
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			TypedNativeView.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => TypedNativeView.InvokeScript("eval", eventArg.Script)));
		}

		async Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			var tcr = new TaskCompletionSource<string>();

			var task = tcr.Task;

			tcr.SetResult((string)TypedNativeView.InvokeScript("eval", new[] { script }));

			return await task.ConfigureAwait(false);
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (TypedNativeView.CanGoBack)
			{
				_eventState = WebNavigationEvent.Back;
				TypedNativeView.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (TypedNativeView.CanGoForward)
			{
				_eventState = WebNavigationEvent.Forward;
				TypedNativeView.GoForward();
			}
			UpdateCanGoBackForward();
		}

		void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			TypedNativeView.Refresh();
		}

		void SendNavigated(UrlWebViewSource source, WebNavigationEvent evnt, WebNavigationResult result)
		{
			_updating = true;
			VirtualView.Source = source;
			_updating = false;

			VirtualView.Navigated(new WebNavigatedEventArgs(evnt, source, source.Url, result));

			UpdateCanGoBackForward();
			_eventState = WebNavigationEvent.NewPage;
		}
	}
}