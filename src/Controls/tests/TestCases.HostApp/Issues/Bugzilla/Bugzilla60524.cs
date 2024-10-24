using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 60524, "NRE when rendering ListView with grouping enabled and HasUnevenRows set to true", PlatformAffected.iOS)]
public class Bugzilla60524 : TestNavigationPage
{

	public class GroupedItemsPage : ContentPage
	{
		private ObservableCollection<Grouping<string, GroupedItem>> model;
		private ListView listView;

		public GroupedItemsPage()
		{
			listView = new ListView(ListViewCachingStrategy.RetainElement) { HasUnevenRows = true };
			listView.IsGroupingEnabled = true;
			listView.ItemTemplate = new GroupedItemsDataTemplateSelector();

			var headerCell = new DataTemplate(typeof(TextCell));
			headerCell.SetBinding(TextCell.TextProperty, "Key");
			this.listView.GroupHeaderTemplate = headerCell;

			this.model = new ObservableCollection<Grouping<string, GroupedItem>>();
			this.GetItems();

			this.SetMainContent();
		}

		private void GetItems()
		{
			var zeroGroup = new Grouping<string, GroupedItem>("Group 0", new List<GroupedItem>
			{
			});

			var firstGroup = new Grouping<string, GroupedItem>("Group 1", new List<GroupedItem> {
				new GroupedItem("Group 1", "Item 1"),
				new GroupedItem("Group 1", "Item 2")
			});

			var secondGroup = new Grouping<string, GroupedItem>("Group 2", new List<GroupedItem> {
				new GroupedItem("Group 2", "Item 3"),
				new GroupedItem("Group 2", "Item 4")
			});

			model.Add(zeroGroup);
			model.Add(firstGroup);
			model.Add(secondGroup);

			this.listView.ItemsSource = model;
		}

		private void SetMainContent()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var content = new StackLayout
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = { new Label { Text = "If this page does not crash, this test has passed." }, this.listView }
			};
#pragma warning restore CS0618 // Type or member is obsolete

			Content = content;
		}
	}

	public class GroupedItemsDataTemplateSelector : Microsoft.Maui.Controls.DataTemplateSelector
	{
		private readonly DataTemplate firstGroupTemplate;
		private readonly DataTemplate secondGroupTemplate;

		public GroupedItemsDataTemplateSelector()
		{
			// Retain instances
			var firstTemplate = new DataTemplate(typeof(TextCell));
			firstTemplate.SetBinding(TextCell.TextProperty, "Item");
			this.firstGroupTemplate = firstTemplate;

			var secondTemplate = new DataTemplate(typeof(ImageCell));
			secondTemplate.SetBinding(ImageCell.TextProperty, "Item");
			this.secondGroupTemplate = secondTemplate;
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var model = item as GroupedItem;

			if (model == null)
			{
				return null;
			}

			return model.Group == "Group 1" ? this.firstGroupTemplate : this.secondGroupTemplate;
		}
	}

	public class GroupedItem
	{
		public string Group { get; set; }

		public string Item { get; set; }

		public GroupedItem(string group, string item)
		{
			this.Group = group;
			this.Item = item;
		}
	}

	public class Grouping<K, T> : ObservableCollection<T>
	{
		public K Key { get; private set; }

		public IList<T> Values
		{
			get { return this.Items; }
		}

		public Grouping(K key, IEnumerable<T> items)
		{
			Key = key;
			foreach (var item in items)
			{
				this.Items.Add(item);
			}
		}

		public Grouping(IGrouping<K, T> grouping)
		{
			Key = grouping.Key;
			foreach (var item in grouping)
			{
				this.Items.Add(item);
			}
		}

		public void AddRange(IList<T> values)
		{
			foreach (var item in values)
			{
				this.Items.Add(item);
			}
		}
	}

	public class GroupingKey
	{
		public string Title { get; private set; }

		public string Abbreviation { get; private set; }

		public GroupingKey(string Title, string abbrevation)
		{
			this.Title = Title;
			this.Abbreviation = abbrevation;
		}
	}

	protected override void Init()
	{
		Navigation.PushAsync(new GroupedItemsPage());
	}
}