namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38080, "Android SIGSEGV crash in GLFunctorDrawable when a ScrollView with an off-screen WebView is overscrolled", PlatformAffected.Android)]
public class Issue38080 : NavigationPage
{
	public Issue38080() : base(new Issue38080HomePage()) { }

	class Issue38080HomePage : ContentPage
	{
		public Issue38080HomePage()
		{
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						AutomationId = "Issue38080NavigateButton",
						Text = "Open repro page",
						Command = new Command(async () => await Navigation.PushAsync(new Issue38080ReproPage()))
					}
				}
			};
		}
	}

	// Reproduces #38080: a single fixed-size WebView placed mid-list in a ScrollView is off-screen
	// at both scroll extremes. Flinging to either extreme (or back-navigating) previously SIGSEGV'd
	// the RenderThread in GLFunctorDrawable because the WebView carried a non-null ClipBounds that
	// routed its off-screen compositing through the AOSP hwui GL-functor path.
	class Issue38080ReproPage : ContentPage
	{
		const int FillerRows = 50;
		const int MaxDiagnosticRefreshAttempts = 50;

		public Issue38080ReproPage()
		{
			var stack = new VerticalStackLayout
			{
				Padding = new Thickness(12),
				Spacing = 12
			};

			stack.Children.Add(new Label
			{
				AutomationId = "Issue38080Ready",
				Text = "Page loaded — no crash"
			});

			var clipBoundsStatus = new Label
			{
				AutomationId = "Issue38080ClipBoundsStatus",
				Text = "ClipBounds=pending"
			};
			stack.Children.Add(clipBoundsStatus);

			var webViewLoadStatus = new Label
			{
				AutomationId = "Issue38080WebViewLoadStatus",
				Text = "WebView load pending"
			};

			// The first/last rows are sentinels the UITest asserts on to prove the scroll
			// actually reached the top/bottom extreme.
			AddFiller(stack, FillerRows, sentinelRow: 1, sentinelId: "Issue38080TopSentinel");

			stack.Children.Add(new Label
			{
				AutomationId = "Issue38080WebViewTopMarker",
				Text = "Issue38080 WebView starts below"
			});

			// Single fixed-size WebView mid-list, so it is off-screen at both extremes.
			var webView = new WebView
			{
				AutomationId = "Issue38080WebView",
				HeightRequest = 220,
				Source = new HtmlWebViewSource
				{
					Html = "<html><body style='background:#d6e4ff; margin:0; padding:24px'><h3 id='Issue38080HtmlProbe'>Issue38080 WebView HTML loaded</h3></body></html>"
				}
			};
			RegisterWebViewDiagnostics(webView, clipBoundsStatus, webViewLoadStatus);
			stack.Children.Add(webView);
			stack.Children.Add(webViewLoadStatus);

			AddFiller(stack, FillerRows, sentinelRow: FillerRows, sentinelId: "Issue38080BottomSentinel");

			Content = new ScrollView
			{
				AutomationId = "Issue38080ScrollView",
				Content = stack
			};
		}

		static void AddFiller(VerticalStackLayout stack, int rows, int sentinelRow, string sentinelId)
		{
			for (int i = 1; i <= rows; i++)
			{
				stack.Children.Add(new Label
				{
					AutomationId = i == sentinelRow ? sentinelId : null,
					Text = $"Filler row {i}",
					FontSize = 20
				});
			}
		}

		static void RegisterWebViewDiagnostics(WebView webView, Label diagnosticsLabel, Label loadStatusLabel)
		{
			webView.Navigated += async (_, args) =>
			{
				if (args.Result != WebNavigationResult.Success)
				{
					loadStatusLabel.Text = $"WebView load result: {args.Result}";
					return;
				}

				var htmlProbe = await webView.EvaluateJavaScriptAsync("document.getElementById('Issue38080HtmlProbe').textContent");
				loadStatusLabel.Text = $"WebView load result: {args.Result}; HtmlProbe={htmlProbe}";
			};

#if ANDROID
			webView.HandlerChanged += (_, _) => UpdateClipBoundsDiagnostics(webView, diagnosticsLabel, attempt: 0);
			webView.Loaded += (_, _) => UpdateClipBoundsDiagnostics(webView, diagnosticsLabel, attempt: 0);
			webView.SizeChanged += (_, _) => UpdateClipBoundsDiagnostics(webView, diagnosticsLabel, attempt: 0);
#endif
		}

#if ANDROID
		static void UpdateClipBoundsDiagnostics(WebView webView, Label diagnosticsLabel, int attempt)
		{
			var nativeWebView = webView.Handler?.PlatformView as Android.Webkit.WebView;
			var diagnosticsReady = false;

			if (nativeWebView is null)
			{
				diagnosticsLabel.Text = "ClipBounds=pending; NativeWebView=null";
			}
			else
			{
				using var clipBounds = nativeWebView.ClipBounds;
				var clipBoundsText = clipBounds is null
					? "null"
					: $"{clipBounds.Left},{clipBounds.Top},{clipBounds.Right},{clipBounds.Bottom}";
				var nativeParent = nativeWebView.Parent;
				var parentViewGroup = nativeParent as Android.Views.ViewGroup;
				var parentClipChildren = parentViewGroup is null
					? "null"
					: parentViewGroup.ClipChildren.ToString();
				var parentType = nativeParent?.GetType().Name ?? "null";
				var isAttachedToWindow = nativeWebView.IsAttachedToWindow;
				var isHardwareAccelerated = nativeWebView.IsHardwareAccelerated;

				diagnosticsReady =
					nativeWebView.Width > 0 &&
					nativeWebView.Height > 0 &&
					isAttachedToWindow &&
					isHardwareAccelerated;

				diagnosticsLabel.Text =
					$"ClipBounds={clipBoundsText}; " +
					$"Size={nativeWebView.Width}x{nativeWebView.Height}; " +
					$"IsAttachedToWindow={isAttachedToWindow}; " +
					$"HardwareAccelerated={isHardwareAccelerated}; " +
					$"Parent={parentType}; " +
					$"ParentClipChildren={parentClipChildren}";
			}

			if (!diagnosticsReady && attempt < MaxDiagnosticRefreshAttempts)
				webView.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () => UpdateClipBoundsDiagnostics(webView, diagnosticsLabel, attempt + 1));
			else if (!diagnosticsReady)
				diagnosticsLabel.Text += "; DiagnosticsTimeout=True";
		}
#endif
	}
}
