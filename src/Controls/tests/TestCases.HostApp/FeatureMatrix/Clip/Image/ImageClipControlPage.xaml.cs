using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class ImageClipControlPage : NavigationPage
	{
		private ImageClipViewModel _viewModel;
		public ImageClipControlPage()
		{
			_viewModel = new ImageClipViewModel();

			PushAsync(new ImageClipControlMainPage(_viewModel));
		}
	}

	public partial class ImageClipControlMainPage : ContentPage
	{
		private ImageClipViewModel _viewModel;

		public ImageClipControlMainPage(ImageClipViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new ImageClipViewModel();
			await Navigation.PushAsync(new ImageClipOptionsPage(_viewModel));
		}
	}
}