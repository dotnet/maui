namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 15695, "Support for Switch OFF State color", PlatformAffected.All)]
public partial class Issue15695 : ContentPage
{
	public Issue15695()
	{
		InitializeComponent();
	}

	private void OnButtonClicked(object sender, EventArgs e)
	{
		mauiSwitch.IsToggled = !mauiSwitch.IsToggled;
	}

	private void OffNullColorButtonClicked(object sender, EventArgs e)
	{
		mauiSwitch.OffColor = null;
	}

	private void Reset_State_Button_Clicked(object sender, EventArgs e)
	{
		mauiSwitch.OnColor = Colors.Green;
		mauiSwitch.OffColor = Colors.Red;
		mauiSwitch.IsToggled = true;
		Application.Current.UserAppTheme = AppTheme.Light;
	}

	private void ChangeThemeButtonClicked(object sender, EventArgs e)
	{
		if (Application.Current != null)
		{
			if (Application.Current.RequestedTheme == AppTheme.Dark)
			{
				Application.Current.UserAppTheme = AppTheme.Light;
			}
			else
			{
				Application.Current.UserAppTheme = AppTheme.Dark;
			}
		}
	}
}