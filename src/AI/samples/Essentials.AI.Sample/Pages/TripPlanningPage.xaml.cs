using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages;

public partial class TripPlanningPage : ContentPage
{
    private readonly TripPlanningViewModel _viewModel;

    public TripPlanningPage(TripPlanningViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
