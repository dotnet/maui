using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ListViewTests : BaseTestFixture
	{
		MockDeviceInfo mockDeviceInfo;

		public ListViewTests()
		{

			DeviceDisplay.SetCurrent(new MockDeviceDisplay());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		[Fact]
		public void TestConstructor()
		{
			var listView = new ListView();

			Assert.Null(listView.ItemsSource);
			Assert.Null(listView.ItemTemplate);
			Assert.Equal(LayoutOptions.FillAndExpand, listView.HorizontalOptions);
			Assert.Equal(LayoutOptions.FillAndExpand, listView.VerticalOptions);
		}

		internal class ListItem
		{
			public string Name { get; set; }
			public string Description { get; set; }

			public override string ToString() => Name ?? base.ToString();
		}

		static ListItem[] CreateListItemCollection()
		{
			return new[]
			{
				new ListItem { Name = "Foo", Description = "Bar" },
				new ListItem { Name = "Baz", Description = "Raz" }
			};
		}

		[Fact]
		public void TestTemplating()
		{
			var cellTemplate = new DataTemplate(typeof(TextCell));
			cellTemplate.SetBinding(TextCell.TextProperty, new Binding("Name"));
			cellTemplate.SetBinding(TextCell.DetailProperty, new Binding("Description"));

			var listView = new ListView
			{
				ItemsSource = CreateListItemCollection(),
				ItemTemplate = cellTemplate
			};

			var cell = (Cell)listView.ItemTemplate.CreateContent();

			var textCell = (TextCell)cell;
			cell.BindingContext = listView.ItemsSource.OfType<ListItem>().First();

			Assert.Equal("Foo", textCell.Text);
			Assert.Equal("Bar", textCell.Detail);
		}

		[Fact]
		public void TemplateNullObject()
		{
			var listView = new ListView
			{
				ItemsSource = new object[] {
					null
				}
			};

			Cell cell = listView.TemplatedItems[0];

			Assert.NotNull(cell);
			Assert.IsType<TextCell>(cell);
			Assert.Null(((TextCell)cell).Text);
		}

		[Fact]
		public void ItemTemplateIsNullObjectExecutesToString()
		{
			var listView = new ListView
			{
				ItemsSource = CreateListItemCollection()
			};

			Assert.Equal(2, listView.TemplatedItems.Count);

			Cell cell = listView.TemplatedItems[0];
			Assert.NotNull(cell);
			Assert.IsType<TextCell>(cell);
			Assert.Equal("Foo", ((TextCell)cell).Text);

			cell = listView.TemplatedItems[1];
			Assert.NotNull(cell);
			Assert.IsType<TextCell>(cell);
			Assert.Equal("Baz", ((TextCell)cell).Text);
		}

		[Fact("Setting BindingContext should trickle down to Header and Footer.")]
		public void SettingBindingContextPassesToHeaderAndFooter()
		{
			var bc = new object();
			var header = new BoxView();
			var footer = new BoxView();
			var listView = new ListView
			{
				Header = header,
				Footer = footer,
				BindingContext = bc,
			};

			Assert.Same(header.BindingContext, bc);
			Assert.Same(footer.BindingContext, bc);
		}

		[Fact("Setting Header and Footer should pass BindingContext.")]
		public void SettingHeaderFooterPassesBindingContext()
		{
			var bc = new object();
			var listView = new ListView
			{
				BindingContext = bc,
			};

			var header = new BoxView();
			var footer = new BoxView();
			listView.Footer = footer;
			listView.Header = header;

			Assert.Same(header.BindingContext, bc);
			Assert.Same(footer.BindingContext, bc);
		}

		[Fact("Setting GroupDisplayBinding or GroupHeaderTemplate when the other is set should set the other one to null.")]
		public void SettingGroupHeaderTemplateSetsDisplayBindingToNull()
		{
			var listView = new ListView
			{
				GroupDisplayBinding = new Binding("Path")
			};

			listView.GroupHeaderTemplate = new DataTemplate(typeof(TextCell));

			Assert.Null(listView.GroupDisplayBinding);
		}

		[Fact("Setting GroupDisplayBinding or GroupHeaderTemplate when the other is set should set the other one to null.")]
		public void SettingGroupDisplayBindingSetsHeaderTemplateToNull()
		{
			var listView = new ListView
			{
				GroupHeaderTemplate = new DataTemplate(typeof(TextCell))
			};

			listView.GroupDisplayBinding = new Binding("Path");

			Assert.Null(listView.GroupHeaderTemplate);
		}

		[Fact("You should be able to set ItemsSource without having set the other properties first without issue")]
		public void SettingItemsSourceWithoutBindingsOrItemsSource()
		{
			var listView = new ListView
			{
				IsGroupingEnabled = true
			};

			listView.ItemsSource = new[] { new[] { new object() } };
		}

		[Fact]
		public void DefaultGroupHeaderTemplates()
		{
			var items = new[] { new[] { new object() } };

			var listView = new ListView
			{
				IsGroupingEnabled = true,
				ItemsSource = items
			};

			var til = (TemplatedItemsList<ItemsView<Cell>, Cell>)((IList)listView.TemplatedItems)[0];
			Cell cell = til.HeaderContent;

			Assert.NotNull(cell);
			Assert.IsType<TextCell>(cell);
			Assert.Equal(((TextCell)cell).Text, items[0].ToString());
		}


		[Fact]
		public void SettingSelectItemManuallyBetweenTwoTapsFiresPropertyChanged()
		{
			var listView = new ListView
			{
				ItemsSource = new[] {
					"item1",
					"item2",
					"item3"
				}
			};

			listView.NotifyRowTapped(0);
			listView.SelectedItem = null;

			bool raised = false;
			listView.PropertyChanged += (sender, arg) =>
			{
				if (arg.PropertyName == ListView.SelectedItemProperty.PropertyName)
					raised = true;
			};

			listView.NotifyRowTapped(1);
			Assert.True(raised);
		}

		[Fact("Tapping a different item (row) that is equal to the current item selection should still raise ItemSelected")]
		public void NotifyRowTappedDifferentIndex()
		{
			string item = "item";

			var listView = new ListView
			{
				ItemsSource = new[] {
					item,
					item
				}
			};

			listView.NotifyRowTapped(0);

			bool raised = false;
			listView.ItemSelected += (sender, arg) => raised = true;

			listView.NotifyRowTapped(1);
			Assert.True(raised);
		}

		[Fact]
		public void DoesNotCrashWhenAddingToSource()
		{
			var items = new ObservableCollection<string> {
				"Foo",
				"Bar",
				"Baz"
			};

			var listView = new ListView
			{
				ItemsSource = items,
				ItemTemplate = new DataTemplate(typeof(TextCell))
			};

			items.Add("Blah");
		}

		[Fact]
		public void DoesNotThrowWhenMovingInSource()
		{
			var items = new ObservableCollection<string> {
				"Foo",
				"Bar",
				"Baz"
			};

			var listView = new ListView
			{
				ItemsSource = items,
				ItemTemplate = new DataTemplate(typeof(TextCell))
			};

			items.Move(0, 1);
		}

		[Fact("A cell being tapped from the UI should raise both tapped events, but not change ItemSelected")]
		public void NotifyTappedSameItem()
		{
			int cellTapped = 0;
			int itemTapped = 0;
			int itemSelected = 0;

			var listView = new ListView
			{
				ItemsSource = new[] { "item" },
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new TextCell();
					cell.Tapped += (s, e) =>
					{
						cellTapped++;
					};
					return cell;
				})
			};

			listView.ItemTapped += (sender, arg) => itemTapped++;
			listView.ItemSelected += (sender, arg) => itemSelected++;

			listView.NotifyRowTapped(0);

			Assert.Equal(1, cellTapped);
			Assert.Equal(1, itemTapped);
			Assert.Equal(1, itemSelected);

			listView.NotifyRowTapped(0);

			Assert.Equal(2, cellTapped);
			Assert.Equal(2, itemTapped);
			Assert.Equal(1, itemSelected);
		}

		[Fact]
		public void ScrollTo()
		{
			var listView = new ListView
			{
				IsPlatformEnabled = true,
			};

			object item = new object();

			bool requested = false;
			listView.ScrollToRequested += (sender, args) =>
			{
				requested = true;

				Assert.Same(args.Item, item);
				Assert.Null(args.Group);
				Assert.Equal(ScrollToPosition.Center, args.Position);
				Assert.True(args.ShouldAnimate);
			};

			listView.ScrollTo(item, ScrollToPosition.Center, animated: true);
			Assert.True(requested);
		}

		[Fact]
		public void ScrollToDelayed()
		{
			var listView = new ListView();

			object item = new object();

			bool requested = false;
			listView.ScrollToRequested += (sender, args) =>
			{
				requested = true;

				Assert.Same(args.Item, item);
				Assert.Null(args.Group);
				Assert.Equal(ScrollToPosition.Center, args.Position);
				Assert.True(args.ShouldAnimate);
			};

			listView.ScrollTo(item, ScrollToPosition.Center, animated: true);
			Assert.False(requested);

			listView.IsPlatformEnabled = true;

			Assert.True(requested);
		}

		[Fact]
		public void ScrollToGroup()
		{
			// Fake a renderer so we pass along messages right away
			var listView = new ListView
			{
				IsPlatformEnabled = true,
				IsGroupingEnabled = true
			};

			object item = new object();
			object group = new object();

			bool requested = false;
			listView.ScrollToRequested += (sender, args) =>
			{
				requested = true;

				Assert.Same(args.Item, item);
				Assert.Same(args.Group, group);
				Assert.Equal(ScrollToPosition.Center, args.Position);
				Assert.True(args.ShouldAnimate);
			};

			listView.ScrollTo(item, group, ScrollToPosition.Center, animated: true);
			Assert.True(requested);
		}

		[Fact]
		public void ScrollToInvalid()
		{
			var listView = new ListView
			{
				IsPlatformEnabled = true,
			};

			Assert.Throws<ArgumentException>(() => listView.ScrollTo(new object(), (ScrollToPosition)500, true));
			Assert.Throws<InvalidOperationException>(() => listView.ScrollTo(new object(), new object(), ScrollToPosition.Start, true));

			listView.IsGroupingEnabled = true;
			Assert.Throws<ArgumentException>(() => listView.ScrollTo(new object(), new object(), (ScrollToPosition)500, true));
		}

		[Fact]
		public void GetSizeRequest()
		{
			var listView = new ListView
			{
				IsPlatformEnabled = true,
				HasUnevenRows = false,
				RowHeight = 50,
				ItemsSource = Enumerable.Range(0, 20).ToList()
			};


			var sizeRequest = listView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
			Assert.Equal(40, sizeRequest.Minimum.Width);
			Assert.Equal(40, sizeRequest.Minimum.Height);
			Assert.Equal(50, sizeRequest.Request.Width);
			Assert.Equal(50 * 20, sizeRequest.Request.Height);
		}

		[Fact]
		public void GetSizeRequestUneven()
		{
			var listView = new ListView
			{
				IsPlatformEnabled = true,
				HasUnevenRows = true,
				RowHeight = 50,
				ItemsSource = Enumerable.Range(0, 20).ToList()
			};


			var sizeRequest = listView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
			Assert.Equal(40, sizeRequest.Minimum.Width);
			Assert.Equal(40, sizeRequest.Minimum.Height);
			Assert.Equal(50, sizeRequest.Request.Width);
			Assert.Equal(100, sizeRequest.Request.Height);
		}

		public class ListItemValue : IComparable<ListItemValue>
		{
			public string Name { get; private set; }

			public ListItemValue(string name)
			{
				Name = name;
			}

			int IComparable<ListItemValue>.CompareTo(ListItemValue value)
			{
				return Name.CompareTo(value.Name);
			}

			public string Label
			{
				get { return Name[0].ToString(); }
			}
		}

		public class ListItemCollection : ObservableCollection<ListItemValue>
		{
			public string Title { get; private set; }

			public ListItemCollection(string title)
			{
				Title = title;
			}

			public static List<ListItemValue> GetSortedData()
			{
				var items = ListItems;
				items.Sort();
				return items;
			}

			// Data used to populate our list.
			static readonly List<ListItemValue> ListItems = new List<ListItemValue>() {
				new ListItemValue ("Babbage"),
				new ListItemValue ("Boole"),
				new ListItemValue ("Berners-Lee"),
				new ListItemValue ("Atanasoff"),
				new ListItemValue ("Allen"),
				new ListItemValue ("Cormack"),
				new ListItemValue ("Cray"),
				new ListItemValue ("Dijkstra"),
				new ListItemValue ("Dix"),
				new ListItemValue ("Dewey"),
				new ListItemValue ("Erdos"),
			};
		}

		public class TestCell : TextCell
		{
			public static int NumberOfCells = 0;

			public TestCell()
			{
				Interlocked.Increment(ref NumberOfCells);
			}

			~TestCell()
			{
				Interlocked.Decrement(ref NumberOfCells);
			}
		}

		ObservableCollection<ListItemCollection> SetupList()
		{
			var allListItemGroups = new ObservableCollection<ListItemCollection>();

			foreach (var item in ListItemCollection.GetSortedData())
			{
				// Attempt to find any existing groups where theg group title matches the first char of our ListItem's name.
				var listItemGroup = allListItemGroups.FirstOrDefault(g => g.Title == item.Label);

				// If the list group does not exist, we create it.
				if (listItemGroup == null)
				{
					listItemGroup = new ListItemCollection(item.Label) { item };
					allListItemGroups.Add(listItemGroup);
				}
				else
				{
					// If the group does exist, we simply add the demo to the existing group.
					listItemGroup.Add(item);
				}
			}
			return allListItemGroups;
		}

		[Fact]
		public void UncollectableHeaderReferences()
		{
			var list = new ListView
			{
				IsPlatformEnabled = true,
				ItemTemplate = new DataTemplate(typeof(TextCell))
				{
					Bindings = {
						{TextCell.TextProperty, new Binding ("Name")}
					}
				},
				GroupHeaderTemplate = new DataTemplate(typeof(TestCell))
				{
					Bindings = {
						{TextCell.TextProperty, new Binding ("Title")}
					}
				},
				IsGroupingEnabled = true,
				ItemsSource = SetupList(),
			};

			Assert.Equal(5, TestCell.NumberOfCells);

			var newList1 = SetupList();
			var newList2 = SetupList();

			for (var i = 0; i < 400; i++)
			{
				list.ItemsSource = i % 2 > 0 ? newList1 : newList2;

				// grab a header just so we can be sure its realized
				var header = list.TemplatedItems.GetGroup(0).HeaderContent;
			}

			GC.Collect();
			GC.WaitForPendingFinalizers();

			// use less or equal because mono will keep the last header var alive no matter what
			Assert.True(TestCell.NumberOfCells <= 6, $"{TestCell.NumberOfCells} <= 6");

			var keepAlive = list.ToString();
		}

		[Fact]
		public void CollectionChangedMultipleFires()
		{
			var source = new ObservableCollection<string> {
				"Foo",
				"Bar"
			};

			var list = new ListView
			{
				IsPlatformEnabled = true,
				ItemsSource = source,
				ItemTemplate = new DataTemplate(typeof(TextCell))
			};

			int fireCount = 0;
			list.TemplatedItems.CollectionChanged += (sender, args) =>
			{
				fireCount++;
			};

			source.Add("Baz");

			Assert.Equal(1, fireCount);
		}

		[Fact]
		public void GroupedCollectionChangedMultipleFires()
		{
			var source = new ObservableCollection<ObservableCollection<string>> {
				new ObservableCollection<string> {"Foo"},
				new ObservableCollection<string> {"Bar"}
			};

			var list = new ListView
			{
				IsPlatformEnabled = true,
				IsGroupingEnabled = true,
				ItemsSource = source,
				ItemTemplate = new DataTemplate(typeof(TextCell))
				{
					Bindings = {
						{TextCell.TextProperty, new Binding (".") }
					}
				}
			};

			int fireCount = 0;
			list.TemplatedItems.GroupedCollectionChanged += (sender, args) =>
			{
				fireCount++;
			};

			source[0].Add("Baz");

			Assert.Equal(1, fireCount);
		}

		[Fact]
		public void HeaderAsView()
		{
			var label = new Label { Text = "header" };
			var lv = new ListView
			{
				Header = label
			};

			IListViewController controller = lv;
			Assert.Same(controller.HeaderElement, label);
		}

		[Fact]
		public void HeaderTemplated()
		{
			var lv = new ListView
			{
				Header = "header",
				HeaderTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding (".") }
					}
				}
			};

			IListViewController controller = lv;
			Assert.NotNull(controller.HeaderElement);
			Assert.IsType<Label>(controller.HeaderElement);
			Assert.Equal(((Label)controller.HeaderElement).Text, lv.Header);
		}

		[Fact]
		public void HeaderObjectTemplatedChanged()
		{
			var lv = new ListView
			{
				Header = "header",
				HeaderTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding (".") }
					}
				}
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changed = true;
			};

			lv.Header = "newheader";

			Assert.False(changing);
			Assert.False(changed);

			IListViewController controller = lv;
			Assert.NotNull(controller.HeaderElement);
			Assert.IsType<Label>(controller.HeaderElement);
			Assert.Equal(((Label)controller.HeaderElement).Text, lv.Header);
		}

		[Fact]
		public void HeaderViewChanged()
		{
			var lv = new ListView
			{
				Header = new Label { Text = "header" }
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changed = true;
			};

			Label label = new Label { Text = "header" };
			lv.Header = label;

			Assert.True(changing);
			Assert.True(changed);

			IListViewController controller = lv;
			Assert.Same(controller.HeaderElement, label);
		}


		[Fact]
		public void HeaderTemplateChanged()
		{
			var lv = new ListView
			{
				Header = "header",
				HeaderTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding (".") }
					}
				}
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changed = true;
			};

			lv.HeaderTemplate = new DataTemplate(typeof(Entry))
			{
				Bindings = {
					{ Entry.TextProperty, new Binding (".") }
				}
			};

			Assert.True(changing);
			Assert.True(changed);

			IListViewController controller = lv;
			Assert.NotNull(controller.HeaderElement);
			Assert.IsType<Entry>(controller.HeaderElement);
			Assert.Equal(((Entry)controller.HeaderElement).Text, lv.Header);
		}

		[Fact]
		public void HeaderTemplateChangedNoObject()
		{
			var lv = new ListView
			{
				HeaderTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding (".") }
					}
				}
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changed = true;
			};

			lv.HeaderTemplate = new DataTemplate(typeof(Entry))
			{
				Bindings = {
					{ Entry.TextProperty, new Binding (".") }
				}
			};

			Assert.False(changing);
			Assert.False(changed);

			IListViewController controller = lv;
			Assert.Null(controller.HeaderElement);
		}

		[Fact]
		public void HeaderNoTemplate()
		{
			var lv = new ListView
			{
				Header = "foo"
			};

			IListViewController controller = lv;
			Assert.NotNull(controller.HeaderElement);
			Assert.IsType<Label>(controller.HeaderElement);
			Assert.Equal(((Label)controller.HeaderElement).Text, lv.Header);
		}

		[Fact]
		public void HeaderChangedNoTemplate()
		{
			var lv = new ListView
			{
				Header = "foo"
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changed = true;
			};

			lv.Header = "bar";

			Assert.True(changing);
			Assert.True(changed);

			IListViewController controller = lv;
			Assert.NotNull(controller.HeaderElement);
			Assert.IsType<Label>(controller.HeaderElement);
			Assert.Equal(((Label)controller.HeaderElement).Text, lv.Header);
		}

		[Fact]
		public void HeaderViewButTemplated()
		{
			var lv = new ListView
			{
				Header = new Entry { Text = "foo" },
				HeaderTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding ("Text") }
					}
				}
			};

			IListViewController controller = lv;
			Assert.NotNull(controller.HeaderElement);
			Assert.IsType<Label>(controller.HeaderElement);
			Assert.Equal(((Label)controller.HeaderElement).Text, ((Entry)lv.Header).Text);
		}

		[Fact]
		public void HeaderTemplatedChangedToView()
		{
			var lv = new ListView
			{
				Header = new Entry { Text = "foo" },
				HeaderTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding ("Text") }
					}
				}
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changed = true;
			};

			lv.HeaderTemplate = null;

			Assert.True(changing);
			Assert.True(changed);

			IListViewController controller = lv;
			Assert.NotNull(controller.HeaderElement);
			Assert.IsType<Entry>(controller.HeaderElement);
			Assert.Equal(((Entry)controller.HeaderElement).Text, ((Entry)lv.Header).Text);
		}

		[Fact]
		public void HeaderTemplatedSetToNull()
		{
			var lv = new ListView
			{
				Header = "header",
				HeaderTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding (".") }
					}
				}
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HeaderElement")
					changed = true;
			};

			lv.Header = null;

			Assert.True(changing);
			Assert.True(changed);

			IListViewController controller = lv;
			Assert.Null(controller.HeaderElement);
		}

		[Fact]
		public void FooterAsView()
		{
			var label = new Label { Text = "footer" };
			var lv = new ListView
			{
				Footer = label
			};

			IListViewController controller = lv;
			Assert.Same(controller.FooterElement, label);
		}

		[Fact]
		public void FooterTemplated()
		{
			var lv = new ListView
			{
				Footer = "footer",
				FooterTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding (".") }
					}
				}
			};

			IListViewController controller = lv;
			Assert.NotNull(controller.FooterElement);
			Assert.IsType<Label>(controller.FooterElement);
			Assert.Equal(((Label)controller.FooterElement).Text, lv.Footer);
		}

		[Fact]
		public void FooterObjectTemplatedChanged()
		{
			var lv = new ListView
			{
				Footer = "footer",
				FooterTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding (".") }
					}
				}
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changed = true;
			};

			lv.Footer = "newfooter";

			Assert.False(changing);
			Assert.False(changed);

			IListViewController controller = lv;
			Assert.NotNull(controller.FooterElement);
			Assert.IsType<Label>(controller.FooterElement);
			Assert.Equal(((Label)controller.FooterElement).Text, lv.Footer);
		}

		[Fact]
		public void FooterViewChanged()
		{
			var lv = new ListView
			{
				Footer = new Label { Text = "footer" }
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changed = true;
			};

			Label label = new Label { Text = "footer" };
			lv.Footer = label;

			Assert.True(changing);
			Assert.True(changed);

			IListViewController controller = lv;
			Assert.Same(controller.FooterElement, label);
		}


		[Fact]
		public void FooterTemplateChanged()
		{
			var lv = new ListView
			{
				Footer = "footer",
				FooterTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding (".") }
					}
				}
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changed = true;
			};

			lv.FooterTemplate = new DataTemplate(typeof(Entry))
			{
				Bindings = {
					{ Entry.TextProperty, new Binding (".") }
				}
			};

			Assert.True(changing);
			Assert.True(changed);

			IListViewController controller = lv;
			Assert.NotNull(controller.FooterElement);
			Assert.IsType<Entry>(controller.FooterElement);
			Assert.Equal(((Entry)controller.FooterElement).Text, lv.Footer);
		}

		[Fact]
		public void FooterTemplateChangedNoObject()
		{
			var lv = new ListView
			{
				FooterTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding (".") }
					}
				}
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changed = true;
			};

			lv.FooterTemplate = new DataTemplate(typeof(Entry))
			{
				Bindings = {
					{ Entry.TextProperty, new Binding (".") }
				}
			};

			Assert.False(changing);
			Assert.False(changed);

			IListViewController controller = lv;
			Assert.Null(controller.FooterElement);
		}

		[Fact]
		public void FooterNoTemplate()
		{
			var lv = new ListView
			{
				Footer = "foo"
			};

			IListViewController controller = lv;
			Assert.NotNull(controller.FooterElement);
			Assert.IsType<Label>(controller.FooterElement);
			Assert.Equal(((Label)controller.FooterElement).Text, lv.Footer);
		}

		[Fact]
		public void FooterChangedNoTemplate()
		{
			var lv = new ListView
			{
				Footer = "foo"
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changed = true;
			};

			lv.Footer = "bar";

			Assert.True(changing);
			Assert.True(changed);

			IListViewController controller = lv;
			Assert.NotNull(controller.FooterElement);
			Assert.IsType<Label>(controller.FooterElement);
			Assert.Equal(((Label)controller.FooterElement).Text, lv.Footer);
		}

		[Fact]
		public void FooterViewButTemplated()
		{
			var lv = new ListView
			{
				Footer = new Entry { Text = "foo" },
				FooterTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding ("Text") }
					}
				}
			};

			IListViewController controller = lv;
			Assert.NotNull(controller.FooterElement);
			Assert.IsType<Label>(controller.FooterElement);
			Assert.Equal(((Label)controller.FooterElement).Text, ((Entry)lv.Footer).Text);
		}

		[Fact]
		public void FooterTemplatedChangedToView()
		{
			var lv = new ListView
			{
				Footer = new Entry { Text = "foo" },
				FooterTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding ("Text") }
					}
				}
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changed = true;
			};

			lv.FooterTemplate = null;

			Assert.True(changing);
			Assert.True(changed);

			IListViewController controller = lv;
			Assert.NotNull(controller.FooterElement);
			Assert.IsType<Entry>(controller.FooterElement);
			Assert.Equal(((Entry)controller.FooterElement).Text, ((Entry)lv.Footer).Text);
		}

		[Fact]
		public void FooterTemplatedSetToNull()
		{
			var lv = new ListView
			{
				Footer = "footer",
				FooterTemplate = new DataTemplate(typeof(Label))
				{
					Bindings = {
						{ Label.TextProperty, new Binding (".") }
					}
				}
			};

			bool changed = false, changing = false;
			lv.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changing = true;
			};
			lv.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "FooterElement")
					changed = true;
			};

			lv.Footer = null;

			Assert.True(changing);
			Assert.True(changed);

			IListViewController controller = lv;
			Assert.Null(controller.FooterElement);
		}

		[Fact]
		public void BeginRefresh()
		{
			var lv = new ListView();

			bool refreshing = false;
			lv.Refreshing += (sender, args) =>
			{
				refreshing = true;
			};

			lv.BeginRefresh();

			Assert.True(refreshing);
			Assert.True(lv.IsRefreshing);
		}

		[Fact]
		public void SendRefreshing()
		{
			var lv = new ListView();

			bool refreshing = false;
			lv.Refreshing += (sender, args) =>
			{
				refreshing = true;
			};

			IListViewController controller = lv;
			controller.SendRefreshing();

			Assert.True(refreshing);
			Assert.True(lv.IsRefreshing);
		}

		[Fact]
		public void RefreshCommand()
		{
			var lv = new ListView();

			bool commandExecuted = false;

			Command refresh = new Command(() => commandExecuted = true);

			lv.RefreshCommand = refresh;

			IListViewController controller = lv;
			controller.SendRefreshing();

			Assert.True(commandExecuted);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void RefreshCommandCanExecute(bool initial)
		{
			var lv = new ListView { IsPullToRefreshEnabled = initial };

			bool commandExecuted = false;

			Command refresh = new Command(() => commandExecuted = true,
				() => !initial);

			lv.RefreshCommand = refresh;

			Assert.Equal((lv as IListViewController).RefreshAllowed, !initial);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void RefreshCommandCanExecuteChanges(bool initial)
		{
			var lv = new ListView { IsPullToRefreshEnabled = initial };

			bool commandExecuted = false;

			Command refresh = new Command(() => commandExecuted = true,
				() => initial);

			lv.RefreshCommand = refresh;

			Assert.Equal((lv as IListViewController).RefreshAllowed, initial);

			initial = !initial;
			refresh.ChangeCanExecute();

			Assert.Equal((lv as IListViewController).RefreshAllowed, initial);
		}

		[Fact]
		public void BeginRefreshDoesNothingWhenCannotExecute()
		{
			var lv = new ListView();

			bool commandExecuted = false, eventFired = false;

			lv.Refreshing += (sender, args) => eventFired = true;

			Command refresh = new Command(() => commandExecuted = true,
				() => false);

			lv.RefreshCommand = refresh;
			lv.BeginRefresh();

			Assert.False(lv.IsRefreshing);
			Assert.False(eventFired);
			Assert.False(commandExecuted);
		}

		[Fact]
		public void SendRefreshingDoesNothingWhenCannotExecute()
		{
			var lv = new ListView();

			bool commandExecuted = false, eventFired = false;

			lv.Refreshing += (sender, args) => eventFired = true;

			Command refresh = new Command(() => commandExecuted = true,
				() => false);

			lv.RefreshCommand = refresh;

			((IListViewController)lv).SendRefreshing();

			Assert.False(lv.IsRefreshing);
			Assert.False(eventFired);
			Assert.False(commandExecuted);
		}

		[Fact]
		public void SettingIsRefreshingDoesntFireEvent()
		{
			var lv = new ListView();

			bool refreshing = false;
			lv.Refreshing += (sender, args) =>
			{
				refreshing = true;
			};

			lv.IsRefreshing = true;

			Assert.False(refreshing);
		}

		[Fact]
		public void EndRefresh()
		{
			var lv = new ListView { IsRefreshing = true };

			Assert.True(lv.IsRefreshing);

			lv.EndRefresh();

			Assert.False(lv.IsRefreshing);
		}

		[Fact]
		public void CanRefreshAfterCantExecuteCommand()
		{
			var lv = new ListView();

			bool commandExecuted = false, eventFired = false;

			lv.Refreshing += (sender, args) => eventFired = true;

			Command refresh = new Command(() => commandExecuted = true,
				() => false);

			lv.RefreshCommand = refresh;
			lv.RefreshCommand = null;

			((IListViewController)lv).SendRefreshing();

			Assert.True(lv.IsRefreshing);
			Assert.True(eventFired);
			Assert.False(commandExecuted);
		}

		[Fact]
		public void StopsListeningToCommandAfterCleared()
		{
			var lv = new ListView();

			bool commandExecuted = false, canExecuteRequested = false;

			Command refresh = new Command(() => commandExecuted = true,
				() => canExecuteRequested = true);

			lv.RefreshCommand = refresh;
			canExecuteRequested = false;

			lv.RefreshCommand = null;

			refresh.ChangeCanExecute();
			Assert.False(canExecuteRequested);

			lv.BeginRefresh();

			Assert.False(commandExecuted);
		}

		[Fact("We should be able to set selected item when using ReadOnlyList")]
		public void SetItemSelectedOnReadOnlyList()
		{
			var source = new ReadOnlySource();
			var listView = new ListView
			{
				ItemsSource = source
			};

			bool raised = false;
			listView.ItemSelected += (sender, arg) => raised = true;

			listView.SelectedItem = source[0];
			Assert.True(raised);
		}

		internal class ReadOnlySource : IReadOnlyList<ListItem>
		{
			List<ListItem> items;
			public ReadOnlySource()
			{
				items = new List<ListItem>();

				for (int i = 0; i < 100; i++)
				{
					items.Add(new ListItem { Name = "person " + i });
				}

			}
			#region IEnumerable implementation
			public IEnumerator<ListItem> GetEnumerator()
			{
				return items.GetEnumerator();
			}
			#endregion
			#region IEnumerable implementation
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return items.GetEnumerator();
			}
			#endregion
			#region IReadOnlyList implementation
			public ListItem this[int index]
			{
				get
				{
					return items[index];
				}
			}
			#endregion
			#region IReadOnlyCollection implementation
			public int Count
			{
				get
				{
					return items.Count;
				}
			}
			#endregion

		}

		[Fact]
		public void ChildElementsParentIsNulledWhenReset()
		{
			var list = new ListView();
			list.ItemsSource = new[] { "Hi", "Bye" };

			var cell = list.TemplatedItems[0];
			Assert.Same(cell.Parent, list);

			list.ItemsSource = null;
			Assert.Null(cell.Parent);
		}

		[Fact]
		public void ChildElementsParentIsNulledWhenRemoved()
		{
			var collection = new ObservableCollection<string> {
				"Hi", "Bye"
			};

			var list = new ListView();
			list.ItemsSource = collection;

			var cell = list.TemplatedItems[0];
			Assert.Same(cell.Parent, list);

			collection.Remove(collection[0]);
			Assert.Null(cell.Parent);
		}

		[Fact]
		public void ChildElementsParentIsNulledWhenCleared()
		{
			var collection = new ObservableCollection<string> {
				"Hi", "Bye"
			};

			var list = new ListView();
			list.ItemsSource = collection;

			var cell = list.TemplatedItems[0];
			Assert.Same(cell.Parent, list);

			collection.Clear();
			Assert.Null(cell.Parent);
		}

		[Theory]
		[InlineData("Android", ListViewCachingStrategy.RecycleElement)]
		[InlineData("iOS", ListViewCachingStrategy.RecycleElement)]
		[InlineData("UWP", ListViewCachingStrategy.RetainElement)]
		[InlineData("Other", ListViewCachingStrategy.RetainElement)]
		public void EnforcesCachingStrategy(string platform, ListViewCachingStrategy expected)
		{
			// we need to do this because otherwise we cant set the caching strategy
			mockDeviceInfo.Platform = DevicePlatform.Create(platform);
			var listView = new ListView(ListViewCachingStrategy.RecycleElement);

			Assert.Equal(expected, listView.CachingStrategy);
		}

		[Fact]
		public void DefaultCacheStrategy()
		{
			var listView = new ListView();

			Assert.Equal(ListViewCachingStrategy.RetainElement, listView.CachingStrategy);
		}

		[Fact]
		public void DoesNotRetainInRecycleMode()
		{
			var items = new ObservableCollection<string> {
				"Foo",
				"Bar"
			};

			// we need to do this because otherwise we cant set the caching strategy
			mockDeviceInfo.Platform = DevicePlatform.Android;

			var bindable = new ListView(ListViewCachingStrategy.RecycleElement);
			bindable.ItemTemplate = new DataTemplate(typeof(TextCell))
			{
				Bindings = {
					{ TextCell.TextProperty, new Binding (".") }
				}
			};

			bindable.ItemsSource = items;
			var item1 = bindable.TemplatedItems[0];
			var item2 = bindable.TemplatedItems[0];

			Assert.False(ReferenceEquals(item1, item2));
		}

		[Fact]
		public void ForceUpdateSizeCalledOnViewCellDoesntCrash()
		{
			var list = new ListView()
			{
				HasUnevenRows = true
			};

			list.ItemTemplate = new DataTemplate(() =>
				{
					return new ViewCell { View = new Label() };
				}
			);

			list.ItemsSource = new[] { "Hi" };

			var element = (ViewCell)list.TemplatedItems[0];

			element.ForceUpdateSize();
		}
	}
}
