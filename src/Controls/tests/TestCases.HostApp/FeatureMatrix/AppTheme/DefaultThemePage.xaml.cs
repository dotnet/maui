namespace Maui.Controls.Sample;

public class DefaultThemePage : NavigationPage
{
    public DefaultThemePage()
    {
        PushAsync(new DefaultMainThemePage());
    }
}

public partial class DefaultMainThemePage : ContentPage
{
    public DefaultMainThemePage()
    {
        InitializeComponent();
    }

    public async void OnNavigateToAppThemePageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AppThemePage());
    }

    public void OnLightThemeButtonClicked(object sender, EventArgs e)
    {
        Application.Current.UserAppTheme = AppTheme.Light;
    }

    public void OnDarkThemeButtonClicked(object sender, EventArgs e)
    {
        Application.Current.UserAppTheme = AppTheme.Dark;
    }
}
