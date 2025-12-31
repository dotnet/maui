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
                _viewModel.ShellBackgroundColor = button.Text switch
                {
                    "SkyBlue" => Colors.SkyBlue,
                    "LightGreen" => Colors.LightGreen,
                    _ => _viewModel.ShellBackgroundColor
                };
            }
        }

        // ForegroundColor
        void OnForegroundColorClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _viewModel.ShellForegroundColor = button.Text switch
                {
                    "Brown" => Colors.Brown,
                    "Magenta" => Colors.Magenta,
                    _ => _viewModel.ShellForegroundColor
                };
            }
        }

        // TitleColor
        void OnTitleColorClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _viewModel.ShellTitleColor = button.Text switch
                {
                    "Red" => Colors.Red,
                    "Green" => Colors.Green,
                    _ => _viewModel.ShellTitleColor
                };
            }
        }

        // DisabledColor
        void OnDisabledColorClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _viewModel.ShellDisabledColor = button.Text switch
                {
                    "Gold" => Colors.Gold,
                    "Violet" => Colors.Violet,
                    _ => _viewModel.ShellDisabledColor
                };
            }
        }

        // UnselectedColor
        void OnUnselectedColorClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _viewModel.ShellUnselectedColor = button.Text switch
                {
                    "Maroon" => Colors.Maroon,
                    "Blue" => Colors.Blue,
                    _ => _viewModel.ShellUnselectedColor
                };
            }
        }

        // NavBarIsVisible
        void OnNavBarVisibleClicked(object sender, EventArgs e)
        {
            _viewModel.ShellNavBarIsVisible = true;
        }

        void OnNavBarHiddenClicked(object sender, EventArgs e)
        {
            _viewModel.ShellNavBarIsVisible = false;
        }

        // NavBarHasShadow
        void OnNavBarHasShadowTrueClicked(object sender, EventArgs e)
        {
            _viewModel.NavBarHasShadow = true;
        }

        void OnNavBarHasShadowFalseClicked(object sender, EventArgs e)
        {
            _viewModel.NavBarHasShadow = false;
        }

        // PresentationMode
        void OnPresentationModeClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _viewModel.PresentationMode = button.Text switch
                {
                    "Animated" => PresentationMode.Animated,
                    "NotAnimated" => PresentationMode.NotAnimated,
                    "Modal" => PresentationMode.Modal,
                    "ModalAnimated" => PresentationMode.ModalAnimated,
                    "ModalNotAnimated" => PresentationMode.ModalNotAnimated,
                    _ => _viewModel.PresentationMode
                };
            }
        }

        // NavBarVisibilityAnimationEnabled
        void OnNavBarAnimTrueClicked(object sender, EventArgs e)
        {
            _viewModel.NavBarVisibilityAnimationEnabled = true;
        }

        void OnNavBarAnimFalseClicked(object sender, EventArgs e)
        {
            _viewModel.NavBarVisibilityAnimationEnabled = false;
        }
    }
}
