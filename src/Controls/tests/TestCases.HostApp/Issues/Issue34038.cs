namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 34038, "[macOS] IsEnabled property false not working on MenuBarItem", PlatformAffected.macOS | PlatformAffected.UWP)]
public class Issue34038 : Shell
{
	public Issue34038()
	{
		// Disable flyout to hide hamburger menu
		FlyoutBehavior = FlyoutBehavior.Disabled;

		// Register routes
		Routing.RegisterRoute("testpage", typeof(Issue34038TestPage));

		// Landing page with navigation button
		var navigateButton = new Button
		{
			Text = "Go to Issue34038 Test",
			AutomationId = "Issue34038NavigateButton"
		};

		navigateButton.Clicked += async (sender, e) =>
		{
			await Shell.Current.GoToAsync("testpage", animate: false);
		};

		var landingPage = new ContentPage
		{
			Title = "Issue34038",
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 12,
				Children =
				{
					new Label { Text = "MenuBarItem IsEnabled Test" },
					navigateButton
				}
			}
		};

		// Add the landing page to the Shell structure directly
		var shellItem = new ShellItem();
		var shellSection = new ShellSection();
		var shellContent = new ShellContent
		{
			Content = landingPage,
			Title = "Main",
			Route = "Main"
		};

		shellSection.Items.Add(shellContent);
		shellItem.Items.Add(shellSection);
		Items.Add(shellItem);
	}
}

// Test page with MenuBarItem IsEnabled functionality
public class Issue34038TestPage : ContentPage
{
	const string InitialStatus = "Failure";

	MenuBarItem _menuBarItem;
	Label _statusLabel;

	public Issue34038TestPage()
	{
		Title = "MenuBarItem IsEnabled Test";

		// MenuBarItem IsEnabled test setup
		_menuBarItem = new MenuBarItem
		{
			Text = "Issue34038MenuBarItem",
			AutomationId = "Issue34038MenuBarItem",
			IsEnabled = false
		};

		var flyoutItem = new MenuFlyoutItem
		{
			Text = "Issue34038MenuFlyoutItem",
			AutomationId = "Issue34038MenuFlyoutItem"
		};

		flyoutItem.Clicked += (_, _) => _statusLabel.Text = "Success";
		_menuBarItem.Add(flyoutItem);
		MenuBarItems.Add(_menuBarItem);

		_statusLabel = new Label
		{
			AutomationId = "Issue34038StatusLabel",
			Text = InitialStatus
		};

		var isEnabledSwitch = new Switch
		{
			AutomationId = "Issue34038MenuEnabledSwitch",
			IsToggled = false
		};

		_menuBarItem.IsEnabled = isEnabledSwitch.IsToggled;
		isEnabledSwitch.Toggled += (_, e) => _menuBarItem.IsEnabled = e.Value;

		var instructions = new Label
		{
			Text = "Toggle the switch off and verify the menu bar item cannot be opened.",
			LineBreakMode = LineBreakMode.WordWrap
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 12,
			Children =
			{
				new Label { Text = "MenuBarItem IsEnabled regression test" },
				instructions,
				new HorizontalStackLayout
				{
					Spacing = 10,
					Children =
					{
						new Label { Text = "MenuBarItem IsEnabled" },
						isEnabledSwitch
					}
				},
				_statusLabel
			}
		};
	}
}