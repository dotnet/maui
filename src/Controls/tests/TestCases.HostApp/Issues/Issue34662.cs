namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34662, "Shell OnNavigated not called for route navigation", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34662 : Shell
{
	public Issue34662()
	{
		// Page1 and Page2 are sub-routes under DashboardPage -- not ShellItems.
		// This allows absolute navigation to "//DashboardPage/Page1/Page2".
		Routing.RegisterRoute("Page1", typeof(Issue34662_Page1));
		Routing.RegisterRoute("Page2", typeof(Issue34662_Page2));

		Items.Add(new ShellContent
		{
			Title = "Login",
			ContentTemplate = new DataTemplate(() => new Issue34662_LoginPage()),
			Route = "LoginPage"
		});

		Items.Add(new ShellContent
		{
			Title = "Dashboard",
			ContentTemplate = new DataTemplate(() => new Issue34662_DashboardPage()),
			Route = "DashboardPage"
		});
	}

	protected override void OnNavigated(ShellNavigatedEventArgs args)
	{
		base.OnNavigated(args);

		// Capture CurrentState.Location inside OnNavigated.
		// On 10.0.50 (bug): CurrentState = "//DashboardPage" (stale) after GoToAsync("//DashboardPage/Page1/Page2").
		// On 10.0.41 (working): CurrentState = "//DashboardPage/Page1/Page2".
		var currentState = CurrentState?.Location?.OriginalString;

		// OnNavigated fires AFTER Page2.OnAppearing, so push the value directly to Page2's label.
		var page2 = CurrentPage as Issue34662_Page2
			?? (CurrentPage as NavigationPage)?.CurrentPage as Issue34662_Page2;
		page2?.SetCurrentStateLocation(currentState);
	}
}

public class Issue34662_LoginPage : ContentPage
{
	public Issue34662_LoginPage()
	{
		Title = "Login";

		var loginButton = new Button
		{
			Text = "Login -> //DashboardPage/Page1/Page2",
			AutomationId = "LoginButton",
			HorizontalOptions = LayoutOptions.Fill
		};
		loginButton.Clicked += OnLoginClicked;

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(40),
			Spacing = 20,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "Tap to navigate to //DashboardPage/Page1/Page2",
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center
				},
				loginButton
			}
		};
	}

	private async void OnLoginClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//DashboardPage/Page1/Page2");
	}
}

public class Issue34662_DashboardPage : ContentPage
{
	public Issue34662_DashboardPage()
	{
		Title = "Dashboard";
		Content = new Label
		{
			Text = "Dashboard Page",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}

public class Issue34662_Page1 : ContentPage
{
	public Issue34662_Page1()
	{
		Title = "Page 1";
		Content = new Label
		{
			Text = "Page 1",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}

public class Issue34662_Page2 : ContentPage
{
	readonly Label _currentStateLabel;

	public Issue34662_Page2()
	{
		Title = "Page 2";

		// Set by OnNavigated via SetCurrentStateLocation.
		// Shows Shell.CurrentState.Location captured inside OnNavigated.
		// Bug on 10.0.50: shows "//DashboardPage" (stale) instead of "//DashboardPage/Page1/Page2".
		_currentStateLabel = new Label
		{
			AutomationId = "OnNavigatedCurrentStateLabel",
			HorizontalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.Center,
			Text = "(not set)"
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(30),
			Spacing = 16,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = "Page 2", HorizontalOptions = LayoutOptions.Center },
				new Label { Text = "CurrentState.Location inside OnNavigated:", HorizontalOptions = LayoutOptions.Center },
				_currentStateLabel
			}
		};
	}

	// Called directly from Issue34662.OnNavigated -- OnNavigated fires AFTER OnAppearing,
	// so we cannot read CurrentState in OnAppearing and must receive it this way.
	internal void SetCurrentStateLocation(string currentState)
	{
		_currentStateLabel.Text = currentState ?? "(null)";
	}
}
