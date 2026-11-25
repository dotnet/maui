namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40355, "LongPressGestureRecognizer Command does not work on Button", PlatformAffected.UWP)]
public class Issue40355 : ContentPage
{
	public Issue40355()
	{
		var statusLabel = new Label
		{
			Text = "Long press not detected",
			AutomationId = "Issue40355StatusLabel"
		};

		var button = new Button
		{
			Text = "Long press me",
			AutomationId = "Issue40355Button"
		};

		button.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() => statusLabel.Text = "Long press detected")
		});

		var clickLabel = new Label
		{
			Text = "Not clicked",
			AutomationId = "Issue40355ClickLabel"
		};

		button.Clicked += (s, e) => clickLabel.Text = "Clicked";

		var imageButtonStatusLabel = new Label
		{
			Text = "ImageButton long press not detected",
			AutomationId = "Issue40355ImageButtonStatusLabel"
		};

		var imageButton = new ImageButton
		{
			Source = "dotnet_bot.png",
			WidthRequest = 100,
			HeightRequest = 100,
			AutomationId = "Issue40355ImageButton"
		};

		imageButton.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() => imageButtonStatusLabel.Text = "ImageButton long press detected")
		});

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children = { button, statusLabel, clickLabel, imageButton, imageButtonStatusLabel }
		};
	}
}
