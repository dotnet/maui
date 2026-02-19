using System.Collections.Specialized;
using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Views;

public partial class ChatOverlayView : ContentView
{
	ChatViewModel? _viewModel;

	public ChatOverlayView()
	{
		InitializeComponent();
		SizeChanged += (s, e) =>
		{
			if (Height > 0)
				ChatPanel.HeightRequest = Math.Min(Height - 16, 800);
		};
	}

	public void Initialize(ChatViewModel viewModel)
	{
		_viewModel = viewModel;
		BindingContext = viewModel;
		viewModel.Messages.CollectionChanged += OnMessagesChanged;
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

	async void OnBackdropTapped(object? sender, TappedEventArgs e) => await Hide();

	async void OnCloseTapped(object? sender, EventArgs e) => await Hide();

	public event EventHandler? Closed;

	public async Task Show()
	{
		// Ensure panel starts offscreen at its actual height
		ChatPanel.TranslationY = ChatPanel.Height > 0 ? ChatPanel.Height : 1000;

		var backdropFade = Backdrop.FadeToAsync(0.4, 250, Easing.CubicOut);
		var panelSlide = ChatPanel.TranslateToAsync(0, 0, 300, Easing.CubicOut);
		await Task.WhenAll(backdropFade, panelSlide);

		MessageEntry.Focus();
	}

	public async Task Hide()
	{
		MessageEntry.Unfocus();

		var targetY = ChatPanel.Height > 0 ? ChatPanel.Height : 700;
		var backdropFade = Backdrop.FadeToAsync(0, 200, Easing.CubicIn);
		var panelSlide = ChatPanel.TranslateToAsync(0, targetY, 250, Easing.CubicIn);
		await Task.WhenAll(backdropFade, panelSlide);

		Closed?.Invoke(this, EventArgs.Empty);
	}
}
