namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30464, "The CharacterSpacing property on the Picker control is not applied to the title or the items", PlatformAffected.UWP)]
public class Issue30464 : ContentPage
{
	public Issue30464()
	{
		Picker pickerTitleCharacterSpacing = new Picker
		{
			Title = "Select an item",
		};
		pickerTitleCharacterSpacing.Items.Add("Item 1");

		Picker pickerItemCharacterSpacing = new Picker
		{
			ItemsSource = new List<string> { "Item 1", "Item 2" },
			SelectedIndex = 1
		};

		Button applyCharacterSpacingBtn = new Button
		{
			Text = "Apply character spacing",
			AutomationId = "Issue30464Btn"
		};

		applyCharacterSpacingBtn.Clicked += (sender, e) =>
		{
			pickerTitleCharacterSpacing.CharacterSpacing = 14;
			pickerItemCharacterSpacing.CharacterSpacing = 14;
		};

		Label descriptionLabel = new Label
		{
			AutomationId = "Issue30464DescriptionLabel",
			Text = "The test case passes only if character spacing is correctly applied to both the Picker title and items, and is maintained after selecting a different item; otherwise, it fails.",
		};

		Picker pickerSelectionChange = new Picker
		{
			ItemsSource = new List<string> { "Item 1", "Item 2" },
			SelectedIndex = 0,
			CharacterSpacing = 14,
			AutomationId = "Issue30464SelectionChangePicker"
		};

		Button changeSelectionBtn = new Button
		{
			Text = "Change selection",
			AutomationId = "Issue30464ChangeSelectionBtn"
		};
		changeSelectionBtn.Clicked += (sender, e) =>
		{
			pickerSelectionChange.SelectedIndex = 1;
		};

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 20,
			Children =
			{
				pickerTitleCharacterSpacing,
				pickerItemCharacterSpacing,
				applyCharacterSpacingBtn,
				pickerSelectionChange,
				changeSelectionBtn,
				descriptionLabel,
			}
		};
	}
}