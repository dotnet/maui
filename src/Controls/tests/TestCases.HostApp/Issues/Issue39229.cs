using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

// Issue: https://github.com/dotnet/maui/issues/39229
// Re-tapping the selected Shell tab while a push is still animating leaves the tab blank.
[Issue(IssueTracker.Github, 39229, "Shell tab re-selection during push leaves content blank", PlatformAffected.iOS)]
public class Issue39229 : TestShell
{
	protected override void Init()
	{
		Routing.RegisterRoute(nameof(Issue39229ChildPage), typeof(Issue39229ChildPage));

		var tabBar = new TabBar();
		tabBar.Items.Add(new ShellContent
		{
			Title = "Home",
			Route = "Issue39229Home",
			AutomationId = "HomeTab",
			ContentTemplate = new DataTemplate(() => new Issue39229HomePage())
		});
		tabBar.Items.Add(new ShellContent
		{
			Title = "Other",
			Route = "Issue39229Other",
			AutomationId = "OtherTab",
			ContentTemplate = new DataTemplate(() => new Issue39229OtherPage())
		});

		Items.Add(tabBar);
	}
}

class Issue39229HomePage : ContentPage
{
	public Issue39229HomePage()
	{
		Title = "Home";
		BackgroundColor = Colors.LightSkyBlue;

		var contentLabel = new Label
		{
			AutomationId = "HomeContent",
			Text = "HOME",
			FontSize = 48,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var openButton = new Button
		{
			AutomationId = "OpenChildPage",
			Text = "Open child page",
			HorizontalOptions = LayoutOptions.Center
		};
		openButton.Clicked += async (_, _) =>
			await Shell.Current.GoToAsync(nameof(Issue39229ChildPage));

		var grid = new Grid
		{
			Padding = 16,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			}
		};

		grid.Add(contentLabel);
		grid.Add(openButton);
		Grid.SetRow(openButton, 1);

		Content = grid;
	}
}

class Issue39229ChildPage : ContentPage
{
	public Issue39229ChildPage()
	{
		Title = "Child";
		BackgroundColor = Colors.LightGreen;
		Content = new Label
		{
			AutomationId = "ChildContent",
			Text = "CHILD",
			FontSize = 48,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}

class Issue39229OtherPage : ContentPage
{
	public Issue39229OtherPage()
	{
		Title = "Other";
		BackgroundColor = Colors.LightYellow;
		Content = new Label
		{
			AutomationId = "OtherContent",
			Text = "OTHER",
			FontSize = 48,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}
