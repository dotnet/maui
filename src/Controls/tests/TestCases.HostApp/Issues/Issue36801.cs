namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36801, "iOS ScrollToAsync clamps without AdjustedContentInset", PlatformAffected.iOS)]
public class Issue36801 : ContentPage
{
	readonly ScrollView _scrollView;
	readonly VerticalStackLayout _content;
	readonly Label _probeLabel;
	readonly Label _endResultLabel;
	readonly Label _topResultLabel;
	readonly Label _elementResultLabel;
	readonly Label _deferredResultLabel;

	public Issue36801()
	{
		NavigationPage.SetHasNavigationBar(this, false);
		SafeAreaEdges = SafeAreaEdges.None;

		_endResultLabel = new Label { Text = "EndPending", AutomationId = "EndResultLabel" };
		_topResultLabel = new Label { Text = "TopPending", AutomationId = "TopResultLabel" };
		_elementResultLabel = new Label { Text = "ElementPending", AutomationId = "ElementResultLabel" };
		_deferredResultLabel = new Label { Text = "DeferredPending", AutomationId = "DeferredResultLabel" };

		var scrollToEndButton = new Button { Text = "Scroll to end", AutomationId = "ScrollToEndButton" };
		scrollToEndButton.Clicked += async (sender, e) =>
		{
			_endResultLabel.Text = "EndPending";
			await _scrollView.ScrollToAsync(0, _scrollView.ContentSize.Height, animated: false);
			await EvaluateUntilSuccess(_endResultLabel, () => EvaluateOffset(expectEnd: true, kind: "end"));
		};

		var scrollToTopButton = new Button { Text = "Scroll to top", AutomationId = "ScrollToTopButton" };
		scrollToTopButton.Clicked += async (sender, e) =>
		{
			_topResultLabel.Text = "TopPending";
			await _scrollView.ScrollToAsync(0, 0, animated: false);
			await EvaluateUntilSuccess(_topResultLabel, () => EvaluateOffset(expectEnd: false, kind: "top"));
		};

		var scrollToProbeButton = new Button { Text = "Scroll to probe (End)", AutomationId = "ScrollToProbeButton" };
		scrollToProbeButton.Clicked += async (sender, e) =>
		{
			_elementResultLabel.Text = "ElementPending";
			await _scrollView.ScrollToAsync(_probeLabel, ScrollToPosition.End, animated: false);
			await EvaluateUntilSuccess(_elementResultLabel, EvaluateElementEnd);
		};

		_content = new VerticalStackLayout { Padding = 16, Spacing = 6 };

		// Spacer so the scrollable content starts below the floating header
		_content.Add(new BoxView { HeightRequest = 220, Color = Colors.Transparent });
		for (int i = 0; i < 60; i++)
		{
			_content.Add(new Label { Text = $"Filler {i}", HeightRequest = 30 });
		}

		_probeLabel = new Label
		{
			Text = "BOTTOM PROBE",
			AutomationId = "ProbeLabel",
			FontAttributes = FontAttributes.Bold
		};
		_content.Add(_probeLabel);

		// Set the mode on the ScrollView itself: SafeAreaEdges does not propagate from the
		// page, and the ScrollView's own value is what selects ContentInsetAdjustmentBehavior.
		_scrollView = new ScrollView { Content = _content, SafeAreaEdges = SafeAreaEdges.Default };

		var modeDefaultButton = new Button { Text = "Mode Default", AutomationId = "ModeDefaultButton" };
		modeDefaultButton.Clicked += (sender, e) => SetMode(SafeAreaEdges.Default);

		var modeNoneButton = new Button { Text = "Mode None", AutomationId = "ModeNoneButton" };
		modeNoneButton.Clicked += (sender, e) => SetMode(SafeAreaEdges.None);

		// SafeAreaEdges.All also resolves to Never, but unlike None it makes MauiScrollView
		// bake the safe area into ContentSize — the case ScrollableContentSize reasons about
		var modeAllButton = new Button { Text = "Mode All", AutomationId = "ModeAllButton" };
		modeAllButton.Clicked += (sender, e) => SetMode(SafeAreaEdges.All);

		var modeContainerButton = new Button { Text = "Mode Container", AutomationId = "ModeContainerButton" };
		// SafeAreaEdges.Container is internal, so build the same value from the public enum.
		// SafeAreaEdges.All would not do: it is mapped to Never, not Always.
		modeContainerButton.Clicked += (sender, e) => SetMode(new SafeAreaEdges(SafeAreaRegions.Container));

		// Floating header keeps the buttons and result labels visible and tappable
		// regardless of the scroll position.
		var header = new VerticalStackLayout
		{
			Padding = new Thickness(16, 60, 16, 8),
			Spacing = 6,
			BackgroundColor = Colors.LightGray,
			VerticalOptions = LayoutOptions.Start,
			Children =
			{
				modeDefaultButton, modeNoneButton, modeAllButton, modeContainerButton,
				scrollToEndButton, scrollToTopButton, scrollToProbeButton,
				_endResultLabel, _topResultLabel, _elementResultLabel, _deferredResultLabel
			}
		};

		Content = new Grid { AutomationId = "PageRoot", Children = { _scrollView, header } };

#if IOS || MACCATALYST
		// Give the native scroll view explicit content insets so it has a non-zero
		// AdjustedContentInset regardless of how the host positions the page relative
		// to the system chrome. This mirrors the issue's "custom chrome" scenario
		// (AdditionalSafeAreaInsets) and deterministically exposes the clamping math:
		// the valid native offset range becomes [-60, ContentSize + 40 - Bounds.Height].
		// Applied from HandlerChanged rather than Loaded so the inset is in place before the
		// first layout pass, which is what drains the deferred scroll request — otherwise the
		// scroll could be clamped against a different inset than the one asserted against.
		_scrollView.HandlerChanged += (sender, e) =>
		{
			if (_scrollView.Handler?.PlatformView is UIKit.UIScrollView nativeScrollView)
			{
				nativeScrollView.ContentInset = new UIKit.UIEdgeInsets(60, 0, 40, 0);
			}
		};
#endif

		// Issue a request before any handler or layout exists so it travels through the
		// deferred PendingScrollToRequest drain in the first layout pass, where the adjusted
		// insets may still be stale (the #35395 OnAppearing scenario).
		RunDeferredScroll();
	}

