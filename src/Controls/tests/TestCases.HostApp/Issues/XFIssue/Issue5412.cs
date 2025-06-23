namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5412, "5412 - (NavigationBar disappears on FlyoutPage)", PlatformAffected.UWP)]
public class Issue5412 : TestContentPage
{
	protected override async void Init()
	{
		await Navigation.PushModalAsync(new Issue5412MainPage());
	}
}

public class Issue5412MainPage : FlyoutPage
{
	public Issue5412MainPage()
	{
		var menuBtn = new Button
		{
			Text = "Settings"
		};
		menuBtn.Clicked += (sender, e) =>
		{
			var mdp = ((sender as Button).Parent.Parent as FlyoutPage);
			mdp.Detail.Navigation.PushAsync(new Issue5412SettingPage());
			mdp.IsPresented = false;
		};

		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
		Flyout = new ContentPage
		{
			Content = menuBtn,
			Title = "Menu title"
		};
		Detail = new NavigationPage(new Issue5412IndexPage());
	}
}

public class Issue5412SettingPage : ContentPage
{
	public Issue5412SettingPage()
	{

		Content = new StackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Children = {
				new Label
				{
					Text = "Settings Page",
					FontSize = 22
				},
				new Label
				{
					Text = "Navigate back and check the navbar & menu are still visible.",
					FontSize = 16
				},
			}
		};
	}
};

public class Issue5412IndexPage : ContentPage
{
	public Issue5412IndexPage()
	{
		Content = new StackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Children = {
				new Label
				{
					Text = "Index Page",
					FontSize = 22
				},
				new Label
				{
					Text = "Open the hamburger menu and navigate to settings page",
					FontSize = 16
				},
			}
		};
	}
}