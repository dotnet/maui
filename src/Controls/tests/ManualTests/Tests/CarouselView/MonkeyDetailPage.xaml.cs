using Microsoft.Maui.ManualTests.Models;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.ManualTests.Tests.CarouselView;

public partial class MonkeyDetailPage : ContentPage
{
	private Monkey _monkey;
	private ObservableCollection<Monkey> _monkeys;

	// Constructor for viewing only (no delete)
	public MonkeyDetailPage(Monkey monkey)
	{
		InitializeComponent();
		_monkey = monkey;
		BindingContext = monkey;
		Title = monkey.Name;
		deleteButton.IsVisible = false;
	}

	// Constructor for viewing with delete option
	public MonkeyDetailPage(Monkey monkey, ObservableCollection<Monkey> monkeys)
	{
		InitializeComponent();
		_monkey = monkey;
		_monkeys = monkeys;
		BindingContext = monkey;
		Title = monkey.Name;
		deleteButton.IsVisible = true;
	}

	private async void OnDeleteClicked(object sender, EventArgs e)
	{
		if (_monkey == null || _monkeys == null)
			return;

		bool confirm = await DisplayAlert("Confirm Delete", 
			$"Are you sure you want to delete '{_monkey.Name}'?", 
			"Delete", "Cancel");

		if (confirm)
		{
			_monkeys.Remove(_monkey);
			await Navigation.PopAsync();
		}
	}
}
