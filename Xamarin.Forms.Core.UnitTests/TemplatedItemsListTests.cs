using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
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

			public static readonly BindableProperty IsGroupingEnabledProperty =
				BindableProperty.Create<MockItemsView, bool>(lv => lv.IsGroupingEnabled, false);

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

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			bindable = new MockItemsView();

			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		MockItemsView bindable;

		[Test]
		public void ListProxyNotNullWithNullItemsSource()
		{
			Assert.IsNotNull(bindable.TemplatedItems.ListProxy);
		}

		[Test]
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

			Assert.IsTrue(raised, "CollectionChanged was not raised");
			Assert.AreEqual(NotifyCollectionChangedAction.Reset, action);
		}
		/*
		[Test]
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

		[Test]
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

			Assert.IsTrue(raised, "CollectionChanged was not raised");
			Assert.AreEqual(NotifyCollectionChangedAction.Reset, action);
		}

		[Test]
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

			Assert.IsTrue(raised, "CollectionChanged was not raised");
			Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
			Assert.AreEqual(0, args.NewStartingIndex);
			Assert.IsNotNull(args.NewItems);
			Assert.AreEqual(1, args.NewItems.Count);
			Assert.AreSame(str, ((Cell)args.NewItems[0]).BindingContext);
		}

		[Test]
		public void ListProxyUpdatesWithItemsSource()
		{
			var collection = new List<string> { "foo bar" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			Assert.IsNotNull(bindable.TemplatedItems.ListProxy);
			Assert.AreEqual(collection.Count, bindable.TemplatedItems.ListProxy.Count);
		}

		[Test]
		public void GetOrCreateContent([Values(0, 1, 2)] int index)
		{
			var collection = new List<string> { "foo", "bar", "baz" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			DataTemplate template = new DataTemplate(typeof(TextCell));
			template.SetBinding(TextCell.TextProperty, new Binding("."));
			bindable.SetValue(ItemsView<BindableObject>.ItemTemplateProperty, template);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.IsNotNull(content);

			TextCell textCell = content as TextCell;
			Assert.IsNotNull(textCell, "Content was did not match the template type, expected {0} but got {1}", typeof(TextCell), content.GetType());

			Assert.AreSame(collection[index], textCell.BindingContext);
			Assert.AreSame(collection[index], textCell.Text);

			BindableObject content2 = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.AreSame(content, content2);
		}

		[Test]
		public void GetOrCreateContentDefault()
		{
			Assert.IsNull(bindable.GetValue(ItemsView<BindableObject>.ItemTemplateProperty));

			var collection = new List<string> { "foo", "bar", "baz" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			const int index = 0;
			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.IsNotNull(content);

			TextCell textCell = content as TextCell;
			Assert.IsNotNull(textCell, "Content was did not match the template type, expected {0} but got {1}", typeof(TextCell), content.GetType());

			Assert.AreSame(collection[index], textCell.BindingContext);
			Assert.AreSame(collection[index], textCell.Text);

			BindableObject content2 = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.AreSame(content, content2);
		}

		[Test]
		public void GetOrCreateContentAfterTemplateChange()
		{
			var collection = new List<string> { "foo", "bar", "baz" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			const int index = 0;

			DataTemplate template = new DataTemplate(typeof(TextCell));
			template.SetBinding(TextCell.TextProperty, new Binding("."));
			bindable.SetValue(ItemsView<BindableObject>.ItemTemplateProperty, template);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.That(content, Is.InstanceOf<TextCell>());

			template = new DataTemplate(typeof(SwitchCell));
			template.SetBinding(SwitchCell.TextProperty, new Binding("."));

			bindable.SetValue(ItemsView<BindableObject>.ItemTemplateProperty, template);

			BindableObject content2 = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.IsNotNull(content2);
			Assert.That(content2, Is.InstanceOf<SwitchCell>());

			var switchCell = (SwitchCell)content2;
			Assert.AreSame(collection[index], switchCell.BindingContext);
		}

		[Test]
		public void GetOrCreateContentAfterItemsSourceChanged()
		{
			var collection = new List<string> { "foo", "bar", "baz" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			const int index = 0;

			DataTemplate template = new DataTemplate(typeof(TextCell));
			template.SetBinding(TextCell.TextProperty, new Binding("."));
			bindable.SetValue(ItemsView<BindableObject>.ItemTemplateProperty, template);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);
			Assert.IsNotNull(content);

			collection = new List<string> { "we", "wee", "weee" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			var content2 = (TextCell)bindable.TemplatedItems.GetOrCreateContent(index, collection[index]);

			Assert.AreNotSame(content, content2);
			Assert.AreSame(collection[index], content2.BindingContext);
			Assert.AreSame(collection[index], content2.Text);
		}

		/*[Test]
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

		[Test]
		[Description("Make sure we're not duplicate cell instances for an equal item if it's a different index")]
		public void GetOrCreateContentEqualItemDifferentItemDifferentIndex()
		{
			Assert.IsNull(bindable.GetValue(ItemsView<BindableObject>.ItemTemplateProperty));

			var collection = new List<string> { "foo", "foo" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(0, collection[0]);
			Assert.IsNotNull(content);

			TextCell textCell = content as TextCell;
			Assert.IsNotNull(textCell, "Content was did not match the template type, expected {0} but got {1}", typeof(TextCell), content.GetType());

			Assert.AreSame(collection[0], textCell.BindingContext);
			Assert.AreSame(collection[0], textCell.Text);

			BindableObject content2 = bindable.TemplatedItems.GetOrCreateContent(1, collection[1]);
			Assert.AreNotSame(content, content2);

			Assert.AreSame(collection[1], textCell.BindingContext);
			Assert.AreSame(collection[1], textCell.Text);
		}

		[Test]
		[Description("Make sure we're not duplicate cell instances for the same item if it's a different index")]
		public void GetOrCreateContentEqualItemSameItemDifferentIndex()
		{
			Assert.IsNull(bindable.GetValue(ItemsView<BindableObject>.ItemTemplateProperty));

			string item = "foo";

			var collection = new List<string> { item, item };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			BindableObject content = bindable.TemplatedItems.GetOrCreateContent(0, item);
			Assert.IsNotNull(content);

			TextCell textCell = content as TextCell;
			Assert.IsNotNull(textCell, "Content was did not match the template type, expected {0} but got {1}", typeof(TextCell), content.GetType());

			Assert.AreSame(item, textCell.BindingContext);
			Assert.AreSame(item, textCell.Text);

			BindableObject content2 = bindable.TemplatedItems.GetOrCreateContent(1, item);
			Assert.AreNotSame(content, content2);

			Assert.AreSame(item, textCell.BindingContext);
			Assert.AreSame(item, textCell.Text);
		}

		[Test]
		public void ItemsSourceInsertPreRealzied()
		{
			var collection = new ObservableCollection<string> { "foo", "bar" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			collection.Insert(1, "baz");

			Assert.That(bindable.TemplatedItems.Count, Is.EqualTo(3));
			Assert.That(bindable.TemplatedItems.GetOrCreateContent(0, collection[0]).BindingContext, Is.SameAs(collection[0]));
			Assert.That(bindable.TemplatedItems.GetOrCreateContent(1, collection[1]).BindingContext, Is.SameAs(collection[1]));
			Assert.That(bindable.TemplatedItems.GetOrCreateContent(2, collection[2]).BindingContext, Is.SameAs(collection[2]));
		}

		[Test]
		public void ItemsSourceInsertPostRealized()
		{
			var collection = new ObservableCollection<string> { "foo", "bar" };
			bindable.SetValue(ItemsView<BindableObject>.ItemsSourceProperty, collection);

			// Force the handler to realize/create the content
			bindable.TemplatedItems.GetOrCreateContent(0, collection[0]);
			bindable.TemplatedItems.GetOrCreateContent(1, collection[1]);

			collection.Insert(1, "baz");

			Assert.That(bindable.TemplatedItems.Count, Is.EqualTo(3));
			Assert.That(bindable.TemplatedItems.GetOrCreateContent(0, collection[0]).BindingContext, Is.SameAs(collection[0]));
			Assert.That(bindable.TemplatedItems.GetOrCreateContent(1, collection[1]).BindingContext, Is.SameAs(collection[1]));
			Assert.That(bindable.TemplatedItems.GetOrCreateContent(2, collection[2]).BindingContext, Is.SameAs(collection[2]));
		}

		[TestCase(0, 0, 0)]
		[TestCase(3, 1, 0)]
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
			Assert.That(index, Is.EqualTo(expectedIndex));
			Assert.That(leftOver, Is.EqualTo(expectedLeftOver));
		}

		[TestCase(1, 0, 1)]
		[TestCase(2, 0, 2)]
		[TestCase(4, 1, 1)]
		[TestCase(5, 1, 2)]
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
			Assert.That(index, Is.EqualTo(expectedIndex));
			Assert.That(leftOver, Is.EqualTo(expectedLeftOver));
		}

		[Test]
		public void GetGroupAndIndexOfItem([Values(0, 1, 2)] int group, [Values(0, 1, 2)] int index)
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
			Assert.That(location.Item1, Is.EqualTo(group), "Group index was incorrect");
			Assert.That(location.Item2, Is.EqualTo(index), "Item index was incorrect");
		}

		[Test]
		public void GetGroupAndIndexOfItemNotGrouped()
		{
			var items = Enumerable.Range(0, 10).ToList();
			bindable.ItemsSource = items;

			var location = bindable.TemplatedItems.GetGroupAndIndexOfItem(null, items[2]);

			Assert.That(location.Item1, Is.EqualTo(0));
			Assert.That(location.Item2, Is.EqualTo(2));
		}

		[Test]
		public void ItemsSourcePropertyChangedWithBindable()
		{
			bool raised = false;
			bindable.TemplatedItems.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == "ItemsSource")
					raised = true;
			};

			bindable.ItemsSource = new object[0];

			Assert.That(raised, Is.True, "INPC not raised for ItemsSource");
		}

		[Test]
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

			Assert.That(index, Is.EqualTo(1));
		}

		[Test]
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

			Assert.That(bindable.TemplatedItems.ShortNames[0], Is.EqualTo("foo"));
		}

		[Test]
		public void ItemAddedWithShortNameSetButUngrouped()
		{
			var items = new ObservableCollection<string> { "foo", "bar" };

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");

			Assert.That(() => items.Add("baz"), Throws.Nothing);
		}

		[Test]
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

			Assert.That(() => items.Add(new ObservableCollection<string>()), Throws.Nothing);
		}

		int GetIndex(BindableObject item)
		{
			return TemplatedItemsList<ItemsView<BindableObject>, BindableObject>.GetIndex(item);
		}

		[Test]
		public void GetIndex()
		{
			var items = new List<string> { "foo", "bar", "baz" };

			bindable.ItemsSource = items;

			BindableObject item = bindable.TemplatedItems.GetOrCreateContent(1, items[1]);
			int index = GetIndex(item);
			Assert.That(index, Is.EqualTo(1));
		}

		[Test]
		public void GetIndexAfterInsert()
		{
			var items = new ObservableCollection<string> { "foo", "bar", "baz" };

			bindable.ItemsSource = items;

			BindableObject originalItem = bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			items.Insert(1, "fad");

			BindableObject item = bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			int index = GetIndex(item);
			Assert.That(index, Is.EqualTo(1));

			index = GetIndex(originalItem);
			Assert.That(index, Is.EqualTo(2));
		}

		[Test]
		public void GetIndexAfterMove()
		{
			var items = new ObservableCollection<string> { "foo", "bar", "baz" };

			bindable.ItemsSource = items;

			BindableObject item0 = bindable.TemplatedItems.GetOrCreateContent(0, items[0]);
			BindableObject item1 = bindable.TemplatedItems.GetOrCreateContent(1, items[1]);
			BindableObject item2 = bindable.TemplatedItems.GetOrCreateContent(2, items[2]);

			items.Move(0, 2); // foo, bar, baz becomes bar (1), baz (2), foo (0)

			Assert.That(GetIndex(item0), Is.EqualTo(2));
			Assert.That(GetIndex(item1), Is.EqualTo(0));
			Assert.That(GetIndex(item2), Is.EqualTo(1));
		}

		[Test]
		public void GetIndexAfterRemove()
		{
			var items = new ObservableCollection<string> { "foo", "bar", "baz" };

			bindable.ItemsSource = items;

			BindableObject item1 = bindable.TemplatedItems.GetOrCreateContent(1, items[1]);
			BindableObject item2 = bindable.TemplatedItems.GetOrCreateContent(2, items[2]);

			items.RemoveAt(0);

			Assert.That(GetIndex(item1), Is.EqualTo(0));
			Assert.That(GetIndex(item2), Is.EqualTo(1));
		}

		[Test]
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

			Assert.That(group, Is.SameAs(til));
		}

		[Test]
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
			Assert.That(index, Is.EqualTo(1));
		}

		[Test]
		public void GetGroupAndIndexOfItem()
		{
			var items = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> { "foo", "faz" },
				new ObservableCollection<string> { "bar", "baz" }
			};

			bindable.ItemsSource = items;
			bindable.GroupShortNameBinding = new Binding(".");
			bindable.IsGroupingEnabled = true;

			var result = bindable.TemplatedItems.GetGroupAndIndexOfItem(items[1], items[1][1]);

			Assert.That(result.Item1, Is.EqualTo(1));
			Assert.That(result.Item2, Is.EqualTo(1));
		}

		[Test]
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

			Assert.That(result.Item1, Is.EqualTo(1));
			Assert.That(result.Item2, Is.EqualTo(1));
		}

		[Test]
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

			Assert.That(result.Item1, Is.EqualTo(-1));
			Assert.That(result.Item2, Is.EqualTo(-1));
		}

		[Test]
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

			Assert.That(result.Item1, Is.EqualTo(1));
			Assert.That(result.Item2, Is.EqualTo(-1));
		}

		[Test]
		[Description("Issue #2464: ANE thrown when moving items in a ListView")]
		public void MovingPastRealizedWindowAndBackDoesntThrow()
		{
			var items = new ObservableCollection<string>(Enumerable.Range(0, 100).Select(i => i.ToString()));

			bindable.ItemsSource = items;

			bindable.TemplatedItems.GetOrCreateContent(0, items[0]);
			bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			items.Move(0, 3);
			Assert.That(() => items.Move(3, 2), Throws.Nothing);

			Assert.That(GetIndex(bindable.TemplatedItems[0]), Is.EqualTo(0));
			Assert.That(GetIndex(bindable.TemplatedItems[1]), Is.EqualTo(1));
			Assert.That(GetIndex(bindable.TemplatedItems[2]), Is.EqualTo(2));
			Assert.That(GetIndex(bindable.TemplatedItems[3]), Is.EqualTo(3));
		}

		[Test]
		public void GetGlobalIndexOfItem()
		{
			var items = new ObservableCollection<string>(Enumerable.Range(0, 100).Select(i => i.ToString()));
			bindable.ItemsSource = items;

			int global = bindable.TemplatedItems.GetGlobalIndexOfItem("50");
			Assert.That(global, Is.EqualTo(50));
		}

		[Test]
		public void GetGlobalIndexOfItemNotFound()
		{
			var items = new ObservableCollection<string>(Enumerable.Range(0, 100).Select(i => i.ToString()));
			bindable.ItemsSource = items;

			int global = bindable.TemplatedItems.GetGlobalIndexOfItem("101");
			Assert.That(global, Is.EqualTo(-1));
		}

		[Test]
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
			Assert.That(global, Is.EqualTo(5));
		}

		[Test]
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
			Assert.That(global, Is.EqualTo(-1));
		}

		[Test]
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
			Assert.That(global, Is.EqualTo(5));
		}

		[Test]
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
			Assert.That(global, Is.EqualTo(-1));
		}

		[Test]
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
			Assert.That(global, Is.EqualTo(-1));
		}

		[Test]
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
				Assert.That(obj, Is.InstanceOf(typeof(TextCell)));
				Assert.That(items, Contains.Item(obj.BindingContext));
			};

			bindable.ItemsSource = items;

			bindable.TemplatedItems.GetOrCreateContent(0, items[0]);
			bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			Assert.That(count, Is.EqualTo(2));
		}

		[Test]
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
					Assert.That(obj.BindingContext, Is.InstanceOf(typeof(TemplatedItemsList<ItemsView<BindableObject>, BindableObject>)));
					Assert.That(((TemplatedItemsList<ItemsView<BindableObject>, BindableObject>)obj.BindingContext).ListProxy.ProxiedEnumerable, Is.SameAs(inner));
				}
				else
				{
					Assert.That(obj.BindingContext, Is.SameAs(inner[0]));
				}

				count++;
			};

			bindable.ItemsSource = items;

			var til = bindable.TemplatedItems.GetGroup(0);
			til.GetOrCreateContent(0, inner[0]);

			Assume.That(til, Is.Not.Null);
			Assume.That(til.HeaderContent, Is.Not.Null);
			Assume.That(count, Is.EqualTo(0));

			items.RemoveAt(0);

			Assert.That(count, Is.EqualTo(2));
		}

		[Test]
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

			Assume.That(til, Is.Not.Null);
			Assume.That(til.HeaderContent, Is.Not.Null);

			int hcount = 0;
			bindable.Hooked += obj =>
			{
				Assert.That(obj.BindingContext, Is.InstanceOf(typeof(TemplatedItemsList<ItemsView<BindableObject>, BindableObject>)));
				hcount++;
			};

			int ucount = 0;
			bindable.Unhooked += obj =>
			{
				Assert.That(obj.BindingContext,
					Is.InstanceOf(typeof(TemplatedItemsList<ItemsView<BindableObject>, BindableObject>))
						.Or.EqualTo("Foo"));

				ucount++;
			};

			items[0] = new ObservableCollection<string> { "Baz" };

			Assert.That(hcount, Is.EqualTo(1));
			Assert.That(ucount, Is.EqualTo(2));
		}

		[Test]
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
				Assert.That(obj, Is.InstanceOf(typeof(TextCell)));
				Assert.That(obj.BindingContext, Is.EqualTo("Foo"));
			};

			bindable.ItemsSource = items;

			bindable.TemplatedItems.GetOrCreateContent(0, items[0]);
			bindable.TemplatedItems.GetOrCreateContent(1, items[1]);

			Assume.That(count, Is.EqualTo(0));

			items.RemoveAt(0);

			Assert.That(count, Is.EqualTo(1));
		}

		[Test]
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
				Assert.That(obj, Is.InstanceOf(typeof(TextCell)));
				Assert.That(obj.BindingContext, Is.EqualTo("Baz"));
			};

			int ucount = 0;
			bindable.Unhooked += obj =>
			{
				ucount++;
				Assert.That(obj, Is.InstanceOf(typeof(TextCell)));
				Assert.That(obj.BindingContext, Is.EqualTo("Foo"));
			};

			items[0] = "Baz";

			Assert.That(hcount, Is.EqualTo(1));
			Assert.That(ucount, Is.EqualTo(1));
		}

		[Test(Description = "If the cell exists and has an index, we still need to check if it's in the group asked for")]
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

			Assume.That(group1.IndexOf(cell), Is.EqualTo(1));

			var group2 = bindable.TemplatedItems.GetGroup(1);

			Assert.That(group2.IndexOf(cell), Is.EqualTo(-1));
		}
	}
}