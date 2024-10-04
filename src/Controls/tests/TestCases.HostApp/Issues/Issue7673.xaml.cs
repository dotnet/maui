namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 7673, "Android Switch view has inconsistent colors when off", PlatformAffected.All)]
public partial class Issue7673 : ContentPage
{
	public Issue7673()
	{
		InitializeComponent();
	}

    private void OnToggleSwitch1(object sender, EventArgs e)
    {
		switch1.IsToggled = !switch1.IsToggled;
	}
    private void OnToggleSwitch2(object sender, EventArgs e)
    {
		switch2.IsToggled = !switch2.IsToggled;
    }
    private void OnToggleSwitch3(object sender, EventArgs e)
    {
		switch3.IsToggled = !switch3.IsToggled;
	}
}