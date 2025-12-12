namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32616, "Shell Flyout appears in Release builds even when FlyoutBehavior=\"Disabled\" (MacCatalyst)", PlatformAffected.macOS)]
public class Issue32616 : Shell
{
	public Issue32616()
	{
		this.FlyoutBehavior = FlyoutBehavior.Disabled;
		
		var label = new Label 
		{ 
			Text = "Flyout is DISABLED. Try to swipe from left edge - flyout should NOT appear.",
			AutomationId = "StatusLabel",
			Margin = new Thickness(20)
		};
		
		var enableButton = new Button 
		{ 
			Text = "Enable Flyout", 
			AutomationId = "EnableButton",
			Margin = new Thickness(20)
		};
		
		var disableButton = new Button 
		{ 
			Text = "Disable Flyout", 
			AutomationId = "DisableButton",
			Margin = new Thickness(20),
			IsEnabled = false
		};
		
		enableButton.Clicked += (s, e) => 
		{
			this.FlyoutBehavior = FlyoutBehavior.Flyout;
			label.Text = "Flyout is ENABLED. Swipe from left edge - flyout SHOULD appear.";
			enableButton.IsEnabled = false;
			disableButton.IsEnabled = true;
		};
		
		disableButton.Clicked += (s, e) => 
		{
			this.FlyoutBehavior = FlyoutBehavior.Disabled;
			label.Text = "Flyout is DISABLED. Try to swipe from left edge - flyout should NOT appear.";
			enableButton.IsEnabled = true;
			disableButton.IsEnabled = false;
		};
		
		var stack = new StackLayout
		{
			Children = { label, enableButton, disableButton }
		};
		
		var contentPage = new ContentPage
		{
			Title = "Issue 32616",
			Content = stack
		};
		
		// Add flyout items so there's something to display when enabled
		var flyoutItem = new FlyoutItem
		{
			Title = "Main",
			Items =
			{
				new ShellContent
				{
					Title = "Page 1",
					Route = "Page1",
					Content = contentPage
				}
			}
		};
		
		Items.Add(flyoutItem);
	}
}
