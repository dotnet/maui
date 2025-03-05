using Microsoft.Maui.Controls;
using System;

namespace  Maui.Controls.Sample
{
    public class EntryControlPage : NavigationPage
	{
		private EntryViewModel _viewModel;

		public EntryControlPage()
		{
			_viewModel = new EntryViewModel();

			PushAsync(new EntryControlMainPage(_viewModel));
		}
	}

	public partial class EntryControlMainPage : ContentPage
	{
		private EntryViewModel _viewModel;

		public EntryControlMainPage(EntryViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
        {
			BindingContext = _viewModel = new EntryViewModel();
            await Navigation.PushAsync(new EntryOptionsPage(_viewModel));
        }
	}
}
