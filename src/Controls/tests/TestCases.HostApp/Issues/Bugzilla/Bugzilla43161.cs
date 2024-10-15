namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 43161, "[iOS] Setting Accessory in ViewCellRenderer breaks layout", PlatformAffected.iOS)]
public class Bugzilla43161 : TestContentPage
{
	const string Instructions = "On iOS, all three of the following ListViews should have ListItems labeled with numbers and a right arrow. If any of the ListViews does not contain numbers, this test has failed.";
	const string ListView1 = "Accessory with Context Actions";
	const string ListView2 = "Accessory with RecycleElement";
	const string ListView3 = "Accessory with RetainElement";


	public class AccessoryViewCell : ViewCell
	{
		public AccessoryViewCell()
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			label.SetBinding(Label.AutomationIdProperty, ".");
			View = label;
		}
	}


	public class AccessoryViewCellWithContextActions : AccessoryViewCell
	{
		public AccessoryViewCellWithContextActions()
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			View = label;

			var delete = new MenuItem { Text = "Delete" };
			ContextActions.Add(delete);
		}
	}

	protected override void Init()
	{
		var label = new Label { Text = Instructions };
		var listView = new ListView { ItemTemplate = new DataTemplate(typeof(AccessoryViewCellWithContextActions)), ItemsSource = Enumerable.Range(0, 9), Header = ListView1 };
		var listView2 = new ListView(ListViewCachingStrategy.RecycleElement) { ItemTemplate = new DataTemplate(typeof(AccessoryViewCell)), ItemsSource = Enumerable.Range(10, 19), Header = ListView2 };
		var listView3 = new ListView { ItemTemplate = new DataTemplate(typeof(AccessoryViewCell)), ItemsSource = Enumerable.Range(20, 29), Header = ListView3 };

		Content = new StackLayout { Children = { label, listView, listView2, listView3 } };
	}
}