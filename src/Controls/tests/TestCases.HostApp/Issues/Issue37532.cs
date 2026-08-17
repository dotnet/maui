namespace Maui.Controls.Sample.Issues;

// A Shell with a flyout menu containing a "Help" menu item that pushes a Help page with a custom
// BackButtonBehavior. The Help page disables the flyout while it is on screen and pops itself via a
// custom BackButtonBehavior Command, rather than the default Shell back navigation. The title bar
// should show the back arrow (not the hamburger/flyout icon) whenever the Help page is on screen.
[Issue(IssueTracker.Github, 37532, "Shell Sometimes Shows Hamburger Icon Instead of Arrow", PlatformAffected.Android)]
public class Issue37532 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		var mainPage = new Issue37532MainPage();

		var flyoutItem = new FlyoutItem
		{
			Title = "Main Page",
			Items =
			{
				new ShellContent
				{
					Title = "Main Page",
					Content = mainPage,
					Route = "Main"
				}
			}
		};

		Items.Add(flyoutItem);

		// Static "Help" link in the flyout, just like MauiHelp's AppShell.xaml <MenuItem Text="Help" .../>
		Items.Add(new MenuItem
		{
			Text = "Help",
			Command = new Command(OnHelpClicked)
		});
	}

	void OnHelpClicked()
	{
		var pageName = Current.CurrentPage?.GetType().Name ?? "index";
		Current.FlyoutIsPresented = false;
		Current.Navigation.PushAsync(new Issue37532HelpPage(pageName));
	}
}

public class Issue37532MainPage : ContentPage
{
	public Issue37532MainPage()
	{
		Title = "Main Page";
		Content = new Label
		{
			Text = "Issue37532 Main Page",
			AutomationId = "Issue37532MainPageLabel",
			FontSize = 32,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}

public class Issue37532HelpPage : ContentPage
{
	public Issue37532HelpPage(string pageNameParameter = "index")
	{
		var pageName = pageNameParameter;
		Title = pageName + " Help";

		var backCommand = new Command(() => Shell.Current.Navigation.PopAsync());

		Shell.SetBackButtonBehavior(this, new BackButtonBehavior
		{
			Command = backCommand
		});

		Content = new Label
		{
			Text = "Help Page",
			AutomationId = "Issue37532HelpPageLabel",
			FontSize = 32,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}

	protected override void OnAppearing()
	{
		Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
		base.OnAppearing();
	}

	protected override void OnDisappearing()
	{
		Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
		base.OnDisappearing();
	}
}
