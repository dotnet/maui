namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38147, "iOS Glass UI bottom tabs are not properly aligned", PlatformAffected.iOS)]
public class Issue38147 : TabbedPage
{
	public Issue38147()
	{
		BarTextColor = Colors.Blue;
		BarBackgroundColor = Colors.DarkGray;

		ContentPage tabOne = new ContentPage
		{
			Title = "Home",
			IconImageSource = "groceries.png",
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						AutomationId = "HomeTabLabel",
						Text = "Home Tab"
					}
				}
			}
		};

		ContentPage tabTwo = new ContentPage
		{
			Title = "Settings",
			IconImageSource = "groceries.png",
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Settings Tab"
					}
				}
			}
		};
		Children.Add(tabOne);
		Children.Add(tabTwo);
	}
}