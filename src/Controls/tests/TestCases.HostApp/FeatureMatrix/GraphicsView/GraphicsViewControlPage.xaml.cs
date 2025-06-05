using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class GraphicsViewControlPage : NavigationPage
{
    private GraphicsViewViewModel _viewModel;

    public GraphicsViewControlPage()
    {
        _viewModel = new GraphicsViewViewModel();
        PushAsync(new GraphicsViewControlMainPage(_viewModel));
    }
}

public partial class GraphicsViewControlMainPage : ContentPage
{
    private GraphicsViewViewModel _viewModel;
    private GraphicsView graphicsView;

    public GraphicsViewControlMainPage(GraphicsViewViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        graphicsView = new GraphicsView
        {
            BackgroundColor = _viewModel.BackgroundColor,
            IsVisible = _viewModel.IsVisible,
            IsEnabled = _viewModel.IsEnabled,
            FlowDirection = _viewModel.FlowDirection
        };

        Content = new StackLayout
        {
            Children =
            {
                graphicsView,
                new Button
                {
                    Text = "Options",
                    Command = new Command(NavigateToOptionsPage_Clicked)
                },
                new Button
                {
                    Text = "Reset",
                    Command = new Command(OnResetClicked)
                }
            }
        };
    }

    private void OnSetBackgroundColor(string colorName)
    {
        switch (colorName)
        {
            case "Blue":
                _viewModel.BackgroundColor = Colors.Blue;
                break;
            case "Green":
                _viewModel.BackgroundColor = Colors.Green;
                break;
            case "Default":
            default:
                _viewModel.BackgroundColor = null;
                break;
        }
        graphicsView.Invalidate();
    }

    private void OnSetDrawableType(string drawableType)
    {
        _viewModel.DrawableType = drawableType;
        graphicsView.Invalidate();
    }

    private void OnResetClicked()
    {
        _viewModel = new GraphicsViewViewModel();
        BindingContext = _viewModel;
        graphicsView.BackgroundColor = _viewModel.BackgroundColor;
        graphicsView.IsVisible = _viewModel.IsVisible;
        graphicsView.IsEnabled = _viewModel.IsEnabled;
        graphicsView.FlowDirection = _viewModel.FlowDirection;
        graphicsView.Invalidate();
    }

    private async void NavigateToOptionsPage_Clicked()
    {
        await Navigation.PushAsync(new GraphicsViewOptionsPage(_viewModel));
    }
}
