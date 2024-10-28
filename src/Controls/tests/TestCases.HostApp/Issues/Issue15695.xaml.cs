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
}