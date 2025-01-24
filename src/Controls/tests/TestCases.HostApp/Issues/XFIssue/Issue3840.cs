using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 3840, "[iOS] Translation change causes ScrollView to reset to initial position (0, 0)",
	PlatformAffected.iOS)]
public class Issue3840 : TestContentPage
{
	const string _failedText = "Test Failed if Visible";
	const string _button1 = "FirstClick";
	const string _button2 = "SecondClick";

	protected override void Init()
	{
		ScrollView scroll = null;
		scroll = new ScrollView
		{
			AutomationId = "scrollView",
			Content = new StackLayout
			{
				new Label()
				{
					Text = _failedText
				},
				new Button()
				{
					Text = "Click Me First",
					AutomationId = _button1,
					Command = new Command(async () =>
					{
						await scroll.ScrollToAsync(0, 100, true);
					}),
					HorizontalOptions = LayoutOptions.Start
				},
				new BoxView { Color = Colors.Red, HeightRequest = 500 },
				new Button()
				{
					Text = "Click Me Second",
					AutomationId = _button2,
					Command = new Command(async () =>
					{
						scroll.TranslationX = 100;
						await Task.Delay(100);
						// using one because of a bug on UWP that doesn't react to being set back to zero
						scroll.TranslationX = 1;

					}),
					HorizontalOptions = LayoutOptions.Start
				},
				new BoxView { Color = Colors.Gray, HeightRequest = 500 },
				new BoxView { Color = Colors.Yellow, HeightRequest = 500 }
			}
		};

		var mainLayout = new AbsoluteLayout();
		mainLayout.Children.Add(scroll);
		AbsoluteLayout.SetLayoutBounds(scroll, new Rect(0, 0, 1, 1));
		AbsoluteLayout.SetLayoutFlags(scroll, AbsoluteLayoutFlags.All);
		Content = mainLayout;
	}
}
