using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class ShellFlyoutControlPage : Shell
{
    private ShellViewModel _viewModel;

    public ShellFlyoutControlPage()
    {
        InitializeComponent();
        _viewModel = new ShellViewModel();
        BindingContext = _viewModel;
        this.Navigated += (s, e) => _viewModel.CurrentItemTitle = CurrentItem?.CurrentItem?.Title ?? "Home";
        GenerateMenuItems();
    }

    private void GenerateMenuItems()
    {
        for (int i = 1; i <= 20; i++)
        {
            var menuItem = new MenuItem
            {
                Text = $"MenuItem{i}",
                AutomationId = $"MenuItem{i}"
            };
            this.Items.Add(menuItem);
        }
    }

    private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        BindingContext = _viewModel = new ShellViewModel();
        await Navigation.PushAsync(new ShellFlyoutOptionsPage(_viewModel));
    }

    private void OnOpenFlyoutClicked(object sender, EventArgs e)
    {
        FlyoutIsPresented = !FlyoutIsPresented;
    }

    private void OnSetFlyoutBehaviorClicked(object sender, EventArgs e)
    {
        if (Application.Current.MainPage is Shell shell)
        {
            shell.FlyoutBehavior = FlyoutBehavior.Flyout;
        }
    }

}