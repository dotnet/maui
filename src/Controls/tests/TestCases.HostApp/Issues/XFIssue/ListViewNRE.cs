namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "ListView crashes when disposed on ItemSelected", PlatformAffected.iOS)]
public class ListViewNRE : TestContentPage
{
	const string Success = "Success";

	protected override void Init()
	{
		var listView = new ListView
		{
			ItemsSource = Enumerable.Range(0, 10)
		};

		listView.ItemSelected += ListView_ItemSelected;

		Content = listView;
	}

	void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
	{
		Content = new Label { Text = Success };
	}
}