using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages;

public partial class LandmarksPage : ContentPage
{
    private readonly LandmarksViewModel _viewModel;

    public LandmarksPage(LandmarksViewModel viewModel)
    {
        InitializeComponent();
        
        _viewModel = viewModel;
        BindingContext = viewModel;

        Loaded += async (_, _) => await viewModel.InitializeAsync();
    }

    private async void OnLandmarkTapped(object? sender, Landmark landmark)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Landmark", landmark }
        };
        
        await Shell.Current.GoToAsync(nameof(TripPlanningPage), parameters);
    }

    private async void OnLanguageButtonClicked(object? sender, EventArgs e)
    {
        string? action = await DisplayActionSheetAsync(
            "Select Language for AI Responses",
            "Cancel",
            null,
            _viewModel.AvailableLanguages);

        if (!string.IsNullOrEmpty(action) && action != "Cancel")
        {
            _viewModel.SelectedLanguage = action;
        }
    }
}
