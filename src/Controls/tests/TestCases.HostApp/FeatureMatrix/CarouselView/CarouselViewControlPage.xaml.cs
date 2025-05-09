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

        // Update ViewModel properties
        viewModel.PreviousItemText = string.IsNullOrEmpty(_previousItem) ? "No previous item" : _previousItem;
        viewModel.CurrentItemText = currentItem;

        _previousItem = currentItem;
    }
}

    private void OnCarouselView_PositionChanged(object sender, PositionChangedEventArgs e)
    {
        if (BindingContext is CarouselViewViewModel viewModel)
        {
            var items = viewModel.Items;

            // Update CurrentItemText and CurrentItemPosition
            if (e.CurrentPosition >= 0 && e.CurrentPosition < items.Count)
            {
                viewModel.CurrentItemText = items[e.CurrentPosition];
                viewModel.CurrentItemPostion = e.CurrentPosition.ToString();
            }

            // Handle PreviousPosition only if it's valid
            if (e.PreviousPosition >= 0 && e.PreviousPosition < items.Count)
            {
                viewModel.PreviousItemText = items[e.PreviousPosition];
                viewModel.PreviousItemPosition = e.PreviousPosition.ToString();
            }
            else
            {
                viewModel.PreviousItemPosition = " ";
            }

            // Update the Position property in the view model
            viewModel.Position = e.CurrentPosition;
        }
    }
}