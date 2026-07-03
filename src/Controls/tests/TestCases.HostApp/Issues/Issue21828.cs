namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21828, "Flyout icon disappears after root page replacement and pop", PlatformAffected.iOS)]
public class Issue21828 : FlyoutPage
{
	public Issue21828()
	{
		Detail = new NavigationPage(new Issue21828Page1());
		Flyout = new ContentPage
		{
			Title = "Flyout Page",
			IconImageSource = "menu_icon.png",
			Content = new Label
			{
				Text = "This is flyout",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			}
		};
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
	}
}

public class Issue21828Page1 : ContentPage
{
	public Issue21828Page1()
	{
		Title = "Start Page";
		var insertAndPopButton = new Button { Text = "Insert Page Before and Pop", AutomationId = "InsertAndPopButton" };
		insertAndPopButton.Clicked += InsertAndPopButton_Clicked;
		Content = new StackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children = { insertAndPopButton }
		};
	}

	private async void InsertAndPopButton_Clicked(object sender, EventArgs e)
	{
		if (Parent is NavigationPage navPage)
		{
			navPage.Navigation.InsertPageBefore(new Issue21828Page2(), navPage.RootPage);
			await navPage.Navigation.PopAsync();
		}
	}
}

public class Issue21828Page2 : ContentPage
{
	public Issue21828Page2()
	{
		Title = "End Page";
		Content = new StackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children =
				{
					new Label { Text = "Flyout icon is visible", AutomationId = "Page2Label" },
				}
		};
	}
}
