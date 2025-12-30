namespace Maui.Controls.Sample.Issues;

using Microsoft.Maui.Controls;

[Issue(IssueTracker.Github, 25114, "NullReferenceException when setting BarBackgroundColor for a NavigationPage", PlatformAffected.All)]
public class Issue25114Flyout : FlyoutPage
{
	public Issue25114Flyout()
	{
		var navPage = new NavigationPage(new Issue25114());
		navPage.SetDynamicResource(NavigationPage.BarBackgroundColorProperty, "Primary");
		Detail = navPage;
		Flyout = new ContentPage
		{
			Title = "Flyout"
		};
	}
}

public class Issue25114 : ContentPage
{
	public Issue25114()
	{
		Content = new Button()
		{
			AutomationId = "button",
			HeightRequest = 100,
			Text = "Change the Navigation Bar's Color",
			Command = new Command(() =>
			{
				Application.Current.Resources["Primary"] = "#0000FF";
			})
		};
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Application.Current.Resources["Primary"] = "#00FF00";
	}
}