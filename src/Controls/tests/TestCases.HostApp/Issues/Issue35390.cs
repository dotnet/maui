namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35390, "[Android] The flyout icon tint is inconsistent across navigation when Shell.ForegroundColor is set", PlatformAffected.Android)]
public class Issue35390 : TestShell
{
	const string SubPageRoute = "issue35390SubPage";

	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		SetBackgroundColor(this, Colors.DarkBlue);
		SetForegroundColor(this, Colors.White);
		SetTitleColor(this, Colors.White);

		Routing.RegisterRoute(SubPageRoute, typeof(Issue35390SubPage));

		Items.Add(new ShellContent
		{
			Title = "Home",
			Content = new ContentPage
			{
				Content = new VerticalStackLayout
				{
					Spacing = 12,
					Padding = 10,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Children =
					{
						new Label
						{
							AutomationId = "Issue35390Label",
							Text = "After navigating to the sub-page, changing the flyout icon there, and returning back, the flyout icon should respect Shell.ForegroundColor."
						},
						new Button
						{
							AutomationId = "Issue35390GoToSubPage",
							Text = "Go to sub-page",
							Command = new Command(async () => await Shell.Current.GoToAsync(SubPageRoute))
						}
					}
				}
			}
		});
	}
}

public class Issue35390SubPage : ContentPage
{
	public Issue35390SubPage()
	{
		Title = "Sub page";
		Content = new VerticalStackLayout
		{
			Spacing = 12,
			Padding = 10,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Button
				{
					AutomationId = "Issue35390SetIcon",
					Text = "Set flyout icon",
					Command = new Command(() =>
					{
						if (Shell.Current is Shell shell)
						{
							shell.FlyoutIcon = "star_flyout.png";
						}
					})
				},
				new Button
				{
					AutomationId = "Issue35390GoBack",
					Text = "Go back",
					Command = new Command(async () => await Shell.Current.GoToAsync(".."))
				}
			}
		};
	}
}
