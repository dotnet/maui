using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class CollectionViewEmptyViewPage : ContentPage
	{

		private CollectionViewViewModel _viewModel;

		public CollectionViewEmptyViewPage()
		{
			InitializeComponent();
			_viewModel = new CollectionViewViewModel();
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new CollectionViewViewModel();
			await Navigation.PushAsync(new EmptyViewOptionsPage(_viewModel));
		}
	}
}
