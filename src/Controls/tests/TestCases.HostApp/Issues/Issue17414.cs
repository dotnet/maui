namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17414, "Default styling for controls does not work", PlatformAffected.UWP)]
public partial class Issue17414Shell : Shell
{
	public Issue17414Shell()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;
		ItemTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				Margin = new Thickness(20),
				Text = "Home"
			};
		});

		var detailContentPage = new ContentPage
		{
			BackgroundColor = Colors.LightGreen,
			Content = new Label
			{
				AutomationId = "MainPageLabel",
				Text = "Verify that the flyout content has no default corner radius",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		Items.Add(new ShellContent
		{
			Content = detailContentPage
		});
	}
}