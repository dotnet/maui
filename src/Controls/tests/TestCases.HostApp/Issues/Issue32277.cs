namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32277, "When a FlyoutPage is pushed Modally it doesn't inset the AppBarLayout", PlatformAffected.Android)]
public partial class Issue32277 : NavigationPage
{
	public Issue32277()
	{
		PushAsync(new Issue32277_ContentPage());
	}
}

public class Issue32277_ContentPage : ContentPage
{
	public Issue32277_ContentPage()
	{
		Title = "Issue 32277";
		
		Button button = new Button
		{
			Text = "Push Modal Flyout Page",
			AutomationId = "PushModalFlyoutButton"
		};
		button.Clicked += async (s, e) =>
		{
			await Navigation.PushModalAsync(new Issue32277_FlyoutPage());
		};
		Content = new StackLayout
		{
			Children =
			{
				button
			}
		};
	}
}

public class Issue32277_FlyoutPage : FlyoutPage
{
	public Issue32277_FlyoutPage()
	{
		Flyout = new ContentPage
		{
			Title = "Flyout",
			Content = new StackLayout
			{
				Children =
				{
					new Label 
					{ 
						Text = "This is the flyout page.",
						AutomationId = "FlyoutLabel"
					}
				}
			}
		};

		Detail = new NavigationPage(new ContentPage
		{
			Title = "Detail",
			Content = new StackLayout
			{
				Children =
				{
					new Label 
					{ 
						Text = "This is the detail page.",
						AutomationId = "DetailLabel"
					}
				}
			}
		});
	}
}
