namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell Gestures Test",
	PlatformAffected.All)]
public class ShellGestures : TestShell
{
	const string SwipeTitle = "Swipe";
	const string SwipeGestureSuccess = "SwipeGesture Success";
	const string SwipeGestureSuccessId = "SwipeGestureSuccessId";

	const string TouchListenerTitle = "IOnTouchListener";
	public const string TouchListenerSuccess = "TouchListener Success";
	const string TouchListenerSuccessId = "TouchListenerSuccessId";

	const string TableViewTitle = "Table View";
	const string TableViewId = "TableViewId";

	const string ListViewTitle = "List View";
	const string ListViewId = "ListViewId";

	protected override void Init()
	{
		this.IncreaseFlyoutItemsHeightSoUITestsCanClickOnThem();
		var gesturePage = CreateContentPage(shellItemTitle: SwipeTitle);
		var label = new Label()
		{
			Text = "Swipe Right and Text Should Change to SwipeGestureSuccess",
			AutomationId = SwipeGestureSuccessId
		};

		var stackLayout = new StackLayout()
		{
			GestureRecognizers =
			{
				new SwipeGestureRecognizer()
				{
					Direction = SwipeDirection.Right,
					Command = new Command(() =>
					{
						label.Text = SwipeGestureSuccess;
					})
				}
			}
		};

		stackLayout.Add(new Label() { Text = "Click through flyout items for all the tests" });
		stackLayout.Add(label);

		gesturePage.Content = stackLayout;

		var webViewPage = CreateContentPage(shellItemTitle: "Webview");
#pragma warning disable CS0618 // Type or member is obsolete
		webViewPage.Content = new StackLayout()
		{
			new Label
			{
				Text = "Make sure you can scroll the web page up and down"
			},
			new WebView()
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				Source = "https://www.dot.net/maui"
			}
		};
#pragma warning restore CS0618 // Type or member is obsolete

		var tableViewPage = CreateContentPage(shellItemTitle: TableViewTitle);

		TableView tableView = new TableView() { Intent = TableIntent.Settings, AutomationId = TableViewId };
		TableRoot tableRoot = new TableRoot();
		tableView.Root = tableRoot;

		for (int i = 0; i < 100; i++)
		{
			TableSection tableSection = new TableSection()
			{
				Title = $"section{++i}"
			};
			var text = $"entry{++i}";
			tableSection.Add(new EntryCell() { Label = text, AutomationId = text });
			text = $"entry{++i}";
			tableSection.Add(new EntryCell() { Label = text, AutomationId = text });

			tableRoot.Add(tableSection);
		}
		tableViewPage.Content = tableView;


		var listViewPage = CreateContentPage(shellItemTitle: ListViewTitle);
		ListView listView = new ListView(ListViewCachingStrategy.RecycleElement) { AutomationId = ListViewId };
		listView.ItemsSource = Enumerable.Range(0, 100).Select(x => $"{x} Entry").ToList();
		listViewPage.Content = listView;

		if (OperatingSystem.IsAndroid())
		{
			var touchListenter = CreateContentPage(shellItemTitle: TouchListenerTitle);
			touchListenter.Content = new TouchTestView();
		}
	}

	public class TouchTestView : ContentView
	{
		public Label Results = new Label() { AutomationId = TouchListenerSuccessId };
		public TouchTestView()
		{
			Content = new StackLayout()
			{
				Results
			};

			Results.Text = "Swipe across the screen. This label should change to say Success";
		}
	}
}
