using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class ControlPage : NavigationPage
	{
		private SliderViewModel _viewModel;

		public ControlPage()
		{
			_viewModel = new SliderViewModel();
			BindingContext = _viewModel;
			PushAsync(new ControlMainPage(_viewModel));
		}
	}

	public partial class ControlMainPage : ContentPage
	{
		private SliderViewModel _viewModel;

		public ControlMainPage(SliderViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new SliderViewModel();
			await Navigation.PushAsync(new OptionsPage(_viewModel));
		}
	}
}
