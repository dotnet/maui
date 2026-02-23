using Microsoft.Maui.ManualTests.Models;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.ManualTests.Tests.CarouselView;

/// <summary>
/// Detail page for displaying monkey information with optional delete functionality.
/// Provides two constructor overloads: one for read-only viewing and one that enables deletion from a collection.
/// </summary>
public partial class MonkeyDetailPage : ContentPage
{
	private Monkey _monkey;
	private ObservableCollection<Monkey> _monkeys;

	/// <summary>
	/// Constructor for viewing only (no delete)
	/// </summary>
	/// <param name="monkey">The monkey to display.</param>
	public MonkeyDetailPage(Monkey monkey)
	{
		InitializeComponent();
		_monkey = monkey;
		BindingContext = monkey;
		Title = monkey.Name;
		deleteButton.IsVisible = false;
	}

	/// <summary>
	/// Constructor for viewing with delete option
	/// </summary>
	/// <param name="monkey">The monkey to display.</param>
	/// <param name="monkeys">The collection of monkeys from which the displayed monkey can be deleted.</param>
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
