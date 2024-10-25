namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 11, "Config changes which restart the app should not crash",
	PlatformAffected.Android)]
public class RestartAppTest : TestContentPage
{
	public static object App;
	public static bool Reinit;

	public const string ForceRestart = "ForceRestart";
	public const string Success = "Android Core Gallery";
	public const string ReinitOk = "Tap Reinit, back out of app, relaunch, and expect 'true': {0}";
	public const string RestartButton = "Restart";
	public const string ReinitButton = "Reinit";

	protected override void Init()
	{
		var restartButton = new Button { Text = RestartButton, AutomationId = RestartButton };
#pragma warning disable CS0618 // Type or member is obsolete
		restartButton.Clicked += (sender, e) => MessagingCenter.Send(this, ForceRestart);
#pragma warning restore CS0618 // Type or member is obsolete

		var reinitButton = new Button { Text = ReinitButton, AutomationId = RestartButton };
		reinitButton.Clicked += (sender, e) => App = Application.Current;

		var stackLayout = new StackLayout
		{
				new Label { Text = Success },
				new Label { Text = string.Format(ReinitOk, Reinit) },
				restartButton,
				reinitButton
		};

		stackLayout.Padding = new Thickness(0, 20, 0, 0);

		Content = stackLayout;
	}
}