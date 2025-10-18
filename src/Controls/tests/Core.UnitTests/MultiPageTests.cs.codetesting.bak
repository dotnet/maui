using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public abstract class MultiPageTests<T> : BaseTestFixture
		where T : Page
	{
		protected abstract MultiPage<T> CreateMultiPage();
		protected abstract T CreateContainedPage();
		protected abstract int GetIndex(T page);

		[Fact]
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

			SurfaceGCBugs();

			container.Children.Add(CreateContainedPage());
			container.Children.Add(CreateContainedPage());

			Assert.Equal(2, childCount);
			Assert.Equal(2, ((IElementController)page).LogicalChildren.Count);
			Assert.Equal(2, pagesAdded);
		}

		[Fact]
		public void TestOverwriteChildren()
		{
			var page = CreateMultiPage();
			page.Children.Add(CreateContainedPage());
			page.Children.Add(CreateContainedPage());

			int childCount = 0;
			int removeCount = 0;
			page.ChildAdded += (sender, args) => childCount++;
			page.ChildRemoved += (sender, args) => removeCount++;

			SurfaceGCBugs();

			foreach (var child in page.Children.ToArray())
				page.Children.Remove((T)child);

			page.Children.Add(CreateContainedPage());
			page.Children.Add(CreateContainedPage());

			Assert.Equal(2, removeCount);
			Assert.Equal(2, childCount);
			Assert.Equal(2, ((IElementController)page).LogicalChildren.Count);
		}

		[Fact]
		public void CurrentPageSetAfterAdd()
		{
			var page = CreateMultiPage();
			Assert.Null(page.CurrentPage);

			var child = CreateContainedPage();

			bool property = false;
			page.PropertyChanged += (o, e) =>
			{
				if (e.PropertyName == "CurrentPage")
					property = true;
			};

			SurfaceGCBugs();

			page.Children.Add(child);

			Assert.Same(page.CurrentPage, child);
			Assert.True(property, "CurrentPage property change did not fire");
		}

		[Fact]
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

			SurfaceGCBugs();

			page.Children.Remove(child);

			Assert.Same(page.CurrentPage, child2);
			Assert.True(property, "CurrentPage property change did not fire");
		}

		[Fact]
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

			SurfaceGCBugs();

			page.Children.Remove(child);

			Assert.Null(page.CurrentPage);
			Assert.True(property, "CurrentPage property change did not fire");
		}

		[Fact]
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

			SurfaceGCBugs();

			page.ItemsSource = new[] { "Foo", "Bar" };

			Action<Page, string> assertPage = (p, s) =>
			{
				Assert.IsType<ContentPage>(p);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			var pages = page.Children.ToArray();
			Assert.Equal(2, pages.Length);
			assertPage((Page)pages[0], "Foo");
			assertPage((Page)pages[1], "Bar");
		}

		[Fact]
		public void SelectedItemSetAfterAdd()
		{
			var page = CreateMultiPage();
			Assert.Null(page.CurrentPage);

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

			SurfaceGCBugs();

			items.Add("foo");

			Assert.Same(page.SelectedItem, items.First());
			Assert.Same(page.CurrentPage.BindingContext, page.SelectedItem);
			Assert.True(current, "CurrentPage property change did not fire");
			Assert.True(selected, "SelectedItem property change did not fire");
		}

		[Fact]
		public void SelectedItemNullAfterRemove()
		{
			var page = CreateMultiPage();
			Assert.Null(page.CurrentPage);

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

			SurfaceGCBugs();

			items.Remove("foo");

			Assert.Null(page.SelectedItem);
			Assert.Null(page.CurrentPage);
			Assert.True(current, "CurrentPage property change did not fire");
			Assert.True(selected, "SelectedItem property change did not fire");
		}

		[Fact("When ItemsSource is set with items, the first item should automatically be selected")]
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

			SurfaceGCBugs();

			page.ItemsSource = new[] { "foo" };

			Assert.Same(page.SelectedItem, ((string[])page.ItemsSource)[0]);
			Assert.Same(page.CurrentPage.BindingContext, page.SelectedItem);
			Assert.True(current, "CurrentPage property change did not fire");
			Assert.True(selected, "SelectedItem property change did not fire");
		}

		[Fact]
		public void SelectedItemNoLongerPresent()
		{
			var page = CreateMultiPage();

			string[] items = new[] { "foo", "bar" };
			page.ItemsSource = items;
			page.SelectedItem = items[1];

			items = new[] { "fad", "baz" };
			page.ItemsSource = items;

			Assert.Same(page.SelectedItem, items[0]);
		}

		[Fact]
		public void SelectedItemAfterMove()
		{
			var page = CreateMultiPage();

			var items = new ObservableCollection<string> { "foo", "bar" };
			page.ItemsSource = items;

			Assert.Same(page.SelectedItem, items[0]);
			Assert.NotNull(page.CurrentPage);
			Assert.Same(page.CurrentPage.BindingContext, items[0]);

			page.SelectedItem = items[1];
			Assert.Same(page.CurrentPage.BindingContext, items[1]);

			items.Move(1, 0);

			Assert.Same(page.SelectedItem, items[0]);
			Assert.NotNull(page.CurrentPage);
			Assert.Same(page.CurrentPage.BindingContext, items[0]);
		}

		[Fact]
		public void UntemplatedItemsSourcePage()
		{
			var page = CreateMultiPage();

			page.ItemsSource = new[] { "Foo", "Bar" };

			var pages = page.Children.ToArray();
			Assert.Equal(2, pages.Length);
			Assert.Equal("Foo", pages[0].Title);
			Assert.Equal("Bar", ((Page)pages[1]).Title);
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);
				Assert.Equal(GetIndex((T)p), index);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			SurfaceGCBugs();

			items.Add("Baz");

			var pages = page.Children.ToArray();
			Assert.Equal(3, pages.Length); // "Children should have 3 pages"
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Bar");
			assertPage(pages, 2, "Baz");
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);
				Assert.Equal(GetIndex((T)p), index);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			int addedCount = 0;
			page.PagesChanged += (sender, e) =>
			{
				if (e.Action != NotifyCollectionChangedAction.Add)
					return;

				addedCount += 1;
				Assert.Equal(2, e.NewItems.Count);
			};

			SurfaceGCBugs();

			items.AddRange(new[] { "Baz", "Bam" });

			Assert.Equal(1, addedCount);

			var pages = page.Children.ToArray();
			Assert.Equal(4, pages.Length);
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Bar");
			assertPage(pages, 2, "Baz");
			assertPage(pages, 3, "Bam");
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);
				Assert.Equal(GetIndex((T)p), index);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			SurfaceGCBugs();

			items.Insert(1, "Baz");

			var pages = page.Children.ToArray();
			Assert.Equal(3, pages.Length);
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Baz");
			assertPage(pages, 2, "Bar");
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);
				Assert.Equal(GetIndex((T)p), index);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			SurfaceGCBugs();

			items.InsertRange(1, new[] { "Baz", "Bam" });

			var pages = page.Children.ToArray();
			Assert.Equal(4, pages.Length);
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Baz");
			assertPage(pages, 2, "Bam");
			assertPage(pages, 3, "Bar");
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);
				Assert.Equal(GetIndex((T)p), index);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			SurfaceGCBugs();

			items.Remove("Foo");

			var pages = page.Children.ToArray();
			Assert.Single(pages);
			assertPage(pages, 0, "Bar");
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);
				Assert.Equal(GetIndex((T)p), index);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			SurfaceGCBugs();

			items.RemoveAt(1, 2);

			var pages = page.Children.ToArray();
			Assert.Equal(3, pages.Length);
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Bam");
			assertPage(pages, 2, "Who");
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);
				Assert.Equal(GetIndex((T)p), index);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			SurfaceGCBugs();

			items.Move(0, 1);

			var pages = page.Children.ToArray();
			Assert.Equal(2, pages.Length);
			assertPage(pages, 0, "Bar");
			assertPage(pages, 1, "Foo");
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);
				Assert.Equal(GetIndex((T)p), index);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			SurfaceGCBugs();

			items.Move(1, 4, 2);

			var pages = page.Children.ToArray();
			Assert.Equal(6, pages.Length);
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Bam");
			assertPage(pages, 2, "Who");
			assertPage(pages, 3, "Bar");
			assertPage(pages, 4, "Baz");
			assertPage(pages, 5, "Where");
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);
				Assert.Equal(GetIndex((T)p), index);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			SurfaceGCBugs();

			items.Move(4, 1, 2);

			var pages = page.Children.ToArray();
			Assert.Equal(7, pages.Length);
			assertPage(pages, 0, "Foo");
			assertPage(pages, 1, "Who");
			assertPage(pages, 2, "Where");
			assertPage(pages, 3, "Bar");
			assertPage(pages, 4, "Baz");
			assertPage(pages, 5, "Bam");
			assertPage(pages, 6, "When");
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);
				Assert.Equal(GetIndex((T)p), index);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			SurfaceGCBugs();

			items[0] = "Baz";

			var pages = page.Children.ToArray();
			Assert.Equal(2, pages.Length);
			assertPage(pages, 0, "Baz");
			assertPage(pages, 1, "Bar");
		}

		[Fact]
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
				Assert.IsType<ContentPage>(p);

				var cp = (ContentPage)p;
				Assert.IsType<Label>(cp.Content);
				Assert.Equal(((Label)cp.Content).Text, s);
			};

			page.ItemsSource = new ObservableCollection<string> { "Baz", "Bar" };

			SurfaceGCBugs();

			var pages = page.Children.ToArray();
			Assert.Equal(2, pages.Length);
			assertPage((Page)pages[0], "Baz");
			assertPage((Page)pages[1], "Bar");
		}

		[Fact("If you have a templated set of items, setting CurrentPage (usually from renderers) should update SelectedItem properly")]
		public void SettingCurrentPageWithTemplatesUpdatesSelectedItem()
		{
			var page = CreateMultiPage();

			var items = new[] { "Foo", "Bar" };
			page.ItemsSource = items;

			// If these aren't correct, the rest of the test is invalid
			Assert.Same(page.CurrentPage, page.Children[0]);
			Assert.Same(page.SelectedItem, items[0]);

			page.CurrentPage = (T)page.Children[1];

			Assert.Same(page.SelectedItem, items[1]);
		}

		[Fact]
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

			SurfaceGCBugs();

			page.ItemsSource = new[] { "Foo", "Bar" };

			Assert.Equal(1, reset); // "PagesChanged wasn't raised or was raised too many times for Reset"
			Assert.Equal(0, fail); // "PagesChanged was raised with an unexpected action"
		}

		[Fact]
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

			SurfaceGCBugs();

			page.ItemTemplate = new DataTemplate(() => new ContentPage
			{
				Content = new Label { Text = "Content" }
			});

			Assert.Equal(1, reset); // "PagesChanged wasn't raised or was raised too many times for Reset"
			Assert.Equal(0, fail); // "PagesChanged was raised with an unexpected action"
		}

		[Fact]
		public void SelectedItemSetBeforeTemplate()
		{
			var page = CreateMultiPage();

			string[] items = new[] { "foo", "bar" };
			page.ItemsSource = items;
			page.SelectedItem = items[1];

			var template = new DataTemplate(typeof(ContentPage));
			template.SetBinding(ContentPage.TitleProperty, ".");
			page.ItemTemplate = template;

			Assert.Same(page.SelectedItem, items[1]);
		}

		[Fact]
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

			Assert.True(raised); // "CurrentPage did not change with the template"
			Assert.NotSame(page.CurrentPage, untemplated);
		}

		[Fact]
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

			Assert.False(raised);

			page.CurrentPage = page.Children[1];

			Assert.True(raised);
		}

		static void SurfaceGCBugs()
		{
			// Ensure a GC happens here to make sure we don't have any missing references to 
			// the collection changed handlers in ListProxy; without the GC calls if we introduce
			// a reference bug the tests will still usually pass on Debug builds and only intermittently
			// fail on Release builds. We don't want these bugs to accidentally slip by.
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
