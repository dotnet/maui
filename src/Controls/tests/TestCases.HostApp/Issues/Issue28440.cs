namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28440, "FlyoutPage IsPresented not updated properly in windows", PlatformAffected.UWP)]
public class Issue28440 : FlyoutPage
{
	public Issue28440()
	{
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

		var flyoutMenuLabel = new Label
		{
			Text = "Flyout Menu",
			AutomationId = "flyoutMenu"
		};

		var flyoutPage = new ContentPage
		{
			Title = "Menu",
			Content = new StackLayout
			{
				Children = { flyoutMenuLabel }
			}
		};

		var openMenuButton = new Button
		{
			Text = "Open Menu",
			AutomationId = "Button"
		};

		openMenuButton.Clicked += (sender, args) => IsPresented = true;

		var detailPage = new ContentPage
		{
			Content = new StackLayout
			{
				Children = { openMenuButton }
			}
		};

		var navigationPage = new NavigationPage(detailPage);

		Flyout = flyoutPage;
		Detail = navigationPage;
	}
}

