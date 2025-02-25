using Maui.Controls.Sample.Issues;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25436, "[.NET 9] Shell Flyout menu not rendering after navigating from a MenuItem page", PlatformAffected.All)]
	public partial class Issue25436 : TestShell
	{
		public Issue25436()
		{
			InitializeComponent();
		}
		protected override void Init()
		{

		}

	}
	public class Issue25436Firstpage : ContentPage
	{
		public Issue25436Firstpage()
		{
			var button = new Button
			{
				Text = "Click here to navigate back",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};
			button.Clicked += async (s, e) => await Shell.Current.GoToAsync("//login");
			Content = button;
			button.AutomationId = "BackButton";
			Title = "_25436 first flyout";
		}
	}

	public class Issue25436LoginPage : ContentPage
	{
		Issue25436INavigationService _navigationService;
		public Issue25436LoginPage(Issue25436INavigationService navigationService)
		{
			_navigationService = navigationService;
			var loginButton = new Button
			{
				Text = "Login"
			};
			loginButton.AutomationId = "Login";
			loginButton.Clicked += OnLoginButtonClicked;

			Content = new StackLayout
			{
				Children =
			{
				new Label { Text = "This is the login page" },
				loginButton
			}
			};
			Title = "_25436 login page";
		}
		private async void OnLoginButtonClicked(object sender, EventArgs e)
		{
			await _navigationService.GoToAsync("//home");
		}
	}

	static class Issue25436Extensions
	{
		public static MauiAppBuilder Issue25436RegisterNavigationService(this MauiAppBuilder builder)
		{
			// Register services
			builder.Services.AddSingleton<Issue25436INavigationService, Issue25436NavigationService>();

			// Register pages
			builder.Services.AddTransient<Issue25436LoginPage>();
			builder.Services.AddSingleton<Issue25436Firstpage>();

			return builder;
		}
	}

	public interface Issue25436INavigationService
	{
		Task GoToAsync(string route);
	}

	public partial class Issue25436NavigationService : Issue25436INavigationService
	{
		public Task GoToAsync(string route)
		{
			if (Shell.Current is null)
			{
				throw new NotSupportedException($"Navigation with the '{nameof(GoToAsync)}' method is currently supported only with a Shell-enabled application.");
			}

			return Shell.Current.GoToAsync(route);
		}
	}

}

