namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40303, "[Android][Shell] Handler-based Shell shows a Tab's top tab strip over a pushed page after switching bottom tabs, and the strip stays tappable", PlatformAffected.Android)]
public class Issue40303 : TestShell
{
	protected override void Init()
	{
		Routing.RegisterRoute("Issue40303Details", typeof(Issue40303DetailsPage));

		var libraryTab = new Tab
		{
			AutomationId = "Library",
			Title = "Library",
			Route = "library"
		};

		libraryTab.Items.Add(new ShellContent
		{
			AutomationId = "Issue40303AnimeTab",
			Title = "Anime",
			Route = "anime",
			ContentTemplate = new DataTemplate(() => CreatePage(
				"Anime",
				"Anime library",
				"Issue40303AnimeLabel",
				new Button
				{
					Text = "Open details",
					AutomationId = "Issue40303OpenDetails",
					Command = new Command(async () => await Shell.Current.GoToAsync("Issue40303Details"))
				}))
		});

		libraryTab.Items.Add(new ShellContent
		{
			AutomationId = "Issue40303MangaTab",
			Title = "Manga",
			Route = "manga",
			ContentTemplate = new DataTemplate(() => CreatePage(
				"Manga",
				"Manga library",
				"Issue40303MangaLabel"))
		});

		var discoverTab = new Tab
		{
			AutomationId = "Discover",
			Title = "Discover",
			Route = "discover"
		};

		discoverTab.Items.Add(new ShellContent
		{
			ContentTemplate = new DataTemplate(() => CreatePage(
				"Discover",
				"Discover",
				"Issue40303DiscoverLabel"))
		});

		var tabBar = new TabBar();
		tabBar.Items.Add(libraryTab);
		tabBar.Items.Add(discoverTab);
		Items.Add(tabBar);
	}

	static ContentPage CreatePage(string title, string text, string automationId, Button button = null)
	{
		var layout = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 16,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = text,
					AutomationId = automationId
				}
			}
		};

		if (button is not null)
			layout.Add(button);

		return new ContentPage
		{
			Title = title,
			Content = layout
		};
	}
}

public class Issue40303DetailsPage : ContentPage
{
	public Issue40303DetailsPage()
	{
		Title = "Details";
		Content = new Label
		{
			Text = "Pushed details page",
			AutomationId = "Issue40303DetailsLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}
