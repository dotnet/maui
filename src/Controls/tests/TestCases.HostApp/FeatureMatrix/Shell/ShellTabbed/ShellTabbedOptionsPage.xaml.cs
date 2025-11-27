using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class ShellTabbedOptionsPage : ContentPage
{
    private ShellViewModel _viewModel;

    public ShellTabbedOptionsPage(ShellViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    private void ApplyButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private void OnTabBarBackgroundColorClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            switch (button.Text)
            {
                case "LightYellow":
                    _viewModel.TabBarBackgroundColor = Colors.LightYellow;
                    break;
                case "LightGreen":
                    _viewModel.TabBarBackgroundColor = Colors.LightGreen;
                    break;
                case "LightBlue":
                    _viewModel.TabBarBackgroundColor = Colors.LightBlue;
                    break;
            }
        }
    }

    private void OnTabBarDisabledColorClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            switch (button.Text)
            {
                case "Maroon":
                    _viewModel.TabBarDisabledColor = Colors.Maroon;
                    break;
                case "Magenta":
                    _viewModel.TabBarDisabledColor = Colors.Magenta;
                    break;
                case "Gold":
                    _viewModel.TabBarDisabledColor = Colors.Gold;
                    break;
            }
        }
    }

    private void OnTabBarForegroundColorClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            switch (button.Text)
            {
                case "Blue":
                    _viewModel.TabBarForegroundColor = Colors.Blue;
                    break;
                case "Red":
                    _viewModel.TabBarForegroundColor = Colors.Red;
                    break;
                case "Green":
                    _viewModel.TabBarForegroundColor = Colors.Green;
                    break;
            }
        }
    }

    private void OnTabBarTitleColorClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            switch (button.Text)
            {
                case "Lavender":
                    _viewModel.TabBarTitleColor = Colors.Lavender;
                    break;
                case "Orange":
                    _viewModel.TabBarTitleColor = Colors.Orange;
                    break;
                case "Purple":
                    _viewModel.TabBarTitleColor = Colors.Purple;
                    break;
            }
        }
    }

    private void OnTabBarUnselectedColorClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            switch (button.Text)
            {
                case "Blue":
                    _viewModel.TabBarUnselectedColor = Colors.Blue;
                    break;
                case "Brown":
                    _viewModel.TabBarUnselectedColor = Colors.Brown;
                    break;
                case "PeachPuff":
                    _viewModel.TabBarUnselectedColor = Colors.PeachPuff;
                    break;
            }
        }
    }

    private void OnTabBarIsVisibleChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is RadioButton radioButton && e.Value)
        {
            _viewModel.TabBarIsVisible = radioButton.Content.ToString() == "True";
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

    private void OnFlowDirectionChanged(object sender, EventArgs e)
    {
        if (sender is RadioButton rb)
        {
            _viewModel.FlowDirection = rb.Content?.ToString() == "LTR" ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
        }
    }

    private void OnCurrentItemClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            if (Application.Current?.MainPage is ShellTabbedControlPage shell)
            {
                switch (button.Text)
                {
                    case "Vegetables":
                        Shell.Current.CurrentItem = shell.VegetablesItem;
                        break;
                    case "Fruits":
                        Shell.Current.CurrentItem = shell.FruitsItem;
                        break;
                    case "Tab2":
                        Shell.Current.CurrentItem = shell.Tab2Item;
                        break;
                    case "Tab3":
                        Shell.Current.CurrentItem = shell.Tab3Item;
                        break;
                    case "Tab4":
                        Shell.Current.CurrentItem = shell.Tab4Item;
                        break;
                    case "Tab5":
                        Shell.Current.CurrentItem = shell.Tab5Item;
                        break;
                    case "Tab6":
                        Shell.Current.CurrentItem = shell.Tab6Item;
                        break;
                }
            }
        }
    }
}