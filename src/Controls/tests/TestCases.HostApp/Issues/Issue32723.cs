namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32723, "Inconsistent appearance of the Title property in Picker control on Windows", PlatformAffected.UWP)]
public class Issue32723 : ContentPage
{
	public Issue32723()
	{
		var picker = new Picker
		{
			Title = "Select Option",
			TitleColor = Colors.Red,
			CharacterSpacing = 8,
			AutomationId = "issue32723Picker",
			WidthRequest = 300,
			HeightRequest = 48,
			HorizontalOptions = LayoutOptions.Center,
			ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" }
		};

		var button = new Button
		{
			Text = "Change Title",
			AutomationId = "issue32723Button",
			HorizontalOptions = LayoutOptions.Center
		};

		button.Clicked += (s, e) =>
		{
			picker.Title = picker.Title == "Select Option" ? "Updated Title" : "Select Option";
			picker.TitleColor = picker.TitleColor == Colors.Red ? Colors.Blue : Colors.Red;
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20,
			Children =
			{
				new Label
				{
					Text = "Test passes if Picker Title displays as placeholder text inside the control.",
					AutomationId = "issue32723Label",
					HorizontalTextAlignment = TextAlignment.Center
				},
				picker,
				button
			}
		};
	}
}
