using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class CollectionViewItemsSourcePage : ContentPage
{
	private CollectionViewViewModel _viewModel;
	public CollectionViewItemsSourcePage()
	{
		InitializeComponent();
		_viewModel = new CollectionViewViewModel();
		_viewModel.PreviousSelectionText = "No previous items";
		_viewModel.CurrentSelectionText = "No current items";
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		_viewModel.ResetItemsSourceProperties();
		await Navigation.PushAsync(new ItemsSourceOptionsPage(_viewModel));
	}

	private void RemoveItems_Clicked(object sender, EventArgs e)
	{
		if (int.TryParse(IndexEntry.Text?.Trim(), out int index))
		{
			_viewModel.RemoveItemAtIndex(index);
		}
		else
		{
			_viewModel.RemoveLastItem();
		}

		IndexEntry.Text = string.Empty;
	}

	private void AddItems_Clicked(object sender, EventArgs e)
	{
		if (int.TryParse(IndexEntry.Text?.Trim(), out int index))
		{
			_viewModel.AddItemAtIndex(index);
		}
		else
		{
			_viewModel.AddSequentialItem();
		}

		IndexEntry.Text = string.Empty;
	}

	private void ReplaceItemsSource_Clicked(object sender, EventArgs e)
	{
		_viewModel.ReplaceItemsSource();
	}

	void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (BindingContext is CollectionViewViewModel vm)
		{
			var previousSelection = e.PreviousSelection.Any()
				? string.Join(", ", e.PreviousSelection.Select(obj =>
				{
					if (obj is CollectionViewViewModel.CollectionViewTestItem item)
						return item.Caption;
					else if (obj is CollectionViewViewModel.CollectionViewTestModelItem modelItem)
						return modelItem.Caption;
					return null;
				}).Where(caption => caption != null))
				: "No previous items";

			var currentSelection = e.CurrentSelection.Any()
				? string.Join(", ", e.CurrentSelection.Select(obj =>
				{
					if (obj is CollectionViewViewModel.CollectionViewTestItem item)
						return item.Caption;
					else if (obj is CollectionViewViewModel.CollectionViewTestModelItem modelItem)
						return modelItem.Caption;
					return null;
				}).Where(caption => caption != null))
				: "No current items";

			vm.PreviousSelectionText = previousSelection;
			vm.CurrentSelectionText = currentSelection;
		}
	}

	private void OnPreSelectionButtonClicked(object sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		var allItems = new List<object>();

		if (_viewModel.ItemsSourceType == ItemsSourceType.ObservableCollectionStringT &&
			_viewModel.ItemsSource is ObservableCollection<CollectionViewViewModel.CollectionViewTestItem> flatItems)
		{
			allItems = flatItems.Cast<object>().ToList();
		}
		else if (_viewModel.ItemsSourceType == ItemsSourceType.ObservableCollectionModelT &&
				 _viewModel.ItemsSource is ObservableCollection<CollectionViewViewModel.CollectionViewTestModelItem> flatModelItems)
		{
			allItems = flatModelItems.Cast<object>().ToList();
		}
		else if (_viewModel.ItemsSourceType == ItemsSourceType.ListT &&
				 _viewModel.ItemsSource is List<CollectionViewViewModel.CollectionViewTestItem> listItems)
		{
			allItems = listItems.Cast<object>().ToList();
		}
		else if (_viewModel.ItemsSourceType == ItemsSourceType.ListModelT &&
				 _viewModel.ItemsSource is List<CollectionViewViewModel.CollectionViewTestModelItem> listModelItems)
		{
			allItems = listModelItems.Cast<object>().ToList();
		}
		else if (_viewModel.ItemsSourceType == ItemsSourceType.GroupedListStringT &&
				 _viewModel.ItemsSource is List<Grouping<string, CollectionViewViewModel.CollectionViewTestItem>> groupedItems)
		{
			allItems = groupedItems.SelectMany(g => g).Cast<object>().ToList();
		}
		else if (_viewModel.ItemsSourceType == ItemsSourceType.GroupedListModelT &&
				 _viewModel.ItemsSource is List<Grouping<string, CollectionViewViewModel.CollectionViewTestModelItem>> groupedModelItems)
		{
			allItems = groupedModelItems.SelectMany(g => g).Cast<object>().ToList();
		}

		var itemsToSelect = allItems.Where(obj =>
		{
			if (obj is CollectionViewViewModel.CollectionViewTestItem item)
				return item.Caption == "Carrot"
					|| item.Caption == "Apple"
					|| item.Caption == "Updated Item 1"
					|| item.Caption == "Updated Item 2";
			else if (obj is CollectionViewViewModel.CollectionViewTestModelItem modelItem)
				return modelItem.Caption == "dotnet_bot.png"
					|| modelItem.Caption == "avatar.png"
					|| modelItem.Caption == "Updated calculator.png"
					|| modelItem.Caption == "Updated avatar.png";
			return false;
		}).ToList();

		if (button.AutomationId == "SingleModePreselection")
		{
			_viewModel.SelectionMode = SelectionMode.Single;
			_viewModel.SelectedItem = itemsToSelect.FirstOrDefault();
		}
		else if (button.AutomationId == "MultipleModePreselection")
		{
			_viewModel.SelectionMode = SelectionMode.Multiple;
			_viewModel.SelectedItems = new ObservableCollection<object>(itemsToSelect);
		}
	}
}