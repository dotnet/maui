using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages;

public partial class TripPlanningPage : ContentPage
{
    public TripPlanningPage(TripPlanningViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        Loaded += async (_, _) => await viewModel.InitializeAsync();

        NavigatingFrom += (_, _) => viewModel.Cancel();
    }

    private async void OnBackButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
