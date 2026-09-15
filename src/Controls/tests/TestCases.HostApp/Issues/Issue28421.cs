namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28421, "ToolbarItems Tooltip wrong theme", PlatformAffected.Android)]
public class Issue28421 : NavigationPage
{
	public Issue28421() : base(new Issue28421ContentPage())
	{
		// Set a specific BarTextColor to reproduce the bug
		// Before the fix: tooltip text was red (same as BarTextColor)
		// After the fix: tooltip text should use native Android theme colors
		BarTextColor = Colors.Red;
	}
}

public class Issue28421ContentPage : ContentPage
{
	public Issue28421ContentPage()
	{
		Title = "Issue 28421";

		// Create a toolbar item that will show tooltip on long-press
		var toolbarItem = new ToolbarItem
		{
			Text = "Action",
			IconImageSource = "dotnet_bot.png",
			AutomationId = "ActionBarIcon"
		};
		ToolbarItems.Add(toolbarItem);

		var instructionLabel = new Label
		{
			Text = "Long-press the toolbar item to see the tooltip. The tooltip should use native Android theme colors (not BarTextColor).",
			AutomationId = "InstructionLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Padding = new Thickness(20)
		};

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children = { instructionLabel }
		};
	}
}
