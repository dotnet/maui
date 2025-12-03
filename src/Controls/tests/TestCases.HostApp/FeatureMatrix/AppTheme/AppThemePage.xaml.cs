namespace Maui.Controls.Sample;

public partial class AppThemePage : ContentPage
{
	public AppThemePage()
	{
		InitializeComponent();
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