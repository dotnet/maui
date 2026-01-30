using Microsoft.Maui.Controls;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maui.Controls.Sample;

public partial class ShellControlPage : Shell
{
    private ShellViewModel _viewModel;
    public ShellContent HomePage => homePage;


    public ShellControlPage()
    {
        InitializeComponent();
        _viewModel = new ShellViewModel();
        BindingContext = _viewModel;

        // Register the TestPage route for navigation
        Routing.RegisterRoute(nameof(ShellOptionsPage), typeof(ShellOptionsPage));
    }

    async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        BindingContext = _viewModel = new ShellViewModel();
        await Navigation.PushAsync(new ShellOptionsPage(_viewModel));
    }

    void OnNavBarAnimTrueClicked(object sender, EventArgs e)
    {
        Shell.SetNavBarIsVisible(this, true);
    }

    void OnNavBarAnimFalseClicked(object sender, EventArgs e)
    {
        Shell.SetNavBarIsVisible(this, false);
    }

    void OnNavBarVisibleClicked(object sender, EventArgs e)
    {
        Shell.SetNavBarIsVisible(this, true);
    }

    void OnNavBarHiddenClicked(object sender, EventArgs e)
    {
        Shell.SetNavBarIsVisible(this, false);
    }

    private async void OnGoToNotAnimated(object sender, EventArgs e)
    {
        var page = new ShellOptionsPage(_viewModel);
        Shell.SetPresentationMode(page, PresentationMode.NotAnimated);
        await Navigation.PushAsync(page);
    }
    private async void OnGoToAnimated(object sender, EventArgs e)
    {
        var page = new ShellOptionsPage(_viewModel);
        Shell.SetPresentationMode(page, PresentationMode.Animated);
        await Navigation.PushAsync(page);
    }
    private async void OnGoToModal(object sender, EventArgs e)
    {
        var page = new ShellOptionsPage(_viewModel);
        Shell.SetPresentationMode(page, PresentationMode.Modal);
        await Navigation.PushModalAsync(page, false);
    }
    private async void OnGoToModalAnimated(object sender, EventArgs e)
    {
        var page = new ShellOptionsPage(_viewModel);
        Shell.SetPresentationMode(page, PresentationMode.ModalAnimated);
        await Navigation.PushModalAsync(page, true);
    }
    private async void OnGoToModalNotAnimated(object sender, EventArgs e)
    {
        var page = new ShellOptionsPage(_viewModel);
        Shell.SetPresentationMode(page, PresentationMode.ModalNotAnimated);
        await Navigation.PushModalAsync(page, false);
    }

    private async void OnGoToHomeClicked(object sender, EventArgs e)
    {
        this.CurrentItem = this.homePage;
    }

    private void SetTitleView()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Auto }
        },
            VerticalOptions = LayoutOptions.Center
        };
        var image = new Image
        {
            Source = "dotnet_bot.png",
            WidthRequest = 28,
            HeightRequest = 28,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };

        var label = new Label
        {
            Text = "Shell TitleView",
            Margin = new Thickness(30, 0, 0, 0),
            VerticalOptions = LayoutOptions.Center,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black
        };

        Grid.SetColumn(label, 1);

        grid.Add(image);
        grid.Add(label);

        Shell.SetTitleView(this, grid);
    }

    private void RemoveTitleView()
    {
        Shell.SetTitleView(this, null);
    }

    private void OnShowTitleViewClicked(object sender, EventArgs e)
    {
        SetTitleView();
    }

    private void OnHideTitleViewClicked(object sender, EventArgs e)
    {
        RemoveTitleView();
    }

}
