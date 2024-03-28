using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using FlyoutPage = Microsoft.Maui.Controls.FlyoutPage;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 17022, "UINavigationBar is Translucent", PlatformAffected.iOS)]
public partial class Issue17022 : ContentPage
{
	public Issue17022()
	{
		InitializeComponent();
	}

	readonly string _topOfScreenText = "Green boxview should be behind navbar and touching very top of screen.";
	readonly string _notTopOfScreenText = "Green boxview should NOT be behind navbar and NOT touching very top of screen.";

	async void NewNavigationPagePressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage(true, _notTopOfScreenText);
		var navPage = new NavigationPage(mainPage);
		await Navigation.PushModalAsync(navPage);
	}

	async void NewNavigationPageTransparentPressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage(true, _notTopOfScreenText);
		var navPage = new NavigationPage(mainPage)
		{
			BarBackgroundColor = Colors.Transparent,
		};
		await Navigation.PushModalAsync(navPage);
	}

	async void NewNavigationPageTranslucentPressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage(true, _notTopOfScreenText);
		var navPage = new NavigationPage(mainPage);
		navPage.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(navPage);
	}

	async void NewNavigationPageTransparentTranslucentPressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage(true, _notTopOfScreenText);
		var navPage = new NavigationPage(mainPage)
		{
			BarBackgroundColor = Colors.Transparent,
		};
		navPage.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(navPage);
	}

	async void NewNavigationPageGridPressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage(false, _notTopOfScreenText);
		var navPage = new NavigationPage(mainPage);
		await Navigation.PushModalAsync(navPage);
	}

	async void NewNavigationPageGridTransparentPressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage(false, _notTopOfScreenText);
		var navPage = new NavigationPage(mainPage)
		{
			BarBackgroundColor = Colors.Transparent,
		};
		await Navigation.PushModalAsync(navPage);
	}

	async void NewNavigationPageGridTranslucentPressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage(false, _notTopOfScreenText);
		var navPage = new NavigationPage(mainPage);
		navPage.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(navPage);
	}

	async void NewNavigationPageGridTransparentTranslucentPressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage(false, _topOfScreenText);
		var navPage = new NavigationPage(mainPage)
		{
			BarBackgroundColor = Colors.Transparent,
		};
		navPage.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(navPage);
	}

	async void NewFlyoutPagePressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage(true, _notTopOfScreenText));
		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage() { Title = "FlyoutPage" },
			Detail = detail
		};
		detail.BarBackgroundColor = Colors.Transparent;
		detail.On<iOS>().SetHideNavigationBarSeparator(true);
		detail.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(flyoutPage);
	}

	async void NewFlyoutPageTransparentPressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage(true, _notTopOfScreenText));
		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage() { Title = "FlyoutPage" },
			Detail = detail
		};
		detail.BarBackgroundColor = Colors.Transparent;
		await Navigation.PushModalAsync(flyoutPage);
	}

	async void NewFlyoutPageTranslucentPressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage(true, _notTopOfScreenText));
		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage() { Title = "FlyoutPage" },
			Detail = detail
		};
		detail.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(flyoutPage);
	}

	async void NewFlyoutPageTransparentTranslucentPressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage(true, _notTopOfScreenText));
		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage() { Title = "FlyoutPage" },
			Detail = detail
		};
		detail.BarBackgroundColor = Colors.Transparent;
		detail.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(flyoutPage);
	}

	async void NewFlyoutPageGridPressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage(false, _notTopOfScreenText));
		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage() { Title = "FlyoutPage" },
			Detail = detail
		};
		await Navigation.PushModalAsync(flyoutPage);
	}

	async void NewFlyoutPageGridTransparentPressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage(false, _notTopOfScreenText));
		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage() { Title = "FlyoutPage" },
			Detail = detail
		};
		detail.BarBackgroundColor = Colors.Transparent;
		await Navigation.PushModalAsync(flyoutPage);
	}

	async void NewFlyoutPageGridTranslucentPressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage(false, _notTopOfScreenText));
		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage() { Title = "FlyoutPage" },
			Detail = detail
		};
		detail.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(flyoutPage);
	}
	async void NewFlyoutPageGridTransparentTranslucentPressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage(false, _topOfScreenText));
		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage() { Title = "FlyoutPage" },
			Detail = detail
		};
		detail.BarBackgroundColor = Colors.Transparent;
		detail.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(flyoutPage);
	}

	async void SemiTransparentNavigationPageBackgroundColorPressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage(false, _topOfScreenText);
		var navPage = new NavigationPage(mainPage)
		{
			BarBackgroundColor = Color.FromRgba(100, 100, 100, 50),
		};
		navPage.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(navPage);
	}

	async void SemiTransparentNavigationPageBrushPressed(System.Object sender, System.EventArgs e)
	{
		var mainPage = CreateMainPage(false, _topOfScreenText);
		var navPage = new NavigationPage(mainPage)
		{
			BarBackground = Color.FromRgba(100, 100, 100, 50),
		};
		navPage.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(navPage);
	}

	async void SemiTransparentFlyoutPageBackgroundColorPressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage(false, _topOfScreenText));
		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage() { Title = "FlyoutPage" },
			Detail = detail
		};
		detail.BarBackgroundColor = Color.FromRgba(100, 100, 100, 50);
		detail.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(flyoutPage);
	}

	async void SemiTransparentFlyoutPageBrushPressed(System.Object sender, System.EventArgs e)
	{
		var detail = new NavigationPage(CreateMainPage(false, _topOfScreenText));
		var flyoutPage = new FlyoutPage()
		{
			Flyout = new ContentPage() { Title = "FlyoutPage" },
			Detail = detail
		};
		detail.BarBackground = Color.FromRgba(100, 100, 100, 50);
		detail.On<iOS>().EnableTranslucentNavigationBar();
		await Navigation.PushModalAsync(flyoutPage);
	}

	ContentPage CreateMainPage(bool useSafeArea, string expectedText)
	{
		var mainPage = new ContentPage()
		{
			AutomationId = "PopupMainPage"
		};
		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = new Microsoft.Maui.GridLength(1, Microsoft.Maui.GridUnitType.Star) },
				new RowDefinition { Height = new Microsoft.Maui.GridLength(1, Microsoft.Maui.GridUnitType.Star) },
				new RowDefinition { Height = new Microsoft.Maui.GridLength(1, Microsoft.Maui.GridUnitType.Star) },
			},
		};

		var button = new Button { Text = "Pop Page", AutomationId = "PopPageButton" };
		button.Clicked += PopModalButtonClicked;

		grid.Add(new BoxView { BackgroundColor = Colors.Green, AutomationId = "TopBoxView" }, 0, 0);
		grid.Add(new Label { TextColor = Colors.Black, Margin = new Microsoft.Maui.Thickness(0, 60, 0, 0), HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center, Text = "Can you see me?" }, 0, 0);
		grid.Add(new Label { Text = expectedText }, 0, 1);
		grid.Add(button, 0, 2);
		grid.IgnoreSafeArea = true;

		mainPage.Content = grid;
		mainPage.On<iOS>().SetUseSafeArea(useSafeArea);
		return mainPage;
	}

	async void PopModalButtonClicked(System.Object sender, System.EventArgs e)
	{
		await Navigation.PopModalAsync();
	}
}
