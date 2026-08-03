namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36801, "iOS ScrollToAsync clamps without AdjustedContentInset", PlatformAffected.iOS)]
public class Issue36801 : ContentPage
{
	readonly ScrollView _scrollView;
	readonly Label _endResultLabel;
	readonly Label _topResultLabel;

	public Issue36801()
	{
		NavigationPage.SetHasNavigationBar(this, false);
		SafeAreaEdges = SafeAreaEdges.None;

		_endResultLabel = new Label { Text = "EndPending", AutomationId = "EndResultLabel" };
		_topResultLabel = new Label { Text = "TopPending", AutomationId = "TopResultLabel" };

		var scrollToEndButton = new Button { Text = "Scroll to end", AutomationId = "ScrollToEndButton" };
		scrollToEndButton.Clicked += async (sender, e) =>
		{
			_endResultLabel.Text = "EndPending";
			await _scrollView.ScrollToAsync(0, _scrollView.ContentSize.Height, animated: false);
			await Task.Delay(100);
			_endResultLabel.Text = EvaluateOffset(expectEnd: true);
		};

		var scrollToTopButton = new Button { Text = "Scroll to top", AutomationId = "ScrollToTopButton" };
		scrollToTopButton.Clicked += async (sender, e) =>
		{
			_topResultLabel.Text = "TopPending";
			await _scrollView.ScrollToAsync(0, 0, animated: false);
			await Task.Delay(100);
			_topResultLabel.Text = EvaluateOffset(expectEnd: false);
		};

		var content = new VerticalStackLayout { Padding = 16, Spacing = 6 };

		// Spacer so the scrollable content starts below the floating header
		content.Add(new BoxView { HeightRequest = 220, Color = Colors.Transparent });
		for (int i = 0; i < 60; i++)
		{
			content.Add(new Label { Text = $"Filler {i}", HeightRequest = 30 });
		}

		content.Add(new Label
		{
			Text = "BOTTOM PROBE",
			AutomationId = "ProbeLabel",
			FontAttributes = FontAttributes.Bold
		});

		_scrollView = new ScrollView { Content = content };

		// Floating header keeps the buttons and result labels visible and tappable
		// regardless of the scroll position.
		var header = new VerticalStackLayout
		{
			Padding = new Thickness(16, 60, 16, 8),
			Spacing = 6,
			BackgroundColor = Colors.LightGray,
			VerticalOptions = LayoutOptions.Start,
			Children = { scrollToEndButton, scrollToTopButton, _endResultLabel, _topResultLabel }
		};

		Content = new Grid { Children = { _scrollView, header } };

#if IOS
		// Give the native scroll view explicit content insets so it has a non-zero
		// AdjustedContentInset regardless of how the host positions the page relative
		// to the system chrome. This mirrors the issue's "custom chrome" scenario
		// (AdditionalSafeAreaInsets) and deterministically exposes the clamping math:
		// the valid native offset range becomes [-60, ContentSize + 40 - Bounds.Height].
		_scrollView.Loaded += (sender, e) =>
		{
			if (_scrollView.Handler?.PlatformView is UIKit.UIScrollView nativeScrollView)
			{
				nativeScrollView.ContentInset = new UIKit.UIEdgeInsets(60, 0, 40, 0);
			}
		};
#endif
	}

	string EvaluateOffset(bool expectEnd)
	{
#if IOS
		if (_scrollView.Handler?.PlatformView is not UIKit.UIScrollView nativeScrollView)
		{
			return "Fail: native scroll view unavailable";
		}

		var adjustedInset = nativeScrollView.AdjustedContentInset;
		if (adjustedInset.Top + adjustedInset.Bottom <= 0)
		{
			// Without insets the buggy and correct math coincide and the test proves nothing
			var frameInWindow = nativeScrollView.ConvertRectToView(nativeScrollView.Bounds, null);
			var screen = UIKit.UIScreen.MainScreen.Bounds;
			var safeArea = nativeScrollView.SafeAreaInsets;
			return $"Fail: scenario invalid, no adjusted content insets " +
				$"(behavior={nativeScrollView.ContentInsetAdjustmentBehavior}, safeArea=({safeArea.Top:F0},{safeArea.Bottom:F0}), " +
				$"frame={frameInWindow.Y:F0}x{frameInWindow.Height:F0}, screen={screen.Height:F0}, " +
				$"contentInset=({nativeScrollView.ContentInset.Top:F0},{nativeScrollView.ContentInset.Bottom:F0}))";
		}

		double expected = expectEnd
			? nativeScrollView.ContentSize.Height + adjustedInset.Bottom - nativeScrollView.Bounds.Height
			: -adjustedInset.Top;
		double actual = nativeScrollView.ContentOffset.Y;
		var kind = expectEnd ? "end" : "top";

		return Math.Abs(actual - expected) <= 1.5
			? $"Success ({kind}): offset={actual:F1}"
			: $"Fail ({kind}): actual={actual:F1} expected={expected:F1}";
#else
		return "Success: not applicable on this platform";
#endif
	}
}
