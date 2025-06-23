using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 45125, "ListView lacks a way to get information about visible elements (such as FirstVisibleItem) to restore visual positions of elements", PlatformAffected.iOS)]
public class Bugzilla45125 : TestContentPage
{
	int _TestNumber = 0;

	const string Instructions = "The black area below should show text listing appearing and disappearing events for the ListView beside it. It should update as you scroll the ListView, with each row firing a single Disappearing event and a single Appearing event as it leaves and enters the visible screen, respectively. If this does not happen, this test has failed.";
	const string AppearingLabelId = "appearing";
	const string DisappearingLabelId = "disappearing";
	const string TestButtonId = "TestButtonId";

	int _Appearing = 0;
	int _Disappearing = 0;

	Label _status;

	Label _groupsAppearing;

	Label _groupsDisappearing;

	ScrollView _scroll;

	class GroupItem
	{
		public string DisplayText { get; set; }
	}


	class GroupedData : List<GroupItem>
	{
		public string GroupName { get; set; }
	}


	class MyCell : ViewCell
	{
		public MyCell()
		{
			Label newLabel = new Label();
			newLabel.SetBinding(Label.TextProperty, nameof(GroupItem.DisplayText));
			View = newLabel;
		}
	}


	class HeaderCell : ViewCell
	{
		public HeaderCell()
		{
			Label newLabel = new Label();
			newLabel.SetBinding(Label.TextProperty, nameof(GroupedData.GroupName));
			View = newLabel;
		}
	}

	protected override void Init()
	{
		InitializeControls();

		_status.Text = _groupsAppearing.Text = _groupsDisappearing.Text = "";
		_Appearing = _Disappearing = 0;
		_scroll.SetScrolledPosition(0, 0);

		InitTest(ListViewCachingStrategy.RecycleElement, true);
	}

	void InitializeControls()
	{

		_status = new Label
		{
			TextColor = Colors.White
		};

		_groupsAppearing = new Label
		{
			TextColor = Colors.Green,
			AutomationId = AppearingLabelId
		};

		_groupsDisappearing = new Label
		{
			TextColor = Colors.Blue,
			AutomationId = DisappearingLabelId
		};

		_scroll = new ScrollView
		{
			BackgroundColor = Colors.Black,
			Content = _status,
			WidthRequest = 200,
			HeightRequest = 500
		};
	}

	void InitTest(ListViewCachingStrategy cachingStrategy, bool useTemplate)
	{
		List<GroupedData> groups = GetGroups();

		var listView = new ListView(cachingStrategy)
		{
			ItemsSource = groups,
			ItemTemplate = new DataTemplate(typeof(MyCell)),
			HasUnevenRows = true,
			HeightRequest = 500,
			WidthRequest = 200,

			// Must be grouped to repro
			IsGroupingEnabled = true
		};

		if (useTemplate)
			listView.GroupHeaderTemplate = new DataTemplate(typeof(HeaderCell));
		else
			listView.GroupDisplayBinding = new Binding(nameof(GroupedData.GroupName));

		// Must attach to the ListView's events to repro
		listView.ItemAppearing += ListView_ItemAppearing;
		listView.ItemDisappearing += ListView_ItemDisappearing;

		InitializeControls();

		var horStack = new HorizontalStackLayout
		{
			HeightRequest = 500
		};
		horStack.Children.Add(_scroll);
		horStack.Children.Add(listView);

		Button nextButton = new Button { Text = "Next", AutomationId = TestButtonId };
		nextButton.Clicked += NextButton_Clicked;
		VerticalStackLayout stack = new VerticalStackLayout
		{
			Children = { new Label { Text = Instructions }, _groupsAppearing, _groupsDisappearing, horStack, nextButton }
		};
		Content = stack;

		var lastGroup = groups.Last();
		var lastItem = lastGroup.First();

		var firstGroup = groups.First();
		var firstItem = firstGroup.First();

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Device.StartTimer(TimeSpan.FromSeconds(1), () => { listView.ScrollTo(lastItem, lastGroup, ScrollToPosition.End, true); return false; });
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Device.StartTimer(TimeSpan.FromSeconds(2), () => { listView.ScrollTo(firstItem, firstItem, ScrollToPosition.MakeVisible, true); return false; });
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

		_TestNumber++;
	}

	void NextButton_Clicked(object sender, EventArgs e)
	{
		_status.Text = _groupsAppearing.Text = _groupsDisappearing.Text = "";
		_Appearing = _Disappearing = 0;
		_scroll.SetScrolledPosition(0, 0);

		switch (_TestNumber)
		{
			default:
				InitTest(ListViewCachingStrategy.RecycleElement, useTemplate: true);
				break;
			case 1:
				InitTest(ListViewCachingStrategy.RetainElement, useTemplate: true);
				break;
			case 2:
				InitTest(ListViewCachingStrategy.RetainElement, useTemplate: false);
				break;
			case 3:
				InitTest(ListViewCachingStrategy.RecycleElement, useTemplate: false);
				break;
		}
	}

	List<GroupedData> GetGroups()
	{
		List<GroupedData> groups = new List<GroupedData>();

		for (int i = 1; i < 100; i++)
		{
			var group = new GroupedData { GroupName = $"Group {i}" };

			group.Add(new GroupItem { DisplayText = $"Item {i}" });

			groups.Add(group);
		}

		return groups;
	}

	static string GetItemText(object item)
	{
		var groupDisplayTextItem = item as TemplatedItemsList<ItemsView<Cell>, Cell>;
		var groupItem = item as GroupedData;
		var itemItem = item as GroupItem;

		var text = item.ToString();

		if (groupDisplayTextItem != null)
			text = groupDisplayTextItem.Name;
		else if (groupItem != null)
			text = groupItem.GroupName;
		else if (itemItem != null)
			text = itemItem.DisplayText;

		return text;
	}

	void ListView_ItemDisappearing(object sender, ItemVisibilityEventArgs e)
	{
		var text = GetItemText(e.Item);

		if (_status.Text?.Length > 500)
			_status.Text = "";

		_Disappearing++;

		_groupsDisappearing.Text = _Disappearing.ToString();

		_status.Text += $"\r\n {text} Disappearing";
		_scroll.ScrollToAsync(_status, ScrollToPosition.MakeVisible, false);
	}

	void ListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
	{
		var text = GetItemText(e.Item);

		if (_status.Text?.Length > 500)
			_status.Text = "";

		_Appearing++;

		_groupsAppearing.Text = _Appearing.ToString();

		_status.Text += $"\r\n {text} Appearing";
		_scroll.ScrollToAsync(_status, ScrollToPosition.MakeVisible, false);
	}
}