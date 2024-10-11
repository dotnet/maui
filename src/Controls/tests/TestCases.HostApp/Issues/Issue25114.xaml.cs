namespace Maui.Controls.Sample.Issues;
using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

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


public partial class Issue25114 : ContentPage
{
	public Issue25114()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Application.Current.Resources["Primary"] = "#00FF00";
	}

	private void OnButtonClicked(object sender, EventArgs e)
	{
		Application.Current.Resources["Primary"] = "#0000FF";
	}
}