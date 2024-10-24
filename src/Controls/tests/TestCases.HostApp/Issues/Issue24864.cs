namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24864, "Button control: inconsistent border visibility behaviour iOS vs Android when BackgroundColor is Transparent", PlatformAffected.iOS)]

public class Issue24864 : ContentPage
{
	public Issue24864()
	{
		Content = new VerticalStackLayout()
		{
			new Button
			{
				AutomationId = "button",
				BorderWidth = 10,
				TextColor = Colors.Black,
				BackgroundColor = Colors.Transparent,
				Text = "Hello World",
				Margin = new Thickness(10)
			},
			new ImageButton
			{
				BorderWidth = 1,
				BackgroundColor = Colors.Transparent,
			}
		};
	}
}
