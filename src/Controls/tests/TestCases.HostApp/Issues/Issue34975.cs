namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34975, "Title view memory leak when using Shell TitleView and x Name", PlatformAffected.iOS)]
public class Issue34975 : Shell
{
	public Issue34975()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;
		Routing.RegisterRoute("Issue34975_second", typeof(Issue34975SecondPage));

		var navigateButton = new Button
		{
			Text = "Navigate to Second Page",
			AutomationId = "NavigateButton",
		};

		var checkButton = new Button
		{
			Text = "Check Memory",
			AutomationId = "CheckMemoryButton",
		};

		var statusLabel = new Label
		{
			Text = "",
			AutomationId = "StatusLabel",
		};

		navigateButton.Clicked += async (s, e) =>
		{
			await Shell.Current.GoToAsync("Issue34975_second");
		};

		checkButton.Clicked += async (s, e) =>
		{
			var instances = Issue34975SecondPage.Instances;
			if (instances.Count == 0)
			{
				statusLabel.Text = "Navigate first";
				return;
			}

			// Give the platform disposal pipeline time to complete before collecting.
			// On Android, UpdateTitleView(null) runs asynchronously after back-navigation.
			await Task.Delay(500);

			for (int i = 0; i < 3; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}

			int alive = 0;
			int collected = 0;
			foreach (var wr in instances)
			{
				if (wr.IsAlive)
					alive++;
				else
					collected++;
			}

			statusLabel.Text = $"Total: {instances.Count}, Alive: {alive}, Collected: {collected}. " +
				(alive > 0 ? "LEAK: instances NOT collected!" : "Success");
		};

		var mainPage = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 15,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "1. Tap Navigate\n2. Tap native Back button\n3. Tap Check Memory",
						FontSize = 14,
						HorizontalOptions = LayoutOptions.Center,
					},
					navigateButton,
					checkButton,
					statusLabel,
				}
			}
		};

		Items.Add(new ShellContent
		{
			Content = mainPage,
			Route = "Issue34975_main",
		});
	}
}
