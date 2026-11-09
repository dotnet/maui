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
		var window = GetWindow();
		if (window is null)
			return;

		_originalPage ??= window.Page;

		window.TitleBar = new TitleBar
		{
			BackgroundColor = Colors.Gold,
			Subtitle = "Window.Page restoration",
			Title = "Issue 38619"
		};
	}

	void OnReplacePageClicked(object sender, EventArgs e)
	{
		var window = GetWindow();
		var originalPage = _originalPage ?? window?.Page;

		if (window is null || originalPage is null)
			throw new InvalidOperationException(
				"The original window page must be captured before replacing it.");

		_originalPage = originalPage;

		var restorePageButton = new Button
		{
			AutomationId = "RestoreOriginalPageButton",
			Text = "Restore Original Page"
		};

		restorePageButton.Clicked += (sender, e) =>
		{
			window.Page = originalPage;
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

	Window GetWindow() =>
		Window ?? Application.Current?.Windows.FirstOrDefault();
}