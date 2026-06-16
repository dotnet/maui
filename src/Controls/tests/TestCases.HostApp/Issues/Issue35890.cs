namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35890,
	"HideSoftInputOnTapped=True on one page permanently affects all other pages if NavigatedFrom is never fired",
	PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue35890 : TestShell
{
	ShellContent _loginShellContent = null!;

	protected override void Init()
	{
		// Use ContentTemplate (DataTemplate) to replicate the exact bug scenario from the issue.
		// The page is lazily instantiated, and NavigatedFrom never fires when IsVisible=false
		// + absolute GoToAsync is used.
		_loginShellContent = new ShellContent
		{
			Title = "Login",
			ContentTemplate = new DataTemplate(() => new LoginPage35890(this)),
			Route = "LoginPage35890"
		};

		var homeShellContent = new ShellContent
		{
			Title = "Home",
			ContentTemplate = new DataTemplate(() => new HomePage35890()),
			Route = "HomePage35890"
		};

		Items.Add(_loginShellContent);
		Items.Add(homeShellContent);
	}

	public void NavigateToHome()
	{
		// This is the exact scenario from the issue:
		// hiding the ShellContent via IsVisible=false and navigating with an absolute path
		// does NOT fire NavigatedFrom on the outgoing page.
		_loginShellContent.IsVisible = false;
		_ = Current.GoToAsync("///HomePage35890");
	}

	class LoginPage35890 : ContentPage
	{
		public LoginPage35890(Issue35890 shell)
		{
			HideSoftInputOnTapped = true;
			Title = "Login";

			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Padding = 20,
				Children =
				{
					new Label
					{
						Text = "Login Page — HideSoftInputOnTapped = True",
						AutomationId = "LoginPageLabel"
					},
					new Entry
					{
						Placeholder = "Username",
						AutomationId = "UsernameEntry"
					},
					new Button
					{
						Text = "Login",
						AutomationId = "LoginButton",
						Command = new Command(() => shell.NavigateToHome())
					}
				}
			};
		}
	}

	class HomePage35890 : ContentPage
	{
		public HomePage35890()
		{
			HideSoftInputOnTapped = false;
			Title = "Home";

			var resultLabel = new Label
			{
				AutomationId = "ResultLabel",
				Text = ""
			};

			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Padding = 20,
				Children =
				{
					new Label
					{
						Text = "Home Page — HideSoftInputOnTapped = False",
						AutomationId = "HomePageLabel"
					},
					new Entry
					{
						Placeholder = "Tap to show keyboard",
						AutomationId = "HomeEntry"
					},
					new Button
					{
						Text = "Do Something",
						AutomationId = "HomeButton",
						Command = new Command(() => resultLabel.Text = "Button Tapped")
					},
					resultLabel
				}
			};
		}
	}
}
