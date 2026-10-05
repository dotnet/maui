using Bielik.ViewModels;

namespace Bielik.Views;

public partial class ChatPage : ContentPage, IQueryAttributable
{
    private readonly ChatViewModel _viewModel;

    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.Messages.CollectionChanged += (_, _) => ScrollToLatest();
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ChatViewModel.IsBusy))
            {
                if (_viewModel.IsBusy)
                {
                    Dispatcher.StartTimer(TimeSpan.FromMilliseconds(300), () =>
                    {
                        ScrollToLatest();
                        return _viewModel.IsBusy;
                    });
                }
                else
                {
                    ScrollToLatest();
                }
            }
        };
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("idea", out var value) && value is string id)
        {
            _viewModel.ChooseIdea(Uri.UnescapeDataString(id));
        }
    }

    private void OnIdeaClicked(object? sender, EventArgs args)
    {
        if (sender is Button { CommandParameter: string id })
        {
            _viewModel.ChooseIdea(id);
        }
    }

    private void OnSendClicked(object? sender, EventArgs args) => Composer.Unfocus();

    private void ScrollToLatest()
    {
        if (_viewModel.Messages.Count > 0 && MessagesList.Handler is not null)
        {
            Dispatcher.Dispatch(() => MessagesList.ScrollTo(_viewModel.Messages[^1], position: ScrollToPosition.End, animate: false));
        }
    }
}
