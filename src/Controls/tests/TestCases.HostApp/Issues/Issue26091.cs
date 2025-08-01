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
			new TextCell{ Text = "Add new"}
		};

		tableView = new TableView
		{
			AutomationId = "TableView",
			Root = new TableRoot
			{
				dataSection
			}
		};

		StackLayout stackLayout = new StackLayout();
		Button button = new Button { Text = "Add new", AutomationId = "AddTextCell", Command = new Command(AddNew) };
		stackLayout.Children.Add(button);
		stackLayout.Children.Add(tableView);
		Content = stackLayout;
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