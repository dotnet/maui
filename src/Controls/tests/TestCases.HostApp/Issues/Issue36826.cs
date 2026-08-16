namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36826, "ScrollView SoftInput safe area should keep bottom content above the keyboard", PlatformAffected.iOS)]
public class Issue36826 : ContentPage
{
	double _scrollPositionWithKeyboard;
	bool _keyboardInputReceived;

	public Issue36826()
	{
		var modeLabel = new Label
		{
			Text = "Select SoftInput mode",
			AutomationId = "ModeLabel"
		};
		var resultLabel = new Label
		{
			Text = "NOT EVALUATED",
			AutomationId = "ResultLabel",
			FontAttributes = FontAttributes.Bold
		};
		var scrollView = new ScrollView
		{
			AutomationId = "ReproScrollView"
		};
		var content = new VerticalStackLayout
		{
			Spacing = 10
		};

		for (var i = 1; i <= 11; i++)
		{
			content.Children.Add(new Label
			{
				Text = $"Test Item {i}",
				HeightRequest = 64,
				BackgroundColor = Colors.LightGray,
				Padding = 12
			});
		}

		var entry = new Entry
		{
			Placeholder = "Tap to raise keyboard",
			AutomationId = "ReproEntry"
		};
		entry.TextChanged += (sender, args) =>
		{
			if (string.IsNullOrEmpty(args.NewTextValue))
				return;

			_scrollPositionWithKeyboard = scrollView.ScrollY;
			_keyboardInputReceived = true;
			resultLabel.Text = "READY: Swipe toward bottom, then evaluate";
		};
		content.Children.Add(entry);
		content.Children.Add(new Label
		{
			Text = "Bottom marker",
			AutomationId = "BottomMarker",
			HeightRequest = 64,
			BackgroundColor = Colors.LightGray,
			Padding = 12
		});
		scrollView.Content = content;

		var setSoftInputButton = new Button
		{
			Text = "Use SoftInput safe area",
			AutomationId = "SetSoftInputButton"
		};
		setSoftInputButton.Clicked += (sender, args) =>
		{
			scrollView.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);
			_keyboardInputReceived = false;
			modeLabel.Text = "READY: SoftInput safe area enabled";
			resultLabel.Text = "NOT EVALUATED";
		};

		var evaluateButton = new Button
		{
			Text = "Evaluate keyboard scrolling",
			AutomationId = "EvaluateButton"
		};
		evaluateButton.Clicked += (sender, args) =>
		{
			if (!_keyboardInputReceived)
			{
				resultLabel.Text = "NOT EVALUATED";
				return;
			}

			var additionalScrollRange = scrollView.ScrollY - _scrollPositionWithKeyboard;
			resultLabel.Text = additionalScrollRange < 80
				? "BUG REPRODUCED: Bottom marker remains behind keyboard"
				: "PASS: Bottom marker can scroll above keyboard";
		};

		var controls = new VerticalStackLayout
		{
			Spacing = 8,
			Children =
			{
				setSoftInputButton,
				evaluateButton,
				modeLabel,
				resultLabel
			}
		};
		var grid = new Grid
		{
			Padding = 16,
			RowSpacing = 12,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};
		grid.Add(controls);
		grid.Add(scrollView, 0, 1);
		Content = grid;
	}
}
