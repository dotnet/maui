namespace Maui.Controls.Sample.Issues;

// A Shell whose FlyoutContent is a ScrollView exercises ShellFlyoutLayoutManager's
// ScrollView branch, which feeds ScrollView.ScrollY into the header layout math. That math
// works in native offsets (which rest at -headerHeight because the header is carried in
// ContentInset), so it has to convert back from the content coordinates ScrollY reports.
// The default flyout uses a UITableView and a different code path, so it does not cover this.
[Issue(IssueTracker.None, 0, "Shell flyout header with ScrollView flyout content", PlatformAffected.iOS)]
public class ShellFlyoutHeaderScrollViewContent : TestShell
{
	const double HeaderHeight = 150;

	readonly Label _resultLabel = new() { Text = "Pending", AutomationId = "ResultLabel" };
	ScrollView _flyoutScroll;
	Grid _headerGrid;

	protected override void Init()
	{
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Locked);
		FlyoutHeaderBehavior = FlyoutHeaderBehavior.Scroll;

		_headerGrid = new Grid
		{
			HeightRequest = HeaderHeight,
			BackgroundColor = Colors.MediumPurple,
			AutomationId = "FlyoutHeaderId",
			Children =
			{
				new Label
				{
					Text = "FLYOUT HEADER",
					TextColor = Colors.White,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				}
			}
		};

		FlyoutHeader = _headerGrid;

		var rows = new VerticalStackLayout();
		for (int i = 0; i < 40; i++)
		{
			rows.Add(new Label { Text = $"Flyout row {i}", HeightRequest = 40 });
		}

		_flyoutScroll = new ScrollView { Content = rows, AutomationId = "FlyoutScroll" };
		FlyoutContent = _flyoutScroll;

		var runButton = new Button { Text = "Scroll flyout and return", AutomationId = "RunButton" };
		runButton.Clicked += async (sender, e) => await RunAsync();

		AddFlyoutItem(new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = 24,
				Spacing = 12,
				Children = { runButton, _resultLabel }
			}
		}, "Item");
	}

	async Task RunAsync()
	{
		_resultLabel.Text = "Pending";

		// Scroll the flyout content and come back to the top. The round trip guarantees the
		// header layout is driven by a Scrolled event (the ScrollView branch) rather than the
		// direct native call that also runs during initial layout.
		await _flyoutScroll.ScrollToAsync(0, 200, animated: false);
		await Task.Delay(150);

		// Assert the scrolled-away state too. The returned-to-top state alone is also what a
		// header that never moved would report (_headerOffset starts at 0), so without this
		// the test would pass even if the Scrolled wiring went dead entirely.
		var scrolledAway = EvaluateHeaderScrolledAway();
		if (!scrolledAway.StartsWith("Success", StringComparison.Ordinal))
		{
			_resultLabel.Text = scrolledAway;
			return;
		}

		await _flyoutScroll.ScrollToAsync(0, 0, animated: false);
		await Task.Delay(150);

		_resultLabel.Text = EvaluateHeaderPosition();
	}

	// With FlyoutHeaderBehavior.Scroll the header must move up as the content scrolls down,
	// which only happens if the ScrollView -> OnScrolled path actually ran
	string EvaluateHeaderScrolledAway()
	{
#if IOS || MACCATALYST
		if (_headerGrid.Handler?.PlatformView is not UIKit.UIView headerView ||
			headerView.Superview is not UIKit.UIView headerContainer)
		{
			return "Fail: header views unavailable";
		}

		double headerY = headerContainer.Frame.Y;

		return headerY < 0
			? $"Success: scrolled headerY={headerY:F1}"
			: $"Fail: header did not move up while scrolled (headerY={headerY:F1})";
#else
		return "Skipped: not applicable on this platform";
#endif
	}

	string EvaluateHeaderPosition()
	{
#if IOS || MACCATALYST
		if (_headerGrid.Handler?.PlatformView is not UIKit.UIView headerView)
		{
			return "Fail: header platform view unavailable";
		}

		// The Shell renderer positions the container that wraps the header content, not the
		// header content itself, so read the container's frame
		if (headerView.Superview is not UIKit.UIView headerContainer)
		{
			return "Fail: header container unavailable";
		}

		// FlyoutHeaderBehavior.Scroll positions the header by its frame Y (the scroll offset,
		// plus the safe area when honoured), so back at the top of the content the header must
		// be fully visible again — never pushed above the flyout's top edge. Computing the
		// header offset from the wrong coordinate space pushes it up by a full header height.
		double headerY = headerContainer.Frame.Y;

		return headerY >= -1
			? $"Success: headerY={headerY:F1} height={headerContainer.Frame.Height:F1}"
			: $"Fail: headerY={headerY:F1}, expected the header to stay at or below the top";
#else
		return "Skipped: not applicable on this platform";
#endif
	}
}
