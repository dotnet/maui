namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30052, "Right-To-Left (RTL) alignment is not applied to Editor placeholder", PlatformAffected.iOS)]
public class Issue30052 : TestContentPage
{
	protected override void Init()
	{
		Content = new VerticalStackLayout
		{
			FlowDirection = FlowDirection.RightToLeft,
			Children =
				{
					new Editor
					{
						Placeholder = "This is a placeholder in an editor",
						AutomationId = "Editor"
					},
					new Editor
					{
						Text = "Right to left - editor"
					},
					new Entry
					{
						Placeholder = "This is a placeholder in an entry"
					},
					new Entry
					{
						Text = "Right to left - entry"
					}
				}
		};
	}
}
