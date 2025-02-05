using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 3019, "Grouped ListView Header empty for adding items", PlatformAffected.UWP)]

public class Issue3019 : TestContentPage
{
	ListView _listViewIsGrouped;



	class MyHeaderViewCell : ViewCell
	{
		public MyHeaderViewCell()
		{
			Height = 25;
			var label = new Label { VerticalOptions = LayoutOptions.Center };
			label.SetBinding(Label.TextProperty, nameof(GroupedItem.Name));
			label.SetBinding(Label.AutomationIdProperty, nameof(GroupedItem.Name));
			View = new StackLayout()
			{
				Children =
				{
					label
				}
			};
		}
	}


	class Item
	{
		static int counter = 0;
		public Item()
		{
			Text = $"Grouped Item: {counter++}";
		}

		public string Text { get; }

	}


	class GroupedItem : List<Item>
	{
		public GroupedItem()
		{
			AddRange(Enumerable.Range(0, 1).Select(i => new Item()));
		}

		public string Name { get; set; }
	}

	void LoadData()
	{
		_listViewIsGrouped.ItemsSource = new ObservableCollection<GroupedItem>(Enumerable.Range(0, 1).Select(x => new GroupedItem() { Name = $"Group {x}" }));
	}

	void AddData()
	{
		var list = _listViewIsGrouped.ItemsSource as IList<GroupedItem>;
		list.Add(new GroupedItem() { Name = $"Group {list.Count}" });
	}

	void ReloadListViews()
	{
		StackLayout content = Content as StackLayout;

		if (_listViewIsGrouped != null)
		{
			content.Children.Remove(_listViewIsGrouped);
		}

		_listViewIsGrouped = new ListView
		{
			IsGroupingEnabled = true,
			GroupHeaderTemplate = new DataTemplate(typeof(MyHeaderViewCell)),
			ItemTemplate = new DataTemplate(() =>
			{
				Label nameLabel = new Label();
				nameLabel.SetBinding(Label.TextProperty, "Text");
				nameLabel.SetBinding(Label.AutomationIdProperty, "Text");
				var cell = new ViewCell
				{
					View = nameLabel,
				};
				return cell;
			}),
			ItemsSource = new ObservableCollection<GroupedItem>()
		};

		content.Children.Add(_listViewIsGrouped);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		AddData();
	}

	protected override void Init()
	{
		Label label = new Label() { Text = "If you see two group headers and can click on each row without crashing test has passed", AutomationId = "MessageLabel" };

		Content = new StackLayout
		{
			Children =
			{
				label,
				new Button()
				{
					Text = "Click to add more rows",
					Command = new Command(() =>
					{
						AddData();
					})
				}
			},
		};

		ReloadListViews();
		LoadData();

		_listViewIsGrouped.ItemSelected += (sender, args) =>
		{
			label.Text = (args.SelectedItem as Item).Text + " Clicked";
		};
	}
}
