namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 59580, "Raising Command.CanExecutChanged causes crash on Android",
	PlatformAffected.Android)]
public class Bugzilla59580 : TestContentPage
{
	protected override void Init()
	{
		var tableView = new TableView();
		var tableSection = new TableSection();
		var switchCell = new TextCell
		{
			AutomationId = "Cell",
			Text = "Cell"
		};

		var menuItem = new MenuItem
		{
			AutomationId = "Fire CanExecuteChanged",
			Text = "Fire CanExecuteChanged",
			Command = new DelegateCommand(_ =>
				((DelegateCommand)switchCell.ContextActions.Single().Command).RaiseCanExecuteChanged()),
			IsDestructive = true
		};
		switchCell.ContextActions.Add(menuItem);
		tableSection.Add(switchCell);
		tableView.Root.Add(tableSection);
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		Content = tableView;
	}
}
