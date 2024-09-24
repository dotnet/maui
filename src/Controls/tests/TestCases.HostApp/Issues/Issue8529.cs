using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8529,
		"[Bug] [Shell] iOS - BackButtonBehavior Command property binding throws InvalidCastException when using a custom command class that implements ICommand",
		PlatformAffected.iOS)]
	public class Issue8529 : TestShell
	{
		const string ContentPageTitle = "Item1";
		const string ButtonId = "ButtonId";

		protected override void Init()
		{
			CreateContentPage(ContentPageTitle).Content =
				new StackLayout
				{
					Children =
					{
						new Button
						{
							AutomationId = ButtonId,
							Text = "Tap to Navigate To the Page With BackButtonBehavior",
							Command = new Command(NavToBackButtonBehaviorPage)
						}
					}
				};
		}

		private void NavToBackButtonBehaviorPage()
		{
			_ = Shell.Current.Navigation.PushAsync(new Issue8529_1());
		}
	}
}