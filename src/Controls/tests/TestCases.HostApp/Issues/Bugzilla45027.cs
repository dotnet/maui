namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 45027, "App crashes when double tapping on ToolbarItem or MenuItem very quickly", PlatformAffected.Android)]
public class Bugzilla45027 : TestContentPage
{
	const string BUTTON_ACTION_TEXT = "Action";
	const string BUTTON_DELETE_TEXT = "Delete";

	List<int> _list;
	public List<int> List
	{
		get
		{
			if (_list == null)
			{
				_list = new List<int>();
				for (var i = 0; i < 10; i++)
					_list.Add(i);
			}

			return _list;
		}
	}

	protected override void Init()
	{
		var stackLayout = new StackLayout
		{
			Orientation = StackOrientation.Vertical,
			Children =
			{
				new Label
				{
					Text = "Long tap list items to display context menu. Double tapping each action rapidly should not crash.",
					HorizontalTextAlignment = TextAlignment.Center
				}
			}
		};

		var listView = new ListView
		{
			ItemsSource = List,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("."));
				label.SetBinding(Label.AutomationIdProperty, new Binding("."));

				return new ViewCell
				{
					View = new ContentView
					{
						Content = label,
					},
					ContextActions = { new MenuItem
					{
						Text = BUTTON_ACTION_TEXT,
						AutomationId = BUTTON_ACTION_TEXT
					},
					new MenuItem
					{
						Text = BUTTON_DELETE_TEXT,
						AutomationId = BUTTON_DELETE_TEXT,
						IsDestructive = true
					} }
				};
			})
		};
		stackLayout.Children.Add(listView);

		Content = stackLayout;
	}
}