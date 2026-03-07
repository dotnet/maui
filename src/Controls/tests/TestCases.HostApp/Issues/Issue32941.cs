namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32941, "Label Overlapped by Android Status Bar When Using SafeAreaEdges=Container in .NET MAUI", PlatformAffected.Android)]
public class Issue32941 : TestShell
{
	protected override void Init()
	{
		var shellContent1 = new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = new Issue32941_MainPage()
		};
		var shellContent2 = new ShellContent
		{
			Title = "SignOut",
			Route = "SignOutPage",
			Content = new Issue32941_SignOutPage()
		};
		Items.Add(shellContent1);
		Items.Add(shellContent2);
	}
}

public class Issue32941_MainPage : ContentPage
{
	public Issue32941_MainPage()
	{
		var goToSignOutButton = new Button
		{
			Text = "Go to SignOut",
			AutomationId = "GoToSignOutButton"
		};
		goToSignOutButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("//SignOutPage", false);

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20),
			Children =
			{
				new Label
				{
					Text = "Main Page",
					FontSize = 24,
					AutomationId = "MainPageLabel"
				},
				goToSignOutButton
			}
		};
	}
}

public class Issue32941_SignOutPage : ContentPage
{
	public Issue32941_SignOutPage()
	{
		Shell.SetNavBarIsVisible(this, false);
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		
		var backButton = new Button
		{
			Text = "Back to Main",
			AutomationId = "BackButton"
		};
		backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("//MainPage", true);
		
		Content = new VerticalStackLayout
		{
			BackgroundColor = Colors.White,
			Children =
			{
				new Label
				{
					Text = "SignOut / Session Expiry Page",
					FontSize = 24,
					BackgroundColor = Colors.Yellow,
					AutomationId = "SignOutLabel"
				},
				backButton
			}
		};
	}
}
