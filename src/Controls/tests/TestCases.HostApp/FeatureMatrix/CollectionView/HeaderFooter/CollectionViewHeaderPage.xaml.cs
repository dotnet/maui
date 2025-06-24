using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;
public partial class CollectionViewHeaderPage : ContentPage
{
	private CollectionViewViewModel _viewModel;

	public CollectionViewHeaderPage()
	{
		InitializeComponent();
		_viewModel = new CollectionViewViewModel();
		_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new CollectionViewViewModel();
		_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
		await Navigation.PushAsync(new HeaderFooterOptionsPage(_viewModel));
	}
}