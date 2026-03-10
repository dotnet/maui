using System;
using System.ComponentModel;
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
    private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        BindingContext = _viewModel = new ShellViewModel(); // Create fresh ViewModel to reset all flyout properties to defaults
        await Navigation.PushAsync(new ShellFlyoutOptionsPage(_viewModel));
    }
    private const int MenuItemCount = 40;
    private const string MenuItemPrefix = "MenuItem";
    private void GenerateMenuItems()
    {
        for (int i = 1; i <= MenuItemCount; i++)
        {
            var itemId = $"{MenuItemPrefix}{i}";
            var menuItem = new MenuItem
            {
                Text = itemId,
                AutomationId = itemId
            };
            this.Items.Add(menuItem);
        }
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
