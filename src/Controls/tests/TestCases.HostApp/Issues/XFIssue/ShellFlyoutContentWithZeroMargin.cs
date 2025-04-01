namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell Flyout Content With Zero Margin offsets correctly", PlatformAffected.All)]
public class ShellFlyoutContentWithZeroMargin : TestShell
{
	public ShellFlyoutContentWithZeroMargin()
	{
	}

	protected override void Init()
	{
		AddFlyoutItem(CreateContentPage(), "Item 1");
		FlyoutContent = new Label()
		{
			Text = "I should not be offset by the safe area",
			AutomationId = "FlyoutLabel",
			Margin = new Thickness(0)
		};
	}

	ContentPage CreateContentPage()
	{
		var layout = new StackLayout()
		{
			new Label()
			{
				AutomationId = "PageLoaded",
				Text = "Open Flyout. Content should not obey safe area",
			}
		};

		return new ContentPage()
		{
			Content = layout
		};
	}
}
