namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32219, "[Windows] Current page indicator does not update properly in Shell flyout menu", PlatformAffected.UWP)]
public class Issue32219 : TestShell
{
	protected override void Init()
	{
		var shellContent = new ShellContent
		{
			Route = "home",
			Content = new Issue32219HomePage() { Title = "Home" }
		};

		var flyoutItem = new FlyoutItem
		{
			Title = "Home Page",
			Items = { shellContent }
		};

		Items.Add(flyoutItem);

		var menuItem = new MenuItem
		{
			Text = "Login Page",
			Command = new Command(async () => await Current.GoToAsync("//login"))
		};

		Items.Add(menuItem);

		var tabBar = new TabBar
		{
			Items =
			{
				new ShellContent
				{
					Route = "login",
					Content = new Issue32219LoginPage() { Title = "Login" }
				}
			}
		};

		Items.Add(tabBar);
	}

	class Issue32219HomePage : ContentPage
	{
		public Issue32219HomePage()
		{
			Content = new Grid
			{
				Children =
				{
					new Label
					{
						Text = "Welcome to .NET MAUI Home Page.",
						AutomationId = "homePageLabel",
						FontSize = 18,
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
					},
				}
			};
		}
	}

	class Issue32219LoginPage : ContentPage
	{
		public Issue32219LoginPage()
		{
			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Padding = new Thickness(30),
				Children =
				{
					new Label
					{
						Text = "Welcome to Login Page",
						FontSize =18,
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
					},
					new Button
					{
						Text = "Login",
						AutomationId = "loginButton",
						Command = new Command(async () => await Shell.Current.GoToAsync("//home")),
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
					}
				}
			};
		}
	}
}