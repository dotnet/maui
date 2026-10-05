using Bielik.ViewModels;

namespace Bielik.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(AppState state)
    {
        InitializeComponent();
        BindingContext = state;
    }
}
