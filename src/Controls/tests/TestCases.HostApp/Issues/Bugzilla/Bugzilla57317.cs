namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 57317, "Modifying Cell.ContextActions can crash on Android", PlatformAffected.Android)]
public class Bugzilla57317 : TestContentPage
{
	protected override void Init()
	{
		var tableView = new TableView();
		var tableSection = new TableSection();
		var switchCell = new TextCell
		{
			Text = "Cell",
			AutomationId = "Cell"
		};

		var menuItem = new MenuItem
		{
			Text = "Self-Deleting item",
			Command = new Command(() => switchCell.ContextActions.RemoveAt(0)),
			IsDestructive = true
		};
		switchCell.ContextActions.Add(menuItem);
		tableSection.Add(switchCell);
		tableView.Root.Add(tableSection);
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		Content = tableView;
	}
}
