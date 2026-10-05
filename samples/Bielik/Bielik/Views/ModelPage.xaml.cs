using Bielik.Core;
using Bielik.ViewModels;

namespace Bielik.Views;

public partial class ModelPage : ContentPage
{
    public ModelPage(AppState state)
    {
        InitializeComponent();
        BindingContext = state;
    }

    private async void OnModelCardClicked(object? sender, EventArgs args)
    {
        try
        {
            if (!await Browser.Default.OpenAsync(ModelInfo.ModelCard, BrowserLaunchMode.SystemPreferred))
            {
                await DisplayAlertAsync("Nie można otworzyć strony", "Spróbuj otworzyć kartę modelu w Safari.", "OK");
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlertAsync("Safari jest niedostępne", "Adres karty modelu znajduje się w dokumentacji aplikacji.", "OK");
        }
    }
}
