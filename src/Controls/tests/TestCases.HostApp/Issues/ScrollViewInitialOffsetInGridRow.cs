namespace Maui.Controls.Sample.Issues;

// Regression reported against 10.0.101: a pushed page with a Grid (Auto,*,Auto) whose "*" row
// holds a ScrollView with tall content appears with the ScrollView not at the top, so the
// first item is partially covered. The content is a VerticalStackLayout with Padding 0,8
// whose first child is a Border with Margin 16,8.
[Issue(IssueTracker.None, 0, "ScrollView in a Grid star row is not at the top when the page appears", PlatformAffected.iOS)]
public class ScrollViewInitialOffsetInGridRow : NavigationPage
{
	public ScrollViewInitialOffsetInGridRow() : base(new StartPage())
	{
	}

	public class StartPage : ContentPage
	{
		public StartPage()
		{
			Title = "Root";
			// Four combinations. The header row being empty puts the ScrollView right under the
			// (translucent) navigation bar, so it gets a top safe-area inset — as it does under
			// a scaffold whose chrome augments the safe area. Async content is how a bound page
			// behaves: the ScrollView appears nearly empty and grows once already on screen,
			// which is when UIKit flips its adjusted inset on.
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Children =
				{
					MakePush("Static, header", "PushButton", populateAsync: false, header: true),
					MakePush("Async, header", "PushAsyncButton", populateAsync: true, header: true),
					MakePush("Static, no header", "PushNoHeaderButton", populateAsync: false, header: false),
					MakePush("Async, no header", "PushAsyncNoHeaderButton", populateAsync: true, header: false),
					// Content is tall and laid out from the start; only the footer grows later.
					// An ordinary resize of a laid-out scroll view does not scroll it (verified),
					// so this guards that the arrange-time offset restore never disturbs one.
					MakePush("Static, footer grows later", "PushStaticResizeButton", populateAsync: false, header: true, resizeLater: true),
				},
			};
		}

