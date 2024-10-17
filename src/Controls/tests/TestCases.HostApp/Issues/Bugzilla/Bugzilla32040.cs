namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 32040, "EntryCell.Tapped or SwitchCell.Tapped does not fire when within a TableView ")]
public class Bugzilla32040 : TestContentPage
{
	protected override void Init()
	{
		var switchCell = new SwitchCell { Text = "blahblah", AutomationId = "blahblah" };
		switchCell.Tapped += (s, e) =>
		{
			switchCell.Text = "Tapped";
		};
		switchCell.OnChanged += (sender, e) =>
		{
			switchCell.Text = "Switched";
		};

		var entryCell = new EntryCell { Text = "yaddayadda", AutomationId = "yaddayadda" };
		entryCell.HorizontalTextAlignment = TextAlignment.End;
		entryCell.Label = "Click Here";
		entryCell.Tapped += (s, e) =>
		{
			entryCell.Text = "Tapped";
		};
		entryCell.Completed += (sender, e) =>
		{
			entryCell.Text = "Completed";
		};

		// The root page of your application
		Content = new TableView
		{
			Intent = TableIntent.Form,
			Root = new TableRoot("Table Title") {
				new TableSection ("Section 1 Title") {
					switchCell,
					entryCell
				}
			}
		};
	}
}
