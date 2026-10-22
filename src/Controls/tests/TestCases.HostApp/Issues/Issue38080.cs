namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38080, "Android WebView in ScrollView should survive overscroll and back navigation", PlatformAffected.Android)]
public class Issue38080 : NavigationPage
{
	public Issue38080() : base(new HomePage()) { }

	class HomePage : ContentPage
	{
		public HomePage()
		{
			Content = new Button
			{
				AutomationId = "OpenIssue38080",
				Text = "Open WebView ScrollView repro",
				Command = new Command(async () => await Navigation.PushAsync(new ReproPage()))
			};
		}
	}

	class ReproPage : ContentPage
	{
		readonly ScrollView _scrollView;
		readonly WebView _webView;
		readonly Label _topSentinel;
		readonly Label _bottomSentinel;
		readonly Label _status;
		bool _loaded;

		public ReproPage()
		{
			_status = new Label { AutomationId = "Issue38080Status", Text = "Status pending", FontSize = 12 };
			_webView = new WebView
			{
				AutomationId = "Issue38080WebView",
				HeightRequest = 220,
				HorizontalOptions = LayoutOptions.Fill,
				Source = new HtmlWebViewSource
				{
					Html = "<html><body style='margin:0;background:#06f;color:white;font:20px sans-serif;display:flex;align-items:center;justify-content:center;height:100vh'>Issue 38080 WebView HTML loaded</body></html>"
				}
			};
			_webView.Navigated += (_, e) =>
			{
				_loaded = e.Result == WebNavigationResult.Success;
				UpdateStatus();
			};

			var stack = new VerticalStackLayout { Padding = 12, Spacing = 12 };
			_topSentinel = AddRows(stack, "Before", top: true);
			stack.Add(_webView);
			_bottomSentinel = AddRows(stack, "After", top: false);
			_scrollView = new ScrollView { AutomationId = "Issue38080ScrollView", Content = stack };

			Content = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				},
				Children =
				{
					Header(),
					_scrollView
				}
			};
			Grid.SetRow(_scrollView, 1);
		}

		VerticalStackLayout Header() => new()
		{
			Padding = new Thickness(12, 8),
			Spacing = 4,
			Children =
			{
				new Label { AutomationId = "Issue38080Ready", Text = "Issue 38080 ready" },
				_status,
				new HorizontalStackLayout
				{
					Spacing = 6,
					Children =
					{
						new Button { AutomationId = "Issue38080Refresh", Text = "R", Command = new Command(UpdateStatus) },
						new Button { AutomationId = "Issue38080Center", Text = "C", Command = new Command(async () => await _scrollView.ScrollToAsync(_webView, ScrollToPosition.Center, false)) },
						new Button { AutomationId = "Issue38080Top", Text = "T", Command = new Command(async () => await _scrollView.ScrollToAsync(_topSentinel, ScrollToPosition.Start, false)) },
						new Button { AutomationId = "Issue38080Bottom", Text = "B", Command = new Command(async () => await _scrollView.ScrollToAsync(_bottomSentinel, ScrollToPosition.End, false)) }
					}
				}
			}
		};

		static Label AddRows(VerticalStackLayout stack, string prefix, bool top)
		{
			Label sentinel = null;
			for (int i = 1; i <= 50; i++)
			{
				var label = new Label
				{
					AutomationId = top && i == 1 ? "Issue38080TopSentinel" : !top && i == 50 ? "Issue38080BottomSentinel" : null,
					Text = $"{prefix} WebView row {i:00}",
					FontSize = 20
				};

				stack.Add(label);
				sentinel ??= label.AutomationId is not null ? label : null;
			}

			return sentinel!;
		}

		void UpdateStatus()
		{
#if ANDROID
			var native = _webView.Handler?.PlatformView as Android.Webkit.WebView;
			_status.Text = native is null
				? $"Loaded={_loaded}; Native=null"
				: $"Loaded={_loaded}; Native={native.Width}x{native.Height}; Attached={native.IsAttachedToWindow}; Hardware={native.IsHardwareAccelerated}";
#else
			_status.Text = $"Loaded={_loaded}";
#endif
		}
	}
}
