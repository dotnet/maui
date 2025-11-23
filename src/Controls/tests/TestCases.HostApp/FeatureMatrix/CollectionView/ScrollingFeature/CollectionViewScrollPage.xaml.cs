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
		scrolledEventLabel.Text = "";
		scrollToRequestedLabel.Text = "";
		remainingItemsThresholdLabel.Text = "";
		await Navigation.PushAsync(new ScrollBehaviorOptionsPage(_viewModel));
	}

	private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
	{
		scrolledEventLabel.Text = "Scrolled Event Fired";
	}

	private void OnCollectionViewScrollToRequested(object sender, ScrollToRequestEventArgs e)
	{
		scrollToRequestedLabel.Text = "ScrollToRequested Event Fired";
	}

	private void OnScrollToButtonClicked(object sender, EventArgs e)
	{
		collectionView.ScrollTo(_viewModel.ScrollToIndex, position: _viewModel.ScrollToPosition, animate: true);
	}

	private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
	{
		remainingItemsThresholdLabel.Text = "RemainingItemsThresholdReached Event Fired";
	}
}