using Microsoft.Maui.ManualTests.Models;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.ManualTests.Tests.CarouselView;

public partial class AddMonkeyPage : ContentPage
{
	private ObservableCollection<Monkey> _monkeys;
	private const string DefaultImageUrl = "https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/baboon.jpg";

	public AddMonkeyPage(ObservableCollection<Monkey> monkeys)
	{
		InitializeComponent();
		_monkeys = monkeys;
	}

	private async void OnSaveClicked(object sender, EventArgs e)
	{
		// Validate required fields
		if (string.IsNullOrWhiteSpace(nameEntry.Text))
		{
			await DisplayAlert("Validation Error", "Please enter a name.", "OK");
			return;
		}

		// Create new monkey
		var newMonkey = new Monkey
		{
			Name = nameEntry.Text.Trim(),
			Location = string.IsNullOrWhiteSpace(locationEntry.Text) ? string.Empty : locationEntry.Text.Trim(),
			Details = string.IsNullOrWhiteSpace(detailsEditor.Text) ? string.Empty : detailsEditor.Text.Trim(),
			ImageUrl = string.IsNullOrWhiteSpace(imageUrlEntry.Text) ? string.Empty : imageUrlEntry.Text.Trim()
		};

		// Add to collection
		_monkeys.Add(newMonkey);

		// Show success and navigate back
		await DisplayAlert("Success", $"'{newMonkey.Name}' has been added!", "OK");
		await Navigation.PopAsync();
	}

	private async void OnCancelClicked(object sender, EventArgs e)
	{
		bool confirm = await DisplayAlert("Cancel", 
			"Are you sure you want to cancel? Your changes will be lost.", 
			"Yes", "No");

		if (confirm)
		{
			await Navigation.PopAsync();
		}
	}
}