	void SetMode(SafeAreaEdges edges)
	{
		_scrollView.SafeAreaEdges = edges;

		// Reset the results so a stale Success from the previous mode can't be read as this
		// mode's outcome
		_endResultLabel.Text = "EndPending";
		_topResultLabel.Text = "TopPending";
		_elementResultLabel.Text = "ElementPending";
	}

	async void RunDeferredScroll()
	{
		await _scrollView.ScrollToAsync(0, 100000, animated: false);
		await EvaluateUntilSuccess(_deferredResultLabel, () => EvaluateOffset(expectEnd: true, kind: "deferred"));
	}

	// Re-evaluates until the offset settles on the expected value so the UI test's polling
	// wait can converge instead of freezing a single too-early sample.
	static async Task EvaluateUntilSuccess(Label label, Func<string> evaluate)
	{
		for (int attempt = 0; attempt < 20; attempt++)
		{
			var result = evaluate();
			label.Text = result;
			if (result.StartsWith("Success", StringComparison.Ordinal))
			{
				return;
			}

			await Task.Delay(250);
		}
	}

#if IOS || MACCATALYST
	// The whole clamp has a mode-specific branch, so a fixture that silently resolved to a
	// different ContentInsetAdjustmentBehavior than intended would test the wrong one.
	string CheckResolvedMode(UIKit.UIScrollView nativeScrollView, string kind)
	{
		var edges = _scrollView.SafeAreaEdges;
		var expected =
			edges.Equals(new SafeAreaEdges(SafeAreaRegions.Container)) ? UIKit.UIScrollViewContentInsetAdjustmentBehavior.Always :
			edges.Equals(SafeAreaEdges.None) || edges.Equals(SafeAreaEdges.All) ? UIKit.UIScrollViewContentInsetAdjustmentBehavior.Never :
			// Default on a vertical scroll view resolves to Never since the landscape-notch
			// fix (#35533): MAUI owns all edges there, and Automatic remains in use only for
			// horizontal scroll views
			UIKit.UIScrollViewContentInsetAdjustmentBehavior.Never;

		return nativeScrollView.ContentInsetAdjustmentBehavior == expected
			? null
			: $"Fail ({kind}): SafeAreaEdges={edges} resolved to " +
				$"{nativeScrollView.ContentInsetAdjustmentBehavior}, expected {expected}";
	}
#endif

