namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 42832, "Scrolling a ListView with active ContextAction Items causes NRE", PlatformAffected.Android)]
public class Bugzilla42832 : TestContentPage
{
	ListView listview;

	protected override void Init()
	{
		var items = new List<string>();
		for (int i = 0; i < 20; i++)
		{
			items.Add($"Item #{i}");
		}

		var template = new DataTemplate(typeof(TestCell));
		template.SetBinding(TextCell.TextProperty, ".");
		template.SetBinding(TextCell.AutomationIdProperty, ".");

		listview = new ListView(ListViewCachingStrategy.RetainElement)
		{
			AutomationId = "mainList",
			ItemsSource = items,
			ItemTemplate = template
		};
		var label = new Label
		{
			Text = "Touch and hold the item #0, until \"Test Item\" appear. So scroll the list until the end. If the app don't crash the test has passed"
		};
		Content = new StackLayout
		{
			Children =
			{
				label,
				listview
			}
		};
	}


	public class TestCell : TextCell
	{
		public TestCell()
		{
			var menuItem = new MenuItem { Text = "Test Item" };
			ContextActions.Add(menuItem);
		}
	}
}
