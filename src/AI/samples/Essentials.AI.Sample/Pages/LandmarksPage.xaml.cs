using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages;

public partial class LandmarksPage : ContentPage
{
    public LandmarksPage(LandmarksViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnFeaturedLandmarkTapped(object? sender, Landmark landmark)
    {
        await NavigateToTripPlanning(landmark);
    }

    private async void OnLandmarkTapped(object? sender, Landmark landmark)
    {
        await NavigateToTripPlanning(landmark);
    }

    private async Task NavigateToTripPlanning(Landmark landmark)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Landmark", landmark }
        };
        
        await Shell.Current.GoToAsync(nameof(TripPlanningPage), parameters);
    }
}
