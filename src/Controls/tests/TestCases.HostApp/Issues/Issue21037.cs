namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21037, "Switching to an existing page with SetTitleView used in Flyout menu causes crash on Windows", PlatformAffected.UWP)]
public class Issue21037 : FlyoutPage
{
	static Issue21037 flyoutPageInstance;

	public Issue21037()
	{
		flyoutPageInstance = this;
		var menuPage = new _21037MenuPage();
		var homePage = new NavigationPage(new _21037MainPage());
		var secondPage = new NavigationPage(new _21037SecondPage());

		Flyout = menuPage;
		Detail = homePage;
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

		menuPage.MenuItemSelected += (sender, page) =>
		{
			Detail = page switch
			{
				"Home" => homePage,
				"Second" => secondPage,
				_ => homePage
			};
			IsPresented = false;
		};
	}

	public class _21037MenuPage : ContentPage
	{
		public event EventHandler<string> MenuItemSelected;

		public _21037MenuPage()
		{
			Title = "Menu";
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
					{
						new Button
						{
							Text = "Home Page",
							AutomationId = "HomeButton",
							Command = new Command(() => MenuItemSelected?.Invoke(this, "Home"))
						},
						new Button
						{
							Text = "Second Page",
							AutomationId = "SecondButton",
							Command = new Command(() => MenuItemSelected?.Invoke(this, "Second"))
						}
					}
			};
		}
	}

	public class _21037MainPage : ContentPage
	{
		public _21037MainPage()
		{
			Title = "Home";

			var titleView = new Button
			{
				Text = "Main Page Title",
				HeightRequest = 44,
				WidthRequest = 200,
				AutomationId = "MainTitleButton"
			};
			NavigationPage.SetTitleView(this, titleView);

			var openMenuButton = new Button
			{
				Text = "Open Menu",
				AutomationId = "OpenMenuButton"
			};
			openMenuButton.Clicked += (s, e) => flyoutPageInstance.IsPresented = true;

			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
					{
						new Label
						{
							Text = "This is the Home Page with Button TitleView",
							AutomationId = "HomeLabel"
						},
						openMenuButton
					}
			};
		}
	}

	public class _21037SecondPage : ContentPage
	{
		public _21037SecondPage()
		{
			Title = "Second";

			var titleView = new Slider
			{
				HeightRequest = 44,
				WidthRequest = 300,
				AutomationId = "SecondTitleSlider"
			};
			NavigationPage.SetTitleView(this, titleView);

			var openMenuButton = new Button
			{
				Text = "Open Menu",
				AutomationId = "OpenMenuButton"
			};
			openMenuButton.Clicked += (s, e) => flyoutPageInstance.IsPresented = true;

			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
					{
						new Label
						{
							Text = "This is Page 2 with Slider TitleView",
							AutomationId = "SecondLabel"
						},
						openMenuButton
					}
			};
		}
	}
}
