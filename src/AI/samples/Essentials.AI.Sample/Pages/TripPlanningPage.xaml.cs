using Maui.Controls.Sample.ViewModels;
using Maui.Controls.Sample.Views;

namespace Maui.Controls.Sample.Pages;

public partial class TripPlanningPage : ContentPage
{
	private readonly ChatOverlayView _chatOverlay;

	public TripPlanningPage(TripPlanningViewModel viewModel, ChatViewModel chatViewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;

		_chatOverlay = new ChatOverlayView();
		_chatOverlay.Initialize(chatViewModel);

		Loaded += async (_, _) => await viewModel.InitializeAsync();

		NavigatingFrom += (_, _) => viewModel.Cancel();
	}

	private async void OnBackButtonClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
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
