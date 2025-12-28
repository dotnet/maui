namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33038, "Layout breaks on first navigation until soft keyboard appears/disappears", PlatformAffected.Android)]
public class Issue33038 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;
		Items.Add(new ShellContent { Title = "Start", Route = "start", Content = new Issue33038_StartPage() });
		Items.Add(new ShellContent { Title = "SignIn", Route = "signin", Content = new Issue33038_SignInPage() });
	}
}

public class Issue33038_StartPage : ContentPage
{
	public Issue33038_StartPage()
	{
		var goToSignInButton = new Button { Text = "Go to SignIn", AutomationId = "GoToSignInButton" };
		goToSignInButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("//signin", false);

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children = { new Label { Text = "Start Page", AutomationId = "StartPageLabel" }, goToSignInButton }
		};
	}
}

public class Issue33038_SignInPage : ContentPage
{
	public Issue33038_SignInPage()
	{
		Shell.SetNavBarIsVisible(this, false);
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);

		Content = new VerticalStackLayout
		{
			Spacing = 16,
			Padding = new Thickness(20),
			Children =
			{
				new Label { Text = "Sign In Page", BackgroundColor = Colors.Yellow, AutomationId = "SignInLabel" },
				new Entry { Placeholder = "Email", AutomationId = "EmailEntry" }
			}
		};
	}
}