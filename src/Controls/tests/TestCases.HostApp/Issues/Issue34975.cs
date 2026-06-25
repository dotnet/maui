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
			IsVisible = false,
		};

		var statusLabel = new Label
		{
			Text = "1. Tap Navigate, then Tap Check Memory",
			FontSize = 14,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "StatusLabel",
		};

		WeakReference[] pageRefs = [];

		navigateButton.Clicked += async (s, e) =>
		{
			Issue34975SecondPage.Instances.Clear();
			await Shell.Current.GoToAsync("Issue34975_second");
			await Shell.Current.GoToAsync("..");
			await Task.Delay(500);
			pageRefs = Issue34975SecondPage.Instances.ToArray();

			// Round 2: on macCatalyst under Appium, the accessibility subsystem holds
			// native refs to the most-recently-visible page. A second navigation
			// replaces those refs, releasing Round 1's page for GC.
			await Shell.Current.GoToAsync("Issue34975_second");
			await Shell.Current.GoToAsync("..");

			checkButton.IsVisible = true;
			statusLabel.Text = "Now tap Check Memory";
		};

		checkButton.Clicked += async (s, e) =>
		{
			statusLabel.Text = "Checking...";
			try
			{
				await GarbageCollectionHelper.WaitForGC(5000, pageRefs);
			}
			catch { }

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
