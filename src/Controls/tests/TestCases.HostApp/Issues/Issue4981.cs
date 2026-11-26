namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 4981, "DisplayPromptAsync respects initial value selection", PlatformAffected.All)]
public class Issue4981 : ContentPage
{
	const string InitialValue = "1234";

	public Issue4981()
	{
		var preselectButton = new Button
		{
			AutomationId = "PreselectTrueButton",
			Text = "Preselect initial value"
		};

		var noPreselectButton = new Button
		{
			AutomationId = "PreselectFalseButton",
			Text = "Do not preselect initial value"
		};
		var resultLabel = new Label
		{
			AutomationId = "PromptResult",
			IsVisible = false
		};
		noPreselectButton.Clicked += async (_, _) =>
		{
			resultLabel.IsVisible = false;
			resultLabel.Text = await DisplayPromptAsync("Initial selection", "Initial value is not selected", "OK", "Cancel", null, -1, null, InitialValue, PromptTextSelection.None);
			resultLabel.IsVisible = true;
		};
		preselectButton.Clicked += async (_, _) =>
		{
			resultLabel.IsVisible = false;
			resultLabel.Text = await DisplayPromptAsync("Initial selection", "Initial value is selected", "OK", "Cancel", null, -1, null, InitialValue, PromptTextSelection.All);
			resultLabel.IsVisible = true;
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				preselectButton,
				noPreselectButton,
				resultLabel
			}
		};
	}
}
