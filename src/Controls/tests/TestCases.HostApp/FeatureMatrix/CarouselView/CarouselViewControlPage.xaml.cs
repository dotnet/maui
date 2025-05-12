using System;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
namespace Maui.Controls.Sample;

public partial class CarouselViewControlPage : ContentPage
{
    private CarouselViewViewModel _viewModel;

    public CarouselViewControlPage()
    {
        InitializeComponent();
        _viewModel = new CarouselViewViewModel();
        _viewModel.PreviousItemText = "No previous item";
        _viewModel.PreviousItemPosition = "No previous items";
        BindingContext = _viewModel;
    }

    private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        BindingContext = _viewModel = new CarouselViewViewModel();
        await Navigation.PushAsync(new CarouselViewOptionsPage(_viewModel));
    }

    string _previousItem = null;
    private void OnCarouselView_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
    {
        if (BindingContext is CarouselViewViewModel viewModel)
        {
            var currentItem = e.CurrentItem?.ToString();

            if (viewModel.CurrentItemText != currentItem)
            {
                _previousItem = viewModel.CurrentItemText;

                viewModel.PreviousItemText = string.IsNullOrEmpty(_previousItem) ? "No previous item" : _previousItem;
                viewModel.CurrentItemText = currentItem;
            }
        }
    }

    private void OnCarouselView_PositionChanged(object sender, PositionChangedEventArgs e)
    {
        if (BindingContext is CarouselViewViewModel viewModel)
        {
            var items = viewModel.Items;

            bool hasCurrent = e.CurrentPosition >= 0 && e.CurrentPosition < items.Count;
            bool hasPrevious = e.PreviousPosition >= 0 && e.PreviousPosition < items.Count;

            viewModel.CurrentItemText = hasCurrent ? items[e.CurrentPosition] : string.Empty;
            viewModel.CurrentItemPostion = hasCurrent ? e.CurrentPosition.ToString() : string.Empty;

            viewModel.PreviousItemText = hasPrevious ? items[e.PreviousPosition] : "No previous item";
            viewModel.PreviousItemPosition = hasPrevious ? e.PreviousPosition.ToString() : "No previous items";

            viewModel.Position = e.CurrentPosition;
        }
    }
}
