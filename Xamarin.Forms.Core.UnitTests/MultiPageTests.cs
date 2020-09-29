using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public abstract class MultiPageTests<T> : BaseTestFixture
		where T : Page
	{
		protected abstract MultiPage<T> CreateMultiPage();
		protected abstract T CreateContainedPage();
		protected abstract int GetIndex(T page);

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void TestSetChildren()
		{
			var container = CreateMultiPage();
			var page = (Page)container;

			int childCount = 0;
			page.ChildAdded += (sender, args) => childCount++;

			int pagesAdded = 0;
			container.PagesChanged += (sender, args) =>
			{
				if (args.Action == NotifyCollectionChangedAction.Add)
					pagesAdded++;
			};

			container.Children.Add(CreateContainedPage());
			container.Children.Add(CreateContainedPage());

			Assert.AreEqual(2, childCount);
			Assert.AreEqual(2, ((IElementController)page).LogicalChildren.Count);
			Assert.AreEqual(2, pagesAdded);
		}

		[Test]
		public void TestOverwriteChildren()
		{
			var page = CreateMultiPage();
			page.Children.Add(CreateContainedPage());
			page.Children.Add(CreateContainedPage());

			int childCount = 0;
			int removeCount = 0;
			page.ChildAdded += (sender, args) => childCount++;
			page.ChildRemoved += (sender, args) => removeCount++;

			foreach (var child in page.Children.ToArray())
				page.Children.Remove((T)child);

			page.Children.Add(CreateContainedPage());
			page.Children.Add(CreateContainedPage());

			Assert.AreEqual(2, removeCount);
			Assert.AreEqual(2, childCount);
			Assert.AreEqual(2, ((IElementController)page).LogicalChildren.Count);
		}

		[Test]
		public void CurrentPageSetAfterAdd()
		{
			var page = CreateMultiPage();
			Assert.That(page.CurrentPage, Is.Null);

			var child = CreateContainedPage();

			bool property = false;
			page.PropertyChanged += (o, e) =>
			{
				if (e.PropertyName == "CurrentPage")
					property = true;
			};

			page.Children.Add(child);

			Assert.That(page.CurrentPage, Is.SameAs(child));
			Assert.That(property, Is.True, "CurrentPage property change did not fire");
		}

		[Test]
		public void CurrentPageChangedAfterRemove()
		{
			var page = CreateMultiPage();
			var child = CreateContainedPage();
			var child2 = CreateContainedPage();
			page.Children.Add(child);
			page.Children.Add(child2);

			bool property = false;
			page.PropertyChanged += (o, e) =>
			{
				if (e.PropertyName == "CurrentPage")
					property = true;
			};

			page.Children.Remove(child);

			Assert.That(page.CurrentPage, Is.SameAs(child2), "MultiPage.CurrentPage is not set to a new page after current was removed");
			Assert.That(property, Is.True, "CurrentPage property change did not fire");
		}

		[Test]
		public void CurrentPageNullAfterRemove()
		{
			var page = CreateMultiPage();
			var child = CreateContainedPage();
			page.Children.Add(child);

			bool property = false;
			page.PropertyChanged += (o, e) =>
			{
				if (e.PropertyName == "CurrentPage")
					property = true;
			};

			page.Children.Remove(child);

			Assert.That(page.CurrentPage, Is.Null, "MultiPage.CurrentPage is still set after that page was removed");
			Assert.That(property, Is.True, "CurrentPage property change did not fire");
		}

		[Test]
		public void TemplatedPage()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			page.ItemsSource = new[] { "Foo", "Bar" };

			Action<Page, string> assertPage = (p, s) =>
			{
				Assert.That(p, Is.InstanceOf<ContentPage>());

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(2));
			assertPage((Page)pages[0], "Foo");
			assertPage((Page)pages[1], "Bar");
		}

		[Test]
		public void SelectedItemSetAfterAdd()
		{
			var page = CreateMultiPage();
			Assert.That(page.CurrentPage, Is.Null);

			var items = new ObservableCollection<string>();

			page.ItemsSource = items;

			bool selected = false;
			bool current = false;
			page.PropertyChanged += (o, e) =>
			{
				if (e.PropertyName == "CurrentPage")
					current = true;
				else if (e.PropertyName == "SelectedItem")
					selected = true;
			};

			items.Add("foo");

			Assert.That(page.SelectedItem, Is.SameAs(items.First()));
			Assert.That(page.CurrentPage.BindingContext, Is.SameAs(page.SelectedItem));
			Assert.That(current, Is.True, "CurrentPage property change did not fire");
			Assert.That(selected, Is.True, "SelectedItem property change did not fire");
		}

		[Test]
		public void SelectedItemNullAfterRemove()
		{
			var page = CreateMultiPage();
			Assert.That(page.CurrentPage, Is.Null);

			var items = new ObservableCollection<string> { "foo" };
			page.ItemsSource = items;

			bool selected = false;
			bool current = false;
			page.PropertyChanged += (o, e) =>
			{
				if (e.PropertyName == "CurrentPage")
					current = true;
				else if (e.PropertyName == "SelectedItem")
					selected = true;
			};

			items.Remove("foo");

			Assert.That(page.SelectedItem, Is.Null, "MultiPage.SelectedItem is still set after that page was removed");
			Assert.That(page.CurrentPage, Is.Null, "MultiPage.CurrentPage is still set after that page was removed");
			Assert.That(current, Is.True, "CurrentPage property change did not fire");
			Assert.That(selected, Is.True, "SelectedItem property change did not fire");
		}

		[Test]
		[Description("When ItemsSource is set with items, the first item should automatically be selected")]
		public void SelectedItemSetAfterItemsSourceSet()
		{
			var page = CreateMultiPage();

			bool selected = false;
			bool current = false;
			page.PropertyChanged += (o, e) =>
			{
				if (e.PropertyName == "CurrentPage")
					current = true;
				else if (e.PropertyName == "SelectedItem")
					selected = true;
			};

			page.ItemsSource = new[] { "foo" };

			Assert.That(page.SelectedItem, Is.SameAs(((string[])page.ItemsSource)[0]));
			Assert.That(page.CurrentPage.BindingContext, Is.SameAs(page.SelectedItem));
			Assert.That(current, Is.True, "CurrentPage property change did not fire");
			Assert.That(selected, Is.True, "SelectedItem property change did not fire");
		}

		[Test]
		public void SelectedItemNoLongerPresent()
		{
			var page = CreateMultiPage();

			string[] items = new[] { "foo", "bar" };
			page.ItemsSource = items;
			page.SelectedItem = items[1];

			items = new[] { "fad", "baz" };
			page.ItemsSource = items;

			Assert.That(page.SelectedItem, Is.SameAs(items[0]));
		}

		[Test]
		public void SelectedItemAfterMove()
		{
			var page = CreateMultiPage();

			var items = new ObservableCollection<string> { "foo", "bar" };
			page.ItemsSource = items;

			Assert.That(page.SelectedItem, Is.SameAs(items[0]));
			Assert.That(page.CurrentPage, Is.Not.Null);
			Assert.That(page.CurrentPage.BindingContext, Is.SameAs(items[0]));

			page.SelectedItem = items[1];
			Assert.That(page.CurrentPage.BindingContext, Is.SameAs(items[1]));

			items.Move(1, 0);

			Assert.That(page.SelectedItem, Is.SameAs(items[0]));
			Assert.That(page.CurrentPage, Is.Not.Null);
			Assert.That(page.CurrentPage.BindingContext, Is.SameAs(items[0]));
		}

		[Test]
		public void UntemplatedItemsSourcePage()
		{
			var page = CreateMultiPage();

			page.ItemsSource = new[] { "Foo", "Bar" };

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(2));
			Assert.That(((Page)pages[0]).Title, Is.EqualTo("Foo"));
			Assert.That(((Page)pages[1]).Title, Is.EqualTo("Bar"));
		}

		[Test]
		public void TemplatePagesAdded()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			var items = new ObservableCollection<string> { "Foo", "Bar" };
			page.ItemsSource = items;

			Action<IList<Element>, int, string> assertPage = (ps, index, s) =>
			{
				Page p = (Page)ps[index];
				Assert.That(p, Is.InstanceOf<ContentPage>());
				Assert.That(GetIndex((T)p), Is.EqualTo(index));

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			items.Add("Baz");

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(3), "Children should have 3 pages");
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Bar");
			assertPage(pages, 2, "Baz");
		}

		[Test]
		public void TemplatePagesRangeAdded()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			var items = new ObservableList<string> { "Foo", "Bar" };
			page.ItemsSource = items;

			Action<IList<Element>, int, string> assertPage = (ps, index, s) =>
			{
				Page p = (Page)ps[index];
				Assert.That(p, Is.InstanceOf<ContentPage>());
				Assert.That(GetIndex((T)p), Is.EqualTo(index));

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			int addedCount = 0;
			page.PagesChanged += (sender, e) =>
			{
				if (e.Action != NotifyCollectionChangedAction.Add)
					return;

				addedCount++;
				Assert.That(e.NewItems.Count, Is.EqualTo(2));
			};

			items.AddRange(new[] { "Baz", "Bam" });

			Assert.That(addedCount, Is.EqualTo(1));

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(4));
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Bar");
			assertPage(pages, 2, "Baz");
			assertPage(pages, 3, "Bam");
		}

		[Test]
		public void TemplatePagesInserted()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			var items = new ObservableCollection<string> { "Foo", "Bar" };
			page.ItemsSource = items;

			Action<IList<Element>, int, string> assertPage = (ps, index, s) =>
			{
				Page p = (Page)ps[index];
				Assert.That(p, Is.InstanceOf<ContentPage>());
				Assert.That(GetIndex((T)p), Is.EqualTo(index));

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			items.Insert(1, "Baz");

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(3));
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Baz");
			assertPage(pages, 2, "Bar");
		}

		[Test]
		public void TemplatePagesRangeInserted()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			var items = new ObservableList<string> { "Foo", "Bar" };
			page.ItemsSource = items;

			Action<IList<Element>, int, string> assertPage = (ps, index, s) =>
			{
				Page p = (Page)ps[index];
				Assert.That(p, Is.InstanceOf<ContentPage>());
				Assert.That(GetIndex((T)p), Is.EqualTo(index));

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			items.InsertRange(1, new[] { "Baz", "Bam" });

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(4));
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Baz");
			assertPage(pages, 2, "Bam");
			assertPage(pages, 3, "Bar");
		}

		[Test]
		public void TemplatePagesRemoved()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			var items = new ObservableCollection<string> { "Foo", "Bar" };
			page.ItemsSource = items;

			Action<IList<Element>, int, string> assertPage = (ps, index, s) =>
			{
				Page p = (Page)ps[index];
				Assert.That(p, Is.InstanceOf<ContentPage>());
				Assert.That(GetIndex((T)p), Is.EqualTo(index));

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			items.Remove("Foo");

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(1));
			assertPage(pages, 0, "Bar");
		}

		[Test]
		public void TemplatePagesRangeRemoved()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			var items = new ObservableList<string> { "Foo", "Bar", "Baz", "Bam", "Who" };
			page.ItemsSource = items;

			Action<IList<Element>, int, string> assertPage = (ps, index, s) =>
			{
				Page p = (Page)ps[index];
				Assert.That(p, Is.InstanceOf<ContentPage>());
				Assert.That(GetIndex((T)p), Is.EqualTo(index));

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			items.RemoveAt(1, 2);

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(3));
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Bam");
			assertPage(pages, 2, "Who");
		}

		[Test]
		public void TemplatePagesReordered()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			var items = new ObservableCollection<string> { "Foo", "Bar" };
			page.ItemsSource = items;

			Action<IList<Element>, int, string> assertPage = (ps, index, s) =>
			{
				Page p = (Page)ps[index];
				Assert.That(p, Is.InstanceOf<ContentPage>());
				Assert.That(GetIndex((T)p), Is.EqualTo(index));

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			items.Move(0, 1);

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(2));
			assertPage(pages, 0, "Bar");
			assertPage(pages, 1, "Foo");
		}

		[Test]
		public void TemplatePagesRangeReorderedForward()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			var items = new ObservableList<string> { "Foo", "Bar", "Baz", "Bam", "Who", "Where" };
			page.ItemsSource = items;

			Action<IList<Element>, int, string> assertPage = (ps, index, s) =>
			{
				Page p = (Page)ps[index];
				Assert.That(p, Is.InstanceOf<ContentPage>());
				Assert.That(GetIndex((T)p), Is.EqualTo(index));

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			items.Move(1, 4, 2);

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(6));
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Bam");
			assertPage(pages, 2, "Who");
			assertPage(pages, 3, "Bar");
			assertPage(pages, 4, "Baz");
			assertPage(pages, 5, "Where");
		}

		[Test]
		public void TemplatePagesRangeReorderedBackward()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			var items = new ObservableList<string> { "Foo", "Bar", "Baz", "Bam", "Who", "Where", "When" };
			page.ItemsSource = items;

			Action<IList<Element>, int, string> assertPage = (ps, index, s) =>
			{
				Page p = (Page)ps[index];
				Assert.That(p, Is.InstanceOf<ContentPage>());
				Assert.That(GetIndex((T)p), Is.EqualTo(index));

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			items.Move(4, 1, 2);

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(7));
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Who");
			assertPage(pages, 2, "Where");
			assertPage(pages, 3, "Bar");
			assertPage(pages, 4, "Baz");
			assertPage(pages, 5, "Bam");
			assertPage(pages, 6, "When");
		}

		[Test]
		public void TemplatePagesReplaced()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			var items = new ObservableCollection<string> { "Foo", "Bar" };
			page.ItemsSource = items;

			Action<IList<Element>, int, string> assertPage = (ps, index, s) =>
			{
				Page p = (Page)ps[index];
				Assert.That(p, Is.InstanceOf<ContentPage>());
				Assert.That(GetIndex((T)p), Is.EqualTo(index));

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			items[0] = "Baz";

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(2));
			assertPage(pages, 0, "Baz");
			assertPage(pages, 1, "Bar");
		}

		[Test]
		public void TemplatedPagesSourceReplaced()
		{
			var page = CreateMultiPage();

			page.ItemTemplate = new DataTemplate(() =>
			{
				var p = new ContentPage();
				p.Content = new Label();
				p.Content.SetBinding(Label.TextProperty, new Binding("."));
				return p;
			});

			page.ItemsSource = new ObservableCollection<string> { "Foo", "Bar" };

			Action<Page, string> assertPage = (p, s) =>
			{
				Assert.That(p, Is.InstanceOf<ContentPage>());

				var cp = (ContentPage)p;
				Assert.That(cp.Content, Is.InstanceOf<Label>());
				Assert.That(((Label)cp.Content).Text, Is.EqualTo(s));
			};

			page.ItemsSource = new ObservableCollection<string> { "Baz", "Bar" };

			var pages = page.Children.ToArray();
			Assert.That(pages.Length, Is.EqualTo(2));
			assertPage((Page)pages[0], "Baz");
			assertPage((Page)pages[1], "Bar");
		}

		[Test]
		[Description("If you have a templated set of items, setting CurrentPage (usually from renderers) should update SelectedItem properly")]
		public void SettingCurrentPageWithTemplatesUpdatesSelectedItem()
		{
			var page = CreateMultiPage();

			var items = new[] { "Foo", "Bar" };
			page.ItemsSource = items;

			// If these aren't correct, the rest of the test is invalid
			Assert.That(page.CurrentPage, Is.SameAs(page.Children[0]));
			Assert.That(page.SelectedItem, Is.SameAs(items[0]));

			page.CurrentPage = (T)page.Children[1];

			Assert.That(page.SelectedItem, Is.SameAs(items[1]));
		}

		[Test]
		public void PagesChangedOnItemsSourceChange()
		{
			var page = CreateMultiPage();

			page.ItemsSource = new[] { "Baz", "Bam" };

			int fail = 0;
			int reset = 0;
			page.PagesChanged += (sender, args) =>
			{
				if (args.Action == NotifyCollectionChangedAction.Reset)
					reset++;
				else
					fail++;
			};

			page.ItemsSource = new[] { "Foo", "Bar" };

			Assert.That(reset, Is.EqualTo(1), "PagesChanged wasn't raised or was raised too many times for Reset");
			Assert.That(fail, Is.EqualTo(0), "PagesChanged was raised with an unexpected action");
		}

		[Test]
		public void PagesChangedOnTemplateChange()
		{
			var page = CreateMultiPage();

			page.ItemsSource = new[] { "Foo", "Bar" };

			int fail = 0;
			int reset = 0;
			page.PagesChanged += (sender, args) =>
			{
				if (args.Action == NotifyCollectionChangedAction.Reset)
					reset++;
				else
					fail++;
			};

			page.ItemTemplate = new DataTemplate(() => new ContentPage
			{
				Content = new Label { Text = "Content" }
			});

			Assert.That(reset, Is.EqualTo(1), "PagesChanged wasn't raised or was raised too many times for Reset");
			Assert.That(fail, Is.EqualTo(0), "PagesChanged was raised with an unexpected action");
		}

		[Test]
		public void SelectedItemSetBeforeTemplate()
		{
			var page = CreateMultiPage();

			string[] items = new[] { "foo", "bar" };
			page.ItemsSource = items;
			page.SelectedItem = items[1];

			var template = new DataTemplate(typeof(ContentPage));
			template.SetBinding(ContentPage.TitleProperty, ".");
			page.ItemTemplate = template;

			Assert.That(page.SelectedItem, Is.SameAs(items[1]));
		}

		[Test]
		public void CurrentPageUpdatedWithTemplate()
		{
			var page = CreateMultiPage();
			string[] items = new[] { "foo", "bar" };
			page.ItemsSource = items;

			var untemplated = page.CurrentPage;

			bool raised = false;
			page.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == "CurrentPage")
					raised = true;
			};

			var template = new DataTemplate(() =>
			{
				var p = new ContentPage { Content = new Label() };
				p.Content.SetBinding(Label.TextProperty, ".");
				return p;
			});

			page.ItemTemplate = template;

			Assert.That(raised, Is.True, "CurrentPage did not change with the template");
			Assert.That(page.CurrentPage, Is.Not.SameAs(untemplated));
		}

		[Test]
		public void CurrentPageChanged()
		{
			var page = CreateMultiPage();
			page.Children.Add(CreateContainedPage());
			page.Children.Add(CreateContainedPage());

			bool raised = false;
			page.CurrentPageChanged += (sender, e) =>
			{
				raised = true;
			};

			page.CurrentPage = page.Children[0];

			Assert.That(raised, Is.False);

			page.CurrentPage = page.Children[1];

			Assert.That(raised, Is.True);
		}
	}
}