namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26977, "Setter.TargetName + ControlTemplate crash", PlatformAffected.Android)]
public partial class Issue26977 : ContentPage
{
	public Issue26977()
	{
		InitializeComponent();
	}

	void OnStateSwitchToggled(object sender, ToggledEventArgs e)
	{
		if (sender is not Switch stateSwitch)
		{
			return;
		}

		VisualStateManager.GoToState(RootStackLayout, stateSwitch.IsToggled ? "State1" : "State2");
	}
}
