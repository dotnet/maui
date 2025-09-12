using static Microsoft.Maui.Controls.Button;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29036, "Button RTL text and image overlap", PlatformAffected.iOS)]
public class Issue29036 : ContentPage
{
	public Issue29036()
	{
		Content = new VerticalStackLayout
		{
			WidthRequest = 400,
			Children =
				{
					new Button
					{
						AutomationId = "button",
						FlowDirection = FlowDirection.RightToLeft,
						BackgroundColor = Colors.LightGreen,
						Text = "This is a Regular Button",
						FontSize = 24,
						ImageSource = "dotnet_bot.png",
						TextColor = Colors.Black,
						VerticalOptions = LayoutOptions.Center
					},
					new Button
					{
						BackgroundColor = Colors.LightGreen,
						Text = "This is a Regular Button",
						FontSize = 24,
						ImageSource = "dotnet_bot.png",
						TextColor = Colors.Black,
						VerticalOptions = LayoutOptions.Center
					},
					new Button
					{
						FlowDirection = FlowDirection.RightToLeft,
						ContentLayout = new ButtonContentLayout(ButtonContentLayout.ImagePosition.Right, 10),
						BackgroundColor = Colors.LightGreen,
						Text = "This is a Regular Button",
						FontSize = 24,
						ImageSource = "dotnet_bot.png",
						TextColor = Colors.Black,
						VerticalOptions = LayoutOptions.Center
					},
					new Button
					{
						BackgroundColor = Colors.LightGreen,
						ContentLayout = new ButtonContentLayout(ButtonContentLayout.ImagePosition.Right, 10),
						Text = "This is a Regular Button",
						FontSize = 24,
						ImageSource = "dotnet_bot.png",
						TextColor = Colors.Black,
						VerticalOptions = LayoutOptions.Center
					}
				}
		};
	}
}