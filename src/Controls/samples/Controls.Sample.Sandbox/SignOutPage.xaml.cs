namespace Maui.Controls.Sample;

public partial class SignOutPage : ContentPage
{
    public SignOutPage()
    {
        InitializeComponent();
        System.Diagnostics.Debug.WriteLine("[SafeArea] SignOutPage initialized");
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage", true);
    }
}