		Button MakePush(string text, string id, bool populateAsync, bool header, bool resizeLater = false)
		{
			var button = new Button { Text = text, AutomationId = id };
			button.Clicked += async (_, _) => await Navigation.PushAsync(new DetailPage(populateAsync, header, resizeLater));
			return button;
		}
	}

	public class DetailPage : ContentPage
	{
		readonly ScrollView _scrollView;
		readonly Label _resultLabel;
		readonly VerticalStackLayout _content;
		readonly bool _populateAsync;
		readonly bool _resizeLater;
		Grid _grid;
		BoxView _footerBanner;
#if IOS || MACCATALYST
		// Captures the managed stack at the exact moment the native offset first leaves the
		// rest position, so a failure names who moved it
		IDisposable _offsetObserver;
		string _offsetMoveStack;
#endif

		public DetailPage(bool populateAsync, bool header, bool resizeLater = false)
		{
			_populateAsync = populateAsync;
			_resizeLater = resizeLater;
			Title = "Detail";

			// Edge-to-edge, as a scaffold hosts its pages: no navigation bar, and the page and
			// its root layout decline the safe area, so the ScrollView reaches under the status
			// bar and is the element that consumes the top inset
			NavigationPage.SetHasNavigationBar(this, false);
			SafeAreaEdges = SafeAreaEdges.None;

			// An empty Auto row (a hidden banner, say) leaves the ScrollView at the very top
			var headerView = new Label
			{
				Text = "HEADER (Auto row)",
				AutomationId = "Header",
				HeightRequest = 44,
				BackgroundColor = Colors.LightGray,
				VerticalTextAlignment = TextAlignment.Center,
				IsVisible = header,
			};

			_content = new VerticalStackLayout { Padding = new Thickness(0, 8) };
			if (!populateAsync)
			{
				Populate();
			}

			_scrollView = new ScrollView { Content = _content, AutomationId = "TheScrollView" };
#if IOS || MACCATALYST
			_scrollView.HandlerChanged += (_, _) =>
			{
				_offsetObserver?.Dispose();
				_offsetObserver = null;
				if (_scrollView.Handler?.PlatformView is UIKit.UIScrollView native)
				{
					_offsetObserver = native.AddObserver("contentOffset", Foundation.NSKeyValueObservingOptions.New, _ =>
					{
						if (_offsetMoveStack is null && native.ContentOffset.Y + native.AdjustedContentInset.Top > 0.5)
						{
							_offsetMoveStack = FormattableString.Invariant(
								$"offset->{native.ContentOffset.Y:F1} bounds=({native.Bounds.X:F1},{native.Bounds.Y:F1},{native.Bounds.Height:F1}) contentSize={native.ContentSize.Height:F1}\n")
								+ Environment.StackTrace;
						}
					});
				}
			};
#endif

			_resultLabel = new Label
			{
				Text = "Pending",
				AutomationId = "ResultLabel",
				FontSize = 10,
				LineBreakMode = LineBreakMode.WordWrap,
				// Fixed: the diagnostics must not change the layout under test
				HeightRequest = 120,
			};

			// A bound footer (a summary strip, say) that arrives together with the content: in
			// the async case it grows in the same layout pass in which the ScrollView's content
			// first exceeds the viewport, so the star row shrinks while the content grows
			_footerBanner = new BoxView { Color = Colors.LightBlue, HeightRequest = populateAsync || resizeLater ? 0 : 36 };

			// No navigation bar, so the test returns through this instead of the back arrow
			var back = new Button { Text = "Back", AutomationId = "BackButton" };
			back.Clicked += async (_, _) => await Navigation.PopAsync();
			var footer = new VerticalStackLayout { Children = { _footerBanner, _resultLabel, back } };

			var grid = new Grid
			{
				AutomationId = "PageRoot",
				SafeAreaEdges = SafeAreaEdges.None,
				RowDefinitions =
				{
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star),
					new RowDefinition(GridLength.Auto),
				},
			};
			grid.Add(headerView, 0, 0);
			grid.Add(_scrollView, 0, 1);
			grid.Add(footer, 0, 2);
			_grid = grid;
			Content = grid;
		}

		void Populate()
		{
			var first = new Border
			{
				Margin = new Thickness(16, 8),
				Padding = 12,
				StrokeThickness = 2,
				Stroke = Colors.Red,
				AutomationId = "FirstItem",
				Content = new Label { Text = "FIRST ITEM", AutomationId = "FirstItemLabel" },
			};
			_content.Add(first);

			for (int i = 1; i < 60; i++)
			{
				_content.Add(new Border
				{
					Margin = new Thickness(16, 8),
					Padding = 12,
					StrokeThickness = 1,
					Stroke = Colors.Gray,
					Content = new Label { Text = $"Item {i}" },
				});
			}
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			if (_populateAsync)
			{
				// Let the page settle on screen with empty content first, as a bound page does;
				// then the data arrives: content and footer in the same tick
				await Task.Delay(400);
				Populate();
				_footerBanner.HeightRequest = 36;
			}
			else if (_resizeLater)
			{
				// Tall content already laid out and its ContentSize settled; only the
				// parent-driven resize happens now — the coincidence the defect needs is absent
				await Task.Delay(400);
				_footerBanner.HeightRequest = 36;
			}

			Evaluate();
		}

		// Re-sample until the layout settles, then report every native quantity that could
		// explain a non-zero rest offset, so the failure names its own cause.
		async void Evaluate()
		{
			for (int attempt = 0; attempt < 20; attempt++)
			{
				var text = Sample();
				_resultLabel.Text = text;
				if (text.StartsWith("Success", StringComparison.Ordinal))
				{
					return;
				}

				await Task.Delay(250);
			}
		}

		string Sample()
		{
#if IOS || MACCATALYST
			if (_scrollView.Handler?.PlatformView is not UIKit.UIScrollView native)
			{
				return "Fail: native scroll view unavailable";
			}

			var offset = native.ContentOffset;
			var adjusted = native.AdjustedContentInset;
			var inset = native.ContentInset;
			var safe = native.SafeAreaInsets;
			var contentView = native.Subviews.Length > 0 ? native.Subviews[0] : null;
			var contentFrame = contentView?.Frame ?? CoreGraphics.CGRect.Empty;

			// Judge only once the (tall) content has actually been laid out — before that the
			// scroll view trivially rests at the top. At rest the native offset is exactly
			// -AdjustedContentInset.Top and the cross-platform ScrollY is 0 (content coordinates).
			var contentLaidOut = native.ContentSize.Height > native.Bounds.Height + 1;
			var restsAtTop = contentLaidOut && Math.Abs(offset.Y + adjusted.Top) < 0.5 && Math.Abs(_scrollView.ScrollY) < 0.5;

			// Invariant formatting: the test parses these numbers regardless of device locale
			var diag = FormattableString.Invariant($"offset=({offset.X:F1},{offset.Y:F1}) adjusted=({adjusted.Top:F1},{adjusted.Left:F1},{adjusted.Bottom:F1},{adjusted.Right:F1}) ") +
				FormattableString.Invariant($"contentInset=({inset.Top:F1},{inset.Bottom:F1}) safe=({safe.Top:F1},{safe.Bottom:F1}) ") +
				FormattableString.Invariant($"contentSize={native.ContentSize.Height:F1} bounds={native.Bounds.Height:F1} contentFrameY={contentFrame.Y:F1} ") +
				FormattableString.Invariant($"behavior={native.ContentInsetAdjustmentBehavior} scrollY={_scrollView.ScrollY:F1} ") +
				// Who consumed the safe area: the scroll view's own frame inside the page, and the grid's
				FormattableString.Invariant($"svFrame=({native.Frame.Y:F1},{native.Frame.Height:F1}) gridFrame=({_grid.Y:F1},{_grid.Height:F1}) pageH={Height:F1}");

			if (!restsAtTop && _offsetMoveStack is not null)
			{
				// Keep the label readable: first frames only
				var frames = _offsetMoveStack.Split('\n');
				diag += " | moved by: " + string.Join(" <- ", System.Linq.Enumerable.Take(
					System.Linq.Enumerable.Select(frames, f => f.Trim().Replace("   at ", "", StringComparison.Ordinal)), 14));
			}

			return restsAtTop ? $"Success: {diag}" : $"Fail: {diag}";
#else
			return FormattableString.Invariant(Math.Abs(_scrollView.ScrollY) < 0.5 ? $"Success: scrollY={_scrollView.ScrollY:F1}" : $"Fail: scrollY={_scrollView.ScrollY:F1}");
#endif
		}
	}
}
