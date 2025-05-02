using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Specialized;

namespace Maui.Controls.Sample;

public partial class CollectionViewItemsSourcePage : ContentPage
{
	private ItemsSourceViewModel _viewModel;
	public CollectionViewItemsSourcePage()
	{
		InitializeComponent();
		_viewModel = new ItemsSourceViewModel();
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new ItemsSourceViewModel();
		await Navigation.PushAsync(new ItemsSourceOptionsPage(_viewModel));
	}

	private void RemoveItems_Clicked(object sender, EventArgs e)
	{
		try
		{
			// Check if the input is a valid integer index
			if (int.TryParse(IndexEntry.Text?.Trim(), out int index))
			{
				// Remove item at the specified index
				_viewModel.RemoveItemAtIndex(index);
			}
			else
			{
				// Remove the last item if no input is provided
				_viewModel.RemoveLastItem();
			}
		}
		catch (Exception ex)
		{
			DisplayAlert("Error", ex.Message, "OK");
		}
		finally
		{
			// Clear the index entry field
			IndexEntry.Text = string.Empty;
		}
	}

	private void AddItems_Clicked(object sender, EventArgs e)
	{
		try
		{
			// Check if the input is a valid integer index
			if (int.TryParse(IndexEntry.Text?.Trim(), out int index))
			{
				// Add item at the specified index
				_viewModel.AddItemAtIndex(index);
			}
			else
			{
				// Add a random item if no input is provided
				_viewModel.AddSequentialItem();
			}
		}
		catch (Exception ex)
		{
			DisplayAlert("Error", ex.Message, "OK");
		}
		finally
		{
			// Clear the index entry field
			IndexEntry.Text = string.Empty;
		}
	}

	void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (BindingContext is ItemsSourceViewModel vm)
		{
			var previousSelection = e.PreviousSelection.Any()
				? string.Join(", ", e.PreviousSelection.Select(obj =>
				{
					if (obj is ItemsSourceViewModel.CollectionViewTestItem item)
						return item.Caption;
					else if (obj is ItemsSourceViewModel.CollectionViewTestModelItem modelItem)
						return modelItem.Caption;
					return null;
				}).Where(caption => caption != null))
				: "No previous items";

			var currentSelection = e.CurrentSelection.Any()
				? string.Join(", ", e.CurrentSelection.Select(obj =>
				{
					if (obj is ItemsSourceViewModel.CollectionViewTestItem item)
						return item.Caption;
					else if (obj is ItemsSourceViewModel.CollectionViewTestModelItem modelItem)
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

		// Handle flat items for both CollectionViewTestItem and CollectionViewTestModelItem
		if (_viewModel.ItemsSourceType1 == ItemsSourceType1.ObservableCollectionT &&
			_viewModel.ItemsSource is ObservableCollection<ItemsSourceViewModel.CollectionViewTestItem> flatItems)
		{
			allItems = flatItems.Cast<object>().ToList();
		}
		else if (_viewModel.ItemsSourceType1 == ItemsSourceType1.ObservableCollectionModelT &&
				 _viewModel.ItemsSource is ObservableCollection<ItemsSourceViewModel.CollectionViewTestModelItem> flatModelItems)
		{
			allItems = flatModelItems.Cast<object>().ToList();
		}
		// Handle grouped items for both CollectionViewTestItem and CollectionViewTestModelItem
		else if (_viewModel.ItemsSourceType1 == ItemsSourceType1.GroupedListT &&
				 _viewModel.ItemsSource is List<GroupingItemsSource<string, ItemsSourceViewModel.CollectionViewTestItem>> groupedItems)
		{
			allItems = groupedItems.SelectMany(g => g).Cast<object>().ToList();
		}
		else if (_viewModel.ItemsSourceType1 == ItemsSourceType1.GroupedListModelT &&
				 _viewModel.ItemsSource is List<GroupingItemsSource<string, ItemsSourceViewModel.CollectionViewTestModelItem>> groupedModelItems)
		{
			allItems = groupedModelItems.SelectMany(g => g).Cast<object>().ToList();
		}

		var itemsToSelect = allItems.Where(obj =>
		{
			if (obj is ItemsSourceViewModel.CollectionViewTestItem item)
				return item.Caption == "Carrot" || item.Caption == "Apple";
			else if (obj is ItemsSourceViewModel.CollectionViewTestModelItem modelItem)
				return modelItem.Caption == "dotnet_bot.png" || modelItem.Caption == "avatar.png";
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