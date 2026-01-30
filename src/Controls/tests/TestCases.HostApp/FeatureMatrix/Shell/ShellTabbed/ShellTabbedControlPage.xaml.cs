using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class ShellTabbedControlPage : Shell
{
    private ShellViewModel _viewModel;
    public ShellContent VegetablesItem => vegetablesItem;
    public ShellContent FruitsItem => fruitsItem;
    public ShellContent Tab1Item => tab1Item;
    public ShellContent Tab3Item => tab3Item;
    public ShellContent Tab4Item => tab4Item;
    public ShellContent Tab5Item => tab5Item;
    public ShellContent Tab6Item => tab6Item;

    public ShellTabbedControlPage()
    {
        InitializeComponent();
        _viewModel = new ShellViewModel();
        BindingContext = _viewModel;
        Navigated += (s, e) => _viewModel.CurrentItemTitle = CurrentItem?.CurrentItem?.Title ?? "Unknown";
    }

    private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        BindingContext = _viewModel = new ShellViewModel();
        await Navigation.PushAsync(new ShellTabbedOptionsPage(_viewModel));
    }

    private void OnGoToTab1Clicked(object sender, EventArgs e)
    {
        this.CurrentItem = this.tab1Item;
    }
}