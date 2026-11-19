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
		preselectButton.Clicked += async (_, _) =>
			await DisplayPromptAsync("Initial selection", "Initial value is selected", initialValue: InitialValue, preselectInitialValue: true);

		var noPreselectButton = new Button
		{
			AutomationId = "PreselectFalseButton",
			Text = "Do not preselect initial value"
		};
		noPreselectButton.Clicked += async (_, _) =>
			await DisplayPromptAsync("Initial selection", "Initial value is not selected", initialValue: InitialValue, preselectInitialValue: false);

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				preselectButton,
				noPreselectButton
			}
		};
	}
}
