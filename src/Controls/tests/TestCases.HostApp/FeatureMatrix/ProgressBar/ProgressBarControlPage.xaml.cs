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
        private ProgressBar progressBar;

        public ProgressBarControlMainPage(ProgressBarViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
        {
            BindingContext = _viewModel = new ProgressBarViewModel();
            ReinitializeSwitch();
            await Navigation.PushAsync(new ProgressBarOptionsPage(_viewModel));
        }
        private void ReinitializeSwitch()
        {
            ProgressBarGrid.Children.Clear();
            progressBar = new ProgressBar
            {
                AutomationId = "ProgressBarControl",
                VerticalOptions = LayoutOptions.Center
            };

            progressBar.SetBinding(ProgressBar.BackgroundColorProperty, nameof(ProgressBarViewModel.BackgroundColor));
            progressBar.SetBinding(ProgressBar.FlowDirectionProperty, nameof(ProgressBarViewModel.FlowDirection));
            progressBar.SetBinding(ProgressBar.IsVisibleProperty, nameof(ProgressBarViewModel.IsVisible));
            progressBar.SetBinding(ProgressBar.ProgressProperty, nameof(ProgressBarViewModel.Progress));
            progressBar.SetBinding(ProgressBar.ProgressColorProperty, nameof(ProgressBarViewModel.ProgressColor));
            progressBar.SetBinding(ProgressBar.ShadowProperty, nameof(ProgressBarViewModel.Shadow));
            ProgressBarGrid.Children.Add(progressBar);
        }
        private void ProgressToButton_Clicked(object sender, EventArgs e)
        {
            if (progressBar == null)
            {
                progressBarControl.ProgressTo(1, 100, Easing.Default);
            }
            else
            {
                progressBar.ProgressTo(1, 100, Easing.Default);
            }
        }
    }
}
