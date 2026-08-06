namespace Maui.Controls.Sample.Issues;

// Covers the deferred *element-mode* request: ScrollToAsync(element, End) issued before the
// handler exists. The target is resolved against the ScrollView's own geometry, so converting
// it at handler-attach — before the first layout, when Width/Height are still -1 — produces a
// target computed from invalid geometry. It has to wait for layout.
[Issue(IssueTracker.None, 0, "Deferred element-mode ScrollToAsync", PlatformAffected.iOS)]
public class Issue36801DeferredElement : ContentPage
{
	readonly ScrollView _scrollView;
	readonly Label _probeLabel;
	readonly Label _resultLabel;

	public Issue36801DeferredElement()
	{
		NavigationPage.SetHasNavigationBar(this, false);

		_resultLabel = new Label { Text = "Pending", AutomationId = "ResultLabel" };

		var content = new VerticalStackLayout { Padding = 16, Spacing = 6 };
		content.Add(new BoxView { HeightRequest = 220, Color = Colors.Transparent });
		for (int i = 0; i < 60; i++)
		{
			content.Add(new Label { Text = $"Filler {i}", HeightRequest = 30 });
		}

		_probeLabel = new Label
		{
			Text = "BOTTOM PROBE",
			AutomationId = "ProbeLabel",
			FontAttributes = FontAttributes.Bold
		};
		content.Add(_probeLabel);

		_scrollView = new ScrollView { Content = content, SafeAreaEdges = SafeAreaEdges.Default };

		var header = new VerticalStackLayout
		{
			Padding = new Thickness(16, 60, 16, 8),
			BackgroundColor = Colors.LightGray,
			VerticalOptions = LayoutOptions.Start,
			Children = { _resultLabel }
		};

		Content = new Grid { Children = { _scrollView, header } };

#if IOS || MACCATALYST
		_scrollView.HandlerChanged += (sender, e) =>
		{
			if (_scrollView.Handler?.PlatformView is UIKit.UIScrollView nativeScrollView)
			{
				nativeScrollView.ContentInset = new UIKit.UIEdgeInsets(60, 0, 40, 0);
			}
		};
#endif

		// Issued before any handler or layout exists
		RunDeferredElementScroll();
	}

	async void RunDeferredElementScroll()
	{
		await _scrollView.ScrollToAsync(_probeLabel, ScrollToPosition.End, animated: false);

		for (int attempt = 0; attempt < 20; attempt++)
		{
			var result = Evaluate();
			_resultLabel.Text = result;
			if (result.StartsWith("Success", StringComparison.Ordinal))
			{
				return;
			}

			await Task.Delay(250);
		}
	}

	string Evaluate()
	{
#if IOS || MACCATALYST
		if (_scrollView.Handler?.PlatformView is not UIKit.UIScrollView nativeScrollView ||
			_probeLabel.Handler?.PlatformView is not UIKit.UIView probeView)
		{
			return "Fail: platform views unavailable";
		}

		var adjustedInset = nativeScrollView.AdjustedContentInset;
		if (adjustedInset.Top + adjustedInset.Bottom <= 0)
		{
			return "Fail: scenario invalid, no adjusted content insets";
		}

		// Same geometric oracle as the non-deferred element test: the probe's bottom edge must
		// rest exactly at the bottom of the unobscured viewport
		var probeInWindow = probeView.ConvertRectToView(probeView.Bounds, null);
		var scrollInWindow = nativeScrollView.ConvertRectToView(nativeScrollView.Bounds, null);
		double visibleBottom = (double)(scrollInWindow.Bottom - adjustedInset.Bottom);
		double actual = (double)probeInWindow.Bottom;

		return Math.Abs(actual - visibleBottom) <= 1.5
			? $"Success: bottom={actual:F1}"
			: $"Fail: actual={actual:F1} expected={visibleBottom:F1}";
#else
		return "Skipped: not applicable on this platform";
#endif
	}
}
