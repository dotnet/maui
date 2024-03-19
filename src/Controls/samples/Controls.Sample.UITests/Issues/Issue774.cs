using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 774, "ActionSheet won't dismiss after rotation to landscape", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Issue774 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children = {
					new Label {
						Text = "Hi"
					},
					new Button {
						AutomationId = "ShowActionSheet",
						Text = "Show ActionSheet",
						Command = new Command (async () => await DisplayActionSheet ("What's up", "Dismiss", "Destroy"))
					}
				}
			};
		}
	}
}