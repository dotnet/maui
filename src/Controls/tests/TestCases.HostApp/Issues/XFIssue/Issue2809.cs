using Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 2809, "Secondary ToolbarItems cause app to hang during PushAsync", PlatformAffected.iOS)]
public class Issue2809 : TestNavigationPage
{
	protected override void Init()
	{
		var contentPage = new ContentPage();
		contentPage.ToolbarItems.Add(new ToolbarItem("Item 1", string.Empty,
			DummyAction, ToolbarItemOrder.Secondary));
		contentPage.ToolbarItems.Add(new ToolbarItem("Item 2", string.Empty,
			DummyAction, ToolbarItemOrder.Secondary));
		Navigation.PushAsync(contentPage);
	}

	public void DummyAction()
	{
	}
}
