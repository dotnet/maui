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
		scrolledEventLabel.Text = "Not Fired";
		scrollToRequestedLabel.Text = "Not Fired";
		ScrollToIndexLabel.Text = "0";
		ScrollToItemLabel.Text = "null";
		remainingItemsThresholdLabel.Text = "Not Fired";
		reorderCompletedLabel.Text = "Not Fired";
		firstIndexLabel.Text = "0";
		centerIndexLabel.Text = "0";
		lastIndexLabel.Text = "0";
		await Navigation.PushAsync(new ScrollBehaviorOptionsPage(_viewModel));
	}

	private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
	{
		scrolledEventLabel.Text = "Scrolled Event Fired";
		firstIndexLabel.Text = e.FirstVisibleItemIndex.ToString();
		centerIndexLabel.Text = e.CenterItemIndex.ToString();
		lastIndexLabel.Text = e.LastVisibleItemIndex.ToString();
	}

	private void OnCollectionViewScrollToRequested(object sender, ScrollToRequestEventArgs e)
	{
		scrollToRequestedLabel.Text = "ScrollToRequested Event Fired";
		ScrollToIndexLabel.Text = e.Index.ToString();
		ScrollToItemLabel.Text = e.Item?.ToString();
	}

	private void OnScrollToButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.ScrollToByIndexOrItem == "Index")
		{
			collectionView.ScrollTo(index: _viewModel.ScrollToIndex, position: _viewModel.ScrollToPosition, animate: true);
		}
		else if (_viewModel.ScrollToByIndexOrItem == "Item")
		{
			var itemsSource = _viewModel.ItemsSource;

			if (itemsSource is ObservableCollection<CollectionViewViewModel.CollectionViewTestItem> items)
			{
				var targetItem = items.FirstOrDefault(x => x.Caption == _viewModel.ScrollToItem);

				if (targetItem != null)
				{
					collectionView.ScrollTo(targetItem, position: _viewModel.ScrollToPosition, animate: true);
				}
			}
		}
	}

	private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
	{
		remainingItemsThresholdLabel.Text = "RemainingItemsThresholdReached Event Fired";
	}

	private void OnReorderCompleted(object sender, EventArgs e)
	{
		reorderCompletedLabel.Text = $"ReorderCompleted Event Fired";
	}
}