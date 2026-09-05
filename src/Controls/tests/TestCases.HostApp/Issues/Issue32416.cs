namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32416, "Shell.FlyoutVerticalScrollMode Disabled does not disable scrolling", PlatformAffected.UWP)]
public class Issue32416 : TestShell
{
	protected override void Init()
	{
		FlyoutVerticalScrollMode = ScrollMode.Disabled;
		FlyoutBehavior = FlyoutBehavior.Locked;
		var flyoutItem = new FlyoutItem
		{
			Title = "Menu"
		};

		// Add a ShellContent
		flyoutItem.Items.Add(new ShellContent
		{
			Title = "Home",
			ContentTemplate = new DataTemplate(typeof(Issue32416_ContentPage))
		});

		// Add FlyoutItem to the Shell
		Items.Add(flyoutItem);

		// Add MenuItems (static links in flyout)
		for (int i = 1; i <= 20; i++)
		{
			Items.Add(new MenuItem
			{
				Text = $"Item {i}"
			});
		}
	}
}

public class Issue32416_ContentPage : ContentPage
{
	public Issue32416_ContentPage()
	{
		Content = new StackLayout
		{
			Children =
			{
				new Label
				{
					Text = "This is the Sample page."
				}
			}
		};
	}
}