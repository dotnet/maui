namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7856,
	"[Bug] Shell BackButtonBehaviour TextOverride breaks back",
	PlatformAffected.iOS)]
public class Issue7856 : TestShell
{
	const string ContentPageTitle = "Item1";
	const string ButtonId = "ButtonId";

	protected override void Init()
	{
		CreateContentPage(ContentPageTitle).Content =
			new StackLayout
			{
				new Button
				{
					AutomationId = ButtonId,
					Text = "Tap to Navigate To the Page With BackButtonBehavior",
					Command = new Command(NavToBackButtonBehaviorPage)
				}
			};
	}

	private void NavToBackButtonBehaviorPage()
	{
		_ = Shell.Current.Navigation.PushAsync(new Issue7856_1());
	}
}
