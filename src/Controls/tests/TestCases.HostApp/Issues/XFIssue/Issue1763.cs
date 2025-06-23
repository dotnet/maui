namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1763, "First item of grouped ListView not firing ItemTapped", PlatformAffected.WinPhone)]
public class Issue1763 : TestTabbedPage
{
	public Issue1763()
	{

	}

	protected override void Init()
	{
		Title = "Contacts";
		Children.Add(new ContactsPage());
	}
}
