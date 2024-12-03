namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26091, "TableView cells become empty after adding a new cell with context actions", PlatformAffected.iOS)]
public partial class Issue26091 : ContentPage
{
	TableSection dataSection;
	TableView tableView;
	int count = 0;
	public Issue26091()
	{

		dataSection = new TableSection {
			new TextCell{ Text = "Cell1" },
			new TextCell{ Text = "Cell2", ContextActions = { new MenuItem{ Text = "Delete" } } },
			new TextCell{ Text = "Add new", AutomationId="AddTextCell", Command = new Command (AddNew) }
		};

		tableView = new TableView
		{
			AutomationId = "TableView",
			Root = new TableRoot
			{
				dataSection
			}
		};

		Content = tableView;
	}

	void AddNew(object parameters)
	{
		count++;
		dataSection.Insert(0, new TextCell
		{
			Text = "Fresh cell " + count,
			ContextActions = { new MenuItem { Text = "Delete" } }
		});
	}
}