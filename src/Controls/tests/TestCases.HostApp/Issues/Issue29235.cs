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
			"Cow",
			"Sheep",
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

		var insertButton = new Button
		{
			Margin = new Thickness(20),
			AutomationId = "InsertButton",
			Text = "Insert Item",
		};

		var selectedItemLabel = new Label
		{
			Margin = new Thickness(20),
			Text = $"Selected Item : Dog",
			AutomationId = "SelectedItemLabel",
		};

		removeButton.Clicked += (s, e) =>
		{
			picker.SelectedIndex = 3;
			pickerItems.RemoveAt(2);
			selectedItemLabel.Text = $"Selected Item: {picker.SelectedItem}";
		};

		insertButton.Clicked += (s, e) =>
		{
			pickerItems.Insert(0, "Goat");
			selectedItemLabel.Text = $"Selected Item: {picker.SelectedItem}";
		};

		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			},

			ColumnDefinitions = new ColumnDefinitionCollection
			{
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = GridLength.Auto },
			}
		};

		grid.Add(removeButton, 0, 0);
		grid.Add(insertButton, 1, 0);
		grid.Add(selectedItemLabel, 0, 1);
		grid.Add(picker, 0, 2);
		Content = grid;
	}
}