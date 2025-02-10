namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 27565, "Buttons with Gradient and ImageSource at the same time: ImageSource doesn't appear", PlatformAffected.iOS)]
public class Issue27565 : ContentPage
{
	public Issue27565()
	{
		Content = new Button
		{
			AutomationId = "Button",
			HeightRequest = 120,
			FontSize = 16,
			CornerRadius = 20,
			BorderColor = Colors.Red,
			FontAttributes = FontAttributes.Bold,
			ImageSource = "dotnet_bot.png",
			ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 10),
			Shadow = new Shadow
			{
				Brush = Brush.Black,
				Offset = new Point(4, 4)
			},
			Background = new LinearGradientBrush
			{
				EndPoint = new Point(1, 0),
				GradientStops =
				[
					new GradientStop { Color = Colors.Red, Offset = 0.1f },
					new GradientStop { Color = Colors.Blue, Offset = 1.0f }
				]
			}
		};
	}
}