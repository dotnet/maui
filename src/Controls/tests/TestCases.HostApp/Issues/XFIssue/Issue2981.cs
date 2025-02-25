namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2981, "Long Press on ListView causes crash")]
public class Issue2981 : TestContentPage
{
	protected override void Init()
	{
		var listView = new ListView();

		listView.ItemsSource = new[] { "Cell1", "Cell2" };
		Content = listView;
	}
}
