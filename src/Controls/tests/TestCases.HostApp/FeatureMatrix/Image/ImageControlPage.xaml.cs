using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class ImageControlPage : NavigationPage
	{
		private ImageViewModel _viewModel;
		public ImageControlPage()
		{
			_viewModel = new ImageViewModel();

			PushAsync(new ImageControlMainPage(_viewModel));
		}
	}

	public partial class ImageControlMainPage : ContentPage
	{
		private ImageViewModel _viewModel;

		public ImageControlMainPage(ImageViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new ImageViewModel();
			await Navigation.PushAsync(new ImageOptionsPage(_viewModel));
		}
	}
}