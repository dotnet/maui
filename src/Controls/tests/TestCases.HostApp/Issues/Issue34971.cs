namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34971, "Picker CharacterSpacing lost after item selection when Title is set", PlatformAffected.iOS)]
public class Issue34971 : ContentPage
{
	public Issue34971()
	{
		Label instructionLabel = new Label
		{
			AutomationId = "InstructionLabel",
			Text = "The test passes if the character spacing is maintained after manual (Done) and programmatic selection; otherwise, it fails."
		};

        Picker picker = new Picker
        {
            AutomationId = "CharSpacingPicker",
            Title = "Select an Item",
            CharacterSpacing = 10,
            ItemsSource = new List<string> { "Apple", "Banana", "Cherry" },
            SelectedIndex = 1,
        };

		Button selectIndexButton = new Button
		{
			AutomationId = "SelectIndexButton",
			Text = "Select Index 2"
		};
		selectIndexButton.Clicked += (sender, e) => picker.SelectedIndex = 2;

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children = { instructionLabel, picker, selectIndexButton }
		};
	}
}
