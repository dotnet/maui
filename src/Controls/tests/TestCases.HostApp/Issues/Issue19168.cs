namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19168, "iOS Picker dismiss does not work when clicking outside of the Picker", PlatformAffected.iOS | PlatformAffected.MacCatalyst)]
public class Issue19168 : ContentPage
{
	Picker picker;
	public Issue19168()
	{
		picker = new Picker
		{
			ItemsSource = new List<string>
			{
				"Baboon",
				"Capuchin Monkey",
				"Blue Monkey",
				"Squirrel Monkey",
				"Golden Lion Tamarin",
				"Howler Monkey",
				"Japanese Macaque"
			},
			AutomationId = "Picker"
		};

		var counterBtn = new Button
		{
			Text = "Click me",
			AutomationId = "Button",
			VerticalOptions = LayoutOptions.End,
			HorizontalOptions = LayoutOptions.Fill
		};
		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25,
				Children =
				{
					picker,
					counterBtn,
				}
			}
		};
	}
}