using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;
using FlyoutPage = Microsoft.Maui.Controls.FlyoutPage;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 17022, "UINavigationBar is Translucent", PlatformAffected.iOS)]
public partial class Issue17022 : ContentPage
{
	public Issue17022()
	{
		InitializeComponent();
	}

	async void NewNavigationPageButtonPressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage();

		var navPage = new NavigationPage(mainPage)
		{
			BarBackgroundColor = Colors.Transparent,
		};
		navPage.On<iOS>().SetHideNavigationBarSeparator(true);
		navPage.On<iOS>().EnableTranslucentNavigationBar();

		await Navigation.PushModalAsync(navPage);
	}

	async void NewFlyoutPageButtonPressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage());

		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage(){Title = "FlyoutPage"},
			Detail = detail
		};

		detail.BarBackgroundColor = Colors.Transparent;
		detail.On<iOS>().SetHideNavigationBarSeparator(true);
		detail.On<iOS>().EnableTranslucentNavigationBar();

		await Navigation.PushModalAsync(flyoutPage);
	}

	ContentPage CreateMainPage ()
	{
		var mainPage = new ContentPage();
		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = new Microsoft.Maui.GridLength(1, Microsoft.Maui.GridUnitType.Star) },
				new RowDefinition { Height = new Microsoft.Maui.GridLength(1, Microsoft.Maui.GridUnitType.Star) },
				new RowDefinition { Height = new Microsoft.Maui.GridLength(1, Microsoft.Maui.GridUnitType.Star) },
			},
		};

		var button = new Button { Text = "Pop Page", AutomationId="PopPageButton" };
		button.Clicked += PopModalButtonClicked;

		grid.Add (new BoxView { BackgroundColor = Colors.Green, AutomationId="TopBoxView" }, 0, 0 );
		grid.Add (new Label { Text = "Green boxview should be behind navbar and touching very top of screen." }, 0, 1 );
		grid.Add (button, 0, 2 );
		grid.IgnoreSafeArea = true;

		mainPage.Content = grid;
		mainPage.On<iOS>().SetUseSafeArea(false);
		return mainPage;
	}

	async void PopModalButtonClicked (System.Object sender, System.EventArgs e)
	{
		await Navigation.PopModalAsync();
	}
}
