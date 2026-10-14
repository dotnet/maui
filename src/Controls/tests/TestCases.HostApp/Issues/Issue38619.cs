namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38619, "Window.Page is blank after restoring a page with a custom TitleBar", PlatformAffected.UWP | PlatformAffected.Android)]

public class Issue38619 : Shell
{
	public Issue38619()
	{
		Items.Add(new ShellContent
		{
			Content = new Issue38619Page()
		});
	}
}

public class Issue38619Page : ContentPage
{
	Page _originalPage;

	public Issue38619Page()
	{
		Title = "Issue 38619";

		var replacePageButton = new Button
		{
			AutomationId = "ReplaceWindowPageButton",
			Text = "Replace Window Page"
		};
		replacePageButton.Clicked += OnReplacePageClicked;

		Content = new VerticalStackLayout
		{
			Padding = 30,
			Spacing = 20,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					AutomationId = "OriginalPageRenderedLabel",
					HorizontalOptions = LayoutOptions.Center,
					Text = "Original page rendered"
				},
				replacePageButton
			}
		};

		Loaded += OnLoaded;
	}

	void OnLoaded(object sender, EventArgs e)
	{
		if (Window is null)
			throw new InvalidOperationException(
				"The issue page must be attached to a window.");

		_originalPage ??= Window.Page;

		Window.TitleBar = new TitleBar
		{
			BackgroundColor = Colors.Gold,
			Subtitle = "Window.Page restoration",
			Title = "Issue 38619"
		};
	}

	void OnReplacePageClicked(object sender, EventArgs e)
	{
		if (Window is null || _originalPage is null)
			throw new InvalidOperationException(
				"The original window page must be captured before replacing it.");

		var window = Window;

		var restorePageButton = new Button
		{
			AutomationId = "RestoreOriginalPageButton",
			Text = "Restore Original Page"
		};

		restorePageButton.Clicked += (sender, e) =>
		{
			window.Page = _originalPage;
		};

		window.Page = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = 30,
				Spacing = 20,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						AutomationId = "ReplacementPageRenderedLabel",
						HorizontalOptions = LayoutOptions.Center,
						Text = "Replacement page rendered"
					},
					restorePageButton
				}
			}
		};
	}
}