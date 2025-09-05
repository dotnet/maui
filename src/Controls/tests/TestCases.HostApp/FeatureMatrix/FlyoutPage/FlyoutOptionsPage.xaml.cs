
using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class FlyoutOptionsPage : ContentPage
{
    private readonly FlyoutPageViewModel _viewModel;

    public FlyoutOptionsPage(FlyoutPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }


    private void ApplyButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private void OnFlowDirectionChanged(object sender, EventArgs e)
    {
        _viewModel.FlowDirection = FlowDirectionLTR.IsChecked ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
    }

    private void IsVisibleRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!(sender is RadioButton rb) || !rb.IsChecked)
            return;
        _viewModel.IsVisible = rb.Content?.ToString() == "True";
    }

    private void IsEnabledRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!(sender is RadioButton rb) || !rb.IsChecked)
            return;
        _viewModel.IsEnabled = rb.Content?.ToString() == "True";
    }

    private void IsPresentedRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!(sender is RadioButton rb) || !rb.IsChecked)
            return;

        _viewModel.IsPresented = rb.Content?.ToString() == "True";
    }


    private void IsGestureEnabledRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!(sender is RadioButton rb) || !rb.IsChecked)
            return;

        _viewModel.IsGestureEnabled = rb.Content?.ToString() == "True";
    }

    private void OnFlyoutLayoutBehaviorButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string param)
        {
            switch (param)
            {
                case "Default":
                    _viewModel.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Default;
                    break;
                case "Split":
                    _viewModel.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
                    break;
                case "Popover":
                    _viewModel.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
                    break;
                case "SplitOnLandscape":
                    _viewModel.FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnLandscape;
                    break;
                case "SplitOnPortrait":
                    _viewModel.FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnPortrait;
                    break;
            }
        }
    }
    private void OnBackgroundColorChanged(object sender, EventArgs e)
    {
        if (BindingContext is FlyoutPageViewModel vm && sender is Button button && button.Text is string color)
        {
            switch (color)
            {
                case "Red":
                    vm.BackgroundColor = Colors.Red;
                    break;
                case "Gray":
                    vm.BackgroundColor = Colors.Gray;
                    break;
                case "LightYellow":
                    vm.BackgroundColor = Colors.LightYellow;
                    break;
                default:
                    vm.BackgroundColor = Colors.White;
                    break;
            }
        }
    }

    private void OnIconSelected(object sender, EventArgs e)
    {
        if (BindingContext is FlyoutPageViewModel vm && sender is Button button && button.Text is string text)
        {
            if (text == "Coffee")
            {
                vm.IconImageSource = ImageSource.FromFile("coffee.png");
            }
            else if (text == "FontIcon")
            {
                vm.IconImageSource = new FontImageSource
                {
                    FontFamily = "Ion",
                    Glyph = "\uf30c",
                    Size = 24,
                    Color = Colors.Black
                };
            }
        }
    }
    private void OnTitleEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is FlyoutPageViewModel vm)
            vm.Title = e.NewTextValue;
    }
}