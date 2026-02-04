using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;

namespace Maui.Controls.Sample
{
    public partial class ShellOptionsPage : ContentPage
    {
        private readonly ShellViewModel _viewModel;

        public ShellOptionsPage(ShellViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        void OnApplyClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
          
        // BackgroundColor
        void OnBackgroundColorClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _viewModel.BackgroundColor = button.Text switch
                {
                    "SkyBlue" => Colors.SkyBlue,
                    "LightGreen" => Colors.LightGreen,
                    "White" => Colors.White,
                    _ => _viewModel.BackgroundColor
                };
            }
        }

        // ForegroundColor
        void OnForegroundColorClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _viewModel.ForegroundColor = button.Text switch
                {
                    "Brown" => Colors.Brown,
                    "Magenta" => Colors.Magenta,
                    "Purple" => Colors.Purple,
                    _ => _viewModel.ForegroundColor
                };
            }
        }

        // TitleColor
        void OnTitleColorClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _viewModel.TitleColor = button.Text switch
                {
                    "Red" => Colors.Red,
                    "Green" => Colors.Green,
                    "Navy" => Colors.Navy,
                    _ => _viewModel.TitleColor
                };
            }
        }

        // DisabledColor
        void OnDisabledColorClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _viewModel.DisabledColor = button.Text switch
                {
                    "Gold" => Colors.Gold,
                    "Violet" => Colors.Violet,
                    "Silver" => Colors.Silver,
                    _ => _viewModel.DisabledColor
                };
            }
        }

        // UnselectedColor
        void OnUnselectedColorClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _viewModel.UnselectedColor = button.Text switch
                {
                    "Maroon" => Colors.Maroon,
                    "Blue" => Colors.Blue,
                    "Indigo" => Colors.Indigo,
                    _ => _viewModel.UnselectedColor
                };
            }
        }
        private void OnIsEnabledChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton radioButton && e.Value)
            {
                _viewModel.IsEnabled = radioButton.Content.ToString() == "True";
            }
        }

        private void OnIsVisibleChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton radioButton && e.Value)
            {
                _viewModel.IsVisible = radioButton.Content.ToString() == "True";
            }
        }

        private void OnFlowDirectionChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton radioButton && e.Value)
            {
                _viewModel.FlowDirection = radioButton.Content?.ToString() == "LTR" ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
            }
        }

    }
}
