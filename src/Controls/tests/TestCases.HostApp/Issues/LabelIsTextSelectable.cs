namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Label.IsTextSelectable lets the user select text", PlatformAffected.All)]
public class LabelIsTextSelectable : ContentPage
{
	const string SampleText = "Select me: the quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog again.";

	public LabelIsTextSelectable()
	{
		var statusLabel = new Label { AutomationId = "StatusLabel", Text = "IsTextSelectable: True" };
		var tapCountLabel = new Label { AutomationId = "TapCountLabel", Text = "Taps: 0" };

		var selectableLabel = new Label
		{
			AutomationId = "SelectableLabel",
			Text = SampleText,
			IsTextSelectable = true,
			BackgroundColor = Colors.LightYellow,
			Padding = new Thickness(8, 4),
		};

		var taps = 0;
		var span = new Span { Text = "tap this span", TextColor = Colors.Blue, TextDecorations = TextDecorations.Underline };
		span.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() => tapCountLabel.Text = $"Taps: {++taps}")
		});

		var formattedLabel = new Label
		{
			AutomationId = "SelectableSpanLabel",
			IsTextSelectable = true,
			FormattedText = new FormattedString
			{
				Spans =
				{
					span,
					new Span { Text = " is followed by formatted text that stays selectable.", FontAttributes = FontAttributes.Italic },
				}
			},
		};

		var toggleButton = new Button { AutomationId = "ToggleButton", Text = "Toggle IsTextSelectable" };
		toggleButton.Clicked += (s, e) =>
		{
			selectableLabel.IsTextSelectable = !selectableLabel.IsTextSelectable;
			formattedLabel.IsTextSelectable = selectableLabel.IsTextSelectable;
			statusLabel.Text = $"IsTextSelectable: {selectableLabel.IsTextSelectable}";
		};

		// A selectable label next to others in a wrapping layout must still measure normally.
		var flex = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
		flex.Add(new Label { Text = "Some information ", VerticalOptions = LayoutOptions.Center });
		flex.Add(new Label
		{
			AutomationId = "FlexSelectableLabel",
			Text = "This text must not wrap every character",
			IsTextSelectable = true,
			VerticalOptions = LayoutOptions.Center,
		});

		var truncated = new Label
		{
			AutomationId = "TruncatedSelectableLabel",
			Text = SampleText + " " + SampleText,
			MaxLines = 2,
			LineBreakMode = LineBreakMode.TailTruncation,
			IsTextSelectable = true,
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = 16,
				Spacing = 12,
				Children =
				{
					statusLabel,
					toggleButton,
					selectableLabel,
					formattedLabel,
					tapCountLabel,
					flex,
					truncated,
					new Entry { AutomationId = "PasteTarget", Placeholder = "Paste copied text here" },
				}
			}
		};
	}
}
