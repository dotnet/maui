namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33038, "Layout breaks on first navigation until soft keyboard appears/disappears", PlatformAffected.Android)]
public class Issue33038 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		var startPage = new ShellContent
		{
			Title = "Start",
			Route = "start",
			Content = new Issue33038_StartPage()
		};

		var signInPage = new ShellContent
		{
			Title = "SignIn",
			Route = "signin",
			Content = new Issue33038_SignInPage()
		};

		Items.Add(startPage);
		Items.Add(signInPage);
	}
}

public class Issue33038_StartPage : ContentPage
{
	public Issue33038_StartPage()
	{
		Title = "Start Page";

		var goToSignInButton = new Button
		{
			Text = "Go to SignIn",
			AutomationId = "GoToSignInButton"
		};
		goToSignInButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("//signin", false);

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Spacing = 20,
			Padding = new Thickness(20),
			Children =
			{
				new Label
				{
					Text = "Start Page",
					FontSize = 24,
					AutomationId = "StartPageLabel"
				},
				goToSignInButton
			}
		};
	}
}

public class Issue33038_SignInPage : ContentPage
{
	public Issue33038_SignInPage()
	{
		Shell.SetNavBarIsVisible(this, false);
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		BackgroundColor = Colors.White;

		var backButton = new Button
		{
			Text = "Back to Start",
			AutomationId = "BackButton"
		};
		backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("//start", true);

		Content = new VerticalStackLayout
		{
			Spacing = 16,
			Padding = new Thickness(20),
			Children =
			{
				new Label
				{
					Text = "Sign In Page",
					FontSize = 24,
					BackgroundColor = Colors.Yellow,
					AutomationId = "SignInLabel"
				},
				new Entry
				{
					Placeholder = "Email",
					AutomationId = "EmailEntry"
				},
				new Entry
				{
					Placeholder = "Password",
					IsPassword = true,
					AutomationId = "PasswordEntry"
				},
				new Button
				{
					Text = "Sign in",
					AutomationId = "SignInButton"
				},
				backButton
			}
		};
	}
}