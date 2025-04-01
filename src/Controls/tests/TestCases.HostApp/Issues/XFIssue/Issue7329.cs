namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7329, "[Android] ListView scroll not working when inside a ScrollView", PlatformAffected.Android)]

public class Issue7329 : TestContentPage
{
	ListView listView = null;
	protected override void Init()
	{
		listView = new ListView() { AutomationId = "NestedListView" };
		listView.ItemsSource = Enumerable.Range(0, 200).Select(x => new Data() { Text = x }).ToList();

		Content = new ScrollView()
		{
			AutomationId = "ParentScrollView",
			Content = new StackLayout()
			{
				new Label() { AutomationId = "ApiLabel" },
				new Label() { Text = "If the List View can scroll the test has passed"},
				listView
			}
		};
	}

	public class Data
	{
		public int Text { get; set; }

		public override string ToString()
		{
			return Text.ToString();
		}
	}
}
