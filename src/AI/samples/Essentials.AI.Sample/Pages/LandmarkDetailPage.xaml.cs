using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages;

public partial class LandmarkDetailPage : ContentPage
{
	private readonly LandmarkDetailViewModel _viewModel;

	public LandmarkDetailPage(LandmarkDetailViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;

		Loaded += (_, _) => _ = viewModel.InitializeAsync(); // fire-and-forget so page renders immediately
		NavigatingFrom += (_, _) => viewModel.Cancel();
	}

	private async void OnBackClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}

	private async void OnPlanTripClicked(object? sender, EventArgs e)
	{
		var parameters = new Dictionary<string, object>
		{
			{ "Landmark", _viewModel.Landmark },
			{ "Language", _viewModel.SelectedLanguage }
		};
		await Shell.Current.GoToAsync(nameof(TripPlanningPage), parameters);
	}

	private async void OnSimilarLandmarkTapped(object? sender, Landmark landmark)
	{
		var parameters = new Dictionary<string, object>
		{
			{ "Landmark", landmark }
		};
		// Navigate to a new detail page for the similar landmark
		await Shell.Current.GoToAsync(nameof(LandmarkDetailPage), parameters);
	}
}
