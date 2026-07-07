namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29921, "Flyout icon not replaced after root page change", PlatformAffected.iOS)]
public class Issue29921 : FlyoutPage
{
	public Issue29921()
	{
		Detail = new NavigationPage(new Issue29921Page1());
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

public class Issue29921Page1 : ContentPage
{
	public Issue29921Page1()
	{
		Title = "Page 1";
		var insertBeforeButton = new Button { Text = "Insert Page Before", AutomationId = "InsertPageButton" };
		insertBeforeButton.Clicked += InsertBefore_Clicked;
		Content = new StackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children = { insertBeforeButton }
		};
	}

	private void InsertBefore_Clicked(object sender, EventArgs e)
	{
		if (Parent is NavigationPage navPage)
		{
			navPage.Navigation.InsertPageBefore(new Issue29921Page2(), navPage.RootPage);
		}
	}
}

public class Issue29921Page2 : ContentPage
{
	public Issue29921Page2()
	{
		Title = "Page 2";
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
