namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35125, "Shell top-tab unselected text should remain visible in Material 3 light theme", PlatformAffected.Android)]
public class Issue35125 : Shell
{
	public Issue35125()
	{
		Tab shellTab = new Tab
		{
			Title = "Top Tabs"
		};

		shellTab.Items.Add(new ShellContent
		{
			Title = "TAB ONE",
			ContentTemplate = new DataTemplate(typeof(Issue35125PageOne))
		});

		shellTab.Items.Add(new ShellContent
		{
			Title = "TAB TWO",
			ContentTemplate = new DataTemplate(typeof(Issue35125PageTwo))
		});

		FlyoutItem flyoutItem = new FlyoutItem
		{
			Title = "Issue35125"
		};

		flyoutItem.Items.Add(shellTab);
		Items.Add(flyoutItem);
	}
}

class Issue35125PageOne : ContentPage
{
	public Issue35125PageOne()
	{
		Title = "Page One";
		Content = new Label
		{
			Text = "The test passes if the unselected tabs are visible in view.",
			AutomationId = "Issue35125PageOneLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}

class Issue35125PageTwo : ContentPage
{
	public Issue35125PageTwo()
	{
		Title = "Page Two";
		Content = new Label
		{
			Text = "The test passes if the unselected tabs are visible in view.",
			AutomationId = "Issue35125PageTwoLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}
