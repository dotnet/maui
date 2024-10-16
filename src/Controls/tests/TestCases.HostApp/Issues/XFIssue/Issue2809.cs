namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2809, "Secondary ToolbarItems cause app to hang during PushAsync", PlatformAffected.iOS)]
public class Issue2809 : TestContentPage
{
	protected override void Init()
	{
		ToolbarItems.Add(new ToolbarItem("Item 1", string.Empty,
			DummyAction, ToolbarItemOrder.Secondary));

		ToolbarItems.Add(new ToolbarItem("Item 2", string.Empty,
			DummyAction, ToolbarItemOrder.Secondary));
	}

	public void DummyAction()
	{
	}
}
