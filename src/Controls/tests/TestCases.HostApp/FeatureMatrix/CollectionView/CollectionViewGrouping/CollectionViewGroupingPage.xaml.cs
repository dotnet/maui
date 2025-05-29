using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class CollectionViewGroupingPage : ContentPage
{
	private CollectionViewViewModel _viewModel;

	public CollectionViewGroupingPage()
	{
		InitializeComponent();
		_viewModel = new CollectionViewViewModel();
		_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new CollectionViewViewModel();
		_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT;
		await Navigation.PushAsync(new GroupingOptionsPage(_viewModel));
	}
}