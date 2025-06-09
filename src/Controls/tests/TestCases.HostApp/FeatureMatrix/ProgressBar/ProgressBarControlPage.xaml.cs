using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
    public class ProgressBarControlPage : NavigationPage
    {
        private ProgressBarViewModel _viewModel;

        public ProgressBarControlPage()
        {
            _viewModel = new ProgressBarViewModel();
#if ANDROID
            BarTextColor = Colors.White;
#endif
            PushAsync(new ProgressBarControlMainPage(_viewModel));
        }
    }

    public partial class ProgressBarControlMainPage : ContentPage
    {
        private ProgressBarViewModel _viewModel;

        public ProgressBarControlMainPage(ProgressBarViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
        {
            BindingContext = _viewModel = new ProgressBarViewModel();
            await Navigation.PushAsync(new ProgressBarOptionsPage(_viewModel));
        }
    }
}
