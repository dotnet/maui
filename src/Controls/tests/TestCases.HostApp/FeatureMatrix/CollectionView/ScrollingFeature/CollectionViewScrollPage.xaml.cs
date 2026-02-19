using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample;

public partial class CollectionViewScrollPage : ContentPage
{
	private CollectionViewViewModel _viewModel;
	public CollectionViewScrollPage()
	{
		InitializeComponent();
		_viewModel = new CollectionViewViewModel();
		_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT3;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new CollectionViewViewModel();
		_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT3;
		_viewModel.ScrollToPosition = ScrollToPosition.MakeVisible;
		ResetScrollEventLabels();
		await Navigation.PushAsync(new ScrollBehaviorOptionsPage(_viewModel));
	}

	public void ResetScrollEventLabels()
	{
#if !WINDOWS // In Windows, CollectionView automatically moves to the first item when navigating to the page, so there is no need to scroll to the first item again.
		collectionView.ScrollTo(0, position: ScrollToPosition.Start);
#endif
		scrolledEventLabel.Text = "Not Fired";
		scrollToRequestedLabel.Text = "Not Fired";
		remainingItemsThresholdLabel.Text = "Not Fired";
		reorderCompletedLabel.Text = "Not Fired";
		firstIndexLabel.Text = "0";
		centerIndexLabel.Text = "0";
		lastIndexLabel.Text = "0";
		indexLabel.Text = "0";
		itemLabel.Text = "None";
		groupIndexLabel.Text = "-1";
		groupLabel.Text = "None";
	}

	private void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
	{
		scrolledEventLabel.Text = "Fired";
		firstIndexLabel.Text = e.FirstVisibleItemIndex.ToString();
		centerIndexLabel.Text = e.CenterItemIndex.ToString();
		lastIndexLabel.Text = e.LastVisibleItemIndex.ToString();
	}

	private void OnScrollToRequested(object sender, ScrollToRequestEventArgs e)
	{
		scrollToRequestedLabel.Text = "Fired";
		indexLabel.Text = e.Index.ToString();
		itemLabel.Text = (e.Item as CollectionViewViewModel.CollectionViewTestItem)?.Caption ?? "None";
		groupIndexLabel.Text = e.GroupIndex.ToString();
		groupLabel.Text = e.Group?.ToString() ?? "None";
	}

	private void OnScrollToButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsGrouped)
		{
			if (_viewModel.ScrollToByIndexOrItem == "Index")
			{
				ScrollToGroupedByIndex();
			}
			else
			{
				ScrollToGroupedByItem();
			}
		}
		else
		{
			if (_viewModel.ScrollToByIndexOrItem == "Index")
			{
				ScrollToUngroupedByIndex();
			}
			else
			{
				ScrollToUngroupedByItem();
			}
		}
	}

	private void ScrollToGroupedByIndex()
	{
		if (_viewModel.ItemsSource is not List<Grouping<string, CollectionViewViewModel.CollectionViewTestItem>> groupedList)
			return;

		if (_viewModel.GroupIndex < 0 || _viewModel.GroupIndex >= groupedList.Count
		|| _viewModel.ScrollToIndex < 0 || _viewModel.ScrollToIndex >= groupedList[_viewModel.GroupIndex].Count)
			return;

		collectionView.ScrollTo(groupIndex: _viewModel.GroupIndex, index: _viewModel.ScrollToIndex, position: _viewModel.ScrollToPosition, animate: true);
	}

	private void ScrollToGroupedByItem()
	{
		if (_viewModel.ItemsSource is not List<Grouping<string, CollectionViewViewModel.CollectionViewTestItem>> groupedList)
			return;

		// Find the selected group
		var selectedGroup = groupedList.FirstOrDefault(g => g.Key == _viewModel.GroupName);
		if (selectedGroup == null)
			return;

		// Find the item in the group
		var targetItem = selectedGroup.FirstOrDefault(item => item.Caption == _viewModel.ScrollToItem);
		if (targetItem == null)
			return;

		collectionView.ScrollTo(item: targetItem, group: selectedGroup.Key, position: _viewModel.ScrollToPosition, animate: true);
	}

	private void ScrollToUngroupedByIndex()
	{
		collectionView.ScrollTo(index: _viewModel.ScrollToIndex, position: _viewModel.ScrollToPosition, animate: true);
	}

	private void ScrollToUngroupedByItem()
	{
		if (_viewModel.ItemsSource is not ObservableCollection<CollectionViewViewModel.CollectionViewTestItem> items)
			return;

		// Find the item by caption
		var targetItem = items.FirstOrDefault(x => x.Caption == _viewModel.ScrollToItem);
		if (targetItem == null)
			return;

		collectionView.ScrollTo(item: targetItem, position: _viewModel.ScrollToPosition, animate: true);
	}

	private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
	{
		remainingItemsThresholdLabel.Text = "Fired";
	}

	private void OnReorderCompleted(object sender, EventArgs e)
	{
		reorderCompletedLabel.Text = "Fired";
	}
}