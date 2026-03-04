namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 26593, "Table View ContextActionCell Test")]
public class ContextActionCellTest : TestContentPage
{
	TableSection dataSection;
	TableView tableView;

	protected override void Init()
	{
		this.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		dataSection = new TableSection {
			new TextCell{ Text = "Text Cell", ContextActions = { new MenuItem{ Text = "Save" } } },
			new TextCell{ Text = "Text Cell 1" },
			new SwitchCell { Text = "Switch Cell", On = true },
		};

		tableView = new TableView
		{
			Root = new TableRoot { dataSection }
		};

		Content = tableView;
	}
}
