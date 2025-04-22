using System;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample
{
	public partial class CollectionViewControlPage : ContentPage
	{
		private CollectionViewViewModel _viewModel;

		public CollectionViewControlPage()
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
			await Navigation.PushAsync(new CollectionViewOptionsPage(_viewModel));
		}
	}
}