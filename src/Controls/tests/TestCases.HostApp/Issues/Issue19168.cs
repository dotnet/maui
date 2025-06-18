using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19168, "iOS Picker dismiss does not work when clicking outside of the Picker", PlatformAffected.iOS)]
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
		picker.SelectedIndexChanged += (s, e) =>
{
	// Picker was used and closed after selecting an item
	var selected = picker.SelectedItem as string;
	Console.WriteLine($"Picker closed. Selected item: {selected}");
};

		var counterBtn = new Button
		{
			Text = "Click me",
			AutomationId = "Button",
			VerticalOptions = LayoutOptions.End,
			HorizontalOptions = LayoutOptions.Fill
		};
		counterBtn.Clicked += OnCounterClicked;


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
	private void OnCounterClicked(object sender, EventArgs e)
	{
		// Your click handler logic here
	}
}