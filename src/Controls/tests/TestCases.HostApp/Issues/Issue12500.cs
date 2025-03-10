using Maui.Controls.Sample.Issues;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Controls.TestCases.Issues;

[Issue(IssueTracker.Github, 12500, "Shell does not always raise Navigating event on Windows", PlatformAffected.UWP)]
public class Issue12500 : Shell
{
	public Issue12500()
	{
		this.FlyoutBehavior = FlyoutBehavior.Disabled;
		// Create TabBar
		var tabBar = new TabBar();

		// Add ShellContent for MainPage
		tabBar.Items.Add(new ShellContent
		{
			Title = "Hello, World!",
			Route = "12500",
			ContentTemplate = new DataTemplate(typeof(Issue12500Main))
		});

		// Add ShellContent for EventsPage
		tabBar.Items.Add(new ShellContent
		{
			Title = "Events",
			Route = "12500EventPage",
			ContentTemplate = new DataTemplate(typeof(Issue12500EventPage))
		});

		// Add ShellContent for DummyPage
		tabBar.Items.Add(new ShellContent
		{
			Title = "Sample",
			Route = "12500SamplePage",
			ContentTemplate = new DataTemplate(typeof(Issue12500Sample))
		});

		// Add TabBar to Shell
		this.Items.Add(tabBar);
	}
	protected async override void OnNavigating(ShellNavigatingEventArgs args)
	{
		base.OnNavigating(args);
		await DisplayAlert("Navigating", "Navigating to " + args.Target.Location.OriginalString, "OK");

	}
}
public class Issue12500EventPage : ContentPage
{
	public Issue12500EventPage()
	{
		Title = "Issue12500 Event Page";

		Content = new VerticalStackLayout
		{
			Children =
				{
					new Label
					{
						Text = "This is Issue12500 Event Page.",
						AutomationId = "Issue12500EventPage",
						FontSize = 24,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				}
		};
	}
}

public class Issue12500Sample : ContentPage
{
	public Issue12500Sample()
	{
		Title = "Issue12500 Sample Page";

		Content = new VerticalStackLayout
		{
			Children =
				{
					new Label
					{
						Text = "This is Issue12500 Sample Page.",
						AutomationId="Issue12500SamplePage",
						FontSize = 24,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				}
		};
	}
}

public class Issue12500Main : ContentPage
{
	public Issue12500Main()
	{
		Title = "Issue12500 Main Page";

		Content = new VerticalStackLayout
		{
			Children =
				{
					new Label
					{
						Text = "This is Issue12500 Main Page.",
						AutomationId="Issue12500MainPage",
						FontSize = 24,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				}
		};
	}
}
