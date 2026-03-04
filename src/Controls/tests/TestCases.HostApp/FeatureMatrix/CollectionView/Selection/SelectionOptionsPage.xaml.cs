using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class SelectionOptionsPage : ContentPage
	{
		private CollectionViewViewModel _viewModel;

		public SelectionOptionsPage(CollectionViewViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			_viewModel.SelectionChangedEventCount = 0;
			_viewModel.PreviousSelectionText = "No previous items";
			BindingContext = _viewModel;
		}

		private void ApplyButton_Clicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
		private void OnSelectionModeButtonClicked(object sender, EventArgs e)
		{
			if (sender is Button button)
			{
				switch (button.AutomationId)
				{
					case "SelectionModeNone":
						_viewModel.SelectionMode = SelectionMode.None;
						break;
					case "SelectionModeSingle":
						_viewModel.SelectionMode = SelectionMode.Single;
						break;
					case "SelectionModeMultiple":
						_viewModel.SelectionMode = SelectionMode.Multiple;
						break;
					default:
						break;
				}
			}
		}
		private void OnIsGroupedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (IsGroupedFalse.IsChecked)
			{
				_viewModel.IsGrouped = false;
			}
			else if (IsGroupedTrue.IsChecked)
			{
				_viewModel.IsGrouped = true;
			}
		}
		private void OnItemsLayoutChanged(object sender, CheckedChangedEventArgs e)
		{
			if (ItemsLayoutVerticalList.IsChecked)
			{
				_viewModel.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
			}
			else if (ItemsLayoutHorizontalList.IsChecked)
			{
				_viewModel.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
			}
			else if (ItemsLayoutVerticalGrid.IsChecked)
			{
				_viewModel.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
			}
			else if (ItemsLayoutHorizontalGrid.IsChecked)
			{
				_viewModel.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal);
			}
		}

		private void OnPreSelectionButtonClicked(object sender, EventArgs e)
		{
			if (sender is not Button button)
				return;

			var allItems = new List<CollectionViewViewModel.CollectionViewTestItem>();

			if (_viewModel.ItemsSourceType == ItemsSourceType.ObservableCollection5T &&
				_viewModel.ItemsSource is ObservableCollection<CollectionViewViewModel.CollectionViewTestItem> flatItems)
			{
				allItems = flatItems.ToList();
			}
			else if (_viewModel.ItemsSourceType == ItemsSourceType.GroupedListT &&
					 _viewModel.ItemsSource is List<Grouping<string, CollectionViewViewModel.CollectionViewTestItem>> groupedItems)
			{
				allItems = groupedItems.SelectMany(g => g).ToList();
			}

			var itemsToSelect = allItems.Where(item =>
				item.Caption == "Orange" ||
				item.Caption == "Apple" ||
				item.Caption == "Carrot" ||
				item.Caption == "Spinach").ToList();

			if (button.AutomationId == "SingleModePreselection" && _viewModel.SelectionMode == SelectionMode.Single)
			{
				_viewModel.SelectedItem = itemsToSelect.FirstOrDefault();
			}
			else if (button.AutomationId == "MultipleModePreselection" && _viewModel.SelectionMode == SelectionMode.Multiple)
			{
				_viewModel.SelectedItems = new ObservableCollection<object>(itemsToSelect.Cast<object>());
			}
		}

		private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
		{
			if (!(sender is RadioButton radioButton) || !e.Value)
				return;
			else if (radioButton == ItemsSourceObservableCollection5)
				_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
			else if (radioButton == ItemsSourceGroupedList)
				_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
			else if (radioButton == ItemsSourceNone)
				_viewModel.ItemsSourceType = ItemsSourceType.None;
		}
	}
}