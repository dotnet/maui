namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 111, "M3 dialogs (Alert, ActionSheet, Prompt) render with M2 styles on Android", PlatformAffected.Android)]
public class IssueTempM3Dialog : ContentPage
{
	public IssueTempM3Dialog()
	{
		var resultLabel = new Label
		{
			Text = "Tap any button to show a dialog. When Material 3 is enabled, dialogs should render with M3 styling (rounded corners, tonal surface, M3 typography).",
			AutomationId = "InstructionLabel",
			Margin = new Thickness(20, 10)
		};

		var statusLabel = new Label
		{
			Text = "No dialog shown yet",
			AutomationId = "StatusLabel",
			FontSize = 16,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(20, 5)
		};

		var alertButton = new Button
		{
			Text = "Show Alert Dialog",
			AutomationId = "ShowAlertButton",
			Margin = new Thickness(20, 5)
		};

		alertButton.Clicked += async (_, _) =>
		{
			await DisplayAlert("Alert Title", "This is an alert dialog. With M3 enabled, it should have rounded corners and M3 styling.", "OK");
			statusLabel.Text = "Alert dismissed";
		};

		var actionSheetButton = new Button
		{
			Text = "Show Action Sheet",
			AutomationId = "ShowActionSheetButton",
			Margin = new Thickness(20, 5)
		};

		actionSheetButton.Clicked += async (_, _) =>
		{
			var result = await DisplayActionSheet("Action Sheet Title", "Cancel", "Delete", "Option 1", "Option 2", "Option 3");
			statusLabel.Text = $"ActionSheet result: {result}";
		};

		var promptButton = new Button
		{
			Text = "Show Prompt Dialog",
			AutomationId = "ShowPromptButton",
			Margin = new Thickness(20, 5)
		};

		promptButton.Clicked += async (_, _) =>
		{
			var result = await DisplayPromptAsync("Prompt Title", "Enter a value. With M3 enabled, this dialog should have M3 styling.");
			statusLabel.Text = $"Prompt result: {result ?? "cancelled"}";
		};

		Content = new VerticalStackLayout
		{
			Spacing = 5,
			Padding = new Thickness(0, 20),
			Children =
			{
				resultLabel,
				alertButton,
				actionSheetButton,
				promptButton,
				statusLabel
			}
		};
	}
}
