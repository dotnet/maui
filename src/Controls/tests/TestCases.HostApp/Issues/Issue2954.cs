namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2954, "Cell becomes empty after adding a new one with context actions (TableView) ")]
public class Issue2954 : TestContentPage // or TestFlyoutPage, etc ...
{
	TableSection _dataSection;
	TableView _tableView;
	int _count = 0;
	protected override void Init()
	{
		_dataSection = new TableSection {
			new TextCell{ Text = "Cell1" },
			new TextCell{ Text = "Cell2", ContextActions = { new MenuItem{ Text = "Delete" } } },
			new TextCell{ Text = "Add new", Command = new Command (AddNew) }
		};

		_tableView = new TableView
		{
			Root = new TableRoot {
				_dataSection
			}
		};

		Content = _tableView;
	}

	void AddNew(object parameters)
	{
		_count++;
		_dataSection.Insert(0, new TextCell
		{
			Text = "Fresh cell " + _count
					,
			ContextActions = { new MenuItem { Text = "Delete" } }
		});
		_tableView.Root = _tableView.Root; //HACK - force table reload
	}
}
