namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35755, "IndexOutOfBoundsException in RecalculateSpanPositions when a Label uses FormattedText, MaxLines, and TailTruncation", PlatformAffected.Android)]
public class Issue35755 : ContentPage
{
	readonly string _paragraphA = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum eleifend, augue nec aliquam interdum, massa nisl viverra orci, non interdum risus arcu id lorem. Curabitur accumsan, urna eu tempor tincidunt, purus neque feugiat tortor, sed tristique nibh nunc et augue.";
	readonly string _paragraphB = "Aliquam erat volutpat. Quisque a mi lacus. Integer vitae malesuada sem. Nunc id dui nec lacus feugiat volutpat. Morbi et sollicitudin erat. Sed varius felis id dignissim facilisis. Vivamus vulputate, augue sed finibus laoreet, enim neque tristique odio, id rhoncus elit purus a turpis.";
	readonly string _paragraphC = "Praesent in lectus non mauris mattis ultrices. Donec non justo ac nunc porta pellentesque. Integer euismod, velit in posuere iaculis, lorem nunc commodo libero, nec interdum lorem nibh ut turpis. Phasellus gravida tristique tortor, id posuere turpis sodales in.";

	Label _crashTargetLabel;

	public Issue35755()
	{
		_crashTargetLabel = new Label
		{
			AutomationId = "CrashTargetLabel",
			MaxLines = 4,
			LineBreakMode = LineBreakMode.TailTruncation,
			FontSize = 14
		};

		var resultLabel = new Label
		{
			AutomationId = "ResultLabel",
			Text = "Waiting for trigger..."
		};

		var triggerButton = new Button
		{
			AutomationId = "TriggerButton",
			Text = "Trigger FormattedText"
		};

		triggerButton.Clicked += (s, e) =>
		{
			_crashTargetLabel.FormattedText = BuildFormattedText();
			resultLabel.Text = "Success";
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(24),
				Spacing = 16,
				Children =
				{
					triggerButton,
					resultLabel,
					_crashTargetLabel
				}
			}
		};
	}

	FormattedString BuildFormattedText()
	{
		var fs = new FormattedString();
		fs.Spans.Add(new Span
		{
			Text = "Content: ",
			FontAttributes = FontAttributes.Bold,
			TextColor = Colors.DarkRed
		});
		fs.Spans.Add(new Span { Text = _paragraphA + "\n\n", TextColor = Colors.Black });
		fs.Spans.Add(new Span { Text = _paragraphB + "\n\n", TextColor = Colors.DarkBlue });
		fs.Spans.Add(new Span { Text = _paragraphC, TextColor = Colors.DarkGreen });
		return fs;
	}
}
