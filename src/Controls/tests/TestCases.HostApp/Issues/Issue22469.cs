namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22469, "Crash with specific Font and text in Label", PlatformAffected.iOS)]
public class Issue22469 : TestContentPage
{
	const string kClickCount = "Click Count: ";
	const string kClickCountAutomationId = "ClickCount";
	const string kLabelTestAutomationId = "SpanningLabel";

	protected override void Init()
	{
		var label = new Label { Text = kClickCount, AutomationId = kClickCountAutomationId, FontSize = 14 };
		var layout = new Grid() { WidthRequest = 335, RowDefinitions = new RowDefinitionCollection { new RowDefinition(), new RowDefinition() } };

		var formattedString = new FormattedString();
		formattedString.Spans.Add(new Span
		{
			Text = "A culture of continuous learning isn't something you can ignore. 76% of employees are more inclined to stay when their workplace offers learning and development 🤔 Here's a few ways you can foster this:🧠 Create channels to encourage knowledge sharing between team members👏 Provide learning opportunities like job shadowing and rotation for hands-on experiences💬 Integrate routine feedback as a celebrated part of the workflow💻 Encourage your teams to dive into digital learning and live events Take a look at some more methods you can adopt from this recent article in Forbes 👇 🔗 ",
			TextColor = Colors.Red,
			FontFamily = "OpenSansRegular",
			FontSize = 14
		});
		var span = new Span { Text = "Just a long link to https://learn.microsoft.com/en-us/dotnet/maui/get-started/first-app?view=net-maui-8.0", FontFamily = "OpenSansRegular", TextDecorations = TextDecorations.Underline, TextColor = Colors.Green };
		int clickCount = 0;
		span.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() =>
			{
				clickCount++;
				label.Text = $"{kClickCount}{clickCount}";
			})
		});
		formattedString.Spans.Add(span);

		// Combination of spans, fotnfamily + fontsize and width 335 throws exception on iOS before Issue22469 fixed
		var topLabel = new Label
		{
			AutomationId = kLabelTestAutomationId,
			FormattedText = formattedString,
			FontSize = 14
		};

		Grid.SetRow(topLabel, 0);
		Grid.SetRow(label, 1);
		layout.Children.Add(topLabel);
		layout.Children.Add(label);

		Title = "Label Demo - Code";
		Content = layout;
	}
}