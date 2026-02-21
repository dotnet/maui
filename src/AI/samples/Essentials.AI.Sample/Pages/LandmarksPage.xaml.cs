using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.ViewModels;
using Maui.Controls.Sample.Views;

namespace Maui.Controls.Sample.Pages;

public partial class LandmarksPage : ContentPage
{
	private readonly LandmarksViewModel _viewModel;
	private readonly ChatOverlayView _chatOverlay;
	private readonly ChatViewModel _chatViewModel;

	public LandmarksPage(LandmarksViewModel viewModel, ChatViewModel chatViewModel)
	{
		InitializeComponent();

		_viewModel = viewModel;
		_chatViewModel = chatViewModel;
		BindingContext = viewModel;

		_chatOverlay = new ChatOverlayView();
		_chatOverlay.Initialize(chatViewModel);

		Loaded += async (_, _) => await viewModel.InitializeAsync();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_chatViewModel.ChatService.NavigateToTripRequested += OnNavigateToTrip;
	}

	protected override void OnDisappearing()
	{
		_chatViewModel.ChatService.NavigateToTripRequested -= OnNavigateToTrip;
		base.OnDisappearing();
	}

	private async void OnNavigateToTrip(Landmark landmark)
	{
		// Close chat overlay first if open
		await _chatOverlay.Hide();

		var parameters = new Dictionary<string, object>
		{
			{ "Landmark", landmark }
		};
		await Shell.Current.GoToAsync(nameof(TripPlanningPage), parameters);
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

	private async void OnChatButtonClicked(object? sender, EventArgs e)
	{
		ChatFab.IsVisible = false;
		var grid = (Grid)Content;
		grid.Children.Add(_chatOverlay);
		_chatOverlay.Closed += OnChatOverlayClosed;
		await _chatOverlay.Show();
	}

	private void OnChatOverlayClosed(object? sender, EventArgs e)
	{
		_chatOverlay.Closed -= OnChatOverlayClosed;
		var grid = (Grid)Content;
		grid.Children.Remove(_chatOverlay);
		ChatFab.IsVisible = true;
	}
}
