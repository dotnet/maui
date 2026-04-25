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
			IsVisible = false,
		};

		var statusLabel = new Label
		{
			Text = "1. Tap Navigate, then Tap Check Memory",
			FontSize = 14,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "StatusLabel",
		};

		navigateButton.Clicked += async (s, e) =>
		{
			Issue34975SecondPage.Instances.Clear();

			await Shell.Current.GoToAsync("Issue34975_second");

			await Shell.Current.GoToAsync("..");

			// A small delay lets that continuation run before we expose CheckMemoryButton.
			await Task.Delay(500);

			checkButton.IsVisible = true;
			statusLabel.Text = "Now tap Check Memory";
		};

		checkButton.Clicked += async (s, e) =>
		{
			var instances = Issue34975SecondPage.Instances;
			if (instances.Count == 0)
			{
				statusLabel.Text = "Navigate first";
				return;
			}

			statusLabel.Text = "Checking...";
			try
			{
				await GarbageCollectionHelper.WaitForGC(5000, instances.ToArray());
				statusLabel.Text = "Test passed";
			}
			catch
			{
				statusLabel.Text = "Memory Leak Detected";
			}
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
