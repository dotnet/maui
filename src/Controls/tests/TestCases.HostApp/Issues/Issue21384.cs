using Microsoft.Maui.Layouts;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 21384, "InputTransparent has no effect on Windows", PlatformAffected.WinRT)]
public class Issue21384 : ContentPage
{
	int _clickCount;
	readonly Button _counterButton;

	public Issue21384()
	{
		_counterButton = new Button
		{
			AutomationId = "CounterButton",
			Text = "Click me",
			HorizontalOptions = LayoutOptions.Fill
		};
		_counterButton.Clicked += OnCounterClicked;

		var inputTransparentLabel = new Label
		{
			AutomationId = "InputTransparentLabel",
			BackgroundColor = Colors.Transparent,
			InputTransparent = true,
			Text = "Help"
		};
		AbsoluteLayout.SetLayoutBounds(inputTransparentLabel, new Rect(0, 0, 1, 1));
		AbsoluteLayout.SetLayoutFlags(inputTransparentLabel, AbsoluteLayoutFlags.All);

		var overlay = new AbsoluteLayout
		{
			HeightRequest = 60,
			HorizontalOptions = LayoutOptions.Fill
		};
		overlay.Add(_counterButton);
		overlay.Add(inputTransparentLabel);

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(30, 0),
			Spacing = 25,
			Children =
			{
				overlay
			}
		};
	}

	void OnCounterClicked(object sender, EventArgs e)
	{
		_clickCount++;
		_counterButton.Text = _clickCount == 1
			? "Clicked 1 time"
			: $"Clicked {_clickCount} times";
	}
}
