namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25200, "Actionsheet maximum size has been hardcoded on windows, creating display issues", PlatformAffected.UWP)]
public class Issue25200 : TestContentPage
{
	protected override void Init()
	{
		var button = new Button
		{
			Text = "Show ActionSheet with 6 Actions",
			AutomationId = "ShowActionSheetButton"
		};

		button.Clicked += async (sender, e) =>
		{
			await DisplayActionSheet("Actionsheet is set to hardcoded maxheight and maxwidth", "Cancel", null,
				"Option 1/6", "Option 2/6", "Option 3/6", "Option 4/6", "Option 5/6", "Option 6/6");
		};

		var button2 = new Button
		{
			Text = "Show ActionSheet with More Actions",
			AutomationId = "ShowLargeActionSheetButton"
		};

		button2.Clicked += async (sender, e) =>
		{
			await DisplayActionSheet("This actionsheet has many more options to test if all are visible", "Cancel", "Confirm",
				"First Option", "Second Option", "Third Option", "Fourth Option", "Fifth Option", "Sixth Option",
				"Seventh Option", "Eighth Option", "Ninth Option", "Tenth Option", "Eleventh Option", "Twelfth Option");
		};

		var button3 = new Button
		{
			Text = "Show ActionSheet with Long Title",
			AutomationId = "ShowLongTitleActionSheetButton"
		};

		button3.Clicked += async (sender, e) =>
		{
			await DisplayActionSheet("This is a very long title that should wrap properly to multiple lines instead of being truncated or causing horizontal overflow issues like it might on Windows", "Cancel", null,
				"First Option",
				"Second Option - this is a very long option text that should also wrap properly to multiple lines just like it does on Android platform to ensure cross-platform consistency",
				"Third Option",
				"Fourth Option - another long option to test that multiple long options can all wrap properly without causing display issues or horizontal scrolling like the old Windows implementation");
		};

		var instructions = new Label
		{
			Text = "Tap the buttons to show ActionSheets. All options, cancel button, and titles should be visible with proper padding and text wrapping.",
			AutomationId = "InstructionLabel"
		};

		Content = new StackLayout
		{
			Padding = 20,
			Children = { instructions, button, button2, button3 }
		};
	}
}
