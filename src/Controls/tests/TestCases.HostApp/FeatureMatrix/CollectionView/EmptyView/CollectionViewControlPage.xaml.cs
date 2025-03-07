using System;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample
{
	public class CollectionViewControlPage : NavigationPage
	{
		private CollectionViewViewModel _viewModel;

		public CollectionViewControlPage()
		{
			_viewModel = new CollectionViewViewModel();
			PushAsync(new CollectionViewControlMainPage(_viewModel));
		}
	}

	public partial class CollectionViewControlMainPage : ContentPage
	{
		private CollectionViewViewModel _viewModel;

		public CollectionViewControlMainPage(CollectionViewViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new CollectionViewViewModel();
			await Navigation.PushAsync(new CollectionViewOptionsPage(_viewModel));
		}
	}
}
