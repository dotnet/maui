namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36322, "Shell.TitleView is not centered on initial load on Windows", PlatformAffected.UWP)]
public class Issue36322 : TestShell
{
	protected override void Init()
	{
		var page = new ContentPage
		{
			Title = "Issue36322",
			Content = new VerticalStackLayout
			{
				Padding = 24,
				Children =
				{
					new Label
					{
						Text = "Verify that the Shell TitleView is centered in the command bar on initial load.",
						AutomationId = "Issue36322Instructions"
					}
				}
			}
		};

		Shell.SetTitleView(page, new ImageButton
		{
			Source = "dotnet_bot.png",
			HeightRequest = 25,
			WidthRequest = 25,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "Issue36322TitleView"
		});

		AddContentPage(page, "Home");
	}
}
