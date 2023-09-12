using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TemplatedItemsListTests : BaseTestFixture
	{
		class MockItemsView
			: ItemsView<BindableObject>
		{
			public MockItemsView()
			{
				TemplatedItems.IsGroupingEnabledProperty = IsGroupingEnabledProperty;
			}

			public event Action<BindableObject> Hooked;
			public event Action<BindableObject> Unhooked;

			public static readonly BindableProperty IsGroupingEnabledProperty = BindableProperty.Create(nameof(MockItemsView.IsGroupingEnabled), typeof(bool), typeof(MockItemsView), false);

			public bool IsGroupingEnabled
			{
				get { return (bool)GetValue(IsGroupingEnabledProperty); }
				set { SetValue(IsGroupingEnabledProperty, value); }
			}

			protected override BindableObject CreateDefault(object item)
			{
				return new TextCell { Text = item.ToString() };
			}

			public new TemplatedItemsList<ItemsView<BindableObject>, BindableObject> TemplatedItems
			{
				get { return base.TemplatedItems; }
			}

			BindingBase groupShortNameBinding;
			public BindingBase GroupShortNameBinding
			{
				get { return groupShortNameBinding; }
				set
				{
					if (groupShortNameBinding == value)
						return;
					OnPropertyChanging();
					groupShortNameBinding = value;
					TemplatedItems.GroupShortNameBinding = value;
					OnPropertyChanged();
				}
			}

			BindingBase groupDisplayBinding;
			public BindingBase GroupDisplayBinding
			{
				get { return groupDisplayBinding; }
				set
				{
					if (groupDisplayBinding == value)
						return;

					OnPropertyChanging();
					groupDisplayBinding = value;
					TemplatedItems.GroupDisplayBinding = value;
					OnPropertyChanged();
				}
			}

			protected override void SetupContent(BindableObject content, int index)
			{
				base.SetupContent(content, index);

				if (Hooked != null)
					Hooked(content);
			}

			protected override void UnhookContent(BindableObject content)
			{
				base.UnhookContent(content);

				if (Unhooked != null)
					Unhooked(content);
			}
		}


		public TemplatedItemsListTests()
		{

			bindable = new MockItemsView();
		}

		MockItemsView bindable;

		[Fact]
		public void ListProxyNotNullWithNullItemsSource()
		{
			Assert.NotNull(bindable.TemplatedItems.ListProxy);
		}

		[Fact]
		public void ResetOnItemsSourceChanged()
		{
			bool raised = false;
			NotifyCollectionChangedAction action = default(NotifyCollectionChangedAction);
			bindable.TemplatedItems.CollectionChanged += (sender, args) =>
			{
				raised = true;
				action = args.Action;
			};

			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, Enumerable.Empty<string>());

			Assert.True(raised, "CollectionChanged was not raised");
			Assert.Equal(NotifyCollectionChangedAction.Reset, action);
		}
		/*
		[Fact]
		public void ResetOnInfiniteScrollingChanged()
		{
			bool raised = false;
			NotifyCollectionChangedAction action = default(NotifyCollectionChangedAction);
			bindable.TemplatedItems.CollectionChanged += (sender, args) => {
				raised = true;
				action = args.Action;
			};

			bindable.SetValue (ItemsView.InfiniteScrollingProperty, true);

			Assert.IsTrue (raised, "CollectionChanged was not raised");
			Assert.AreEqual (NotifyCollectionChangedAction.Reset, action);
		}*/

		[Fact]
		public void ResetOnTemplateChanged()
		{
			// Template changes won't trigger a reset if there's no items.
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, new[] { "Foo" });

			bool raised = false;
			NotifyCollectionChangedAction action = default(NotifyCollectionChangedAction);
			bindable.TemplatedItems.CollectionChanged += (sender, args) =>
			{
				raised = true;
				action = args.Action;
			};

			bindable.SetValue(ItemsView<BindableObject>.ItemTemplateProperty, new DataTemplate());

			Assert.True(raised, "CollectionChanged was not raised");
			Assert.Equal(NotifyCollectionChangedAction.Reset, action);
		}

		[Fact]
		public void PassThroughChanges()
		{
			var collection = new ObservableCollection<string>();
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			bool raised = false;
			NotifyCollectionChangedEventArgs args = null;
			bindable.TemplatedItems.CollectionChanged += (sender, eventArgs) =>
			{
				raised = true;
				args = eventArgs;
			};

			string str = "foo bar";
			collection.Add(str);

			Assert.True(raised, "CollectionChanged was not raised");
			Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
			Assert.Equal(0, args.NewStartingIndex);
			Assert.NotNull(args.NewItems);
			Assert.Single(args.NewItems);
			Assert.Same(str, ((Cell)args.NewItems[0]).BindingContext);
		}

		[Fact]
		public void ListProxyUpdatesWithItemsSource()
		{
			var collection = new List<string> { "foo bar" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			Assert.NotNull(bindable.TemplatedItems.ListProxy);
			Assert.Equal(collection.Count, bindable.TemplatedItems.ListProxy.Count);
		}

		[Theory, InlineData(0), InlineData(1), InlineData(2)]
		public void GetOrCreateContent(int index)
		{
			var collection = new List<string> { "foo", "bar", "baz" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			DataTemplate template = new DataTemplate(typeof(TextCell));
			template.SetBinding(TextCell.TextProperty, new Binding("."));
			bindable.SetValue(ItemsView<BindableObject>.ItemTemplateProperty, template);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.NotNull(content);

			TextCell textCell = content as TextCell;
			Assert.NotNull(textCell);

			Assert.Same(collection[index], textCell.BindingContext);
			Assert.Same(collection[index], textCell.Text);

			BindableObject content2 = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.Same(content, content2);
		}

		[Fact]
		public void GetOrCreateContentDefault()
		{
			Assert.Null(bindable.GetValue(ItemsView<BindableObject>.ItemTemplateProperty));

			var collection = new List<string> { "foo", "bar", "baz" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			const int index = 0;
			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.NotNull(content);

			TextCell textCell = content as TextCell;
			Assert.True(textCell != null, $"Content was did not match the template type, expected {typeof(TextCell)} but got {content.GetType()}");

			Assert.Same(collection[index], textCell.BindingContext);
			Assert.Same(collection[index], textCell.Text);

			BindableObject content2 = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.Same(content, content2);
		}

		[Fact]
		public void GetOrCreateContentAfterTemplateChange()
		{
			var collection = new List<string> { "foo", "bar", "baz" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			const int index = 0;

			DataTemplate template = new DataTemplate(typeof(TextCell));
			template.SetBinding(TextCell.TextProperty, new Binding("."));
			bindable.SetValue(ItemsView<BindableObject>.ItemTemplateProperty, template);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.IsType<TextCell>(content);

			template = new DataTemplate(typeof(SwitchCell));
			template.SetBinding(SwitchCell.TextProperty, new Binding("."));

			bindable.SetValue(ItemsView<BindableObject>.ItemTemplateProperty, template);

			BindableObject content2 = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.NotNull(content2);
			Assert.IsType<SwitchCell>(content2);

			var switchCell = (SwitchCell)content2;
			Assert.Same(collection[index], switchCell.BindingContext);
		}

		[Fact]
		public void GetOrCreateContentAfterItemsSourceChanged()
		{
			var collection = new List<string> { "foo", "bar", "baz" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			const int index = 0;

			DataTemplate template = new DataTemplate(typeof(TextCell));
			template.SetBinding(TextCell.TextProperty, new Binding("."));
			bindable.SetValue(ItemsView<BindableObject>.ItemTemplateProperty, template);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.NotNull(content);

			collection = new List<string> { "we", "wee", "weee" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			var content2 = (TextCell)bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);

			Assert.NotSame(content, content2);
			Assert.Same(collection[index], content2.BindingContext);
			Assert.Same(collection[index], content2.Text);
		}

		/*[Fact]
		public void GetOrCreateContentAfterInfiniteScrollingChanged()
		{
			var collection = new List<string> { "foo", "bar", "baz" };
			bindable.SetValue (ItemsView.ItemsSourceProperty, collection);

			const int index = 0;

			DataTemplate template = new DataTemplate (typeof (TextCell));
			template.SetBinding (TextCell.TextProperty, new Binding ("."));
			bindable.SetValue (ItemsView.ItemTemplateProperty, template);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent (index, collection[index], o => new TextCell (o.ToString()));
			Assert.IsNotNull (content);

			bindable.SetValue (ItemsView.InfiniteScrollingProperty, true);

			var content2 = (TextCell)bindable.TemplatedItems.GetOrCreateContent (index, collection[index], o => new TextCell (o.ToString()));

			Assert.AreNotSame (content, content2);
			Assert.AreSame (collection[index], content2.BindingContext);
			Assert.AreSame (collection[index], content2.Text);
		}*/

		[Fact("Make sure we're not duplicate cell instances for an equal item if it's a different index")]
		public void GetOrCreateContentEqualItemDifferentItemDifferentIndex()
		{
			Assert.Null(bindable.GetValue(ItemsView<BindableObject>.ItemTemplateProperty));

			var collection = new List<string> { "foo", "foo" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(0, collection[0]);
			Assert.NotNull(content);

			TextCell textCell = content as TextCell;
			Assert.True(textCell != null, $"Content was did not match the template type, expected {typeof(TextCell)} but got {content.GetType()}");

			Assert.Same(collection[0], textCell.BindingContext);
			Assert.Same(collection[0], textCell.Text);

			BindableObject content2 = bindable.TemplatedItems.GetOrCreateContent(1, collection[1]);
			Assert.NotSame(content, content2);

			Assert.Same(collection[1], textCell.BindingContext);
			Assert.Same(collection[1], textCell.Text);
		}

		[Fact("Make sure we're not duplicate cell instances for the same item if it's a different index")]
		public void GetOrCreateContentEqualItemSameItemDifferentIndex()
		{
			Assert.Null(bindable.GetValue(ItemsView<BindableObject>.ItemTemplateProperty));

			string item = "foo";

			var collection = new List<string> { item, item };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(0, item);
			Assert.NotNull(content);

			TextCell textCell = content as TextCell;
			Assert.True(textCell != null, $"Content was did not match the template type, expected {typeof(TextCell)} but got {content.GetType()}");

			Assert.Same(item, textCell.BindingContext);
			Assert.Same(item, textCell.Text);

			BindableObject content2 = bindable.TemplatedItems.GetOrCreateContent(1, item);
			Assert.NotSame(content, content2);

			Assert.Same(item, textCell.BindingContext);
			Assert.Same(item, textCell.Text);
		}

		[Fact]
		public void ItemsSourceInsertPreRealzied()
		{
			var collection = new ObservableCollection<string> { "foo", "bar" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			collection.Insert(1, "baz");

			Assert.Equal(3, bindable.TemplatedItems.Count);
			Assert.Same(bindable.TemplatedItems.GetOrCreateContent(0, collection[0]).BindingContext, collection[0]);
			Assert.Same(bindable.TemplatedItems.GetOrCreateContent(1, collection[1]).BindingContext, collection[1]);
			Assert.Same(bindable.TemplatedItems.GetOrCreateContent(2, collection[2]).BindingContext, collection[2]);
		}

		[Fact]
		public void ItemsSourceInsertPostRealized()
		{
			var collection = new ObservableCollection<string> { "foo", "bar" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			// Force the handler to realize/create the content
			bindable.TemplatedItems.GetOrCreateContent(0, collection[0]);
			bindable.TemplatedItems.GetOrCreateContent(1, collection[1]);

			collection.Insert(1, "baz");

			Assert.Equal(3, bindable.TemplatedItems.Count);
			Assert.Same(bindable.TemplatedItems.GetOrCreateContent(0, collection[0]).BindingContext, collection[0]);
			Assert.Same(bindable.TemplatedItems.GetOrCreateContent(1, collection[1]).BindingContext, collection[1]);
			Assert.Same(bindable.TemplatedItems.GetOrCreateContent(2, collection[2]).BindingContext, collection[2]);
		}

		[Theory]
		[InlineData(0, 0, 0)]
		[InlineData(3, 1, 0)]
		public void GetGroupIndexFromGlobalGroupIndex(int globalIndex, int expectedIndex, int expectedLeftOver)
		{
			var collection = new[] {
				new[] { "foo", "fad" },
				new[] { "bar", "baz" }
			};
			bindable.ItemsSource = collection;
			bindable.IsGroupingEnabled = true;

			int leftOver;
			int index = bindable.TemplatedItems.GetGroupIndexFromGlobal(globalIndex, out leftOver);
			Assert.Equal(index, expectedIndex);
			Assert.Equal(leftOver, expectedLeftOver);
		}

		[Theory]
		[InlineData(1, 0, 1)]
		[InlineData(2, 0, 2)]
		[InlineData(4, 1, 1)]
		[InlineData(5, 1, 2)]
		public void GetGroupIndexFromGlobalItemIndex(int globalIndex, int expectedIndex, int expectedLeftOver)
		{
			var collection = new[] {
				new[] { "foo", "fad" },
				new[] { "bar", "baz" }
			};
			bindable.ItemsSource = collection;
			bindable.IsGroupingEnabled = true;

			int leftOver;
			int index = bindable.TemplatedItems.GetGroupIndexFromGlobal(globalIndex, out leftOver);
			Assert.Equal(index, expectedIndex);
			Assert.Equal(leftOver, expectedLeftOver);
		}



		[Theory]
		[InlineData(0, 0)]
		[InlineData(0, 1)]
		[InlineData(0, 2)]
		[InlineData(1, 0)]
		[InlineData(1, 1)]
		[InlineData(1, 2)]
		[InlineData(2, 0)]
		[InlineData(2, 1)]
		[InlineData(2, 2)]
		public void GetGroupAndIndexOfItem(int group, int index)
		{
			var collection = new[] {
				new[] { "foo", "fad" },
				new[] { "bar", "baz" }
			};
			bindable.ItemsSource = collection;
			bindable.IsGroupingEnabled = true;

			object item = null;
			if (group < collection.Length)
			{
				if (index < collection[group].Length)
					item = collection[group][index];
			}

			if (item == null)
			{
				item = "not in here";
				group = index = -1;
			}

			var location = bindable.TemplatedItems.GetGroupAndIndexOfItem(item);
			Assert.Equal(group, location.Item1);
			Assert.Equal(index, location.Item2);
		}

		[Fact]
		public void GetGroupAndIndexOfItemNotGrouped()
		{
			var items = Enumerable.Range(0, 10).ToList();
			bindable.ItemsSource = items;

			var location = bindable.TemplatedItems.GetGroupAndIndexOfItem(null, items[2]);

			Assert.Equal(0, location.Item1);
			Assert.Equal(2, location.Item2);
		}

		[Fact]
		public void ItemsSourcePropertyChangedWithBindable()
		{
			bool raised = false;
			bindable.TemplatedItems.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == "ItemsSource")
					raised = true;
			};

			bindable.ItemsSource = new object[0];

			Assert.True(raised, "INPC not raised for ItemsSource");
		}

		[Fact]
		public void IndexCorrectAfterMovingGroups()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.IsGroupingEnabled = true;

			items.Move(0, 1);

			var til = bindable.TemplatedItems.GetGroup(1);
			int index = GetIndex(til.HeaderContent);

			Assert.Equal(1, index);
		}

		[Fact]
		public void ShortNameSetBeforeGrouping()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupDisplayBinding = new Binding(".[0]");
			bindable.GroupShortNameBinding = new Binding(".[0]");
			bindable.IsGroupingEnabled = true;

			Assert.Equal("foo", bindable.TemplatedItems.ShortNames[0]);
		}

		[Fact]
		public void ItemAddedWithShortNameSetButUngrouped()
		{
			var items = new ObservableCollection<string> { "foo", "bar" };

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");

			items.Add("baz");
		}

		[Fact]
		public void ItemAddedWithShortNameSetButGroupingDisabled()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;
			bindable.IsGroupingEnabled = false;

			items.Add(new ObservableCollection<string>());
		}

		int GetIndex(BindableObject item)
		{
			return TemplatedItemsList<ItemsView<BindableObject>, BindableObject>.GetIndex(item);
		}

		[Fact]
		public void GetIndexTest()
		{
			var items = new List<string> { "foo", "bar", "baz" };

			bindable.ItemsSource = items;

			BindableObject item = bindable.TemplatedItems.GetOrCreateContent(1, items[1]);
			int index = GetIndex(item);
			Assert.Equal(1, index);
		}

		[Fact]
		public void GetIndexAfterInsert()
		{
			var items = new ObservableCollection<string> { "foo", "bar", "baz" };

			bindable.ItemsSource = items;

			BindableObject originalItem = bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			items.Insert(1, "fad");

			BindableObject item = bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			int index = GetIndex(item);
			Assert.Equal(1, index);

			index = GetIndex(originalItem);
			Assert.Equal(2, index);
		}

		[Fact]
		public void GetIndexAfterMove()
		{
			var items = new ObservableCollection<string> { "foo", "bar", "baz" };

			bindable.ItemsSource = items;

			BindableObject item0 = bindable.TemplatedItems.GetOrCreateContent(0, items[0]);
			BindableObject item1 = bindable.TemplatedItems.GetOrCreateContent(1, items[1]);
			BindableObject item2 = bindable.TemplatedItems.GetOrCreateContent(2, items[2]);

			items.Move(0, 2); // foo, bar, baz becomes bar (1), baz (2), foo (0)

			Assert.Equal(2, GetIndex(item0));
			Assert.Equal(0, GetIndex(item1));
			Assert.Equal(1, GetIndex(item2));
		}

		[Fact]
		public void GetIndexAfterRemove()
		{
			var items = new ObservableCollection<string> { "foo", "bar", "baz" };

			bindable.ItemsSource = items;

			BindableObject item1 = bindable.TemplatedItems.GetOrCreateContent(1, items[1]);
			BindableObject item2 = bindable.TemplatedItems.GetOrCreateContent(2, items[2]);

			items.RemoveAt(0);

			Assert.Equal(0, GetIndex(item1));
			Assert.Equal(1, GetIndex(item2));
		}

		[Fact]
		public void GetGroup()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			var til = bindable.TemplatedItems.GetGroup(1);
			var item = til[1];

			var group = TemplatedItemsList<ItemsView<BindableObject>, BindableObject>.GetGroup(item);

			Assert.Same(group, til);
		}

		[Fact]
		public void GetIndexGrouped()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			var til = bindable.TemplatedItems.GetGroup(1);
			var item = til[1];

			int index = GetIndex(item);
			Assert.Equal(1, index);
		}

		[Fact]
		public void GetGroupAndIndexOfItemTest()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			var result = bindable.TemplatedItems.GetGroupAndIndexOfItem(items[1], items[1][1]);

			Assert.Equal(1, result.Item1);
			Assert.Equal(1, result.Item2);
		}

		[Fact]
		public void GetGroupAndIndexOfItemNoGroup()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			var result = bindable.TemplatedItems.GetGroupAndIndexOfItem(null, items[1][1]);

			Assert.Equal(1, result.Item1);
			Assert.Equal(1, result.Item2);
		}

		[Fact]
		public void GetGroupAndIndexOfItemNotFound()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			var group = new ObservableCollection<string> { "bam" };

			var result = bindable.TemplatedItems.GetGroupAndIndexOfItem(group, group[0]);

			Assert.Equal(-1, result.Item1);
			Assert.Equal(-1, result.Item2);
		}

		[Fact]
		public void GetGroupAndIndexOfItemItemNotFound()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			var result = bindable.TemplatedItems.GetGroupAndIndexOfItem(items[1], "bam");

			Assert.Equal(1, result.Item1);
			Assert.Equal(-1, result.Item2);
		}

		[Fact("Issue #2464: ANE thrown when moving items in a ListView")]
		public void MovingPastRealizedWindowAndBackDoesntThrow()
		{
			var items = new ObservableCollection<string>(Enumerable.Range(0, 100).Select(i => i.ToString()));

			bindable.ItemsSource = items;

			bindable.TemplatedItems.GetOrCreateContent(0, items[0]);
			bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			items.Move(0, 3);
			items.Move(3, 2);

			Assert.Equal(0, GetIndex(bindable.TemplatedItems[0]));
			Assert.Equal(1, GetIndex(bindable.TemplatedItems[1]));
			Assert.Equal(2, GetIndex(bindable.TemplatedItems[2]));
			Assert.Equal(3, GetIndex(bindable.TemplatedItems[3]));
		}

		[Fact]
		public void GetGlobalIndexOfItem()
		{
			var items = new ObservableCollection<string>(Enumerable.Range(0, 100).Select(i => i.ToString()));
			bindable.ItemsSource = items;

			int global = bindable.TemplatedItems.GetGlobalIndexOfItem("50");
			Assert.Equal(50, global);
		}

		[Fact]
		public void GetGlobalIndexOfItemNotFound()
		{
			var items = new ObservableCollection<string>(Enumerable.Range(0, 100).Select(i => i.ToString()));
			bindable.ItemsSource = items;

			int global = bindable.TemplatedItems.GetGlobalIndexOfItem("101");
			Assert.Equal(-1, global);
		}

		[Fact]
		public void GetGlobalIndexOfItemGrouped()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			int global = bindable.TemplatedItems.GetGlobalIndexOfItem("baz");
			Assert.Equal(5, global);
		}

		[Fact]
		public void GetGlobalIndexOfItemGroupedNotFound()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			int global = bindable.TemplatedItems.GetGlobalIndexOfItem("101");
			Assert.Equal(-1, global);
		}

		[Fact]
		public void GetGlobalIndexOfGroupItemGrouped()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "baz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			int global = bindable.TemplatedItems.GetGlobalIndexOfItem(items[1], "baz");
			Assert.Equal(5, global);
		}

		[Fact]
		public void GetGlobalIndexOfGroupItemGroupedNotFound()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "foo", "faz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			int global = bindable.TemplatedItems.GetGlobalIndexOfItem(items[1], "bar");
			Assert.Equal(-1, global);
		}

		[Fact]
		public void GetGlobalIndexOfGroupItemGroupedGroupNotFound()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "foo", "faz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			int global = bindable.TemplatedItems.GetGlobalIndexOfItem(new object(), "foo");
			Assert.Equal(-1, global);
		}

		[Fact]
		public void SetupContentOnCreation()
		{
			var items = new ObservableCollection<string> {
				"Foo",
				"Bar"
			};

			bindable.ItemTemplate = new DataTemplate(typeof(TextCell))
			{
				Bindings = {
					{ TextCell.TextProperty, new Binding (".") }
				}
			};

			int count = 0;
			bindable.Hooked += obj =>
			{
				count++;
				Assert.IsType<TextCell>(obj);
				Assert.Contains(obj.BindingContext, items);
			};

			bindable.ItemsSource = items;

			bindable.TemplatedItems.GetOrCreateContent(0, items[0]);
			bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			Assert.Equal(2, count);
		}

		[Fact]
		public void UnhookGroupOnRemoval()
		{
			var inner = new ObservableCollection<string> {
				"Foo",
			};

			var items = new ObservableCollection<ObservableCollection<string>> {
				inner
			};

			bindable.IsGroupingEnabled = true;
			bindable.GroupDisplayBinding = new Binding(".[0]");

			bindable.ItemTemplate = new DataTemplate(typeof(TextCell))
			{
				Bindings = {
					{ TextCell.TextProperty, new Binding (".") }
				}
			};

			int count = 0;
			bindable.Unhooked += obj =>
			{
				if (count == 0)
				{
					Assert.IsType<TemplatedItemsList<ItemsView<BindableObject>, BindableObject>>(obj.BindingContext);
					Assert.Same(((TemplatedItemsList<ItemsView<BindableObject>, BindableObject>)obj.BindingContext).ListProxy.ProxiedEnumerable, inner);
				}
				else
				{
					Assert.Same(obj.BindingContext, inner[0]);
				}

				count++;
			};

			bindable.ItemsSource = items;

			var til = bindable.TemplatedItems.GetGroup(0);
			til.GetOrCreateContent(0, inner[0]);

			Assert.NotNull(til);
			Assert.NotNull(til.HeaderContent);
			Assert.Equal(0, count);

			items.RemoveAt(0);

			Assert.Equal(2, count);
		}

		[Fact]
		public void HookAndUnhookGroupOnReplace()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> {
					"Foo",
				}
			};

			bindable.IsGroupingEnabled = true;
			bindable.GroupDisplayBinding = new Binding(".[0]");
			bindable.ItemTemplate = new DataTemplate(typeof(TextCell))
			{
				Bindings = {
					{ TextCell.TextProperty, new Binding (".") }
				}
			};

			bindable.ItemsSource = items;

			var til = bindable.TemplatedItems.GetGroup(0);
			til.GetOrCreateContent(0, items[0][0]);

			Assert.NotNull(til);
			Assert.NotNull(til.HeaderContent);

			int hcount = 0;
			bindable.Hooked += obj =>
			{
				Assert.IsType<TemplatedItemsList<ItemsView<BindableObject>, BindableObject>>(obj.BindingContext);
				hcount++;
			};

			int ucount = 0;
			bindable.Unhooked += obj =>
			{
				Assert.True(obj.BindingContext.GetType() == typeof(TemplatedItemsList<ItemsView<BindableObject>, BindableObject>)
					|| (string)obj.BindingContext == "Foo");

				ucount++;
			};

			items[0] = new ObservableCollection<string> { "Baz" };

			Assert.Equal(1, hcount);
			Assert.Equal(2, ucount);
		}

		[Fact]
		public void UnhookContentOnRemoval()
		{
			var items = new ObservableCollection<string> {
				"Foo",
				"Bar"
			};

			bindable.ItemTemplate = new DataTemplate(typeof(TextCell))
			{
				Bindings = {
					{ TextCell.TextProperty, new Binding (".") }
				}
			};

			int count = 0;
			bindable.Unhooked += obj =>
			{
				count++;
				Assert.IsType<TextCell>(obj);
				Assert.Equal("Foo", obj.BindingContext);
			};

			bindable.ItemsSource = items;

			bindable.TemplatedItems.GetOrCreateContent(0, items[0]);
			bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			Assert.Equal(0, count);

			items.RemoveAt(0);

			Assert.Equal(1, count);
		}

		[Fact]
		public void HookAndUnhookContentOnReplace()
		{
			var items = new ObservableCollection<string> {
				"Foo",
				"Bar"
			};

			bindable.ItemTemplate = new DataTemplate(typeof(TextCell))
			{
				Bindings = {
					{ TextCell.TextProperty, new Binding (".") }
				}
			};

			bindable.ItemsSource = items;

			bindable.TemplatedItems.GetOrCreateContent(0, items[0]);
			bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			int hcount = 0;
			bindable.Hooked += obj =>
			{
				hcount++;
				Assert.IsType<TextCell>(obj);
				Assert.Equal("Baz", obj.BindingContext);
			};

			int ucount = 0;
			bindable.Unhooked += obj =>
			{
				ucount++;
				Assert.IsType<TextCell>(obj);
				Assert.Equal("Foo", obj.BindingContext);
			};

			items[0] = "Baz";

			Assert.Equal(1, hcount);
			Assert.Equal(1, ucount);
		}

		[Fact("If the cell exists and has an index, we still need to check if it's in the group asked for")]
		public void IndexOfFailsForCellInAnotherGroup()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			var group1 = bindable.TemplatedItems.GetGroup(0);
			var cell = group1[1];

			Assert.Equal(1, group1.IndexOf(cell));

			var group2 = bindable.TemplatedItems.GetGroup(1);

			Assert.Equal(-1, group2.IndexOf(cell));
		}
	}
}
