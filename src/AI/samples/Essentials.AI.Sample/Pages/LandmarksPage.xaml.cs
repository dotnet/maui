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

		// Remove native border and focus ring from the search entry
		SearchEntry.HandlerChanged += (s, _) =>
		{
#if MACCATALYST || IOS
			if (SearchEntry.Handler?.PlatformView is UIKit.UITextField textField)
			{
				textField.BorderStyle = UIKit.UITextBorderStyle.None;
				if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15))
					textField.FocusEffect = null;
			}
#elif WINDOWS
			if (SearchEntry.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
			{
				textBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
				textBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
				// Remove the focus underline
				textBox.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
			}
#endif
		};

		Loaded += async (_, _) => await viewModel.InitializeAsync();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_chatViewModel.ChatService.NavigateToTripRequested += OnNavigateToTrip;
	}

	protected override void OnDisappearing()
	{
		_viewModel.CancelPendingSearch();
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
		await NavigateToDetail(landmark);
	}

	async Task NavigateToDetail(Landmark landmark)
	{
		var parameters = new Dictionary<string, object>
		{
			{ "Landmark", landmark },
			{ "RecentSearches", _viewModel.RecentSearches }
		};
		await Shell.Current.GoToAsync(nameof(LandmarkDetailPage), parameters);
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
