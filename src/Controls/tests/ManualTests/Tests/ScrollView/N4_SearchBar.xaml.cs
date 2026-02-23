using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.Models;
using Microsoft.Maui.ManualTests.ViewModels;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.ManualTests.Tests.ScrollView;

[Test(
	id: "N4",
	title: "Search Bar",
	category: Category.ScrollView)]
public partial class N4_SearchBar: ContentPage
{
	private MonkeysViewModel _viewModel;
	private ObservableCollection<Monkey> _allMonkeys;

	public N4_SearchBar()
	{
		InitializeComponent();
		
		_viewModel = new MonkeysViewModel();
		_allMonkeys = new ObservableCollection<Monkey>(_viewModel.Monkeys);
		
		BindingContext = _viewModel;
	}

	private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
	{
		var searchText = e.NewTextValue?.ToLowerInvariant() ?? string.Empty;
		
		if (string.IsNullOrWhiteSpace(searchText))
		{
			// Show all monkeys
			_viewModel.Monkeys.Clear();
			foreach (var monkey in _allMonkeys)
			{
				_viewModel.Monkeys.Add(monkey);
			}
		}
		else
		{
			// Filter monkeys by name or location
			var filtered = _allMonkeys.Where(m => 
				m.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
			
			_viewModel.Monkeys.Clear();
			foreach (var monkey in filtered)
			{
				_viewModel.Monkeys.Add(monkey);
			}
		}
	}
}
