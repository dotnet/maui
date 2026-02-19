using System.Collections.Specialized;
using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Views;

public partial class ChatOverlayView : ContentView
{
	ChatViewModel? _viewModel;

	const double WidthPercent = 0.9;
	const double HeightPercent = 0.9;
	const double MaxPanelWidth = 400;
	const double MaxPanelHeight = 700;

	public ChatOverlayView()
	{
		InitializeComponent();
		SizeChanged += OnSizeChanged;
	}

	public void Initialize(ChatViewModel viewModel)
	{
		_viewModel = viewModel;
		BindingContext = viewModel;
		viewModel.Messages.CollectionChanged += OnMessagesChanged;
	}

	void OnSizeChanged(object? sender, EventArgs e)
	{
		UpdatePanelSize();
	}

	void UpdatePanelSize()
	{
		if (Width <= 0 || Height <= 0)
			return;

		ChatPanel.WidthRequest = Math.Min(Width * WidthPercent, MaxPanelWidth);
		ChatPanel.HeightRequest = Math.Min(Height * HeightPercent, MaxPanelHeight);

		// Keep TranslationY in sync for the hide animation
		if (ChatPanel.TranslationY > 0)
			ChatPanel.TranslationY = ChatPanel.HeightRequest;
	}

	void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Add && _viewModel is not null && _viewModel.Messages.Count > 0)
		{
			Dispatcher.Dispatch(() =>
			{
				MessagesView.ScrollTo(_viewModel.Messages.Count - 1, position: ScrollToPosition.End, animate: true);
			});
		}
	}

	async void OnBackdropTapped(object? sender, TappedEventArgs e)
	{
		await Hide();
	}

	async void OnCloseTapped(object? sender, EventArgs e)
	{
		await Hide();
	}

	public event EventHandler? Closed;

	public async Task Show()
	{
		UpdatePanelSize();

		var backdropFade = Backdrop.FadeToAsync(0.4, 250, Easing.CubicOut);
		var panelSlide = ChatPanel.TranslateToAsync(0, 0, 300, Easing.CubicOut);
		await Task.WhenAll(backdropFade, panelSlide);

		MessageEntry.Focus();
	}

	public async Task Hide()
	{
		MessageEntry.Unfocus();

		var backdropFade = Backdrop.FadeToAsync(0, 200, Easing.CubicIn);
		var panelSlide = ChatPanel.TranslateToAsync(0, ChatPanel.HeightRequest, 250, Easing.CubicIn);
		await Task.WhenAll(backdropFade, panelSlide);

		Closed?.Invoke(this, EventArgs.Empty);
	}
}
