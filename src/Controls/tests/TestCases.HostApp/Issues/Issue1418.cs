namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 1418, "Shell flyout should honor Material 3 surface color instead of hardcoded background_light", PlatformAffected.Android)]
public class Issue1418 : Shell
{
	public Issue1418()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		ContentPage mainPage = new ContentPage
		{
			Title = "M3 Flyout",
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(16),
				Children =
				{
					new Label
					{
						Text = "Open the flyout to verify M3 background rendering.",
						AutomationId = "Issue1418MainLabel"
					},
					new Button
					{
						Text = "Open Flyout",
						AutomationId = "OpenFlyoutButton",
						Command = new Command(() => FlyoutIsPresented = true)
					}
				}
			}
		};

		ContentPage secondPage = new ContentPage
		{
			Title = "Second",
			Content = new Label
			{
				Text = "Issue1418 second page",
				AutomationId = "Issue1418SecondLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		Items.Add(new FlyoutItem
		{
			Title = "Groceries One",
			Icon = "groceries.png",
			Items =
			{
				new ShellSection
				{
					Items = { new ShellContent { Route = "issue1418main", Content = mainPage } }
				}
			}
		});

		Items.Add(new FlyoutItem
		{
			Title = "Groceries Two",
			Icon = "groceries.png",
			Items =
			{
				new ShellSection
				{
					Items = { new ShellContent { Route = "issue1418second", Content = secondPage } }
				}
			}
		});
	}
}
