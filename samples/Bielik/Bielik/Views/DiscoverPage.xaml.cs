using Bielik.Core;
using Bielik.ViewModels;

namespace Bielik.Views;

public partial class DiscoverPage : ContentPage
{
    private readonly AppState _state;
    private bool _checked;

    public DiscoverPage(AppState state)
    {
        InitializeComponent();
        BindingContext = _state = state;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_checked)
        {
            _checked = true;
            await _state.CheckConnectionAsync();
        }
    }

    private async void OnStartChat(object? sender, EventArgs args) =>
        await Shell.Current.GoToAsync("//chat");

    private async void OnIdeaClicked(object? sender, EventArgs args)
    {
        if (sender is Button { BindingContext: PromptIdea idea })
        {
            await Shell.Current.GoToAsync($"//chat?idea={Uri.EscapeDataString(idea.Id)}");
        }
    }
}
