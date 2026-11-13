namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40180, "Standalone long press should not fire after a quick tap", PlatformAffected.Android)]
public class Issue40180 : ContentPage
{
	public Issue40180()
	{
		var longPressCount = 0;
		var countLabel = new Label
		{
			AutomationId = "LongPressCountLabel",
			Text = "Long Press Count: 0"
		};

		var target = new Label
		{
			AutomationId = "LongPressTarget",
			Text = "Tap quickly or press and hold",
			Background = Colors.LightBlue,
			TextColor = Colors.Black,
			HeightRequest = 100,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center
		};
		target.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				longPressCount++;
				countLabel.Text = $"Long Press Count: {longPressCount}";
			})
		});

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children = { target, countLabel }
		};
	}
}
