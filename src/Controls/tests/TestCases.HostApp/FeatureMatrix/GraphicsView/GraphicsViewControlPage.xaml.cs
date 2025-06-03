using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class GraphicsViewControlPage : ContentPage
{
    private GraphicsViewFeatureMatrixViewModel _viewModel;

    public GraphicsViewControlPage()
    {
        InitializeComponent();
        _viewModel = new GraphicsViewFeatureMatrixViewModel();
        BindingContext = _viewModel;
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
        if (Enum.TryParse(drawableType, out DrawableType result))
        {
            _viewModel.DrawableType = result;
            graphicsView.Invalidate();
        }
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        _viewModel = new GraphicsViewFeatureMatrixViewModel();
        BindingContext = _viewModel;
        graphicsView.Invalidate();
    }
}
