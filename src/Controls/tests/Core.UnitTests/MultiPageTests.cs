#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
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

        /// <summary>
        /// Tests that the MultiPage constructor initializes successfully without throwing exceptions
        /// when all dependencies are available.
        /// Expected result: Constructor completes successfully and creates a valid instance.
        /// </summary>
        [Fact]
        public void Constructor_InitializesSuccessfully_WithValidDependencies()
        {
            // Arrange & Act
            TestMultiPage testPage = null;
            var exception = Record.Exception(() => testPage = new TestMultiPage());

            // Assert
            Assert.Null(exception);
            Assert.NotNull(testPage);
            Assert.NotNull(testPage.Children);
        }

        /// <summary>
        /// Tests that the MultiPage constructor properly subscribes to the TemplatedItemsList CollectionChanged event
        /// by verifying that changes to the templated items trigger the appropriate event handler.
        /// Expected result: OnTemplatedItemsChanged is called when templated items collection changes.
        /// </summary>
        [Fact]
        public void Constructor_SubscribesToTemplatedItemsCollectionChanged_EventHandlerAttached()
        {
            // Arrange
            var testPage = new TestMultiPage();
            var itemsSource = new ObservableCollection<string> { "Item1", "Item2" };
            var template = new DataTemplate(() => new TestPage());

            // Act
            testPage.ItemsSource = itemsSource;
            testPage.ItemTemplate = template;

            // Trigger collection change
            testPage.ClearOnTemplatedItemsChangedCallCount();
            itemsSource.Add("Item3");

            // Assert
            Assert.True(testPage.OnTemplatedItemsChangedCallCount > 0);
        }

        /// <summary>
        /// Tests that the MultiPage constructor properly subscribes to the InternalChildren CollectionChanged event
        /// by verifying that changes to the children collection trigger the appropriate event handler.
        /// Expected result: OnChildrenChanged is called when children collection changes.
        /// </summary>
        [Fact]
        public void Constructor_SubscribesToInternalChildrenCollectionChanged_EventHandlerAttached()
        {
            // Arrange
            var testPage = new TestMultiPage();
            var childPage = new TestPage();

            // Act
            testPage.ClearOnChildrenChangedCallCount();
            testPage.Children.Add(childPage);

            // Assert
            Assert.True(testPage.OnChildrenChangedCallCount > 0);
        }

        /// <summary>
        /// Tests that the MultiPage constructor handles multiple sequential operations correctly
        /// and maintains proper event subscriptions throughout the object lifecycle.
        /// Expected result: All operations complete successfully and event handlers continue to work.
        /// </summary>
        [Fact]
        public void Constructor_HandlesMultipleOperations_MaintainsEventSubscriptions()
        {
            // Arrange
            var testPage = new TestMultiPage();
            var itemsSource = new ObservableCollection<string> { "Item1" };
            var template = new DataTemplate(() => new TestPage());
            var childPage = new TestPage();

            // Act
            testPage.ItemsSource = itemsSource;
            testPage.ItemTemplate = template;
            testPage.Children.Add(childPage);

            testPage.ClearOnTemplatedItemsChangedCallCount();
            testPage.ClearOnChildrenChangedCallCount();

            itemsSource.Add("Item2");
            testPage.Children.Add(new TestPage());

            // Assert
            Assert.True(testPage.OnTemplatedItemsChangedCallCount > 0);
            Assert.True(testPage.OnChildrenChangedCallCount > 0);
        }

        /// <summary>
        /// Simple test page implementation for use as child pages in MultiPage tests.
        /// </summary>
        private class TestPage : Page
        {
            public TestPage()
            {
                Title = "Test Page";
            }
        }

        /// <summary>
        /// Tests that GetIndex throws ArgumentNullException when passed a null page parameter.
        /// This test verifies the null check validation in the method.
        /// </summary>
        [Fact]
        public void GetIndex_NullPage_ThrowsArgumentNullException()
        {
            // Arrange
            Page nullPage = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => TestMultiPage.GetIndex(nullPage));
            Assert.Equal("page", exception.ParamName);
        }

        /// <summary>
        /// Tests that GetIndex returns the default index value (-1) for a page that hasn't had its index explicitly set.
        /// This verifies the default behavior when no index has been assigned.
        /// </summary>
        [Fact]
        public void GetIndex_PageWithDefaultIndex_ReturnsNegativeOne()
        {
            // Arrange
            var page = new ContentPage();

            // Act
            var result = TestMultiPage.GetIndex(page);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that GetIndex returns the correct index value for pages with various explicitly set index values.
        /// This verifies that the method properly retrieves the index that was previously set.
        /// </summary>
        /// <param name="expectedIndex">The index value to set and verify</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void GetIndex_PageWithSetIndex_ReturnsCorrectValue(int expectedIndex)
        {
            // Arrange
            var page = new ContentPage();
            TestMultiPage.SetIndex(page, expectedIndex);

            // Act
            var result = TestMultiPage.GetIndex(page);

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests that SetIndex throws ArgumentNullException when page parameter is null.
        /// Verifies proper null validation and exception message.
        /// </summary>
        [Fact]
        public void SetIndex_NullPage_ThrowsArgumentNullException()
        {
            // Arrange
            Page page = null;
            int index = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => MultiPage<Page>.SetIndex(page, index));
            Assert.Equal("page", exception.ParamName);
        }

        /// <summary>
        /// Tests that SetIndex successfully sets the index value on a valid page.
        /// Verifies the index is properly stored using the IndexProperty.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetIndex_ValidPageAndIndex_SetsIndexCorrectly(int index)
        {
            // Arrange
            var page = new Page();

            // Act
            MultiPage<Page>.SetIndex(page, index);

            // Assert
            var retrievedIndex = MultiPage<Page>.GetIndex(page);
            Assert.Equal(index, retrievedIndex);
        }

        /// <summary>
        /// Tests that SetIndex can be called multiple times on the same page with different values.
        /// Verifies that subsequent calls overwrite the previous index value.
        /// </summary>
        [Fact]
        public void SetIndex_MultipleCallsOnSamePage_OverwritesPreviousValue()
        {
            // Arrange
            var page = new Page();
            int firstIndex = 5;
            int secondIndex = 10;

            // Act
            MultiPage<Page>.SetIndex(page, firstIndex);
            MultiPage<Page>.SetIndex(page, secondIndex);

            // Assert
            var retrievedIndex = MultiPage<Page>.GetIndex(page);
            Assert.Equal(secondIndex, retrievedIndex);
        }
    }


    public class MultiPageOnBackButtonPressedTests
    {
        /// <summary>
        /// Tests that OnBackButtonPressed returns false when CurrentPage is null.
        /// This should call the base implementation and return its result.
        /// </summary>
        [Fact]
        public void OnBackButtonPressed_CurrentPageIsNull_ReturnsFalse()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            // CurrentPage is null by default

            // Act
            var result = multiPage.TestOnBackButtonPressed();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that OnBackButtonPressed returns true when CurrentPage is not null 
        /// and SendBackButtonPressed returns true (indicating the event was handled).
        /// </summary>
        [Fact]
        public void OnBackButtonPressed_CurrentPageHandlesEvent_ReturnsTrue()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var currentPage = new TestPage { ShouldHandleBackButton = true };
            multiPage.CurrentPage = currentPage;

            // Act
            var result = multiPage.TestOnBackButtonPressed();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that OnBackButtonPressed returns false when CurrentPage is not null 
        /// but SendBackButtonPressed returns false (indicating the event was not handled).
        /// This should fall through to the base implementation.
        /// </summary>
        [Fact]
        public void OnBackButtonPressed_CurrentPageDoesNotHandleEvent_ReturnsFalse()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var currentPage = new TestPage { ShouldHandleBackButton = false };
            multiPage.CurrentPage = currentPage;

            // Act
            var result = multiPage.TestOnBackButtonPressed();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test implementation of MultiPage for testing purposes.
        /// </summary>
        private class TestMultiPage : MultiPage<TestPage>
        {
            /// <summary>
            /// Exposes the protected OnBackButtonPressed method for testing.
            /// </summary>
            public bool TestOnBackButtonPressed()
            {
                return OnBackButtonPressed();
            }

            protected override TestPage CreateDefault(object item)
            {
                return new TestPage();
            }
        }

        /// <summary>
        /// Test implementation of Page for testing purposes.
        /// </summary>
        private class TestPage : Page
        {
            public bool ShouldHandleBackButton { get; set; }

            protected override bool OnBackButtonPressed()
            {
                return ShouldHandleBackButton;
            }
        }
    }


    /// <summary>
    /// Tests for MultiPage<T>.GetPageByIndex method
    /// </summary>
    public partial class MultiPageGetPageByIndexTests
    {
        /// <summary>
        /// Concrete implementation of MultiPage for testing purposes
        /// </summary>
        private class TestMultiPage : MultiPage<ContentPage>
        {
            protected override ContentPage CreateDefault(object item)
            {
                return new ContentPage();
            }
        }

        /// <summary>
        /// Tests GetPageByIndex with empty InternalChildren collection.
        /// Should return null for any index when no pages are present.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void GetPageByIndex_EmptyCollection_ReturnsNull(int index)
        {
            // Arrange
            var multiPage = new TestMultiPage();

            // Act
            var result = multiPage.GetPageByIndex(index);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetPageByIndex with single page having index 0.
        /// Should return the page when searching for index 0, null otherwise.
        /// </summary>
        [Fact]
        public void GetPageByIndex_SinglePageWithIndex0_ReturnsPageForMatchingIndex()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var page = new ContentPage();
            MultiPage<ContentPage>.SetIndex(page, 0);
            multiPage.Children.Add(page);

            // Act
            var result = multiPage.GetPageByIndex(0);

            // Assert
            Assert.Same(page, result);
        }

        /// <summary>
        /// Tests GetPageByIndex with single page having index 0.
        /// Should return null when searching for non-matching index.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(5)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void GetPageByIndex_SinglePageWithIndex0_ReturnsNullForNonMatchingIndex(int searchIndex)
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var page = new ContentPage();
            MultiPage<ContentPage>.SetIndex(page, 0);
            multiPage.Children.Add(page);

            // Act
            var result = multiPage.GetPageByIndex(searchIndex);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetPageByIndex with single page having a specific index.
        /// Should return the page when searching for matching index, null otherwise.
        /// </summary>
        [Fact]
        public void GetPageByIndex_SinglePageWithSpecificIndex_ReturnsPageForMatchingIndex()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var page = new ContentPage();
            var pageIndex = 42;
            MultiPage<ContentPage>.SetIndex(page, pageIndex);
            multiPage.Children.Add(page);

            // Act
            var result = multiPage.GetPageByIndex(pageIndex);

            // Assert
            Assert.Same(page, result);
        }

        /// <summary>
        /// Tests GetPageByIndex with multiple pages having different indices.
        /// Should return the correct page for each valid index.
        /// </summary>
        [Fact]
        public void GetPageByIndex_MultiplePagesWithDifferentIndices_ReturnsCorrectPage()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var page3 = new ContentPage();

            MultiPage<ContentPage>.SetIndex(page1, 0);
            MultiPage<ContentPage>.SetIndex(page2, 5);
            MultiPage<ContentPage>.SetIndex(page3, 10);

            multiPage.Children.Add(page1);
            multiPage.Children.Add(page2);
            multiPage.Children.Add(page3);

            // Act & Assert
            Assert.Same(page1, multiPage.GetPageByIndex(0));
            Assert.Same(page2, multiPage.GetPageByIndex(5));
            Assert.Same(page3, multiPage.GetPageByIndex(10));
        }

        /// <summary>
        /// Tests GetPageByIndex with multiple pages searching for non-existent index.
        /// Should return null when no page has the specified index.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(7)]
        [InlineData(-1)]
        [InlineData(15)]
        public void GetPageByIndex_MultiplePagesNonExistentIndex_ReturnsNull(int searchIndex)
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var page3 = new ContentPage();

            MultiPage<ContentPage>.SetIndex(page1, 0);
            MultiPage<ContentPage>.SetIndex(page2, 5);
            MultiPage<ContentPage>.SetIndex(page3, 10);

            multiPage.Children.Add(page1);
            multiPage.Children.Add(page2);
            multiPage.Children.Add(page3);

            // Act
            var result = multiPage.GetPageByIndex(searchIndex);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetPageByIndex with negative index values.
        /// Should return null for all negative indices as pages cannot have negative indices.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void GetPageByIndex_NegativeIndex_ReturnsNull(int negativeIndex)
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var page = new ContentPage();
            MultiPage<ContentPage>.SetIndex(page, 0);
            multiPage.Children.Add(page);

            // Act
            var result = multiPage.GetPageByIndex(negativeIndex);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetPageByIndex with very large index values.
        /// Should return null for large indices that don't match any page.
        /// </summary>
        [Theory]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(int.MaxValue)]
        public void GetPageByIndex_LargeIndex_ReturnsNull(int largeIndex)
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var page = new ContentPage();
            MultiPage<ContentPage>.SetIndex(page, 0);
            multiPage.Children.Add(page);

            // Act
            var result = multiPage.GetPageByIndex(largeIndex);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetPageByIndex boundary case with index 0.
        /// Should handle zero index correctly.
        /// </summary>
        [Fact]
        public void GetPageByIndex_IndexZeroBoundary_ReturnsCorrectPage()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var page = new ContentPage();
            MultiPage<ContentPage>.SetIndex(page, 0);
            multiPage.Children.Add(page);

            // Act
            var result = multiPage.GetPageByIndex(0);

            // Assert
            Assert.Same(page, result);
        }

        /// <summary>
        /// Tests GetPageByIndex with multiple pages having the same index.
        /// Should return the first page found with the matching index.
        /// </summary>
        [Fact]
        public void GetPageByIndex_MultiplePagesWithSameIndex_ReturnsFirstMatch()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var duplicateIndex = 5;

            MultiPage<ContentPage>.SetIndex(page1, duplicateIndex);
            MultiPage<ContentPage>.SetIndex(page2, duplicateIndex);

            multiPage.Children.Add(page1);
            multiPage.Children.Add(page2);

            // Act
            var result = multiPage.GetPageByIndex(duplicateIndex);

            // Assert
            Assert.Same(page1, result);
        }

        /// <summary>
        /// Tests GetPageByIndex with pages having maximum integer index.
        /// Should handle maximum index values correctly.
        /// </summary>
        [Fact]
        public void GetPageByIndex_MaxIntegerIndex_ReturnsCorrectPage()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var page = new ContentPage();
            MultiPage<ContentPage>.SetIndex(page, int.MaxValue);
            multiPage.Children.Add(page);

            // Act
            var result = multiPage.GetPageByIndex(int.MaxValue);

            // Assert
            Assert.Same(page, result);
        }
    }


    public partial class MultiPageItemTemplateTests : BaseTestFixture
    {
        /// <summary>
        /// Concrete implementation of MultiPage for testing purposes.
        /// </summary>
        private class TestMultiPage : MultiPage<ContentPage>
        {
            protected override ContentPage CreateDefault(object item)
            {
                return new ContentPage { Title = item?.ToString() };
            }
        }

        [Fact]
        /// <summary>
        /// Tests that the ItemTemplate property returns null by default.
        /// Verifies the default state of the ItemTemplate property when no value has been set.
        /// Expected result: ItemTemplate should return null.
        /// </summary>
        public void ItemTemplate_DefaultValue_ReturnsNull()
        {
            // Arrange
            var multiPage = new TestMultiPage();

            // Act
            var result = multiPage.ItemTemplate;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        /// <summary>
        /// Tests that setting a valid DataTemplate to ItemTemplate property stores and retrieves the value correctly.
        /// Verifies that a DataTemplate instance can be set and retrieved from the ItemTemplate property.
        /// Expected result: ItemTemplate should return the same DataTemplate instance that was set.
        /// </summary>
        public void ItemTemplate_SetValidDataTemplate_ReturnsSetValue()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var dataTemplate = new DataTemplate();

            // Act
            multiPage.ItemTemplate = dataTemplate;
            var result = multiPage.ItemTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        [Fact]
        /// <summary>
        /// Tests that setting null to ItemTemplate property works correctly.
        /// Verifies that the ItemTemplate property can be explicitly set to null.
        /// Expected result: ItemTemplate should return null after being set to null.
        /// </summary>
        public void ItemTemplate_SetNull_ReturnsNull()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var dataTemplate = new DataTemplate();
            multiPage.ItemTemplate = dataTemplate;

            // Act
            multiPage.ItemTemplate = null;
            var result = multiPage.ItemTemplate;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        /// <summary>
        /// Tests that setting ItemTemplate multiple times with different values works correctly.
        /// Verifies that the ItemTemplate property correctly updates when set to different DataTemplate instances.
        /// Expected result: ItemTemplate should return the most recently set DataTemplate instance.
        /// </summary>
        public void ItemTemplate_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var firstTemplate = new DataTemplate();
            var secondTemplate = new DataTemplate();

            // Act
            multiPage.ItemTemplate = firstTemplate;
            multiPage.ItemTemplate = secondTemplate;
            var result = multiPage.ItemTemplate;

            // Assert
            Assert.Same(secondTemplate, result);
        }

        [Fact]
        /// <summary>
        /// Tests that setting ItemTemplate with a DataTemplate created from a Type constructor works correctly.
        /// Verifies that DataTemplate instances created with the Type constructor can be set and retrieved.
        /// Expected result: ItemTemplate should return the DataTemplate instance created with Type.
        /// </summary>
        public void ItemTemplate_SetDataTemplateWithType_ReturnsSetValue()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var dataTemplate = new DataTemplate(typeof(ContentPage));

            // Act
            multiPage.ItemTemplate = dataTemplate;
            var result = multiPage.ItemTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        [Fact]
        /// <summary>
        /// Tests that setting ItemTemplate with a DataTemplate created from a Func constructor works correctly.
        /// Verifies that DataTemplate instances created with the Func constructor can be set and retrieved.
        /// Expected result: ItemTemplate should return the DataTemplate instance created with Func.
        /// </summary>
        public void ItemTemplate_SetDataTemplateWithFunc_ReturnsSetValue()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var dataTemplate = new DataTemplate(() => new ContentPage());

            // Act
            multiPage.ItemTemplate = dataTemplate;
            var result = multiPage.ItemTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        [Fact]
        /// <summary>
        /// Tests that the ItemTemplate property fires PropertyChanged event when the value changes.
        /// Verifies that property change notifications are properly triggered when ItemTemplate is modified.
        /// Expected result: PropertyChanged event should be fired with the correct property name.
        /// </summary>
        public void ItemTemplate_PropertyChangedNotification_FiresWhenChanged()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var dataTemplate = new DataTemplate();
            var propertyChangedFired = false;
            string changedPropertyName = null;

            multiPage.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(MultiPage<ContentPage>.ItemTemplate))
                {
                    propertyChangedFired = true;
                    changedPropertyName = e.PropertyName;
                }
            };

            // Act
            multiPage.ItemTemplate = dataTemplate;

            // Assert
            Assert.True(propertyChangedFired);
            Assert.Equal(nameof(MultiPage<ContentPage>.ItemTemplate), changedPropertyName);
        }

        [Fact]
        /// <summary>
        /// Tests that setting ItemTemplate to the same value does not fire unnecessary PropertyChanged events.
        /// Verifies that setting the same DataTemplate instance multiple times doesn't trigger redundant notifications.
        /// Expected result: PropertyChanged event should only fire once when the value actually changes.
        /// </summary>
        public void ItemTemplate_SetSameValue_DoesNotFirePropertyChanged()
        {
            // Arrange
            var multiPage = new TestMultiPage();
            var dataTemplate = new DataTemplate();
            multiPage.ItemTemplate = dataTemplate;

            var propertyChangedCount = 0;
            multiPage.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(MultiPage<ContentPage>.ItemTemplate))
                {
                    propertyChangedCount++;
                }
            };

            // Act
            multiPage.ItemTemplate = dataTemplate;

            // Assert
            Assert.Equal(0, propertyChangedCount);
        }
    }
}