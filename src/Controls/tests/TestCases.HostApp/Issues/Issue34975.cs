namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34975, "Title view memory leak when using Shell TitleView and x Name", PlatformAffected.iOS | PlatformAffected.macOS)]
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
			Text = "Navigate and return twice, then tap Check Memory",
			FontSize = 14,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "StatusLabel",
		};

		Issue34975SecondPage.Instances.Clear();

		navigateButton.Clicked += async (s, e) =>
		{
			await Shell.Current.GoToAsync("Issue34975_second");
		};

		checkButton.Clicked += async (s, e) =>
		{
			statusLabel.Text = "Checking...";
			var pageRefs = Issue34975SecondPage.Instances.Take(1).ToArray();
			if (Issue34975SecondPage.Instances.Count != 2)
			{
				statusLabel.Text = "Expected two rendered page instances";
				return;
			}
			try
			{
				await GarbageCollectionHelper.WaitForGC(5000, pageRefs);
			}
			catch (Exception ex) when (ex.Message == "Assertion timed out")
			{
				// Report the live reference count instead of crashing the UI on a regression.
			}

			var alive = pageRefs.Count(wr => wr.IsAlive);
			statusLabel.Text = $"Still alive: {alive}";
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
					statusLabel,
					navigateButton,
					checkButton,
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
