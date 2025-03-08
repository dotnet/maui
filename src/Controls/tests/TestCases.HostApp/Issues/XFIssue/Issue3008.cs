using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 3008, "Setting ListView.ItemSource to null doesn't cause it clear out its contents", PlatformAffected.UWP)]

public class Issue3008 : TestContentPage
{
	ListView _listView;
	ListView _listViewIsGrouped;
	const string success1 = "InitialLoad: you should see a grouped and not grouped list view";
	const string successEmpty1 = "Source is set to null: you should see nothing";
	const string success2 = "Reload1: you should see a grouped and not grouped list view";
	const string successEmpty2 = "If you see nothing now test has passed";
	const string successEmpty3 = "List loaded and ItemSource not set: you should see nothing";



	class MyHeaderViewCell : ViewCell
	{
		public MyHeaderViewCell()
		{
			Height = 25;
			var label = new Label { VerticalOptions = LayoutOptions.Center };
			label.SetBinding(Label.TextProperty, nameof(GroupedItem.Name));
			View = label;
		}
	}


	class GroupedItem : List<Item>
	{
		public GroupedItem()
		{
			AddRange(Enumerable.Range(0, 3).Select(i => new Item()));
		}
		public string Name { get; set; }
	}



	class Item
	{

	}

	void LoadData()
	{
		_listViewIsGrouped.ItemsSource = new ObservableCollection<GroupedItem>(Enumerable.Range(0, 3).Select(x => new GroupedItem() { Name = $"Group {x}" }));
		_listView.ItemsSource = new ObservableCollection<Item>(Enumerable.Range(0, 13).Select(x => new Item()));

	}

	void ReloadListViews()
	{
		StackLayout content = Content as StackLayout;

		if (_listView != null)
		{
			content.Children.Remove(_listView);
			content.Children.Remove(_listViewIsGrouped);
		}
		_listView = new ListView
		{
			ItemTemplate = new DataTemplate(() =>
			{
				Label nameLabel = new Label() { Text = "Not Grouped Item", AutomationId = "NotGroupedItem" };
				var cell = new ViewCell
				{
					View = nameLabel,
				};
				return cell;
			}),
		};
		_listViewIsGrouped = new ListView
		{
			IsGroupingEnabled = true,
			GroupHeaderTemplate = new DataTemplate(typeof(MyHeaderViewCell)),
			ItemTemplate = new DataTemplate(() =>
			{
				Label nameLabel = new Label() { Text = "Grouped Item", AutomationId = "GroupedItem" };
				var cell = new ViewCell
				{
					View = nameLabel,
				};
				return cell;
			}),
		};

		content.Children.Add(_listView);
		content.Children.Add(_listViewIsGrouped);
	}

	protected override void Init()
	{
		Label label = new Label();

		int clickCount = 0;
		Content = new StackLayout
		{
			Children =
			{
				label,
				new Button()
				{
					Text = "Click Until Success",
					AutomationId = "ClickUntilSuccess",
					Command = new Command(() =>
					{
						if(clickCount == 0)
						{
							LoadData();
							label.Text = success1;
						}
						else if(clickCount == 1)
						{
							ReloadListViews();
							LoadData();
							label.Text = success1;
						}
						else if(clickCount <= 3)
						{
							if(_listViewIsGrouped.ItemsSource != null)
							{
								_listViewIsGrouped.ItemsSource = null;
								_listView.ItemsSource = null;
								label.Text = successEmpty1;
							}
							else
							{
								LoadData();
								label.Text = success2;
							}
						}
						else if(clickCount <= 5)
						{
							if(_listViewIsGrouped.ItemsSource != null)
							{
								ReloadListViews();
								label.Text = successEmpty3;
							}
							else
							{
								LoadData();
								label.Text = success2;
							}
						}
						else
						{
							if(_listViewIsGrouped.ItemsSource != null)
							{
								_listViewIsGrouped.ItemsSource = new List<GroupedItem>();
								_listView.ItemsSource = new List<Item>();
								label.Text = successEmpty2;
							}
						}

						clickCount++;
					})
				}
			},
		};

		ReloadListViews();
	}
}
