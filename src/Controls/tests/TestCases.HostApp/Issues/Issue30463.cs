namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30463, "Picker title is not displayed again", PlatformAffected.iOS)]
public class Issue30463 : ContentPage
{
	public Issue30463()
	{
		VerticalStackLayout stackLayout = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10
		};

		Picker picker = new Picker
		{
			AutomationId = "RegainingPickerTitle",
			Title = "Select an item",
			ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
			SelectedIndex = 1
		};

		Button button = new Button
		{
			AutomationId = "ToggleSelectedIndexBtn",
			Text = "Click to change selected index to -1"
		};
		button.Clicked += (sender, e) =>
		{
			picker.SelectedIndex = -1;
		};

		stackLayout.Children.Add(picker);
		stackLayout.Children.Add(button);
		Content = stackLayout;
	}
}

