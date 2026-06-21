using Microsoft.Maui.Controls;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maui.Controls.Sample;

public partial class ShellControlPage : Shell
{
    private ShellViewModel _viewModel;
    public ShellContent HomePage => homePage;
    private const string NotAnimatedRoute = "shell-feature/notanimated";
    private const string AnimatedRoute = "shell-feature/animated";
    private const string ModalRoute = "shell-feature/modal";
    private const string ModalAnimatedRoute = "shell-feature/modalanimated";
    private const string ModalNotAnimatedRoute = "shell-feature/modalnotanimated";


    public ShellControlPage()
    {
        InitializeComponent();
        _viewModel = new ShellViewModel();
        BindingContext = _viewModel;
        Routing.RegisterRoute(NotAnimatedRoute, typeof(NotAnimatedPresentationPage));
        Routing.RegisterRoute(AnimatedRoute, typeof(AnimatedPresentationPage));
        Routing.RegisterRoute(ModalRoute, typeof(ModalPresentationPage));
        Routing.RegisterRoute(ModalAnimatedRoute, typeof(ModalAnimatedPresentationPage));
        Routing.RegisterRoute(ModalNotAnimatedRoute, typeof(ModalNotAnimatedPresentationPage));
    }
    async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        BindingContext = _viewModel = new ShellViewModel();
        await Navigation.PushAsync(new ShellOptionsPage(_viewModel));
    }
    void OnNavBarAnimTrueClicked(object sender, EventArgs e)
    {
        Shell.SetNavBarVisibilityAnimationEnabled(this, true);
    }
    void OnNavBarAnimFalseClicked(object sender, EventArgs e)
    {
        Shell.SetNavBarVisibilityAnimationEnabled(this, false);
    }
    void OnNavBarVisibleClicked(object sender, EventArgs e)
    {
        Shell.SetNavBarIsVisible(this, true);
    }
    void OnNavBarHiddenClicked(object sender, EventArgs e)
    {
        Shell.SetNavBarIsVisible(this, false);
    }
    void OnNavBarHasShadowTrueClicked(object sender, EventArgs e)
    {
        Shell.SetNavBarHasShadow(this, true);
    }
    void OnNavBarHasShadowFalseClicked(object sender, EventArgs e)
    {
        Shell.SetNavBarHasShadow(this, false);
    }
    private async void OnGoToNotAnimated(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(NotAnimatedRoute);

    private async void OnGoToAnimated(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(AnimatedRoute);

    private async void OnGoToModal(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(ModalRoute);

    private async void OnGoToModalAnimated(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(ModalAnimatedRoute);

    private async void OnGoToModalNotAnimated(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(ModalNotAnimatedRoute);
    private async void OnGoToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//home");
    }
    private async void OnGoBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
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

    private abstract class PresentationModePage : ContentPage
    {
        protected PresentationModePage(string title, PresentationMode mode)
        {
            Title = title;
            BackgroundColor = Colors.White;
            Shell.SetPresentationMode(this, mode);

            var backButton = new Button
            {
                Text = "Go Back",
                AutomationId = "GoBackButton",
                BackgroundColor = Color.FromArgb("F5F5F5"),
                TextColor = Colors.Black
            };
            backButton.Clicked += async (_, __) => await Shell.Current.GoToAsync("..");

            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 20,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = title, HorizontalOptions = LayoutOptions.Center },
                    backButton
                }
            };
        }
    }

    private sealed class NotAnimatedPresentationPage : PresentationModePage
    {
        public NotAnimatedPresentationPage()
            : base("NotAnimated Page", PresentationMode.NotAnimated)
        {
        }
    }

    private sealed class AnimatedPresentationPage : PresentationModePage
    {
        public AnimatedPresentationPage()
            : base("Animated Page", PresentationMode.Animated)
        {
        }
    }

    private sealed class ModalPresentationPage : PresentationModePage
    {
        public ModalPresentationPage()
            : base("Modal Page", PresentationMode.Modal)
        {
        }
    }

    private sealed class ModalAnimatedPresentationPage : PresentationModePage
    {
        public ModalAnimatedPresentationPage()
            : base("ModalAnimated Page", PresentationMode.ModalAnimated)
        {
        }
    }

    private sealed class ModalNotAnimatedPresentationPage : PresentationModePage
    {
        public ModalNotAnimatedPresentationPage()
            : base("ModalNotAnimated Page", PresentationMode.ModalNotAnimated)
        {
        }
    }

}
