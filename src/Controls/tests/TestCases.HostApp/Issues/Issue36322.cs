namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36322, "Shell.TitleView is not centered on initial load on Windows", PlatformAffected.UWP)]
public class Issue36322 : Shell
{
	public Issue36322()
	{
		var rootPage = new ContentPage
		{
			Title = "Issue36322",
			Content = new VerticalStackLayout
			{
				Padding = 24,
				Children =
				{
					new Label
					{
						AutomationId = "Issue36322Instructions",
						Text = "Verify that the Shell TitleView is centered in the command bar on initial load.",
					}
				}
			}
		};

		Shell.SetTitleView(rootPage, new Label
		{
			AutomationId = "Issue36322TitleView",
			Text = "Centered TitleView",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			BackgroundColor = Colors.LightBlue,
			Padding = new Thickness(8, 2)
		});

		Items.Add(new FlyoutItem
		{
			Title = "Home",
			Items =
			{
				new ShellContent
				{
					Content = rootPage
				}
			}
		});
	}
}
