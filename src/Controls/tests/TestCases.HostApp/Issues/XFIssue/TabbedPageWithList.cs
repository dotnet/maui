namespace Maui.Controls.Sample.Issues;

public class TabbedPageWithListName
{
	public string Name { get; set; }
}

[Issue(IssueTracker.None, 0, "TabbedPage with list", PlatformAffected.All)]
public class TabbedPageWithList : TestTabbedPage
{
	protected override void Init()
	{
		Title = "Tabbed Page with List";
		Children.Add(new ContentPage { Title = "Tab Two" });
		Children.Add(new ListViewTest());
	}

	public class ListViewTest : ContentPage
	{
		public ListViewTest()
		{
			Title = "List Page";

			var items = new[] {
				new TabbedPageWithListName () { Name = "Jason" },
				new TabbedPageWithListName () { Name = "Ermau" },
				new TabbedPageWithListName () { Name = "Seth" }
			};

			var cellTemplate = new DataTemplate(typeof(TextCell));
			cellTemplate.SetBinding(TextCell.TextProperty, "Name");

			Content = new ListView()
			{
				ItemTemplate = cellTemplate,
				ItemsSource = items
			};
		}
	}
}
