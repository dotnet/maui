namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 99999, "Shell CoordinatorLayout 0x0 after repeated GoToAsync navigation cycles on Android", PlatformAffected.Android)]
public class Issue99999 : Shell
{
	public Issue99999()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		// Pre-register routes
		Routing.RegisterRoute("testroute", typeof(Issue99999TargetPage));
		Routing.RegisterRoute("finalpage", typeof(Issue99999FinalPage));

		var mainPage = new ContentPage
		{
			Title = "Shell Layout Test",
			Content = CreateMainContent()
		};

		var shellContent = new ShellContent
		{
			Content = mainPage,
			Title = "Main",
			Route = "Main"
		};

		var shellSection = new ShellSection { Title = "Home" };
		shellSection.Items.Add(shellContent);

		var shellItem = new ShellItem();
		shellItem.Items.Add(shellSection);
		Items.Add(shellItem);
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

		var runCyclesButton = new Button
		{
			Text = "Run Rapid Cycles",
			AutomationId = "RunCyclesButton",
			Margin = new Thickness(20, 10)
		};

		runCyclesButton.Clicked += async (s, e) =>
		{
			runCyclesButton.IsEnabled = false;
			statusLabel.Text = "Running...";

			try
			{
				// Run rapid push/pop cycles without animation.
				// This exercises the fragment Hide/Show path on the ShellSectionRenderer.
				// The bug manifests when the CoordinatorLayout goes GONE→VISIBLE many times
				// and the Choreographer can't run layout passes between transitions.
				for (int i = 0; i < 20; i++)
				{
					await Current.GoToAsync("testroute", false);
					await Current.GoToAsync("..", false);
				}

				statusLabel.Text = "CyclesDone";
			}
			catch (Exception ex)
			{
				statusLabel.Text = $"ERROR: {ex.GetType().Name}: {ex.Message}";
			}
			finally
			{
				runCyclesButton.IsEnabled = true;
			}
		};

		var navigateButton = new Button
		{
			Text = "Navigate After Cycles",
			AutomationId = "NavigateButton",
			Margin = new Thickness(20, 10)
		};

		navigateButton.Clicked += async (s, e) =>
		{
			await Current.GoToAsync("finalpage");
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
		Content = new Label
		{
			Text = "Target",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}

public class Issue99999FinalPage : ContentPage
{
	public Issue99999FinalPage()
	{
		Title = "Final Page";

		var sizeInfo = new Label
		{
			Text = "Size: waiting...",
			AutomationId = "FinalPageSizeLabel",
			FontSize = 20,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var layout = new VerticalStackLayout
		{
			AutomationId = "FinalPageLayout",
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			Children = { sizeInfo }
		};

		Content = layout;

		// Report page dimensions once laid out (or report -1x-1 if never laid out)
		Loaded += async (s, e) =>
		{
			// Wait 3 seconds — if the bug is present, layout never happens
			await Task.Delay(3000);
			sizeInfo.Text = $"Size: {Width}x{Height}";
		};

		SizeChanged += (s, e) =>
		{
			sizeInfo.Text = $"Size: {Width}x{Height}";
		};
	}
}
