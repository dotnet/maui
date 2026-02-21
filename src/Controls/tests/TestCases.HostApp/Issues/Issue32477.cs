namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32477, "[Android] Shell flyout does not disable scrolling when FlyoutVerticalScrollMode is set to Disabled", PlatformAffected.Android)]
public class Issue32477 : TestShell
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
			ContentTemplate = new DataTemplate(typeof(Issue32477ContentPage))
		});

		// Add FlyoutItem to the Shell
		Items.Add(flyoutItem);

		// Add MenuItems (static links in flyout)
		for (int i = 1; i <= 30; i++)
		{
			Items.Add(new MenuItem
			{
				Text = $"Item {i}"
			});
		}
	}

	class Issue32477ContentPage : ContentPage
	{
		public Issue32477ContentPage()
		{
			Content = new StackLayout
			{
				Children =
			{
				new Label
				{
					Text = "The flyout menu items should not be scrollable when the scroll mode is disabled."
				}
			}
			};
		}
	}

}
