using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Github, 3524, "ICommand binding from a TapGestureRecognizer on a Span doesn't work")]

public class Issue3524 : TestContentPage
{
	const string kText = "Click Me To Increment";

	public Command TapCommand { get; set; }
	public String Text { get; set; } = kText;

	protected override void Init()
	{
		int i = 0;

		FormattedString formattedString = new FormattedString();
		var span = new Span() { AutomationId = kText };
		span.Text = kText;
		var tapGesture = new TapGestureRecognizer();
		tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, "TapCommand");
		span.GestureRecognizers.Add(tapGesture);
		formattedString.Spans.Add(span);
		BindingContext = this;
		var label = new Label()
		{
			AutomationId = kText,
			HorizontalOptions = LayoutOptions.Center
		};

		label.FormattedText = formattedString;
		TapCommand = new Command(() =>
		{
			i++;
			span.Text = $"{kText}: {i}";
		});

		Content = new ContentView()
		{
			Content = new StackLayout()
			{
				Children =
				{
					label
				}
			}
		};
	}
}
