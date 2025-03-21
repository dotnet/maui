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
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new CollectionViewViewModel();
			await Navigation.PushAsync(new GroupingOptionsPage(_viewModel));
		}
	}
}