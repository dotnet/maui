using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
    public partial class ProgressBarOptionsPage : ContentPage
    {
        private ProgressBarViewModel _viewModel;

        public ProgressBarOptionsPage(ProgressBarViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private void ApplyButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void OnProgressChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(ProgressEntry.Text, out double progress))
            {
                _viewModel.Progress = progress;
            }
        }

        private void ProgressColorButton_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (button.Text == "Green")
                _viewModel.ProgressColor = Colors.Green;
            else if (button.Text == "Red")
                _viewModel.ProgressColor = Colors.Red;
        }

        private void BackgroundColorButton_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (button.Text == "Gray")
                _viewModel.BackgroundColor = Colors.Gray;
            else if (button.Text == "Light Blue")
                _viewModel.BackgroundColor = Colors.LightBlue;
        }

        private void OnIsVisibleCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.IsChecked)
            {
                _viewModel.IsVisible = radioButton.Content.ToString() == "True";
            }
        }

        private void OnFlowDirectionChanged(object sender, EventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.IsChecked)
            {
                _viewModel.FlowDirection = radioButton.Content.ToString() == "LTR" ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
            }
        }

        private void OnShadowCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            if (radioButton != null && radioButton.IsChecked)
            {
                _viewModel.ShadowOpacity = radioButton.Content.ToString() == "True" ? 1f : 0f;
            }
        }
    }
}
