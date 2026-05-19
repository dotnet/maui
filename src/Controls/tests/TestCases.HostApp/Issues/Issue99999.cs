namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 99999, "Shell CoordinatorLayout 0x0 after repeated GoToAsync navigation cycles on Android", PlatformAffected.Android)]
public class Issue99999 : Shell
{
	public Issue99999()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		// Create a Shell with TabBar containing multiple tabs
		// This exercises ShellSectionRenderer.OnHiddenChanged when switching tabs
		var tab1Page = new ContentPage
		{
			Title = "Tab 1",
			Content = CreateMainContent()
		};

		var tab2Page = new ContentPage
		{
			Title = "Tab 2",
			Content = new Label
			{
				Text = "Tab 2 Content",
				AutomationId = "Tab2Label",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		var tabBar = new TabBar();

		var shellSection1 = new ShellSection { Title = "Home", Route = "home" };
		shellSection1.Items.Add(new ShellContent { Content = tab1Page, Route = "main" });
		tabBar.Items.Add(shellSection1);

		var shellSection2 = new ShellSection { Title = "Other", Route = "other" };
		shellSection2.Items.Add(new ShellContent { Content = tab2Page, Route = "tab2" });
		tabBar.Items.Add(shellSection2);

		Items.Add(tabBar);

		// Register the route for push navigation
		Routing.RegisterRoute("navtarget", typeof(Issue99999TargetPage));
	}

	StackLayout CreateMainContent()
	{
		var statusLabel = new Label
		{
			Text = "Ready",
			AutomationId = "StatusLabel",
			FontSize = 16,
			Margin = new Thickness(20, 10)
		};

		// Button that runs rapid push/pop cycles combined with tab switches
		var runCyclesButton = new Button
		{
			Text = "Run Cycles",
			AutomationId = "RunCyclesButton",
			Margin = new Thickness(20, 10)
		};

		runCyclesButton.Clicked += async (s, e) =>
		{
			runCyclesButton.IsEnabled = false;
			statusLabel.Text = "Running...";

			try
			{
				// Cycle pattern: push/pop on tab1, switch to tab2, switch back, repeat
				// This exercises both fragment Hide/Show (tab switch) and fragment Add/Remove (push/pop)
				for (int i = 0; i < 10; i++)
				{
					// Push and pop on current tab
					await Current.GoToAsync("navtarget");
					await Current.GoToAsync("..");

					// Switch to other tab and back (triggers ShellSectionRenderer.OnHiddenChanged)
					await Current.GoToAsync("//other");
					await Current.GoToAsync("//home");
				}

				statusLabel.Text = "Cycles done";
			}
			catch (Exception ex)
			{
				statusLabel.Text = $"ERROR: {ex.Message}";
			}
			finally
			{
				runCyclesButton.IsEnabled = true;
			}
		};

		// Button to navigate to target page (for verification after cycles)
		var navigateButton = new Button
		{
			Text = "Navigate",
			AutomationId = "NavigateButton",
			Margin = new Thickness(20, 10)
		};

		navigateButton.Clicked += async (s, e) =>
		{
			await Current.GoToAsync("navtarget");
		};

		return new StackLayout
		{
			Children =
			{
				statusLabel,
				runCyclesButton,
				navigateButton
			}
		};
	}
}

public class Issue99999TargetPage : ContentPage
{
	public Issue99999TargetPage()
	{
		Title = "Target Page";

		var contentLabel = new Label
		{
			Text = "Target Page Content",
			AutomationId = "TargetPageLabel",
			FontSize = 20,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var goBackButton = new Button
		{
			Text = "Go Back",
			AutomationId = "GoBackButton",
			Margin = new Thickness(20, 10)
		};

		goBackButton.Clicked += async (s, e) =>
		{
			await Shell.Current.GoToAsync("..");
		};

		var sizeInfo = new Label
		{
			Text = "Size: waiting...",
			AutomationId = "SizeInfoLabel",
			FontSize = 14,
			Margin = new Thickness(20, 5)
		};

		var layout = new VerticalStackLayout
		{
			AutomationId = "TargetPageLayout",
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			Children = { contentLabel, sizeInfo, goBackButton }
		};

		Content = layout;

		SizeChanged += (s, e) =>
		{
			sizeInfo.Text = $"Size: {Width}x{Height}";
		};
	}
}
