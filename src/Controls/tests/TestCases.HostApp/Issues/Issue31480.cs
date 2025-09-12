namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31480, "RightToLeft does not apply for FormattedText", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue31480 : TestContentPage
{
	const string ToggleButton = "ToggleFlowDirection";
	const string FormattedTextLabel = "FormattedTextLabel";

	Label _formattedTextLabel;
	bool _isRtl = false;

	protected override void Init()
	{
		var layout = new StackLayout { Padding = new Thickness(20), Spacing = 20 };

		var instructions = new Label
		{
			Text = "This test demonstrates FormattedText alignment with FlowDirection. " +
			       "Tap the button to toggle between LTR and RTL. " +
			       "The text should align properly based on the FlowDirection.",
			FontSize = 14
		};

		layout.Children.Add(instructions);

		// Create FormattedText label
		var formattedString = new FormattedString();
		formattedString.Spans.Add(new Span
		{
			Text = "This is RTL formatted text that should align correctly", FontSize = 16
		});

		_formattedTextLabel = new Label
		{
			AutomationId = FormattedTextLabel,
			FormattedText = formattedString,
			FlowDirection = FlowDirection.LeftToRight,
			HorizontalTextAlignment = TextAlignment.Start,
			BackgroundColor = Colors.LightGray,
			Padding = new Thickness(10),
			Margin = new Thickness(0, 10)
		};

		layout.Children.Add(_formattedTextLabel);

		// Add a status label to show current flow direction
		var statusLabel = new Label
		{
			Text = "Current FlowDirection: LeftToRight", FontSize = 12, TextColor = Colors.Blue
		};

		layout.Children.Add(statusLabel);

		// Add toggle button
		var toggleButton = new Button
		{
			AutomationId = ToggleButton,
			Text = "Toggle FlowDirection",
			BackgroundColor = Colors.Blue,
			TextColor = Colors.White
		};

		toggleButton.Clicked += (sender, e) =>
		{
			_isRtl = !_isRtl;
			_formattedTextLabel.FlowDirection = _isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
			statusLabel.Text = $"Current FlowDirection: {_formattedTextLabel.FlowDirection}";
		};

		layout.Children.Add(toggleButton);

		// Add additional test labels for different scenarios
		AddTestScenarios(layout);

		Content = layout;
	}

	private void AddTestScenarios(StackLayout layout)
	{
		// Test scenario 1: RTL with Start alignment
		var formattedStringRtlStart = new FormattedString();
		formattedStringRtlStart.Spans.Add(new Span { Text = "RTL Start alignment test", FontSize = 14 });

		var rtlStartLabel = new Label
		{
			AutomationId = "FormattedTextStartRTL",
			FormattedText = formattedStringRtlStart,
			FlowDirection = FlowDirection.RightToLeft,
			HorizontalTextAlignment = TextAlignment.Start,
			BackgroundColor = Colors.LightYellow,
			Padding = new Thickness(10),
			Margin = new Thickness(0, 5)
		};

		layout.Children.Add(new Label
		{
			Text = "RTL with Start alignment:", FontSize = 12, FontAttributes = FontAttributes.Bold
		});
		layout.Children.Add(rtlStartLabel);

		// Test scenario 2: LTR with Start alignment
		var formattedStringLtrStart = new FormattedString();
		formattedStringLtrStart.Spans.Add(new Span { Text = "LTR Start alignment test", FontSize = 14 });

		var ltrStartLabel = new Label
		{
			AutomationId = "FormattedTextStartLTR",
			FormattedText = formattedStringLtrStart,
			FlowDirection = FlowDirection.LeftToRight,
			HorizontalTextAlignment = TextAlignment.Start,
			BackgroundColor = Colors.LightGreen,
			Padding = new Thickness(10),
			Margin = new Thickness(0, 5)
		};

		layout.Children.Add(new Label
		{
			Text = "LTR with Start alignment:", FontSize = 12, FontAttributes = FontAttributes.Bold
		});
		layout.Children.Add(ltrStartLabel);

		// Test scenario 3: RTL label for main test
		var formattedStringRtl = new FormattedString();
		formattedStringRtl.Spans.Add(new Span { Text = "Fixed RTL FormattedText alignment", FontSize = 14 });

		var rtlLabel = new Label
		{
			AutomationId = "FormattedTextRTLLabel",
			FormattedText = formattedStringRtl,
			FlowDirection = FlowDirection.RightToLeft,
			HorizontalTextAlignment = TextAlignment.Start,
			BackgroundColor = Colors.LightPink,
			Padding = new Thickness(10),
			Margin = new Thickness(0, 5)
		};

		layout.Children.Add(new Label
		{
			Text = "RTL FormattedText (should align right):", FontSize = 12, FontAttributes = FontAttributes.Bold
		});
		layout.Children.Add(rtlLabel);
	}
}