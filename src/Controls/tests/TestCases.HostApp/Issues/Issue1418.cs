namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 1418, "Shell top-tab unselected text should remain visible in Material 3 light theme", PlatformAffected.Android)]
public class Issue1418 : Shell
{
	public Issue1418()
	{
		Tab shellTab = new Tab
		{
			Title = "Top Tabs"
		};

		shellTab.Items.Add(new ShellContent
		{
			Title = "TAB ONE",
			ContentTemplate = new DataTemplate(typeof(Issue1418PageOne))
		});

		shellTab.Items.Add(new ShellContent
		{
			Title = "TAB TWO",
			ContentTemplate = new DataTemplate(typeof(Issue1418PageTwo))
		});

		FlyoutItem flyoutItem = new FlyoutItem
		{
			Title = "Issue1418"
		};

		flyoutItem.Items.Add(shellTab);
		Items.Add(flyoutItem);
	}
}

class Issue1418PageOne : ContentPage
{
	public Issue1418PageOne()
	{
		Title = "Page One";
		Content = new Label
		{
			Text = "The test passes if the unselected tabs are visible in view.",
			AutomationId = "Issue1418PageOneLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}

class Issue1418PageTwo : ContentPage
{
	public Issue1418PageTwo()
	{
		Title = "Page Two";
		Content = new Label
		{
			Text = "The test passes if the unselected tabs are visible in view.",
			AutomationId = "Issue1418PageTwoLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}
