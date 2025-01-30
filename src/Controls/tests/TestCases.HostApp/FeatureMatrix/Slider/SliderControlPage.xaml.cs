using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class SliderControlPage : NavigationPage
	{
		private SliderViewModel _viewModel;

		public SliderControlPage()
		{
			_viewModel = new SliderViewModel();
#if ANDROID
			BarTextColor = Colors.White;
#endif
			PushAsync(new SliderControlMainPage(_viewModel));
		}
	}

	public partial class SliderControlMainPage : ContentPage
	{
		private SliderViewModel _viewModel;

		public SliderControlMainPage(SliderViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new SliderViewModel();
			await Navigation.PushAsync(new SliderOptionsPage(_viewModel));
		}
	}
}