	string EvaluateOffset(bool expectEnd, string kind)
	{
#if IOS || MACCATALYST
		if (_scrollView.Handler?.PlatformView is not UIKit.UIScrollView nativeScrollView)
		{
			return $"Fail ({kind}): native scroll view unavailable";
		}

		if (_content.Handler?.PlatformView is not UIKit.UIView contentView)
		{
			return $"Fail ({kind}): content platform view unavailable";
		}

		if (CheckResolvedMode(nativeScrollView, kind) is string modeFailure)
		{
			return modeFailure;
		}

		var adjustedInset = nativeScrollView.AdjustedContentInset;
		if (adjustedInset.Top + adjustedInset.Bottom <= 0)
		{
			// Without insets the buggy and correct math coincide and the test proves nothing
			return $"Fail ({kind}): scenario invalid, no adjusted content insets " +
				$"(behavior={nativeScrollView.ContentInsetAdjustmentBehavior}, " +
				$"contentInset=({nativeScrollView.ContentInset.Top:F0},{nativeScrollView.ContentInset.Bottom:F0}))";
		}

		// Independent oracle: measure where the content platform view actually sits inside the
		// scroll view instead of re-deriving the implementation's ContentSize arithmetic.
		// At the end, the content's last pixel must rest exactly at the bottom of the
		// unobscured viewport; at the top, the offset must be the natural rest position.
		double expected = expectEnd
			? (double)(contentView.Frame.Bottom + adjustedInset.Bottom - nativeScrollView.Bounds.Height)
			: -(double)adjustedInset.Top;
		double actual = (double)nativeScrollView.ContentOffset.Y;

		if (Math.Abs(actual - expected) > 1.5)
		{
			return $"Fail ({kind}): actual={actual:F1} expected={expected:F1}";
		}

		// Also pin the public contract: ScrollY is published in cross-platform content
		// coordinates, i.e. the native offset shifted by the adjusted inset (0 at rest)
		double expectedScrollY = expected + (double)adjustedInset.Top;
		if (Math.Abs(_scrollView.ScrollY - expectedScrollY) > 1.5)
		{
			return $"Fail ({kind}): ScrollY={_scrollView.ScrollY:F1} expected={expectedScrollY:F1}";
		}

		return $"Success ({kind}): mode={nativeScrollView.ContentInsetAdjustmentBehavior} offset={actual:F1} scrollY={_scrollView.ScrollY:F1}";
#else
		return $"Skipped ({kind}): not applicable on this platform";
#endif
	}

	string EvaluateElementEnd()
	{
#if IOS || MACCATALYST
		if (_scrollView.Handler?.PlatformView is not UIKit.UIScrollView nativeScrollView)
		{
			return "Fail (element): native scroll view unavailable";
		}

		if (_probeLabel.Handler?.PlatformView is not UIKit.UIView probeView)
		{
			return "Fail (element): probe platform view unavailable";
		}

		if (CheckResolvedMode(nativeScrollView, "element") is string modeFailure)
		{
			return modeFailure;
		}

		var adjustedInset = nativeScrollView.AdjustedContentInset;
		if (adjustedInset.Top + adjustedInset.Bottom <= 0)
		{
			return "Fail (element): scenario invalid, no adjusted content insets";
		}

		// Independent geometric oracle: after ScrollToAsync(probe, End) the probe's bottom edge
		// must sit exactly at the bottom of the unobscured viewport, in window coordinates.
		// In the mode where MauiScrollView applies the safe area itself (SafeAreaEdges.All maps
		// to Never with the safe area baked into the content) the obscured bottom never appears
		// in AdjustedContentInset; the view-level SafeAreaInsets is what the platform view baked
		// in, so it obscures the viewport all the same.
		double bakedBottom = _scrollView.SafeAreaEdges.Equals(SafeAreaEdges.All)
			? (double)nativeScrollView.SafeAreaInsets.Bottom
			: 0;
		var probeInWindow = probeView.ConvertRectToView(probeView.Bounds, null);
		var scrollInWindow = nativeScrollView.ConvertRectToView(nativeScrollView.Bounds, null);
		double visibleBottom = (double)(scrollInWindow.Bottom - adjustedInset.Bottom) - bakedBottom;
		double actual = (double)probeInWindow.Bottom;

		return Math.Abs(actual - visibleBottom) <= 1.5
			? $"Success (element): mode={nativeScrollView.ContentInsetAdjustmentBehavior} bottom={actual:F1}"
			: $"Fail (element): actual={actual:F1} expected={visibleBottom:F1}";
#else
		return "Skipped (element): not applicable on this platform";
#endif
	}
}
