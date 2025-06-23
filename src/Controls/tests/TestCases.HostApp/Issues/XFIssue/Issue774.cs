namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 774, "ActionSheet won't dismiss after rotation to landscape", PlatformAffected.Android)]
public class Issue774 : TestContentPage
{
	protected override void Init()
	{
		Content = new StackLayout
		{
			new Label {
				Text = "Hi"
			},
			new Button {
				Text = "Show ActionSheet",
				Command = new Command (async () => await DisplayActionSheet ("What's up", "Dismiss", "Destroy"))
			}
		};
	}
}
