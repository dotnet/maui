using System;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 29235, "Incorrect logic of the Picker element when an item in a bind collection is deleted", PlatformAffected.All)]
public class Issue29235 : ContentPage
{
	public Issue29235()
	{
		var pickerItems = new ObservableCollection<string>
		{
			"Cat",
			"Dog",
			"Rabbit",
		};

		var picker = new Picker
		{
			ItemsSource = pickerItems,
			SelectedIndex = 1,
			VerticalOptions = LayoutOptions.Start,
			HorizontalOptions = LayoutOptions.Center,
		};

		var removeButton = new Button
		{
			Margin = new Thickness(20),
			AutomationId = "RemoveButton",
			Text = "Remove Item",
		};

		var selectedItemLabel = new Label
		{
			Margin = new Thickness(20),
			Text = "The test case passes if the currently selected item is 'Dog'."
		};

		removeButton.Clicked += (s, e) =>
		{
			pickerItems.RemoveAt(0);
		};

		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(selectedItemLabel, 0, 0);
		grid.Add(removeButton, 0, 1);
		grid.Add(picker, 0, 2);
		Content = grid;
	}
}

