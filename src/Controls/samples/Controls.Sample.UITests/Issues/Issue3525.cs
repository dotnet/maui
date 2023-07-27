using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 3525, "Finicky tap gesture recognition on Spans")]
public class Issue3525 : TestContentPage
{
	const string kClickCount = "Click Count: ";
	const string kClickCountAutomationId = "ClickCount";
	const string kLabelTestAutomationId = "SpanningLabel";

	protected override void Init()
	{
		var label = new Label() { Text = kClickCount, AutomationId = kClickCountAutomationId };
		Padding = new Thickness(20);
		var layout = new StackLayout { Padding = new Thickness(5, 10) };

		var formattedString = new FormattedString();
		formattedString.Spans.Add(new Span { Text = "Not Clickable, ", TextColor = Colors.Red, FontAttributes = FontAttributes.Bold, LineHeight = 1.8 });
		formattedString.Spans.Add(new Span { Text = Environment.NewLine });
		formattedString.Spans.Add(new Span { Text = Environment.NewLine });
		var span = new Span { Text = "Clickable, " };
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
		formattedString.Spans.Add(new Span { Text = Environment.NewLine });
		formattedString.Spans.Add(new Span { Text = Environment.NewLine });

		formattedString.Spans.Add(new Span { Text = "You also cannot click on me sorry about that.", FontAttributes = FontAttributes.Italic });

		layout.Children.Add(new Label { AutomationId = kLabelTestAutomationId, FormattedText = formattedString });
		layout.Children.Add(label);

		this.Title = "Label Demo - Code";
		this.Content = layout;
	}
}
