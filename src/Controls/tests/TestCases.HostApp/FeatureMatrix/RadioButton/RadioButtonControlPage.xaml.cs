using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class RadioButtonControlPage : NavigationPage
	{
		private RadioButtonViewModel _viewModel;

		public RadioButtonControlPage()
		{
			_viewModel = new RadioButtonViewModel();
			PushAsync(new RadioButtonControlMainPage(_viewModel));
		}
	}

	public partial class RadioButtonControlMainPage : ContentPage
	{
		private RadioButtonViewModel _viewModel;

		public RadioButtonControlMainPage(RadioButtonViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new RadioButtonViewModel();
			await Navigation.PushAsync(new RadioButtonOptionsPage(_viewModel));
		}
	}
}
